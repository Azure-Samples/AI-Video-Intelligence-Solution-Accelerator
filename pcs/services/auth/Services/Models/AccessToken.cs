using System;
using Microsoft.IdentityModel.Clients.ActiveDirectory;

namespace Microsoft.Azure.IoTSolutions.Auth.Services.Models
{
    public class AccessToken
    {
        public AccessToken(string audience, AuthenticationResult authenticationResult)
        {
            this.Audience = audience;
            this.Value = authenticationResult.AccessToken;
            this.Type = authenticationResult.AccessTokenType;
            this.ExpiresOn = authenticationResult.ExpiresOn;
            this.Authority = authenticationResult.Authority;
        }

        public string Audience { get; set; }
        public string Type { get; set; }
        public string Value { get; set; }
        public string Authority { get; set; }
        public DateTimeOffset ExpiresOn { get; set; }
    }
}
