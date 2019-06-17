// Copyright (c) Microsoft. All rights reserved.

using System;

namespace Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Runtime
{
    public class StorageConfig
    {
        public string CosmosDbDatabase { get; set; }
        public string CosmosDbCollection { get; set; }

        public StorageConfig(
            string cosmosDbDatabase,
            string cosmosDbCollection)
        {
            this.CosmosDbDatabase = cosmosDbDatabase;
            if (string.IsNullOrEmpty(this.CosmosDbDatabase))
            {
                throw new Exception("CosmosDb database name is empty in configuration");
            }

            this.CosmosDbCollection = cosmosDbCollection;
            if (string.IsNullOrEmpty(this.CosmosDbCollection))
            {
                throw new Exception("CosmosDb collection name is empty in configuration");
            }
        }
    }
}
