using NumSharp;
using System;
using System.Collections.Generic;
using System.Text;

namespace FpgaClient
{
    public partial class FpgaPostProcess
    {
        static List<(NDArray, NDArray, NDArray, NDArray)> ComputeAnchors()
        {
            Tuple<int, int> imageShape = new Tuple<int, int>(300, 300);

            int[][] featureShapes =
            {
                new int[] { 37, 37, 4 },
                new int[] { 19, 19, 6 },
                new int[] { 10, 10, 6 },
                new int[] { 5, 5, 6 },
                new int[] { 3, 3, 4 },
                new int[] { 1, 1, 4 }
            };

            float[][] anchorSizes =
            {
                new float[] { 21.0F, 45.0F },
                new float[] { 45.0F, 99.0F },
                new float[] { 99.0F, 153.0F },
                new float[] { 153.0F, 207.0F },
                new float[] { 207.0F, 261.0F },
                new float[] { 261.0F, 315.0F }
            };

            float[][] anchorRatios =
            {
                new float[] { 2, .5F },
                new float[] { 2, .5F, 3, (float)(1.0/3) },
                new float[] { 2, .5F, 3, (float)(1.0/3) },
                new float[] { 2, .5F, 3, (float)(1.0/3) },
                new float[] { 2, .5F },
                new float[] { 2, .5F }
            };

            int[] anchorSteps = { 8, 16, 32, 64, 100, 300 };

            List<(NDArray, NDArray, NDArray, NDArray)> result = new List<(NDArray, NDArray, NDArray, NDArray)>();

            for (int i = 0; i < featureShapes.Length; i++)
            {
                var layerAnchors = ComputeLayerAnchors(imageShape,
                    featureShapes[i], anchorSizes[i], anchorRatios[i], anchorSteps[i]);
                result.Add(layerAnchors);
            }

            return result;
        }

        static (NDArray, NDArray, NDArray, NDArray) ComputeLayerAnchors(Tuple<int, int> imageShape,
            int[] featureShape, float[] anchorSizes, float[] anchorRatios, int anchorStep)
        {
            float offset = 0.5F;
            Type dtype = typeof(Single);

            NDArray g1 = np.arange(0F, (float)featureShape[0], 1F);
            NDArray g2 = np.arange(0F, (float)featureShape[1], 1F);
            var G = np.mgrid(g1, g2);
            var y = G.Item1;
            var x = G.Item2;

            y += offset;
            x += offset;

            y *= (float)((float)anchorStep / imageShape.Item1);
            x *= (float)((float)anchorStep / imageShape.Item2);

            y = np.expand_dims(y, -1);
            x = np.expand_dims(x, -1);

            var num_anchors = anchorSizes.Length + anchorRatios.Length;

            var zeros_shape = new Shape(new int[] { num_anchors });
            var h = np.zeros(zeros_shape, dtype);
            var w = np.zeros(zeros_shape, dtype);

            h[0] = anchorSizes[0] / imageShape.Item1;
            w[0] = anchorSizes[0] / imageShape.Item2;
            int di = 1;
            if (anchorSizes.Length > 1)
            {
                h[1] = Math.Sqrt(anchorSizes[0] * anchorSizes[1]) / imageShape.Item1;
                w[1] = Math.Sqrt(anchorSizes[0] * anchorSizes[1]) / imageShape.Item2;
                di += 1;
            }
            for (int i = 0; i < anchorRatios.Length; i++)
            {
                h[i + di] = anchorSizes[0] / imageShape.Item1 / Math.Sqrt(anchorRatios[i]);
                w[i + di] = anchorSizes[0] / imageShape.Item2 * Math.Sqrt(anchorRatios[i]);
            }

            return (y, x, h, w);
        }
    }
}
