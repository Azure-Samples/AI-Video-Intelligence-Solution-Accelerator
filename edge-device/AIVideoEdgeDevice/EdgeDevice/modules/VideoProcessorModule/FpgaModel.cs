using System;
using System.Collections.Generic;
using System.Text;
using FpgaGrpc;
using Google.Protobuf;
using Grpc.Core;

namespace VideoProcessorModule
{
    class FpgaModel : IProcessImage
    {
        Channel grpcChannel = null;
        FpgaGrpcChannel.FpgaGrpcChannelClient grpcClient = null;

        public FpgaModel(string serverName)
        {
            this.ServerName = serverName;
        }

        public string ServerName { get; }

        public const string FpgaModelProcessorType = "FPGA";

        public string ProcessorType { get { return FpgaModelProcessorType; } }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="image"></param>
        /// <returns>Null on error</returns>
        public List<ImageFeature> Process(ByteString image)
        {
            bool succeeded = false;
            try
            {
                List<ImageFeature> result = new List<ImageFeature>();
                EnsureConnection();
                ImageBody body = new ImageBody
                {
                    Image = image
                };
                ImageReply reply = grpcClient.SubmitImage(body);
                for (int i = 0; i < reply.Classes.Count; i++)
                {
                    ImageFeature feature = new ImageFeature(reply.Classes[i], reply.Scores[i],
                        reply.Boxes[i].YMin, reply.Boxes[i].XMin, reply.Boxes[i].YMax, reply.Boxes[i].XMax);
                    result.Add(feature);
                }
                succeeded = true;
                return result;
            }
            catch (Grpc.Core.RpcException ex)
            {
                if (ex.StatusCode == StatusCode.Unavailable)
                {
                    Console.WriteLine($"Error sending grpc message: Grpc server for FPGA is unavailable");
                    return null;
                }
                else
                {
                    Console.WriteLine($"Error sending grpc for FPGA message");
                    Console.WriteLine(ex);
                    return null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending grpc for FPGA message");
                Console.WriteLine(ex);
                return null;
            }
            finally
            {
                if (!succeeded)
                {
                    Disconnect();
                }
            }
        }

        public void Disconnect()
        {
            if (grpcChannel != null)
            {
                grpcChannel.ShutdownAsync().Wait();
                grpcChannel = null;
                grpcClient = null;
            }
        }

        private void EnsureConnection()
        {
            if (grpcChannel == null)
            {
                const int Port = (int)FpgaGrpc.PortNumbers.VideoProcessorToFpgaPort;
                grpcChannel = new Channel(this.ServerName + ":" + Port.ToString(), ChannelCredentials.Insecure);
                grpcClient = new FpgaGrpcChannel.FpgaGrpcChannelClient(grpcChannel);
            }
        }
    }
}
