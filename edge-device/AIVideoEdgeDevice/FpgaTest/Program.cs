using FpgaClient;
using NumSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using Tensorflow;
using Tensorflow.Serving;

namespace FpgaTest
{
    class Program
    {
        static void Main(string[] args)
        {
            var imageFile = @"..\..\..\..\EdgeDevice\modules\CameraModule\simulated-images\300x300\counter.png";

            // Change this IP address to point to your DBE's compute role IP address
            var client = new FpgaClient.FpgaClient("192.168.1.192", 50051);

            float[] offsets = new float[] { 123, 117, 104 };
            NDArray rrr = new NDArray(offsets);

            using (Bitmap bitmap = new Bitmap(imageFile))
            {
                int width = bitmap.Width;
                int height = bitmap.Height;
                float[] values = new float[3 * width * height];
                int valuesIdx = 0;

                for (int i = 0; i < height; i++)
                {
                    for (int j = 0; j < width; j++)
                    {
                        var pixel = bitmap.GetPixel(j, i);
                        values[valuesIdx++] = pixel.B - offsets[0];
                        values[valuesIdx++] = pixel.G - offsets[1];
                        values[valuesIdx++] = pixel.R - offsets[2];
                    }
                }
                int[] shape = new int[] { 1, 300, 300, 3 };
                Tuple<float[], int[]> tuple = new Tuple<float[], int[]>(values, shape);
                Dictionary<string, Tuple<float[], int[]>> inputs = new Dictionary<string, Tuple<float[], int[]>>
                {
                    { "brainwave_ssd_vgg_1_Version_0.1_input_1:0", tuple }
                };

                IScoringRequest request = new FloatRequest(inputs);
                try
                {

                    PredictResponse response = client.Predict(request.MakePredictRequest());
                    FpgaPostProcess.PostProcess(response, selectThreshold: 0.5F, jaccardThreshold: 0.45F);
                    //var vs = result.Values;
                    //var ks = result.Keys;
                    //for (int i = 0; i < result.GetLength(0); i++)
                    //{
                    //    //Console.WriteLine($"Batch {i}:");
                    //    //var length = result.GetLength(1);
                    //    //var results = new Dictionary<int, float>();
                    //    //for (int j = 0; j < length; j++)
                    //    //{
                    //    //    results.Add(j, result[i, j]);
                    //    //}

                    //    //foreach (var kvp in results.Where(x => x.Value > 0.001).OrderByDescending(x => x.Value).Take(5))
                    //    //{
                    //    //    Console.WriteLine(
                    //    //        $"    {GetLabel(kvp.Key)} {kvp.Value * 100}%");
                    //    //}
                    //}
                    int j = 0;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Prediction request failed: {ex.Message}");
                }
            }
        }
    }
}
