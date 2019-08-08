using FpgaClient;
using Google.Protobuf;
using NumSharp;
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
            var imageFile = @"..\..\..\..\EdgeDevice\modules\CameraModule\simulated-images\300x300\counter.png";

            byte[] bytes = File.ReadAllBytes(imageFile);
            ByteString bs = ByteString.CopyFrom(bytes);

            // Change this IP address to point to your DBE's compute role IP address
            var client = new FpgaClient.FpgaModel("192.168.1.192", 50051);

            var result = client.Process(bs);

            Console.WriteLine("Done");
        }
    }
}
