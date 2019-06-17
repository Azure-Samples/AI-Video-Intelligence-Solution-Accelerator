// Copyright (c) Microsoft. All rights reserved.

using System.Collections.Generic;
using Microsoft.Azure.IoTSolutions.IotHubManager.Services.Models;
using Newtonsoft.Json;

namespace Microsoft.Azure.IoTSolutions.IotHubManager.WebService.v1.Models
{
    public class DeploymentListApiModel
    {
        [JsonProperty(PropertyName = "Items")]
        public List<DeploymentApiModel> Items { get; set; }

        [JsonProperty(PropertyName = "$metadata")]
        public Dictionary<string, string> Metadata { get; set; }

        public DeploymentListApiModel()
        {
            this.Metadata = new Dictionary<string, string>
            {
                { "$type", $"DevicePropertyList;{Version.NUMBER}" },
                { "$url", $"/{Version.PATH}/deviceproperties" }
            };
        }

        public DeploymentListApiModel(DeploymentServiceListModel deployments)
        {
            this.Items = new List<DeploymentApiModel>();
            deployments.Items.ForEach(deployment => this.Items.Add(new DeploymentApiModel(deployment)));

            this.Metadata = new Dictionary<string, string>
            {
                { "$type", $"DevicePropertyList;{Version.NUMBER}" },
                { "$url", $"/{Version.PATH}/deviceproperties" }
            };
        }
    }
}