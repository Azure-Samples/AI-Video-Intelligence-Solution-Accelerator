using System;
using System.Collections.Generic;
using System.Text;
using Tensorflow.Serving;
using NumSharp;
using VideoProcessorModule;
using NumSharp.Utilities;

namespace FpgaClient
{
    public partial class FpgaPostProcess
    {
        static readonly List<(NDArray, NDArray, NDArray, NDArray)> g_ssdAnchors;

        static string[] tensorOutputs =
        {
            "ssd_300_vgg/block4_box/Reshape_1:0",
            "ssd_300_vgg/block7_box/Reshape_1:0",
            "ssd_300_vgg/block8_box/Reshape_1:0",
            "ssd_300_vgg/block9_box/Reshape_1:0",
            "ssd_300_vgg/block10_box/Reshape_1:0",
            "ssd_300_vgg/block11_box/Reshape_1:0",
            "ssd_300_vgg/block4_box/Reshape:0",
            "ssd_300_vgg/block7_box/Reshape:0",
            "ssd_300_vgg/block8_box/Reshape:0",
            "ssd_300_vgg/block9_box/Reshape:0",
            "ssd_300_vgg/block10_box/Reshape:0",
            "ssd_300_vgg/block11_box/Reshape:0"
        };

        static FpgaPostProcess()
        {
            g_ssdAnchors = ComputeAnchors();
        }

        public static List<ImageFeature> PostProcess(PredictResponse networkOutput_raw, 
            float selectThreshold, float jaccardThreshold)
        {
            List<NDArray> networkOutput = new List<NDArray>();
            foreach(string str in tensorOutputs)
            {
                NDArray ndarray = NDArrayEx.FromTensorProto(networkOutput_raw.Outputs[str]);
                if (ndarray.shape.Length != 5)
                {
                    ndarray = np.expand_dims(ndarray, axis: 0);
                    if (ndarray.shape.Length != 5)
                    {
                        throw new ApplicationException($"NDArray shape length expected 5, is {ndarray.shape.Length}");
                    }
                }
                networkOutput.Add(ndarray);
            }

            List<ImageFeature> result = new List<ImageFeature>();

            ExtractDetections(networkOutput.GetRange(0, 6), networkOutput.GetRange(6, 6), g_ssdAnchors, 
                selectThreshold, jaccardThreshold, imageShape: (300, 300), numClasses: 21);



            return result;
        }

        static (NDArray, NDArray, NDArray) ExtractDetections(List<NDArray> predictions, List<NDArray> localizations,
            List<(NDArray, NDArray, NDArray, NDArray)> ssdAnchors,
            float selectThreshold, float jaccardThreshold, (int, int) imageShape, int numClasses)
        {
            var rbboxImg = new float[]{0.0F, 0.0F, 1.0F, 1.0F};

            List<NDArray> l_classes = new List<NDArray>();
            List<NDArray> l_scores = new List<NDArray>();
            List<NDArray> l_bboxes = new List<NDArray>();
            for (int i = 0; i < predictions.Count; i++)
            {
                NDArray preds = SoftMax(predictions[i], axis: 4);
                (NDArray classesIn, NDArray scoresIn, NDArray bboxesIn) = 
                    SelectLayerBoxes(preds, localizations[i], ssdAnchors[i], 
                        selectThreshold, jaccardThreshold, imageShape, numClasses);
                if (classesIn.size > 0)
                {
                    l_classes.Add(classesIn);
                    l_scores.Add(scoresIn);
                    l_bboxes.Add(bboxesIn);
                }
            }

            if (l_classes.Count > 0)
            {
                var classes = np.concatenate(l_classes.ToArray(), 0);
                var scores = np.concatenate(l_scores.ToArray(), 0);
                var bboxes = np.concatenate(l_bboxes.ToArray(), 0);

                ClipBBoxes(rbboxImg, bboxes);
                (classes, scores, bboxes) = SortBBoxes(classes, scores, bboxes);
                (classes, scores, bboxes) = SelectBBoxes(classes, scores, bboxes, jaccardThreshold);
                // Lots of work here

                return (classes, scores, bboxes);
            }
            else
            {
                var emptyFloats = new NDArray(new float[0]);
                return (new NDArray(new int[0]), emptyFloats, emptyFloats);
            }
        }

