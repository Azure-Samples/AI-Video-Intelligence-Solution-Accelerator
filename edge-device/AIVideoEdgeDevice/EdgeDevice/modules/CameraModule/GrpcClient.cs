using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace VideoProcessorGrpc
{
    public class GrpcClient
    {
        Channel grpcChannel = null;
        GrpcChannel.GrpcChannelClient grpcClient = null;
        readonly object uploadLock = new object();

        /// <summary>
        /// GrpcClient contains retry logic, and the constructor does not throw.
        /// Call Disconnect on program shutdown.
        /// </summary>
        /// <param name="serverName">The name of the Module rather than the server for IoT Edge Modules</param>
        public GrpcClient(string serverName)
        {
            this.ServerName = serverName;
            this.HostAddress = IpHelper.Get172SubnetIpV4(ServerName);
        }

        public string ServerName { get; private set; }

        public string HostAddress { get; private set; }

        /// <summary>
        /// Disconnect the internal channel. Use on shutdown.
        /// </summary>
        public void Disconnect()
        {
            if (grpcChannel != null)
            {
                grpcChannel.ShutdownAsync().Wait();
                grpcChannel = null;
                grpcClient = null;
            }
        }

        /// <summary>
        /// Upload an image
        /// </summary>
        /// <param name="body"></param>
        /// <returns>True for success</returns>
        public bool UploadImage(ImageBody body)
        {
            lock (uploadLock)
            {
                bool succeeded = false;
                try
                {
                    EnsureConnection();
                    grpcClient.SubmitImage(body);
                    succeeded = true;
                    return true;
                }
                catch (Grpc.Core.RpcException ex)
                {
                    if (ex.StatusCode == StatusCode.Unavailable)
                    {
                        Console.WriteLine($"Error sending grpc message: Grpc server is unavailable");
                        return false;
                    }
                    else
                    {
                        Console.WriteLine($"Error sending grpc message");
                        Console.WriteLine(ex);
                        return false;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error sending grpc message");
                    Console.WriteLine(ex);
                    return false;
                }
                finally
                {
                    if (!succeeded)
                    {
                        Disconnect();
                    }
                }
            }
        }

        public Task<bool> UploadImageAsync(ImageBody body)
        {
            Task<bool> task = new Task<bool>(() => UploadImage(body));
            task.Start();
            return task;
        }

        private void EnsureConnection()
        {
            if (grpcChannel == null)
            {
                const int Port = (int)VideoProcessorGrpc.PortNumbers.CameraToVideoProcessorPort;
                grpcChannel = new Channel(this.HostAddress + ":" + Port.ToString(), ChannelCredentials.Insecure);
                grpcClient = new GrpcChannel.GrpcChannelClient(grpcChannel);
            }
        }
    }
}
