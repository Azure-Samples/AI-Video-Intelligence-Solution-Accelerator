using System;
using System.Threading.Tasks;
using BlobStorage;
using CameraSim;
using Grpc.Core;
using VideoProcessorGrpc;
using System.Net;

namespace GrpcTestClient
{
    class Program
    {
        static void Main(string[] args)
        {
            GrpcClient client = new GrpcClient(Dns.GetHostName());

            Console.WriteLine("Press 's' to resend, any other key to exit...");
            const string cameraId = "bld666room2117/grid01x04look27";
            while (true)
            {

                DateTime nowDateTruncatedToMilliseconds = BlobStorageHelper.GetImageUtcTime();
                string nowString = BlobStorageHelper.FormatImageUtcTime(nowDateTruncatedToMilliseconds);

                CameraSimulator sim = new CameraSimulator(cameraId);
                byte[] content = sim.GetSimulatedImage(nowString);

                Google.Protobuf.ByteString image = Google.Protobuf.ByteString.CopyFrom(content);
                try
                {
                    Task<bool> messageTask = client.UploadImageAsync(new ImageBody { CameraId = cameraId, Image = image, Time = nowString, Type = "jpg" });
                    Console.WriteLine(messageTask.Result ? "OK" : "Failed");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Failed: " + ex.Message);
                }

                ConsoleKeyInfo key = Console.ReadKey();
                if (key.KeyChar != 's')
                {
                    client.Disconnect();
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
