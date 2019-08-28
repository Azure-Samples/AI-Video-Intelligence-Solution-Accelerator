using FpgaClient;
using Google.Protobuf;
using NumSharp;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using Tensorflow;
using Tensorflow.Serving;

namespace FpgaTest
{
    class Program
    {
        static void Main(string[] args)
        {
            var bigImage = @"..\..\..\..\EdgeDevice\modules\CameraModule\simulated-images\counter.jpg";
#if false
            byte[] bytes0 = File.ReadAllBytes(bigImage);

            DateTime now = DateTime.Now;
            for (int i = 0; i < 1000; i++)
            {
                var src = Cv2.ImDecode(bytes0, ImreadModes.Color);
                OpenCvSharp.Size size = new OpenCvSharp.Size(300, 300);

                Mat dest = new Mat();

                Cv2.Resize(src, dest, size, 0, 0, InterpolationFlags.Area);

            }
            TimeSpan diff = DateTime.Now - now;
            Console.WriteLine($"Took {diff.ToString()}");
            return;
#endif

            //var imageFile = @"..\..\..\..\EdgeDevice\modules\CameraModule\simulated-images\300x300\counter.png";

            //byte[] bytes = File.ReadAllBytes(imageFile);
            //ByteString bs = ByteString.CopyFrom(bytes);

            byte[] bytes0 = File.ReadAllBytes(bigImage);

            var small = CameraModule.Camera.ShrinkJpegTo300x300(bytes0);

            // Change this IP address to point to your DBE's compute role IP address
            var client = new FpgaClient.FpgaModel("192.168.1.192", 50051, "123, 117, 104");

            var result = client.Process(small);

            Console.WriteLine("Done");
        }
    }
}
