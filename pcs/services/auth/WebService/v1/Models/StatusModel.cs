// Copyright (c) Microsoft. All rights reserved.

using Newtonsoft.Json;

namespace Microsoft.Azure.IoTSolutions.Auth.WebService.v1.Models
{
    public class StatusModel
    {
        [JsonProperty(PropertyName = "IsHealthy", Order = 10)]
        public bool IsHealthy { get; set; }

        [JsonProperty(PropertyName = "Message", Order = 20)]
        public string Message { get; set; }
    }
}
