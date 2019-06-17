// Copyright (c) Microsoft. All rights reserved.

using System.Collections.Generic;
using Microsoft.Azure.IoTSolutions.UIConfig.Services.Models.Actions;
using Newtonsoft.Json;

namespace Microsoft.Azure.IoTSolutions.UIConfig.WebService.v1.Models
{
    public class ActionSettingsListApiModel
    {
        [JsonProperty("Items")]
        public List<ActionSettingsApiModel> Items { get; set; }

        [JsonProperty("$metadata")]
        public Dictionary<string, string> Metadata { get; set; }

        public ActionSettingsListApiModel(List<IActionSettings> actionSettingsList)
        {
            this.Items = new List<ActionSettingsApiModel>();

            foreach (var actionSettings in actionSettingsList)
            {
                this.Items.Add(new ActionSettingsApiModel(actionSettings));
            }

            this.Metadata = new Dictionary<string, string>
            {
                { "$type", $"ActionSettingsList;{Version.NUMBER}" },
                { "$url", $"/{Version.PATH}/solution-settings/actions" }
            };
        }
    }
}
