using System;
using System.Collections.Generic;
using System.Text;

namespace VideoProcessorModule
{
    public class UploadThreshold
    {
        private static int s_threshold = 1;

        // Count many zero-feature images have been seen per camera
        private static Dictionary<string, int> s_perCameraCounters = new Dictionary<string, int>();

        public static int Threshold
        {
            get { return s_threshold; }
            set
            {
                if (value < 1)
                {
                    value = 1;
                }
                s_threshold = value;
            }
        }

        /// <summary>
        /// Keeps track of zero-result images on a per-camera basis. Always returns
        /// true for non-zero featureCount, and returns True for every 
        /// Threshold-th 0 feature count
        /// </summary>
        /// <param name="cameraId"></param>
        /// <param name="featureCount"></param>
        /// <returns>True if the image should be uploaded</returns>
        public static bool ShouldUpload(string cameraId, int featureCount)
        {
            if (!s_perCameraCounters.TryGetValue(cameraId, out int countForThisCamera))
            {
                // Always send the first one
                s_perCameraCounters.Add(cameraId, 0);
                return true;
            }
            else
            {
                if(featureCount > 0)
                {
                    s_perCameraCounters[cameraId] = 0;
                    return true;
                }
                else
                {
                    s_perCameraCounters[cameraId] = s_perCameraCounters[cameraId] + 1;
                    if (s_perCameraCounters[cameraId] >= s_threshold)
                    {
                        s_perCameraCounters[cameraId] = 0;
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
        }
    }
}
