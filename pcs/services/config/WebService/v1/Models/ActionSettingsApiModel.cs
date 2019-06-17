// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using Microsoft.Azure.IoTSolutions.UIConfig.Services.Models.Actions;
using Newtonsoft.Json;

namespace Microsoft.Azure.IoTSolutions.UIConfig.WebService.v1.Models
{
    public class ActionSettingsApiModel
    {
        [JsonProperty("Type")]
        public string Type { get; set; }

        [JsonProperty("Settings")]
        public IDictionary<string, object> Settings { get; set; }

        public ActionSettingsApiModel()
        {
            this.Type = ActionType.Email.ToString();
            this.Settings = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
        }

        public ActionSettingsApiModel(IActionSettings actionSettings)
        {
            this.Type = actionSettings.Type.ToString();

            this.Settings = new Dictionary<string, object>(
                actionSettings.Settings,
                StringComparer.OrdinalIgnoreCase); 
        }
    }
}
