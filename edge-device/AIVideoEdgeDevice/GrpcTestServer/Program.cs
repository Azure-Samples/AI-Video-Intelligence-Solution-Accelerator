using System;
using System.Net;
using System.Threading.Tasks;
using Grpc.Core;
using VideoProcessorGrpc;

namespace GrpcTestServer
{
    class Program
    {
        static void OnImageReceived(ImageBody image)
        {
            Console.WriteLine("    Received {0} bytes from camera:{1}, time:{2}, type:{3}", image.Image.Length, image.CameraId, image.Time, image.Type);
        }

        static void Main(string[] args)
        {
            GrpcServer server = new GrpcServer(Dns.GetHostName(), OnImageReceived);
            server.Start();

            Console.WriteLine($"Grpc server listening as {server.HostAddress} on port {(int)(VideoProcessorGrpc.PortNumbers.CameraToVideoProcessorPort)}");
            Console.WriteLine("Press any key to stop the server...");
            Console.ReadKey();

            server.ShutdownAsync().Wait();
        }
    }
}
