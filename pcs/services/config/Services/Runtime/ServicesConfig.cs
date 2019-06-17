// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.Azure.IoTSolutions.UIConfig.Services.Runtime
{
    public interface IServicesConfig
    {
        string SolutionType { get; set; }
        string StorageAdapterApiUrl { get; }
        string DeviceSimulationApiUrl { get; }
        string TelemetryApiUrl { get; }
        string SeedTemplate { get; }
        string AzureMapsKey { get; }
        string UserManagementApiUrl { get; }
        string Office365LogicAppUrl { get; }
        string ResourceGroup { get; }
        string SubscriptionId { get; }
        string ManagementApiVersion { get; }
        string ArmEndpointUrl { get; }
    }

    public class ServicesConfig : IServicesConfig
    {
        public string SolutionType { get; set; }
        public string StorageAdapterApiUrl { get; set; }
        public string DeviceSimulationApiUrl { get; set; }
        public string TelemetryApiUrl { get; set; }
        public string SeedTemplate { get; set; }
        public string AzureMapsKey { get; set; }
        public string UserManagementApiUrl { get; set; }
        public string Office365LogicAppUrl { get; set; }
        public string ResourceGroup { get; set; }
        public string SubscriptionId { get; set; }
        public string ManagementApiVersion { get; set; }
        public string ArmEndpointUrl { get; set; }
    }
}
