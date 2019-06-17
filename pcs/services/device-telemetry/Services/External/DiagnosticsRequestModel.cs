// Copyright (c) Microsoft. All rights reserved.

using System.Collections.Generic;
using Newtonsoft.Json;

namespace Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.External
{
    public class DiagnosticsRequestModel
    {
        [JsonProperty(PropertyName = "EventType", Order = 10)]
        public string EventType { get; set; }

        [JsonProperty(PropertyName = "EventProperties", Order = 20)]
        public Dictionary<string, object> EventProperties { get; set; }
    }
}
