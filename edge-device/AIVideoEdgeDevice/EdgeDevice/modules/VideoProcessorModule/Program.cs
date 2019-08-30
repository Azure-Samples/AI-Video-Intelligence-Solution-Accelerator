namespace VideoProcessorModule
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using System.Runtime.Loader;
    using System.Security.Cryptography.X509Certificates;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Azure.Devices.Client;
    using Microsoft.Azure.Devices.Client.Transport.Mqtt;
    using Newtonsoft.Json;
    using BlobStorage;
    using VideoProcessorGrpc;
    using Microsoft.Azure.Devices.Shared;
    using FpgaClient;
    using Newtonsoft.Json.Linq;

    class Program
    {
        private static ModuleClient s_moduleClient = null;
        private static GrpcServer s_grpcServer = null;
        private static BlobStorageHelper s_blobHelper = null;
        private readonly static Object s_twinUpdateLocker = new Object();
        private static IProcessImage s_currentModel = new CpuModel();

        static void Main(string[] args)
        {
            Init();

            // Wait until the app unloads or is cancelled
            var cts = new CancellationTokenSource();
            AssemblyLoadContext.Default.Unloading += (ctx) => cts.Cancel();
            Console.CancelKeyPress += (sender, cpe) => cts.Cancel();
            WhenCancelled(cts.Token).Wait();
        }

        /// <summary>
        /// Handles cleanup operations when app is cancelled or unloads
        /// </summary>
        public static Task WhenCancelled(CancellationToken cancellationToken)
        {
            var tcs = new TaskCompletionSource<bool>();
            cancellationToken.Register(s => ((TaskCompletionSource<bool>)s).SetResult(true), tcs);
            return tcs.Task;
        }

        /// <summary>
        /// </summary>
        static void Init()
        {
            Console.WriteLine($"Init() called on thread {Thread.CurrentThread.ManagedThreadId}");

            MqttTransportSettings mqttSetting = new MqttTransportSettings(TransportType.Mqtt_Tcp_Only)
            {
                RemoteCertificateValidationCallback =
                (sender, certificate, chain, sslPolicyErrors) => true
            };
            ITransportSettings[] settings = { mqttSetting };

            // Open a connection to the Edge runtime
            var mcTask = ModuleClient.CreateFromEnvironmentAsync(settings);
            mcTask.Wait();
            s_moduleClient = mcTask.Result;
            s_moduleClient.OpenAsync().Wait();

            // Get module twin for initial settings
            Task<Twin> twinTask = s_moduleClient.GetTwinAsync();
            twinTask.Wait();
            Twin twin = twinTask.Result;

            OnDesiredPropertiesUpdate(twin.Properties.Desired, s_moduleClient).Wait();
            s_moduleClient.SetDesiredPropertyUpdateCallbackAsync(OnDesiredPropertiesUpdate, null);

            s_grpcServer = new GrpcServer("VideoProcessorModule", OnImageReceived);
            Console.WriteLine("Starting gRPC server");
            s_grpcServer.Start();

            Task processingTask = new Task(ProcessingLoop);
            processingTask.Start();

            Console.WriteLine("IoT Hub module client initialized.");
        }

        static Task OnDesiredPropertiesUpdate(TwinCollection desiredProperties, object userContext)
        {
            Console.WriteLine($"OnDesiredPropertiesUpdate() called on thread {Thread.CurrentThread.ManagedThreadId}");

            lock (s_twinUpdateLocker)
            {
                try
                {
                    Console.WriteLine("Desired property change:");
                    Console.WriteLine(JsonConvert.SerializeObject(desiredProperties));
                    string fpgaRefLevelRGB = "104, 117, 123";

                    string mlModelType = CpuModel.CpuModelProcessorType;
                    if (desiredProperties.Contains("mlModelType"))
                    {
                        mlModelType = desiredProperties["mlModelType"];
                    }
                    if (desiredProperties.Contains("FPGA"))
                    {
                        JObject fpgaProps = desiredProperties["FPGA"];
                        if (fpgaProps["rgbRefLevel"] != null)
                        {
                            fpgaRefLevelRGB = (string)fpgaProps["rgbRefLevel"];
                        }
                    }
                    Console.WriteLine($"Model type: {mlModelType}");
                    // Default to "CPU" on malformed input
                    switch(mlModelType)
                    {
                        // Don't change valid non-"CPU" inputs
                        case GpuModel.GpuModelProcessorType:
                        case FpgaModel.FpgaModelProcessorType:
                        case CpuModel.CpuModelProcessorType:
                            break;
                        default:
                            Console.WriteLine($"Malformed mlModelType: {mlModelType} -- defaulting to 'CPU'");
                            mlModelType = CpuModel.CpuModelProcessorType;
                            break;
                    }
                    if (mlModelType != s_currentModel.ProcessorType)
                    {
                        try
                        {
                            s_currentModel.Disconnect();
                        }
                        catch(Exception ex)
                        {
                            Console.WriteLine($"Error disconnecting {s_currentModel.ProcessorType} model: {ex.Message}");
                        }
                        Console.WriteLine($"Switching to {mlModelType} model.");
                        switch (mlModelType)
                        {
                            // Don't change valid non-"CPU" inputs
                            case GpuModel.GpuModelProcessorType:
                                s_currentModel = new GpuModel();
                                break;
                            case FpgaModel.FpgaModelProcessorType:
                                s_currentModel = new FpgaModel("voiddetectionbrainwave", 50051, fpgaRefLevelRGB);
                                break;
                            case CpuModel.CpuModelProcessorType:
                            default:
                                s_currentModel = new CpuModel();
                                break;
                        }
                    }

                    if (desiredProperties.Contains("blobStorageSasUrl"))
                    {
                        string sasUrl = desiredProperties["blobStorageSasUrl"];
                        // TODO: it would be nice to be able to compare the URL to the one
                        // in the current s_blobHelper (if any).
                        Console.WriteLine("Creating new BlobStorageHelper");
                        s_blobHelper = new BlobStorageHelper("still-images", sasUrl);
                    }
                }
                catch (AggregateException ex)
                {
                    foreach (Exception exception in ex.InnerExceptions)
                    {
                        Console.WriteLine();
                        Console.WriteLine("Error when receiving desired property: {0}", exception);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine();
                    Console.WriteLine("Error when receiving desired property: {0}", ex.Message);
                }
            }
            return Task.CompletedTask;
        }

        private static void OnImageReceived(ImageBody body)
        {
            InputBuffer.Add(body);
        }

        /// <summary>
        /// Wake up every second and do all the work
        /// </summary>
        private static void ProcessingLoop()
        {
            while (true)
            {
                lock (s_twinUpdateLocker)
                {
                    ImageBody found = InputBuffer.GetNext();
                    while (found != null)
                    {
                        try
                        {
                            ImageProcessor.Process(s_blobHelper, s_moduleClient, s_currentModel, found);
                        }
                        catch(Exception ex)
                        {
                            Console.WriteLine($"Error from ImageProcessor.Process: {ex.Message}");
                        }
                        found = InputBuffer.GetNext();
                    }
                }
                Task.Delay(1000).Wait();
            }
        }
    }
}
