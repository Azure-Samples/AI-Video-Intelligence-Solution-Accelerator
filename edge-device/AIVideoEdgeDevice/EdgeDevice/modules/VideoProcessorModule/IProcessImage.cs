using System;
using System.Collections.Generic;
using System.Text;
using FpgaGrpc;

namespace VideoProcessorModule
{
    /// <summary>
    /// ImageFeature defines a feature that ML recognized 
    /// </summary>
    public class ImageFeature
    {
        public ImageFeature(int featureClass, double score,
            double bbymin, double bbxmin, double bbymax, double bbxmax)
        {
            FeatureClass = featureClass;
            Score = score;
            BbYMin = bbymin;
            BbXMin = bbxmin;
            BbYMax = bbymax;
            BbXMax = bbxmax;
        }

        public ImageFeature(int featureClass, double score, double[] box)
        {
            FeatureClass = featureClass;
            Score = score;
            BbYMin = box[0];
            BbXMin = box[1];
            BbYMax = box[2];
            BbXMax = box[3];
        }

        public int FeatureClass { get; private set; }
        public double Score { get; private set; }
        public double BbYMin { get; private set; }
        public double BbXMin { get; private set; }
        public double BbYMax { get; private set; }
        public double BbXMax { get; private set; }
    }

    interface IProcessImage
    {
        /// <summary>
        /// Process the 300x300 input image
        /// </summary>
        /// <param name="image"></param>
        /// <returns>Null on error, otherwise a list of recognized features</returns>
        List<ImageFeature> Process(Google.Protobuf.ByteString image);

        string ProcessorType { get; }

        void Disconnect();
    }
}
