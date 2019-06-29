using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace VideoProcessorGrpc
{
    public class GrpcServer : Server
    {
        private class GrpcListener : GrpcChannel.GrpcChannelBase
        {
            readonly Action<ImageBody> callback;
            public GrpcListener(Action<ImageBody> callback)
            {
                this.callback = callback;
            }

            public override Task<ImageReply> SubmitImage(ImageBody request, ServerCallContext context)
            {
                // Start the callback task before responding
                Task task = new Task(() => callback(request));
                task.Start();
                return Task.FromResult(new ImageReply { Error = string.Empty });
            }
        }

        /// <summary>
        /// A gRPC server specialized for AI Video inter-process communication
        /// </summary>
        /// <param name="callback">The callback function runs asynchronously, so it may be synchronous</param>
        public GrpcServer(string hostName, Action<ImageBody> callback)
        {
            const int Port = (int)VideoProcessorGrpc.PortNumbers.CameraToVideoProcessorPort;
            HostAddress = IpHelper.Get172SubnetIpV4(hostName);
            this.Ports.Add(new ServerPort(HostAddress, Port, ServerCredentials.Insecure));
            this.Services.Add(GrpcChannel.BindService(new GrpcListener(callback)));
        }

        public string HostAddress { get; private set; }
    }
}
