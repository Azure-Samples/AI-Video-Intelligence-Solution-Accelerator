using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace VideoProcessorModule
{
    /// <summary>
    /// Encapsulates the details of the CPU model
    /// </summary>
    class CpuModel : IProcessImage
    {
        public const string CpuModelProcessorType = "CPU";

        public string ProcessorType { get { return CpuModelProcessorType; } }

        public List<ImageFeature> Process(Google.Protobuf.ByteString image)
        {
            string imageJson = MakeImageJson(image);
            if (imageJson != null)
            {
                CpuModelResponse response = InvokeCpuModel(imageJson);
                if (response != null)
                {
                    List<ImageFeature> result = new List<ImageFeature>();
                    for (int i = 0; i < response.classes.Length; i++)
                    {
                        ImageFeature feature = new ImageFeature(response.classes[i], 
                            response.scores[i], response.bboxes[i]);
                        result.Add(feature);
                    }
                    return result;
                }
            }
            return null;
        }

        /// <summary>
        /// Create a string in JSON format containing the representation
        /// of the image to be sent to the CPU-based model.
        /// </summary>
        /// <returns>
        /// A string containing the JSON representation of the image to be
        /// processed by the model.
        /// </returns>
        private string MakeImageJson(Google.Protobuf.ByteString image)
        {
            DateTime then = DateTime.Now;

            try
            {
                StringBuilder sb = new StringBuilder("{\"img\": [");

                // Rehydrate the .png file as a Bitmap
                using (MemoryStream stream = new MemoryStream(image.ToByteArray()))
                using (Bitmap bitmap = new Bitmap(stream))
                {
                    int width = bitmap.Width;
                    int height = bitmap.Height;

                    for (int i = 0; i < height; i++)
                    {
                        sb.Append('[');
                        for (int j = 0; j < width; j++)
                        {
                            var pixel = bitmap.GetPixel(j, i);
                            sb.AppendFormat("[{0},{1},{2}]", pixel.R, pixel.G, pixel.B);
                            if (j < width - 1)
                                sb.Append(',');
                        }

                        sb.Append(']');
                        if (i < height - 1)
                            sb.Append(',');
                    }
                    sb.Append("]}");
                }

                return sb.ToString();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error converting small image to JSON:");
                Console.WriteLine(ex);
                return "";
            }
            finally
            {
                // TODO: timing imageToJsonDuration = DateTime.Now - then;
            }
        }

        /// <summary>
        /// Call the CPU-based model for our image
        /// </summary>
        /// <param name="jsonContent">
        /// The JSON representing the image, to be sent to the model.
        /// </param>
        /// <returns>
        /// If the call is successful, returns a ModelResponse object
        /// representing the result of the call. Otherwise returns null.
        /// </returns>
        private CpuModelResponse InvokeCpuModel(string jsonContent)
        {
            const string url = "http://grocerymodel:5001/score";

            try
            {
                using (var client = new HttpClient())
                {
                    var content = new StringContent(jsonContent);
                    content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                    DateTime then = DateTime.Now;
                    var response = client.PostAsync(url, content).Result;
                    string text = response.Content.ReadAsStringAsync().Result;

                    Console.WriteLine($"POST return status code {response.StatusCode}");
                    Console.WriteLine(text);

                    if (response.IsSuccessStatusCode)
                    {
                        CpuModelResponse modelResponse = JsonConvert.DeserializeObject<CpuModelResponse>(text);
                        return modelResponse;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failure uploading to model.");
                Console.WriteLine(ex);
            }

            return null;
        }

        public void Disconnect()
        {
        }
    }
}
