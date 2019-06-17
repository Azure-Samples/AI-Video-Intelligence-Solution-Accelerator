// Copyright (c) Microsoft. All rights reserved.

using System;
using Newtonsoft.Json;
namespace Microsoft.Azure.IoTSolutions.UIConfig.Services.External
{ 
    public class TokenApiModel
    {
        [JsonProperty(PropertyName = "Audience", Order = 10)]
        public string Audience { get; set; }

        [JsonProperty(PropertyName = "AccessTokenType", Order = 20)]
        public string AccessTokenType { get; set; }

        [JsonProperty(PropertyName = "AccessToken", Order = 30)]
        public string AccessToken { get; set; }

        [JsonProperty(PropertyName = "Authority", Order = 40)]
        public string Authority { get; set; }

        [JsonProperty(PropertyName = "ExpiresOn", Order = 50)]
        public DateTimeOffset ExpiresOn { get; set; }
    }
}