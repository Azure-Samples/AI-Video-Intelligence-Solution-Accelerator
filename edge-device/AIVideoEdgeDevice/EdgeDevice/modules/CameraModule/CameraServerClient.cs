// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using CameraServer;
using Grpc.Core;

namespace CameraModule
{
    class CameraServerClient : IImageSource
    {
        Channel grpcChannel = null;
        GrpcChannel.GrpcChannelClient grpcClient = null;

        /// <summary>
        /// CameraServerClient contains retry logic, and the constructor does not throw.
        /// Call Disconnect on program shutdown.
        /// </summary>
        /// <param name="port">The name of the machine hosting the CameraServer plus the camera hardware ID (e.g. 'GORT//700')</param>
        public CameraServerClient(string port)
        {
            string[] parts = port.Split("//");
            this.ServerName = parts[0];
            this.HardwareId = parts[1];
        }

        public string ServerName { get; private set; }

        public string HardwareId { get; private set; }

        /// <summary>
        /// Disconnect the internal channel. Use on shutdown.
        /// </summary>
        void IImageSource.Disconnect()
        {
            if (grpcChannel != null)
            {
                grpcChannel.ShutdownAsync().Wait();
                grpcChannel = null;
                grpcClient = null;
            }
        }

        /// <summary>
        /// Request an image from the camera server
        /// </summary>
        /// <param name="body"></param>
        /// <returns>True for success</returns>
        private byte[] RequestImage(ImageRequest body)
        {
            bool succeeded = false;
            try
            {
                EnsureConnection();
                ImageReply result = grpcClient.RequestImage(body);
                if (result.Error != string.Empty)
                {
                    Console.WriteLine($"Error returned from CameraServer: {result.Error}");
                    return null;
                }
                else
                {
                    succeeded = true;
                    return result.Image.ToByteArray();
                }
            }
            catch (Grpc.Core.RpcException ex)
            {
                if (ex.StatusCode == StatusCode.Unavailable)
                {
                    Console.WriteLine($"Error sending grpc message: CameraServer gRPC is unavailable");
                    return null;
                }
                else
                {
                    Console.WriteLine($"Error sending CameraServer gRPC message");
                    Console.WriteLine(ex);
                    return null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending CameraServer gRPC message");
                Console.WriteLine(ex);
                return null;
            }
            finally
            {
                if (!succeeded)
                {
                    ((IImageSource)this).Disconnect();
                }
            }
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

        byte[] IImageSource.RequestImage()
        {
            ImageRequest request = new ImageRequest() { CameraHardwareId = HardwareId };
            byte[] reply = RequestImage(request);
            return reply;
        }
    }
}
