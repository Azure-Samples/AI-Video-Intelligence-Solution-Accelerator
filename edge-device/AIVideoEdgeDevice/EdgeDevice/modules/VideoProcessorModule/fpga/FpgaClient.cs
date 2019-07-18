using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Tensorflow.Serving;

namespace FpgaClient
{
    public class FpgaClient
    {
        Channel grpcChannel = null;
        PredictionService.PredictionServiceClient grpcClient = null;

        /// <summary>
        /// GrpcClient contains retry logic, and the constructor does not throw.
        /// Call Disconnect on program shutdown.
        /// </summary>
        /// <param name="hostAddress">The name of the Module rather than the server for IoT Edge Modules</param>
        public FpgaClient(string hostAddress, int portNumber)
        {
            this.HostAddress = hostAddress;
            PortNumber = portNumber;
        }

        public string HostAddress { get; private set; }

        public int PortNumber { get; private set; }

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
        public PredictResponse Predict(PredictRequest predictRequest)
        {
            bool succeeded = false;
            try
            {
                EnsureConnection();
                PredictResponse result = grpcClient.Predict(predictRequest);
                succeeded = true;
                return result;
            }
            catch(Grpc.Core.RpcException ex)
            {
                if (ex.StatusCode == StatusCode.Unavailable)
                {
                    Console.WriteLine($"Error sending grpc message: Grpc server is unavailable");
                    return null;
                }
                else
                {
                    Console.WriteLine($"Error sending grpc message");
                    Console.WriteLine(ex);
                    return null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending grpc message");
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

        private void EnsureConnection()
        {
            if (grpcChannel == null)
            {
                grpcChannel = new Channel(this.HostAddress + ":" + this.PortNumber.ToString(), ChannelCredentials.Insecure);
                grpcClient = new PredictionService.PredictionServiceClient(grpcChannel);
            }
        }
    }
}
