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

    class Program
    {
        /// <summary>
        /// Specifies how often to do a file upload even if there is no recognition
        /// </summary>
        private static int s_uploadThreshold = 5;
        private static ModuleClient s_moduleClient = null;
        private static GrpcServer s_grpcServer = null;
        private static BlobStorageHelper s_blobHelper = null;
        private readonly static Object s_twinUpdateLocker = new Object();

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

                    if (desiredProperties.Contains("uploadThreshold"))
                    {
                        int threshold = desiredProperties["uploadThreshold"];
                        if (threshold < 0)
                            threshold = 0;
                        Console.WriteLine($"Setting uploadThreshold threshold to {threshold}");
                        s_uploadThreshold = threshold;
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
            int forceCounter = s_uploadThreshold;
            bool force = false;

            while (true)
            {
                lock (s_twinUpdateLocker)
                {
                    ImageBody found = InputBuffer.GetNext();
                    while (found != null)
                    {
                        Console.WriteLine($"Processing image from {found.CameraId}   {found.Time}");
                        bool uploaded = ProcessImage(found, force);

                        if (uploaded)
                        {
                            forceCounter = s_uploadThreshold;
                            force = false;
                        }
                        else
                        {
                            if (s_uploadThreshold > 0)
                                force = (--forceCounter == 0);
                        }

                        found = InputBuffer.GetNext();
                    }
                }
                Task.Delay(1000).Wait();
            }
        }

        /// <summary>
        /// This call is made from within a Task, so there is no need to make it async
        /// </summary>
        /// <param name="body"></param>
        private static bool ProcessImage(ImageBody body, bool force)
        {
            try
            {
                Console.WriteLine($"Received a {body.Image.Length} byte image plus a {body.SmallImage.Length} 300x300 png.");

                var processor = new ImageProcessor(s_blobHelper, s_moduleClient);

                return processor.Process(body, force);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed handling received image from {body.CameraId} at {body.Time}");
                Console.WriteLine(ex);
                return false;
            }
        }
    }
}
