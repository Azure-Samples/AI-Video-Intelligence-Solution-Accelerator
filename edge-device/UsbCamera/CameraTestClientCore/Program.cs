// Copyright (c) Microsoft. All rights reserved.

using CameraModule;
using CameraServer;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading.Tasks;
using VideoProcessorGrpc;

namespace CameraTestClientCore
{
    class Program
    {
        static void Main(string[] args)
        {
            CameraServerClient client = new CameraServerClient("PURL2//700");

            Console.WriteLine("Press 's' to resend, any other key to exit...");
            while (true)
            {
                try
                {
                    ImageBody reply = ((IImageSource)client).RequestImages();
                    if (reply != null)
                    {
                        Console.WriteLine($"Received {reply.Image.Length} byte jpeg plus {reply.SmallImage.Length} 300x300 png");
                        using (MemoryStream stream = new MemoryStream(reply.SmallImage.ToByteArray()))
                        using (Bitmap bitmap = new Bitmap(stream))
                        {
                            bitmap.Save(@"..\..\..\smallImage.bmp", ImageFormat.Bmp);
                            bitmap.Save(@"..\..\..\smallImage.png", ImageFormat.Png);
                        }
                        File.WriteAllBytes(@"..\..\..\fullImage.jpeg", reply.Image.ToByteArray());
                    }
                    else
                    {
                        Console.WriteLine($"CameraServer returned null");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Failed: " + ex.Message);
                }

                ConsoleKeyInfo key = Console.ReadKey();
                if (key.KeyChar != 's')
                {
                    ((IImageSource)client).Disconnect();
                    return;
                }
                else
                {
                    Console.Write("\b");
                }
            }
        }
    }
}
