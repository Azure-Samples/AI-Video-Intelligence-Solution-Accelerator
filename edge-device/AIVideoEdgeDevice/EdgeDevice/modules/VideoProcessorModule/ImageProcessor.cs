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
using System.Collections.Generic;

namespace VideoProcessorModule
{
    /// <summary>
    /// Handles the processing of a received message
    /// </summary>
    internal class ImageProcessor
    {
        private readonly ModuleClient moduleClient;
        private readonly BlobStorageHelper blobHelper;
        private readonly ImageBody body;
        private readonly string processorType;
        private List<ImageFeature> features;
        private TimeSpan recognitionDuration;

        private static readonly Object reportLock = new object();

        private ImageProcessor(BlobStorageHelper blobHelper, ModuleClient client, string processorType, ImageBody body)
        {
            this.moduleClient = client;
            this.blobHelper = blobHelper;
            this.processorType = processorType;
            this.body = body;
        }

        /// <summary>
        /// Process the image. The image is given to the model ML and if a
        /// valid recognition is found then the image is uploaded to storage.
        /// Processing times are tracked.
        /// </summary>
        /// <param name="body">
        /// An ImageBody object containing the image from the camera.
        /// </param>
        public static void Process(BlobStorageHelper blobHelper, ModuleClient client, IProcessImage imageProc, ImageBody body)
        {
            ImageProcessor proc = new ImageProcessor(blobHelper, client, imageProc.ProcessorType, body);

            // Perform and measure elapsed time for the ML model work
            DateTime startTime = DateTime.Now;
            proc.features = imageProc.Process(body.SmallImage);
            proc.recognitionDuration = DateTime.Now - startTime;

            // Loop to the next recognition task without waiting for the report to process
            if (proc.features != null)
            {
                Task reportTask = new Task(() => proc.Report());
                reportTask.Start();
            }
        }

        private void Report()
        {
            lock(reportLock)
            {
                Console.WriteLine($"Processing took the {processorType} {recognitionDuration.TotalMilliseconds} msec for {body.CameraId}   {body.Time}.");
                bool doUpload = UploadThreshold.ShouldUpload(body.CameraId, features.Count);
                if (doUpload)
                {
                    Console.WriteLine($"  Recognized {features.Count} features in {body.SmallImage.Length} byte 300x300 image.");

                    byte[] imageBytes = body.Image.ToByteArray();
                    DateTime uploadDurationStart = DateTime.Now;
                    string verb = "upload image to BLOB store";
                    try
                    {
                        Task task = blobHelper.UploadBlobAsync(body.CameraId,
                                                            body.Time,
                                                            body.Type,
                                                            imageBytes);
                        task.Wait();

                        TimeSpan blobUploadDuration = DateTime.Now - uploadDurationStart;
                        Console.WriteLine($"  BLOB upload took {blobUploadDuration.TotalMilliseconds} msec for  {imageBytes.LongLength} bytes");

                        DateTime messageStart = DateTime.Now;
                        verb = "send messages to IoT Hub";
                        SendRecognitionMessages();
                        SendImageMessage();
                        TimeSpan messagesDuration = DateTime.Now - messageStart;
                        Console.WriteLine($"  Sending messages took {messagesDuration.TotalMilliseconds} msec");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"  Failed to {verb}: {ex.Message}");
                    }
                }
                else
                {
                    Console.WriteLine($"  No features were recognized in {body.SmallImage.Length} byte 300x300 image. Not uploading to BLOB store.");
                }
            }
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
                Console.WriteLine("  {1}--Sent: {0}", messageJson, DateTime.Now);
            else
                throw new ApplicationException("Failed to send IoT Hub message");
        }

        /// <summary>
        /// Send recognition messages to the hub, one for each feature recognized.
        /// </summary>
        private void SendRecognitionMessages()
        {
            const string schema = "recognition:v1";

            foreach (ImageFeature feature in this.features)
            {
                try
                {
                    var telemetryDataPoint = new
                    {
                        cameraId = body.CameraId,
                        time = body.Time,
                        cls = feature.FeatureClass,
                        score = feature.Score,
                        bbymin = feature.BbYMin,
                        bbxmin = feature.BbXMin,
                        bbymax = feature.BbYMax,
                        bbxmax = feature.BbXMax
                    };
                    var messageJson = JsonConvert.SerializeObject(telemetryDataPoint);

                    SendIotHubMessage(messageJson, schema);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }

        private void SendImageMessage()
        {
            const string schema = "image-upload:v1";

            // Report processing time as milliseconds with one decimal place
            double reportedMsec = double.Parse(this.recognitionDuration.TotalMilliseconds.ToString("#.#"));

            var telemetryDataPoint = new
            {
                cameraId = body.CameraId,
                time = body.Time,
                type = body.Type,
                featureCount = this.features != null ? this.features.Count : 0,
                procType = this.processorType,
                procMsec = reportedMsec
            };
            var messageJson = JsonConvert.SerializeObject(telemetryDataPoint);

            SendIotHubMessage(messageJson, schema);
        }
    }
}
