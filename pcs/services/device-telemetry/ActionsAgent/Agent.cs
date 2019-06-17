// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.EventHubs.Processor;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.ActionsAgent.EventHub;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Diagnostics;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Runtime;

namespace Microsoft.Azure.IoTSolutions.DeviceTelemetry.ActionsAgent
{
    public interface IAgent
    {
        Task RunAsync(CancellationToken runState);
    }

    public class Agent : IAgent
    {
        private readonly ILogger logger;
        private readonly IServicesConfig servicesConfig;
        private readonly IEventProcessorFactory actionsEventProcessorFactory;
        private readonly IEventProcessorHostWrapper eventProcessorHostWrapper;

        public Agent(ILogger logger,
            IServicesConfig servicesConfig,
            IEventProcessorHostWrapper eventProcessorHostWrapper,
            IEventProcessorFactory actionsEventProcessorFactory)
        {
            this.logger = logger;
            this.servicesConfig = servicesConfig;
            this.actionsEventProcessorFactory = actionsEventProcessorFactory;
            this.eventProcessorHostWrapper = eventProcessorHostWrapper;
        }

        public async Task RunAsync(CancellationToken runState)
        {
            this.logger.Info("Actions Agent started", () => { });
            await this.SetupEventHub(runState);
        }

        private async Task SetupEventHub(CancellationToken runState)
        {
            if (!runState.IsCancellationRequested)
            {
                try
                {
                    var eventProcessorHost = this.eventProcessorHostWrapper.CreateEventProcessorHost(
                        this.servicesConfig.ActionsEventHubName,
                        this.servicesConfig.ActionsEventHubConnectionString,
                        this.servicesConfig.BlobStorageConnectionString,
                        this.servicesConfig.ActionsBlobStorageContainer);
                    await this.eventProcessorHostWrapper.RegisterEventProcessorFactoryAsync(eventProcessorHost, this.actionsEventProcessorFactory);
                }
                catch (Exception e)
                {
                    this.logger.Error("Received error setting up event hub. Will not receive information from the eventhub", () => new { e });
                }
            }
        }
    }
}
