// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.EventHubs;
using Microsoft.Azure.EventHubs.Processor;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.ActionsAgent.Actions;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.ActionsAgent.Models;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Diagnostics;

namespace Microsoft.Azure.IoTSolutions.DeviceTelemetry.ActionsAgent.EventHub
{
    public class ActionsEventProcessor : IEventProcessor
    {
        private readonly ILogger logger;
        private readonly IActionManager actionManager;

        public ActionsEventProcessor(IActionManager actionManager, ILogger logger)
        {
            this.logger = logger;
            this.actionManager = actionManager;
        }

        public Task OpenAsync(PartitionContext context)
        {
            this.logger.Debug("Event Processor initialized.", () => new { context.PartitionId });
            return Task.CompletedTask;
        }

        public async Task CloseAsync(PartitionContext context, CloseReason reason)
        {
            this.logger.Debug("Event Processor Shutting Down.", () => new { context.PartitionId, reason });
            await context.CheckpointAsync();
        }
        
        /**
         * Processes all alarms and executes any actions associated with the alarms
         */
        public async Task ProcessEventsAsync(PartitionContext context, IEnumerable<EventData> messages)
        {
            foreach (EventData eventData in messages)
            {
                if (eventData.Body.Array != null)
                {
                    string data = Encoding.UTF8.GetString(eventData.Body.Array);
                    IEnumerable<AsaAlarmApiModel> alarms = AlarmParser.ParseAlarmList(data, this.logger);
                        await this.actionManager.ExecuteAlarmActions(alarms);
                }
            }

            await context.CheckpointAsync();
        }

        public async Task ProcessErrorAsync(PartitionContext context, Exception error)
        {
            await context.CheckpointAsync();
        }
    }
}