        static unsafe (NDArray, NDArray, NDArray) SelectBBoxes(NDArray classesIn, 
            NDArray scores, NDArray bboxes, float jaccardThreshold)
        {
            int boxCount = scores.size;
            var keepBboxes = new bool[boxCount];
            for (int i = 0; i < keepBboxes.Length; i++)
            {
                keepBboxes[i] = true;
            }
            var classes = classesIn.GetData<int>();
            // Examine following boxes for redundancy. We don't need to examine
            // the last box since nothing follows it, so we just go to boxCount -1
            for (int i = 0; i < boxCount - 1; i++)
            {
                if (keepBboxes[i])
                {
                    var t0 = bboxes[i];
                    string range = string.Format($"{i + 1}:");
                    var t1 = bboxes[range];
                    var overlap = JaccardBoxes(t0, t1);
                    int overlapIdx = 0;
                    for (int j = i + 1; j < boxCount; j++)
                    {
                        // Discard following boxes whose classes are the same
                        // and which have a high enough Jaccard overlap.
                        if (overlap[overlapIdx] > jaccardThreshold && classes[j] == classes[i])
                        {
                            keepBboxes[j] = false;
                        }
                        overlapIdx++;
                    }
                }
            }
            List<int> indices = new List<int>();
            for (int i = 0; i < boxCount; i++)
            {
                if (keepBboxes[i])
                {
                    indices.Add(i);
                }
            }
            // NumSharp currently has no "where"
            int resultCount = indices.Count;
            var c = new int[resultCount];
            var s = new float[resultCount];
            var bb = new NDArray[resultCount];
            for (int i = 0; i < resultCount; i++)
            {
                c[i] = classes[indices[i]];
                s[i] = scores[indices[i]];
                bb[i] = bboxes[indices[i]].copy();
            }
            var b = np.vstack(bb);
            return (c, s, b);
        }

        static unsafe float[] JaccardBoxes(NDArray bbox, NDArray bboxes)
        {
            // NumSharp does not yet have maximum or minimum, so we iterate
            int boxCount = bboxes.shape[0];
            var boxes = bboxes.GetData<float>();
            var box = bbox.GetData<float>();
            float[] jaccard = new float[boxCount];
            float volBox = (box[2] - box[0]) * (box[3] - box[1]);

            for (int i = 0; i < boxCount; i++)
            {
                int start = i * 4;
                float intYmin = Math.Max(box[0], boxes[start]);
                float intXmin = Math.Max(box[1], boxes[start + 1]);
                float intYmax = Math.Min(box[2], boxes[start + 2]);
                float intXmax = Math.Min(box[3], boxes[start + 3]);

                float intH = Math.Max(intYmax - intYmin, 0);
                float intW = Math.Max(intXmax - intXmin, 0);
                float intVolume = intH * intW;

                float volThiBox = (boxes[start + 2] - boxes[start + 0]) * (boxes[start + 3] - boxes[start + 1]);
                jaccard[i] = intVolume / (volBox + volThiBox - intVolume);
            }

            return jaccard;
        }

        static (NDArray, NDArray, NDArray) SortBBoxes(NDArray classes, NDArray scores, NDArray bboxes, int topK = 400)
        {
            var idxes = np.argsort<float>(-scores);
            string range = string.Format($":{topK}");
            var c = classes[idxes][range];
            var s = scores[idxes][range];
            var b = bboxes[idxes][range];
            return (c, s, b);
        }

        static unsafe void ClipBBoxes(float[] bboxRef, NDArray bboxes)
        {
            int boxCount = bboxes.shape[0];
            var data = bboxes.GetData<float>();
            for (int i = 0; i < boxCount; i++)
            {
                int start = i * 4;
                if (data[start] < bboxRef[0])
                {
                    data[start] = bboxRef[0];
                }
                if (data[start + 1] < bboxRef[1])
                {
                    data[start] = bboxRef[1];
                }
                if (data[start + 2] > bboxRef[2])
                {
                    data[start] = bboxRef[2];
                }
                if (data[start + 3] > bboxRef[3])
                {
                    data[start] = bboxRef[3];
                }
            }
        }

