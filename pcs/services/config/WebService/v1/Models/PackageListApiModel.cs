// Copyright (c) Microsoft. All rights reserved.

using System.Collections.Generic;
using System.Linq;
using Microsoft.Azure.IoTSolutions.UIConfig.Services.Models;
using Newtonsoft.Json;

namespace Microsoft.Azure.IoTSolutions.UIConfig.WebService.v1.Models
{
    public class PackageListApiModel
    {
        public IEnumerable<PackageApiModel> Items { get; set; }

        [JsonProperty(PropertyName = "$metadata")]
        public Dictionary<string, string> Metadata { get; set; }

        public PackageListApiModel(IEnumerable<PackageServiceModel> models)
        {
            this.Items = models.Select(m => new PackageApiModel(m));

            this.Metadata = new Dictionary<string, string>
            {
                { "$type", $"DevicePropertyList;{Version.NUMBER}" },
                { "$url", $"/{Version.PATH}/deviceproperties" }
            };
        }
    }
}