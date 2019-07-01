using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BlobStorage;
using Newtonsoft.Json.Linq;
using VideoProcessorGrpc;

namespace CameraModule
{
    public class Camera
    {
        /// <summary>
        /// The list of all defined cameras
        /// </summary>
        public static List<Camera> s_cameras = new List<Camera>();

        public string CameraId { get; private set; }

        public string Port { get; private set; }

        public string PortType { get; private set; }

        private IImageSource ImageSource { get; set; }

        /// <summary>
        /// Add a camera as defined in Module Twin to the list
        /// </summary>
        /// <param name="cameraFromTwin"></param>
        public static void AddCamera(JObject cameraFromTwin)
        {
            Camera cam = new Camera(cameraFromTwin);
            // Ensure uniqueness for real enabled cameras
            if (cam.PortType != "disabled" && cam.PortType != "simulator")
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
                    throw new ApplicationException("Unknown camera hardware type");
            }

        }

        public static void DisconnectAll()
        {
            foreach(Camera camera in s_cameras)
            {
                camera.ImageSource.Disconnect();
            }
            s_cameras.Clear();
        }

        /// <summary>
        /// Construct a Camera object with the given semantic ID.
        /// </summary>
        /// <param name="camera">The camera object from Module Twin.</param>
        /// <remarks>This really should take a delegate to provide image notification.</remarks>
        private Camera(JObject camera)
        {
            CameraId = (string)camera["id"];
            Port = (string)camera["port"];
            PortType = (string)camera["type"];
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
            }

            return result;
        }
    }
}