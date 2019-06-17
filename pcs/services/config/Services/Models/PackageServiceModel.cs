// Copyright (c) Microsoft. All rights reserved.

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Microsoft.Azure.IoTSolutions.UIConfig.Services.Models
{
    public class PackageServiceModel
    {
        public string Id { get; set; }

        public string Name { get; set; }

        [JsonProperty("Type")]
        [JsonConverter(typeof(StringEnumConverter))]
        public PackageType PackageType { get; set; }

        public string ConfigType { get; set; }

        public string Content { get; set; }

        public string DateCreated { get; set; }
    }

    // Sync these variables with PackageType in IotHubManager
    public enum PackageType
    {
        EdgeManifest,
        DeviceConfiguration
    }

    // Used for validation, these are pre-defined constants for configuration type
    public enum ConfigType
    {
        Custom,
        Firmware
    }
}