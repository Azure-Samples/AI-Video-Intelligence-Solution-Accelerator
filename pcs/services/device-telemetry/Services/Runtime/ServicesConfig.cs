// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Text.RegularExpressions;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Exceptions;

namespace Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Runtime
{
    public interface IServicesConfig
    {
        string StorageAdapterApiUrl { get; set; }
        int StorageAdapterApiTimeout { get; set; }
        string UserManagementApiUrl { get; }
        StorageConfig MessagesConfig { get; set; }
        AlarmsConfig AlarmsConfig { get; set; }
        string StorageType { get; set; }
        Uri CosmosDbUri { get; }
        string CosmosDbKey { get; }
        int CosmosDbThroughput { get; set; }
        string TimeSeriesFqdn { get; }
        string TimeSeriesAuthority { get; }
        string TimeSeriesAudience { get; }
        string TimeSeriesExplorerUrl { get; }
        string TimeSertiesApiVersion { get; }
        string TimeSeriesTimeout { get; }
        string ActiveDirectoryTenant { get; }
        string ActiveDirectoryAppId { get; }
        string ActiveDirectoryAppSecret { get; }
        string DiagnosticsApiUrl { get; }
        int DiagnosticsMaxLogRetries { get; }
        string ActionsEventHubConnectionString { get; }
        string ActionsEventHubName { get; }
        string BlobStorageConnectionString { get; }
        string ActionsBlobStorageContainer { get; }
        string LogicAppEndpointUrl { get; }
        string SolutionUrl { get; }
        string TemplateFolder { get; }
    }

    public class ServicesConfig : IServicesConfig
    {
        public string StorageAdapterApiUrl { get; set; }

        public int StorageAdapterApiTimeout { get; set; }

        public string UserManagementApiUrl { get; set; }

        public StorageConfig MessagesConfig { get; set; }

        public AlarmsConfig AlarmsConfig { get; set; }

        public string StorageType { get; set; }

        public Uri CosmosDbUri { get; set; }

        public string CosmosDbKey { get; set; }

        public int CosmosDbThroughput { get; set; }

        public string DiagnosticsApiUrl { get; set; }

        public int DiagnosticsMaxLogRetries { get; set; }

        public string CosmosDbConnString
        {
            set
            {
                var match = Regex.Match(value,
                    @"^AccountEndpoint=(?<endpoint>.*);AccountKey=(?<key>.*);$");

                Uri endpoint;

                if (!match.Success ||
                    !Uri.TryCreate(match.Groups["endpoint"].Value,
                        UriKind.RelativeOrAbsolute,
                        out endpoint))
                {
                    var message = "Invalid connection string for CosmosDB";
                    throw new InvalidConfigurationException(message);
                }

                this.CosmosDbUri = endpoint;
                this.CosmosDbKey = match.Groups["key"].Value;
            }
        }

        public string TimeSeriesFqdn { get; set; }

        public string TimeSeriesAuthority { get; set; }

        public string TimeSeriesAudience { get; set; }

        public string TimeSeriesExplorerUrl { get; set; }

        public string TimeSertiesApiVersion { get; set; }

        public string TimeSeriesTimeout { get; set; }

        public string ActiveDirectoryTenant { get; set; }

        public string ActiveDirectoryAppId { get; set; }

        public string ActiveDirectoryAppSecret { get; set; }

        public string ActionsEventHubConnectionString { get; set; }

        public string ActionsEventHubName { get; set; }

        public string BlobStorageConnectionString { get; set; }

        public string ActionsBlobStorageContainer { get; set; }

        public string LogicAppEndpointUrl { get; set; }

        public string SolutionUrl { get; set; }
        
        public string TemplateFolder { get; set; }
    }
}