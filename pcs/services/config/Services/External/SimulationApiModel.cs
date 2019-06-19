﻿// Copyright (c) Microsoft. All rights reserved.

using System.Collections.Generic;
using Newtonsoft.Json;

namespace Microsoft.Azure.IoTSolutions.UIConfig.Services.External
{
    public class SimulationApiModel
    {
        [JsonProperty(PropertyName = "ETag")]
        public string ETag { get; set; }

        [JsonProperty(PropertyName = "Id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "Name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "Enabled")]
        public bool Enabled { get; set; }

        [JsonProperty(PropertyName = "StartTime")]
        public string StartTime { get; set; }

        [JsonProperty(PropertyName = "DeviceModels")]
        public List<DeviceModelRef> DeviceModels { get; set; }
    }
}
