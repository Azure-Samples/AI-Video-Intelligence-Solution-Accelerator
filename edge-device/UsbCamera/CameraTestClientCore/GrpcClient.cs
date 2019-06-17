// Copyright (c) Microsoft. All rights reserved.

using CameraServer;
using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CameraTestClientCore
{
    class GrpcClient
    {
        Channel grpcChannel = null;
        GrpcChannel.GrpcChannelClient grpcClient = null;

        /// <summary>
        /// GrpcClient contains retry logic, and the constructor does not throw.
        /// Call Disconnect on program shutdown.
        /// </summary>
        /// <param name="serverName">The name of the Module rather than the server for IoT Edge Modules</param>
        public GrpcClient(string serverName)
        {
            this.ServerName = serverName;
        }

        public string ServerName { get; private set; }

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
        /// <param name="request"></param>
        /// <returns>True for success</returns>
        public ImageReply RequestImage(ImageRequest request)
        {
            ImageReply result = null;
            try
            {
                EnsureConnection();
                result = grpcClient.RequestImage(request);
            }
            catch (Grpc.Core.RpcException ex)
            {
                if (ex.StatusCode == StatusCode.Unavailable)
                {
                    Console.WriteLine($"Error sending grpc message: Grpc server is unavailable");
                    result = null;
                }
                else
                {
                    Console.WriteLine($"Error sending grpc message");
                    Console.WriteLine(ex);
                    result = null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending grpc message");
                Console.WriteLine(ex);
                result = null;
            }
            finally
            {
                if (result == null)
                {
                    Disconnect();
                }
            }
            return result;
        }

        public Task<ImageReply> RequestImageAsync(string cameraHardwareId)
        {
            Task<ImageReply> task = new Task<ImageReply>(() => RequestImage(new ImageRequest() { CameraHardwareId = cameraHardwareId }));
            task.Start();
            return task;
        }

        private void EnsureConnection()
        {
            if (grpcChannel == null)
            {
                const int Port = (int)CameraServer.PortNumbers.CameraServerPort;
                grpcChannel = new Channel(this.ServerName + ":" + Port.ToString(), ChannelCredentials.Insecure);
                grpcClient = new GrpcChannel.GrpcChannelClient(grpcChannel);
            }
        }
    }
}
