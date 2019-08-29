// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using OpenCvSharp;

namespace CameraServer
{
    class Program
    {
        static Dictionary<int, VideoCapture> g_cameras = new Dictionary<int, VideoCapture>();
        static readonly object SimpleLock = new object();

        static void Main(string[] args)
        {
            try
            {
                OpenCVDeviceEnumerator devEnum = new OpenCVDeviceEnumerator();
                List<int> cameraIds = devEnum.EnumerateCameras();

                foreach (int cameraId in cameraIds)
                {
                    VideoCapture camera = new VideoCapture(cameraId);
                    if (camera.IsOpened())
                    {
                        g_cameras.Add(cameraId, camera);
                    }
                    else
                    {
                        throw new ApplicationException($"Failed to open detected camera {cameraId}");
                    }
                }

                // Show the camera views if requested
                if (args.Length > 0 && args[0] == "-id")
                {
                    Console.WriteLine("Showing camera views.");
                    Console.WriteLine("Close all camera views to start server.");
                    Console.WriteLine("Server will not start until the camera views are closed.");
                    List<Window> windows = new List<Window>();
                    try
                    {
                        foreach (int cameraId in cameraIds)
                        {
                            Mat frame = new Mat();
                            VideoCapture camera = g_cameras[cameraId];
                            camera.Read(frame);
                            windows.Add(new Window("CameraID: " + cameraId.ToString(), frame));
                        }
                        Cv2.WaitKey();
                    }
                    finally
                    {
                        foreach(Window window in windows)
                        {
                            window.Close();
                            window.Dispose();
                        }
                    }
                }

                Console.WriteLine("Starting the server");

                Server.Start();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failure: {ex.Message}");
            }

            Console.WriteLine($"Server listening on port {Server.Port}");
            Console.WriteLine("Press any key to stop the server...");
            Console.ReadKey();

            Server.Stop();

            // Close the open cameras
            foreach (VideoCapture camera in g_cameras.Values)
            {
                camera.Release();
            }
        }

        internal static (byte[], string) OnImageRequestReceived(string cameraId)
        {
            Console.WriteLine($"    Received request for camera:{cameraId}");
            lock (SimpleLock)
            {
                try
                {
                    // This naive implementation uses integers as cameraHardwareId
                    bool parsedOk = int.TryParse(cameraId, out int cameraHardwareId);
                    if (!parsedOk)
                    {
                        return (null, "Camera hardware Id must be an integer for this version of CameraServer");
                    }

                    // Get the requested camera
                    bool foundCamera = g_cameras.TryGetValue(cameraHardwareId, out VideoCapture camera);
                    if (!foundCamera)
                    {
                        return (null, "Requested camera ID " + cameraHardwareId.ToString() + " not found");
                    }

                    // Grab a frame
                    Mat frame = new Mat();
                    camera.Read(frame);
                    if (!frame.Empty())
                    {
                        byte[] fullImageBytes = new byte[0];
                        Cv2.ImEncode(".jpeg", frame, out fullImageBytes);
                        return (fullImageBytes, null);
                    }
                    else
                    {
                        return (null, "Empty frame from camera ID " + cameraHardwareId.ToString());
                    }
                }
                catch (Exception ex)
                {
                    return (null, "Error while processing frame for camera ID " + cameraId + ": " + ex.Message);
                }
            }
        }
    }
}
