// Copyright (c) Microsoft. All rights reserved.

using System.Collections.Generic;
using Microsoft.Azure.IoTSolutions.IotHubManager.Services.Models;
using Newtonsoft.Json;

namespace Microsoft.Azure.IoTSolutions.IotHubManager.WebService.v1.Models
{
    public class TwinPropertiesListApiModel
    {
        [JsonProperty(PropertyName = "$metadata")]
        public Dictionary<string, string> Metadata => new Dictionary<string, string>
        {
            { "$type", "DeviceList;" + Version.NUMBER },
            { "$uri", "/" + Version.PATH + "/devices" }
        };

        [JsonProperty(PropertyName = "ContinuationToken")]
        public string ContinuationToken { get; set; }

        [JsonProperty(PropertyName = "Items")]
        public List<TwinPropertiesApiModel> Items { get; set; }

        public TwinPropertiesListApiModel()
        {
        }

        public TwinPropertiesListApiModel(TwinServiceListModel twins)
        {
            this.Items = new List<TwinPropertiesApiModel>();
            this.ContinuationToken = twins.ContinuationToken;
            foreach (var t in twins.Items)
            {
                this.Items.Add(new TwinPropertiesApiModel(t.DesiredProperties, t.ReportedProperties,
                                                          t.DeviceId, t.ModuleId));
            }
        }
    }
}
