// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.Azure.IoTSolutions.AsaManager.Services.Runtime
{
    // Types of storage where ASA writes messages and alarms and case insensitive
    public enum AsaOutputStorageType
    {
        CosmosDbSql = 1,
        CosmosDb = CosmosDbSql,
        TimeSeriesInsights = 2,
        Tsi = TimeSeriesInsights
    }

    public interface IServicesConfig
    {
        string DeviceTelemetryWebServiceUrl { get; set; }
        int DeviceTelemetryWebServiceTimeout { get; set; }
        string ConfigServiceUrl { get; set; }
        int ConfigServiceTimeout { get; set; }
        string IotHubManagerServiceUrl { get; set; }
        int IotHubManagerServiceTimeout { get; set; }
        int InitialIotHubManagerRetryIntervalMs { get; set; }
        int IotHubManagerRetryIntervalIncreaseFactor { get; set; }
        int IotHubManagerRetryCount { get; set; }

        // Type of storage where ASA write messages. Only value currently supported is "CosmosDbSql"
        AsaOutputStorageType MessagesStorageType { get; set; }

        // CosmosDb configuration for the table containing messages
        CosmosDbTableConfiguration MessagesCosmosDbConfig { get; set; }

        // Type of storage where ASA write alarms. Only value currently supported is "CosmosDbSql"
        AsaOutputStorageType AlarmsStorageType { get; set; }

        // CosmosDb configuration for the table containing alarms
        CosmosDbTableConfiguration AlarmsCosmosDbConfig { get; set; }

        // Connection Information for event hub
        string EventHubConnectionString { get; }
        string EventHubName { get; }
        int EventHubCheckpointTimeMs { get; }
    }

    public class ServicesConfig : IServicesConfig
    {
        public string DeviceTelemetryWebServiceUrl { get; set; }
        public int DeviceTelemetryWebServiceTimeout { get; set; }
        public string ConfigServiceUrl { get; set; }
        public int ConfigServiceTimeout { get; set; }
        public string IotHubManagerServiceUrl { get; set; }
        public int IotHubManagerServiceTimeout { get; set; }
        public int InitialIotHubManagerRetryIntervalMs { get; set; }
        public int IotHubManagerRetryIntervalIncreaseFactor { get; set; }
        public int IotHubManagerRetryCount { get; set; }
        public AsaOutputStorageType MessagesStorageType { get; set; }
        public CosmosDbTableConfiguration MessagesCosmosDbConfig { get; set; }
        public AsaOutputStorageType AlarmsStorageType { get; set; }
        public CosmosDbTableConfiguration AlarmsCosmosDbConfig { get; set; }
        public string EventHubConnectionString { get; set; }
        public string EventHubName { get; set; }
        public int EventHubCheckpointTimeMs { get; set; }
    }
}
