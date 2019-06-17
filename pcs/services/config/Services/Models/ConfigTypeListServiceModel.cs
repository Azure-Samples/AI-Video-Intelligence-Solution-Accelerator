// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Azure.IoTSolutions.UIConfig.Services.Models;
using Newtonsoft.Json;

namespace Microsoft.Azure.IoTSolutions.UIConfig.Services.External
{
    public class ConfigTypeListServiceModel
    {
        private HashSet<String> configTypes = new HashSet<String>();

        [JsonProperty("configtypes")]
        public string[] ConfigTypes
        {
            get
            {
                return configTypes.ToArray<String>();
            }
            set
            {
                Array.ForEach<String>(value, (c => configTypes.Add(c)));
            }
        }

        internal void add(string customConfig)
        {
            configTypes.Add(customConfig.Trim());
        }

    }
}
