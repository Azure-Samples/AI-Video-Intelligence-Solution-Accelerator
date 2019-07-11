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

                Console.WriteLine("Starting the gRPC server");
                string hostName = "0.0.0.0";
                GrpcServer server = new GrpcServer(hostName, OnImageRequestReceived);
                server.Start();

                Console.WriteLine($"Grpc server listening as {hostName} on port {(int)(CameraServer.PortNumbers.CameraServerPort)}");
                Console.WriteLine("Press any key to stop the server...");
                Console.ReadKey();

                server.ShutdownAsync().Wait();

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failure: {ex.Message}");
            }
            finally
            {
                // Close the open cameras
                foreach (VideoCapture camera in g_cameras.Values)
                {
                    camera.Release();
                }
            }
        }

        static ImageReply OnImageRequestReceived(ImageRequest request)
        {
            Console.WriteLine($"    Received request for camera:{request.CameraHardwareId}");
            lock (SimpleLock)
            {
                try
                {
                    // This naive implementation uses integers as cameraHardwareId
                    bool parsedOk = int.TryParse(request.CameraHardwareId, out int cameraHardwareId);
                    if (!parsedOk)
                    {
                        return CreateErrorReply("Camera hardware Id must be an integer for this version of CameraServer");
                    }

                    // Get the requested camera
                    bool foundCamera = g_cameras.TryGetValue(cameraHardwareId, out VideoCapture camera);
                    if (!foundCamera)
                    {
                        return CreateErrorReply("Requested camera ID " + cameraHardwareId.ToString() + " not found");
                    }

                    // Grab a frame
                    Mat frame = new Mat();
                    camera.Read(frame);
                    if (!frame.Empty())
                    {
                        byte[] fullImageBytes = new byte[0];
                        Cv2.ImEncode(".jpeg", frame, out fullImageBytes);
                        Google.Protobuf.ByteString fullImage = Google.Protobuf.ByteString.CopyFrom(fullImageBytes);
                        Mat smallFrame = new Mat();
                        Cv2.Resize(frame, smallFrame, new Size(300, 300), 0, 0, InterpolationFlags.Area);
                        byte[] smallImageBytes = new byte[0];
                        Cv2.ImEncode(".png", smallFrame, out smallImageBytes);
                        Google.Protobuf.ByteString smallImage = Google.Protobuf.ByteString.CopyFrom(smallImageBytes);
                        return new ImageReply() { Error = string.Empty, FullImage = fullImage, SmallImage = smallImage };
                    }
                    else
                    {
                        return CreateErrorReply("Empty frame from camera ID " + cameraHardwareId.ToString());
                    }
                }
                catch (Exception ex)
                {
                    return CreateErrorReply("Error while processing frame for camera ID " + request.CameraHardwareId + ": " + ex.Message);
                }
            }
        }

        static ImageReply CreateErrorReply(string error)
        {
            Google.Protobuf.ByteString image = Google.Protobuf.ByteString.CopyFrom(new byte[0]);
            return new ImageReply() { Error = error, FullImage = image, SmallImage = image };
        }
    }
}
