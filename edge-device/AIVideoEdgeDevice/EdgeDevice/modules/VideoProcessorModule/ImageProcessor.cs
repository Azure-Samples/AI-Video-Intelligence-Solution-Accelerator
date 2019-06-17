using System;
using System.Drawing;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;

using BlobStorage;
using VideoProcessorGrpc;

namespace VideoProcessorModule
{
    /// <summary>
    /// Handles the processing of a received message
    /// </summary>
    internal class ImageProcessor
    {
        #region Instance data
        private ModuleClient moduleClient;
        private BlobStorageHelper blobHelper;
        private ImageBody body;
        private ModelResponse recognition;
        int numRecognitionMessages = 0;
        private TimeSpan recognitionDuration = new TimeSpan(0);
        private TimeSpan uploadDuration = new TimeSpan(0);
        private TimeSpan imageToJsonDuration = new TimeSpan(0);
        private TimeSpan recognitionMessageDuration = new TimeSpan(0);
        private TimeSpan totalDuration = new TimeSpan(0);

        #endregion Instance data

        #region Properties
        /// <summary>
        /// Return true if the ML module recognized anything.
        /// </summary>
        private bool IsRecognized
        {
            get { return recognition != null && !recognition.IsEmpty; }
        }
        #endregion Properties

        #region Construction
        /// <summary>
        /// Construct an ImageProcess object.
        /// </summary>
        /// <param name="blobHelper">
        /// A BlobStorageHelper object used to upload the image to storage.
        /// </param>
        public ImageProcessor(BlobStorageHelper blobHelper, ModuleClient client)
        {
            this.moduleClient = client;
            this.blobHelper = blobHelper;
            this.recognition = null;
        }
        #endregion Construction

        #region Public methods
        /// <summary>
        /// Process the image. The image is given to the model ML and if a
        /// valid recognition is found then the image is uploaded to storage.
        /// Processing times are tracked.
        /// </summary>
        /// <param name="body">
        /// An ImageBody object containing the image from the camera.
        /// </param>
        /// <param name="forceUpload">
        /// Specifies whether an upload should be done even if there was no recognition.
        /// </param>
        /// <returns>
        /// True if the image was uploaded to storage, false otherwise
        /// </returns>
        public bool Process(ImageBody body, bool forceUpload)
        {
            bool rv = false;
            DateTime startTime = DateTime.Now;

            this.body = body;
            recognition = GetModelRecognition();

            if (IsRecognized || forceUpload)
            {
                if (recognition != null)
                    Console.WriteLine($"Result contains {recognition.scores.Length} empty spaces.");

                byte[] imageBytes = body.Image.ToByteArray();
                DateTime then = DateTime.Now;
                Task task = blobHelper.UploadBlobAsync(body.CameraId,
                                                       body.Time,
                                                       body.Type,
                                                       imageBytes);
                task.Wait();
                uploadDuration = DateTime.Now - then;
                Console.WriteLine($"Uploaded {imageBytes.LongLength} byte image to storage");
                SendRecognitionMessages(recognition);
                SendImageMessage();
                rv = true;
            }

            if (!IsRecognized)
                Console.WriteLine("Image isn't recognized");

            totalDuration = DateTime.Now - startTime;
            Console.WriteLine($"Image processing took {totalDuration.TotalMilliseconds} ms");
            Console.WriteLine($"  Conversion of image to JSON took {imageToJsonDuration.TotalMilliseconds} ms");
            Console.WriteLine($"  Recognition took {recognitionDuration.TotalMilliseconds} ms");
            if (rv == true) // upload happened
            {
                Console.WriteLine($"  Upload took {uploadDuration.TotalMilliseconds} ms");
                Console.WriteLine($"  Sending {numRecognitionMessages} recognition messages took {recognitionMessageDuration.TotalMilliseconds} ms");
            }

            return rv;
        }
        #endregion Public methods

        #region Private methods
        /// <summary>
        /// Create a string in JSON format containing the representation
        /// of the image to be sent to the CPU-based model.
        /// </summary>
        /// <returns>
        /// A string containing the JSON representation of the image to be
        /// processed by the model.
        /// </returns>
        private string MakeImageJson()
        {
            DateTime then = DateTime.Now;

            try
            {
                StringBuilder   sb = new StringBuilder("{\"img\": [");

                // Rehydrate the .png file as a Bitmap
                using (MemoryStream stream = new MemoryStream(body.SmallImage.ToByteArray()))
                using (Bitmap bitmap = new Bitmap(stream))
                {
                    int width  = bitmap.Width;
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
                imageToJsonDuration = DateTime.Now - then;
            }
        }

        /// <summary>
        /// Send the image to the model ML and get the response.
        /// </summary>
        /// <returns>
        /// If invoking the model ML succeeds, returns a ModelResponse object
        /// representing the result of the call. Otherwise returns null.
        /// </returns>
        private ModelResponse GetModelRecognition()
        {
            string jsonContent = MakeImageJson();

            return InvokeCpuModel(jsonContent);
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
        private ModelResponse InvokeCpuModel(string jsonContent)
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
                    recognitionDuration = DateTime.Now - then;

                    Console.WriteLine($"POST return status code {response.StatusCode}");
                    Console.WriteLine(text);

                    if (response.IsSuccessStatusCode)
                    {
                        ModelResponse modelResponse = JsonConvert.DeserializeObject<ModelResponse>(text);
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

        private void SendIotHubMessage(string messageJson, string schema)
        {
            var message = new Message(Encoding.UTF8.GetBytes(messageJson))
            {
                CreationTimeUtc = DateTime.UtcNow,
                MessageSchema = schema
            };

            Task msgTask = moduleClient.SendEventAsync(message);
            msgTask.Wait();
            if (msgTask.IsCompletedSuccessfully)
                Console.WriteLine("{1}--Sent: {0}", messageJson, DateTime.Now);
            else
                throw new ApplicationException("Failed to send IoT Hub message");
        }

        /// <summary>
        /// Send recognition messages to the hub, one for each space recognized.
        /// </summary>
        /// <param name="recognition"></param>
        private void SendRecognitionMessages(ModelResponse recognition)
        {
            const string schema = "recognition:v1";

            if (recognition != null)
            {
                DateTime then = DateTime.Now;
                for (int i=0; i < recognition.classes.Length; i++)
                {
                    try
                    {
                        var telemetryDataPoint = new
                        {
                            cameraId = body.CameraId,
                            time = body.Time,
                            cls = recognition.classes[i],
                            score = recognition.scores[i],
                            bbymin = recognition.bboxes[i][0],
                            bbxmin = recognition.bboxes[i][1],
                            bbymax = recognition.bboxes[i][2],
                            bbxmax = recognition.bboxes[i][3]
                        };
                        var messageJson = JsonConvert.SerializeObject(telemetryDataPoint);

                        SendIotHubMessage(messageJson, schema);
                        ++numRecognitionMessages;
                    }
                    catch(Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
                recognitionMessageDuration = DateTime.Now - then;
            }
        }

        private void SendImageMessage()
        {
            const string schema = "image-upload:v1";

            var telemetryDataPoint = new
                {
                    cameraId = body.CameraId,
                    time = body.Time,
                    type = body.Type
                };
            var messageJson = JsonConvert.SerializeObject(telemetryDataPoint);

            SendIotHubMessage(messageJson, schema);
        }
        #endregion Private methods
    }
}
