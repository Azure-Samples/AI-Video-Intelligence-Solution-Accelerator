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

            byte[] bytes0 = File.ReadAllBytes(bigImage);

            var small = CameraModule.Camera.ShrinkJpegTo300x300(bytes0);

            // Change this IP address to point to your DBE's compute role IP address
            var client = new FpgaClient.FpgaModel("192.168.1.192", 50051, "104, 117, 123");

            var result = client.Process(small);

            Console.WriteLine("Done");
        }
    }
}
