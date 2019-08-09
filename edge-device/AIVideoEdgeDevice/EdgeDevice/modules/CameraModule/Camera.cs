using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BlobStorage;
using Newtonsoft.Json.Linq;
using VideoProcessorGrpc;

namespace CameraModule
{
    public class Camera
    {
        private bool isDone = true;
        private readonly GrpcClient grpcClient = null;
        /// <summary>
        /// The list of all defined cameras
        /// </summary>
        public static List<Camera> s_cameras = new List<Camera>();

        public string CameraId { get; private set; }

        public string Port { get; private set; }

        public string PortType { get; private set; }

        public int MillisecondsBetweenImages { get; private set; }

        public bool IsDisabled { get; private set; }

        /// <summary>
        /// We don't want to do ML processing when the Camera is collecting training images
        /// </summary>
        public bool SkipMLProcessing { get; private set; }

        private IImageSource ImageSource { get; set; }

        private Task GetImageTask { get; set; }

        /// <summary>
        /// Add a camera as defined in Module Twin to the list
        /// </summary>
        /// <param name="cameraFromTwin"></param>
        public static void AddCamera(JObject cameraFromTwin, GrpcClient client)
        {
            Camera cam = new Camera(cameraFromTwin, client);
            // Ensure uniqueness for real enabled cameras
            if (cam.PortType != "disabled" && cam.IsDisabled == false && cam.PortType != "simulator")
            {
                foreach(Camera test in s_cameras)
                {
                    if (cam.CameraId == test.CameraId)
                    {
                        throw new ApplicationException("Camera Ids must be unique among all Edge Devices");
                    }
                    if (cam.PortType == test.PortType && cam.Port == test.Port)
                    {
                        throw new ApplicationException("Camera port definitions (Port and Type) must be unique within an Edge Device");
                    }
                }
            }
            
            if (!cam.IsDisabled)
            {
                switch (cam.PortType)
                {
                    case "simulator": cam.ImageSource = new Simulator(cam.Port);
                        s_cameras.Add(cam);
                        break;
                    case "CameraServer": cam.ImageSource = new CameraServerClient(cam.Port);
                        s_cameras.Add(cam);
                        break;
                    case "disabled":
                        break;
                    default:
                        throw new ApplicationException($"Unknown camera hardware type: {cam.PortType}");
                }
            }
        }

        public static void DisconnectAll()
        {
            foreach(Camera camera in s_cameras)
            {
                camera.Stop();
                camera.ImageSource.Disconnect();
            }
            s_cameras.Clear();
        }

        public static void StartAll()
        {
            foreach (Camera camera in s_cameras)
            {
                camera.Start();
            }
        }

        /// <summary>
        /// Construct a Camera object with the given semantic ID.
        /// </summary>
        /// <param name="camera">The camera object from Module Twin.</param>
        private Camera(JObject camera, GrpcClient client)
        {
            this.grpcClient = client;
            CameraId = (string)camera["id"];
            Port = (string)camera["port"];
            PortType = (string)camera["type"];
            var secondsBetweenImages = camera["secondsBetweenImages"];
            MillisecondsBetweenImages = (int)(1000 * 
                (secondsBetweenImages != null ? (double)secondsBetweenImages : 10.0));
            var disabled = camera["disabled"];
            IsDisabled = disabled != null ? (bool)disabled : false;
            var skipMLProcessing = camera["skipMLProcessing"];
            SkipMLProcessing = skipMLProcessing != null ? (bool)skipMLProcessing : false;
        }

        /// <summary>
        /// Retrieve an image and its metadata from a camera.
        /// </summary>
        /// <returns>The image and its metadata</returns>
        public ImageBody GetImage()
        {
            DateTime now = BlobStorageHelper.GetImageUtcTime();
            string nowString = BlobStorageHelper.FormatImageUtcTime(now);

            ImageBody result = this.ImageSource.RequestImages();
            if (result != null)
            {
                result.CameraId = CameraId;
                result.Time = nowString;
                result.Type = "jpg";
                result.SkipMlProcessing = this.SkipMLProcessing;
            }

            return result;
        }

        private void Start()
        {
            this.isDone = false;
            this.GetImageTask = new Task(ImageLoop);
            this.GetImageTask.Start();
        }

        private void Stop()
        {
            this.isDone = true;
            if (this.GetImageTask != null)
            {
                this.GetImageTask.Wait();
            }
        }

        private void ImageLoop()
        {
            while (!isDone)
            {
                try
                {
                    ImageBody body = this.GetImage();
                    if (body != null)
                    {
                        Console.WriteLine($"Sending a {body.Image.Length} byte image from {body.CameraId} at {body.Time}");
                        this.grpcClient.UploadImage(body);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error sending grpc message to VideoProcessor module: {ex.Message}");
                }
                Task.Delay(MillisecondsBetweenImages).Wait();
            }
        }
    }
}