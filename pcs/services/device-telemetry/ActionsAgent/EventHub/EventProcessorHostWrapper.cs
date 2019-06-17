// Copyright (c) Microsoft. All rights reserved.

using System.Threading.Tasks;
using Microsoft.Azure.EventHubs;
using Microsoft.Azure.EventHubs.Processor;

namespace Microsoft.Azure.IoTSolutions.DeviceTelemetry.ActionsAgent.EventHub
{
    public interface IEventProcessorHostWrapper
    {
        EventProcessorHost CreateEventProcessorHost(
            string eventHubPath,
            string eventHubConnectionString,
            string storageConnectionString,
            string leaseContainerName);
        Task RegisterEventProcessorFactoryAsync(EventProcessorHost host, IEventProcessorFactory factory);
    }

    public class EventProcessorHostWrapper : IEventProcessorHostWrapper
    {
        public EventProcessorHost CreateEventProcessorHost(
            string eventHubPath,
            string eventHubConnectionString,
            string storageConnectionString,
            string leaseContainerName)
        {
            return new EventProcessorHost(
                eventHubPath,
                PartitionReceiver.DefaultConsumerGroupName,
                eventHubConnectionString,
                storageConnectionString,
                leaseContainerName);
        }

        public Task RegisterEventProcessorFactoryAsync(EventProcessorHost host, IEventProcessorFactory factory)
        {
            return host.RegisterEventProcessorFactoryAsync(factory);
        }
    }
}