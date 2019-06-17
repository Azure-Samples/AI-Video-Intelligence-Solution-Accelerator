// Copyright (c) Microsoft. All rights reserved.

using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CameraServer
{
    public class GrpcServer : Server
    {
        private class GrpcListener : GrpcChannel.GrpcChannelBase
        {
            readonly Func<ImageRequest, ImageReply> callback;
            public GrpcListener(Func<ImageRequest, ImageReply> callback)
            {
                this.callback = callback;
            }

            public override Task<ImageReply> RequestImage(ImageRequest request, ServerCallContext context)
            {
                // Start the callback task before responding
                ImageReply result = callback(request);
                return Task.FromResult(result);
            }
        }

        /// <summary>
        /// A gRPC server specialized for AI Video inter-process communication
        /// </summary>
        /// <param name="callback">The callback function is called asynchronously, so it may be synchronous</param>
        public GrpcServer(string hostName, Func<ImageRequest, ImageReply> callback)
        {
            const int Port = (int)CameraServer.PortNumbers.CameraServerPort;
            this.Ports.Add(new ServerPort(hostName, Port, ServerCredentials.Insecure));
            this.Services.Add(GrpcChannel.BindService(new GrpcListener(callback)));
        }

        public string HostAddress { get; private set; }
    }
}