        static (NDArray, NDArray, NDArray) SelectLayerBoxes(
            NDArray predictions, 
            NDArray localizationsIn,
            (NDArray, NDArray, NDArray, NDArray) anchors,
            float selectThreshold,
            float nmsThreshold,
            (int, int) imageShape,
            int numClasses
            )
        {
            NDArray localizations = DecodeLayerBoxes(localizationsIn, anchors);

            var pShape = predictions.shape;
            var batchSize = pShape.Length == 5 ? pShape[0] : 1;
            predictions = np.reshape(predictions, batchSize, -1, pShape[pShape.Length - 1]);
            var lShape = localizations.shape;
            localizations = np.reshape(localizations, batchSize, -1, lShape[lShape.Length - 1]);

            if (selectThreshold == 0.0)
            {
                //# Class prediction and scores: assign 0. to 0-class
                //classes = np.argmax(predictions, axis = 2)
                //scores = np.amax(predictions, axis = 2)
                //mask = (classes > 0)
                //classes = classes[mask]
                //scores = scores[mask]
                //bboxes = localizations[mask]
                throw new ApplicationException("Logic for selectThreshold == 0.0 is not implemented");
            }

            var subPredictions = predictions[":, :, 1:"];
            bool notDone = true;
            NDCoordinatesIncrementor inc = new NDCoordinatesIncrementor(subPredictions.shape, (x) => { notDone = false; });
            int[] idx = inc.Index;
            var found = new List<NDArray>();
            var foundShape = new Shape(3, 1);
            while (notDone)
            {
                float val = subPredictions[idx];
                if (val >= selectThreshold)
                {
                    found.Add(new NDArray((int[])idx.Clone(), foundShape));
                }
                idx = inc.Next();
            }

            if (found.Count > 0)
            {
                var foundArray = found.ToArray();
                var indices = np.hstack(foundArray);

                var classes = indices[2] + 1;

                int foundCount = indices.shape[1];
                float[] scores = new float[foundCount];
                var locsIndices = indices[":-1"];
                NDArray[] bboxes = new NDArray[foundCount];
                // NumSharp array indexing is not complete yet
                // We really wanted 
                // var scores = subPredictions[indices];
                // and 
                // var bboxes = localizations[indices[":-1"]];

                for (int i = 0; i < foundCount; i++)
                {
                    int i0 = indices[0, i];
                    int i1 = indices[1, i];
                    int i2 = indices[2, i];
                    scores[i] = subPredictions[i0, i1, i2];
                    int j0 = locsIndices[0, i];
                    int j1 = locsIndices[1, i];
                    bboxes[i] = np.expand_dims(localizations[j0, j1], 0);
                }

                NDArray bboxesResult = np.concatenate(bboxes, 0);

                return (classes, scores, bboxesResult);
            }
            else
            {
                return (new NDArray(typeof(int)), null, null);
            }

        }

        static NDArray DecodeLayerBoxes(NDArray localizations, (NDArray, NDArray, NDArray, NDArray) anchors)
        {
            float[] priorScaling = new float[] { 0.1F, 0.1F, 0.2F, 0.2F };

            int[] lShape = localizations.shape;
            int idxNeg1 = lShape[lShape.Length - 1];
            int idxNeg2 = lShape[lShape.Length - 2];

            int[] tempShape = { -1, idxNeg2, idxNeg1 };

            localizations = np.reshape(localizations, tempShape);
            (NDArray yref, NDArray xref, NDArray href, NDArray wref) = anchors;

            xref = np.reshape(xref, new int[] { -1, 1 });
            yref = np.reshape(yref, new int[] { -1, 1 });

            var localizations0 = localizations[":, :, 0"];
            var localizations1 = localizations[":, :, 1"];
            var localizations2 = localizations[":, :, 2"];
            var localizations3 = localizations[":, :, 3"];
            // These clones work around bugs in pre-release slicing
            localizations0 = localizations0.Clone();
            localizations1 = localizations1.Clone();
            localizations2 = localizations2.Clone();
            localizations3 = localizations3.Clone();

            var cx1 = localizations0 * wref;
            var cx0 = cx1 * priorScaling[0];
            var cx = cx0 + xref;

            var cy1 = localizations1 * href;
            var cy0 = cy1 * priorScaling[1];
            var cy = cy0 + yref;

            var w = wref * np.exp(localizations2 * priorScaling[2]);
            var h = href * np.exp(localizations3 * priorScaling[3]);

            var bboxes = np.zeros_like(localizations);
            bboxes[":, :, 0"] = cy - h / 2.0;
            bboxes[":, :, 1"] = cx - w / 2.0;
            bboxes[":, :, 2"] = cy + h / 2.0;
            bboxes[":, :, 3"] = cx + w / 2.0;

            bboxes = np.reshape(bboxes, lShape);

            return bboxes;
        }

        static NDArray SoftMax(NDArray x, int axis)
        {
            // Original Python
            //e_x = np.exp(x - np.expand_dims(np.max(x, axis = axis), axis))
            //result = e_x / np.expand_dims(np.sum(e_x, axis = axis), axis)
            //return result

            var max = np.max(x, axis: axis);
            var maxExpanded = np.expand_dims(max, axis);
            var e_xMinusMaxExpanded = np.exp(x - maxExpanded);

            var sum = np.sum(e_xMinusMaxExpanded, axis: axis);
            var sumExpanded = np.expand_dims(sum, axis);
            var result = e_xMinusMaxExpanded / sumExpanded;

            return result;
        }
    }
}
