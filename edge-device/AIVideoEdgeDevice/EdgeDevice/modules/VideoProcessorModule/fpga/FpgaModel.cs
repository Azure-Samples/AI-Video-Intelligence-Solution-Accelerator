using Google.Protobuf;
using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Tensorflow.Serving;
using VideoProcessorModule;

namespace FpgaClient
{
    public class FpgaModel : IProcessImage
    {
        Channel grpcChannel = null;
        PredictionService.PredictionServiceClient grpcClient = null;

        /// <summary>
        /// GrpcClient contains retry logic, and the constructor does not throw.
        /// Call Disconnect on program shutdown.
        /// </summary>
        /// <param name="hostAddress">The name of the Module rather than the server for IoT Edge Modules</param>
        public FpgaModel(string hostAddress, int portNumber)
        {
            this.HostAddress = hostAddress;
            PortNumber = portNumber;
        }

        public string HostAddress { get; private set; }

        public int PortNumber { get; private set; }

        public const string FpgaModelProcessorType = "FPGA";

        public string ProcessorType => FpgaModelProcessorType;

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

        public List<ImageFeature> Process(ByteString image)
        {
            List<ImageFeature> result = new List<ImageFeature>();

            try
            {
                IScoringRequest request = null;
                float[] offsets = new float[] { 123, 117, 104 };
                using (MemoryStream stream = new MemoryStream(image.ToByteArray()))
                using (Bitmap bitmap = new Bitmap(stream))
                {
                    int width = bitmap.Width;
                    int height = bitmap.Height;
                    float[] values = new float[3 * width * height];
                    int valuesIdx = 0;

                    for (int i = 0; i < height; i++)
                    {
                        for (int j = 0; j < width; j++)
                        {
                            var pixel = bitmap.GetPixel(j, i);
                            values[valuesIdx++] = pixel.B - offsets[0];
                            values[valuesIdx++] = pixel.G - offsets[1];
                            values[valuesIdx++] = pixel.R - offsets[2];
                        }
                    }
                    int[] shape = new int[] { 1, 300, 300, 3 };
                    Tuple<float[], int[]> tuple = new Tuple<float[], int[]>(values, shape);
                    Dictionary<string, Tuple<float[], int[]>> inputs = new Dictionary<string, Tuple<float[], int[]>>
                {
                    { "brainwave_ssd_vgg_1_Version_0.1_input_1:0", tuple }
                };

                    request = new FloatRequest(inputs);
                }

                PredictResponse response = Predict(request.MakePredictRequest());
                if (response != null)
                {
                    result = FpgaPostProcess.PostProcess(response, selectThreshold: 0.5F, jaccardThreshold: 0.45F);
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Failed during FPGA Pre- or Post- Process: {ex.Message}");
            }

            return result;
        }

        /// <summary>
        /// Upload an image
        /// </summary>
        /// <param name="body"></param>
        /// <returns>True for success</returns>
        private PredictResponse Predict(PredictRequest predictRequest)
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
