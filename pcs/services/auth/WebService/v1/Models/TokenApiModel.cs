// Copyright (c) Microsoft. All rights reserved.

using System;
using Microsoft.Azure.IoTSolutions.Auth.Services.Models;
using Newtonsoft.Json;

namespace Microsoft.Azure.IoTSolutions.Auth.WebService.v1.Models
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

        public TokenApiModel(AccessToken token)
        {
            this.Audience = token.Audience;
            this.AccessTokenType = token.Type;
            this.AccessToken = token.Value;
            this.Authority = token.Authority;
            this.ExpiresOn = token.ExpiresOn;
        }
    }
}
