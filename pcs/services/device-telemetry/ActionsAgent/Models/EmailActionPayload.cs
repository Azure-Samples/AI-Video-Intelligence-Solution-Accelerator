// Copyright (c) Microsoft. All rights reserved.

using System.Collections.Generic;
using Newtonsoft.Json;

namespace Microsoft.Azure.IoTSolutions.DeviceTelemetry.ActionsAgent.Models
{
    // Payload to send to logic app to generate an email alert
    public class EmailActionPayload
    {
        [JsonProperty(PropertyName = "recipients")]
        public List<string> Recipients { get; set; }

        [JsonProperty(PropertyName = "body")]
        public string Body { get; set; }

        [JsonProperty(PropertyName = "subject")]
        public string Subject { get; set; }
    }
}
