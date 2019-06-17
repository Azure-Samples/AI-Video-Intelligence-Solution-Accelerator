// Copyright (c) Microsoft. All rights reserved.

using System.Collections.Generic;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Models.Actions;
using Newtonsoft.Json;

namespace Microsoft.Azure.IoTSolutions.DeviceTelemetry.ActionsAgent.Models
{
    public class AsaAlarmApiModel
    {
        [JsonProperty(PropertyName = "created")]
        public long DateCreated { get; set; }

        [JsonProperty(PropertyName = "modified")]
        public long DateModified { get; set; }

        [JsonProperty(PropertyName = "rule.description")]
        public string RuleDescription { get; set; }

        [JsonProperty(PropertyName = "rule.severity")]
        public string RuleSeverity { get; set; }

        [JsonProperty(PropertyName = "rule.id")]
        public string RuleId { get; set; }

        [JsonProperty(PropertyName = "rule.actions")]
        public IList<IAction> Actions { get; set; }

        [JsonProperty(PropertyName = "device.id")]
        public string DeviceId { get; set; }

        [JsonProperty(PropertyName = "device.msg.received")]
        public long MessageReceived { get; set; }
    }
}
