namespace CameraModule
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net;
    using System.Runtime.InteropServices;
    using System.Runtime.Loader;
    using System.Security.Cryptography.X509Certificates;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Grpc.Core;
    using Microsoft.Azure.Devices.Client;
    using Microsoft.Azure.Devices.Client.Transport.Mqtt;
    using Microsoft.Azure.Devices.Shared;
    using Newtonsoft.Json.Linq;
    using VideoProcessorGrpc;

    class Program
    {
        static ModuleClient s_moduleClient = null;
        private readonly static Object s_twinUpdateLocker = new Object();
        private static bool s_mainLoopRunning = true;

        static GrpcClient s_grpcClient = new GrpcClient("VideoProcessorModule");

        static void Main(string[] args)
        {
            Console.WriteLine("start");
            Init();

            Task mainTask = new Task(MainLoop);
            mainTask.Start();

            // Wait until the app unloads or is cancelled
            var cts = new CancellationTokenSource();
            AssemblyLoadContext.Default.Unloading += (ctx) => cts.Cancel();
            Console.CancelKeyPress += (sender, cpe) => cts.Cancel();
            WhenCancelled(cts.Token).Wait();
            Console.WriteLine("Shutting down");
            lock (s_twinUpdateLocker)
            {
                s_mainLoopRunning = false;
                Camera.DisconnectAll();
                s_grpcClient.Disconnect();
            }
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
        /// Initializes the ModuleClient.
        /// </summary>
        static void Init()
        {
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


            // Read configuration from Module Twin
            Task<Twin> twinTask = s_moduleClient.GetTwinAsync();
            twinTask.Wait();
            Twin twin = twinTask.Result;
            OnDesiredPropertiesUpdate(twin.Properties.Desired, null);

            Console.WriteLine("IoT Hub module client initialized.");
        }

        static Task OnDesiredPropertiesUpdate(TwinCollection desiredProperties, object userContext)
        {
            lock(s_twinUpdateLocker)
            {
                JObject camerasDefinition = desiredProperties["cameras"];
                if (camerasDefinition != null)
                {
                    Camera.DisconnectAll();
                    Console.WriteLine("Reading camera definitions from Module Twin");
                    foreach (KeyValuePair<string, JToken> x in camerasDefinition)
                    {
                        // Each camera entry arrives here as an KeyValuePair. We don't use the key, just the value.
                        if (x.Value is JObject found)
                        {
                            Camera.AddCamera(found);
                        }
                        Console.WriteLine($"    Camera value: {x.Value.ToString()}");
                    }
                }
                else
                {
                    Console.WriteLine("Configuration error: The CameraModule's ModuleTwin lacks a 'cameras' entry");
                }
            }
            return Task.CompletedTask;
        }

        /// <summary>
        /// Performs the main processing.
        /// </summary>
        private static void MainLoop()
        {
            do
            {
                lock (s_twinUpdateLocker)
                {
                    // Send images from each camera every ten seconds
                    foreach (Camera camera in Camera.s_cameras)
                    {
                        try
                        {
                            ImageBody body = camera.GetImage();
                            if (body != null)
                            {
                                Console.WriteLine($"Sending a {body.Image.Length} byte image from {body.CameraId} at {body.Time}");
                                s_grpcClient.UploadImage(body);
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error sending grpc message to VideoProcessor module: {ex.Message}");
                        }
                    }
                }
                Task.Delay(10000).Wait();
            } while (s_mainLoopRunning);
        }
    }
}
