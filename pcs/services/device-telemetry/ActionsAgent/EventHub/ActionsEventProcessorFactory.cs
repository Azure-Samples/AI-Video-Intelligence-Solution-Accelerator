// Copyright (c) Microsoft. All rights reserved.

using Microsoft.Azure.EventHubs.Processor;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.ActionsAgent.Actions;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Diagnostics;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Http;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Runtime;

namespace Microsoft.Azure.IoTSolutions.DeviceTelemetry.ActionsAgent.EventHub
{
    public class ActionsEventProcessorFactory : IEventProcessorFactory
    {
        private readonly ILogger logger;
        private readonly IActionManager actionManager;

        public ActionsEventProcessorFactory(
            ILogger logger,
            IServicesConfig servicesConfig,
            IHttpClient httpClient)
        {
            this.logger = logger;
            this.actionManager = new ActionManager(logger, servicesConfig, httpClient);
        }

        public IEventProcessor CreateEventProcessor(PartitionContext context)
        {
            return new ActionsEventProcessor(this.actionManager, this.logger);
        }
    }
}
