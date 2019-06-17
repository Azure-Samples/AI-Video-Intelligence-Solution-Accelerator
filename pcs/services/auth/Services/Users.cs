// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.Azure.IoTSolutions.Auth.Services.Diagnostics;
using Microsoft.Azure.IoTSolutions.Auth.Services.Exceptions;
using Microsoft.Azure.IoTSolutions.Auth.Services.Models;
using Microsoft.Azure.IoTSolutions.Auth.Services.Runtime;
using System.Threading.Tasks;

namespace Microsoft.Azure.IoTSolutions.Auth.Services
{
    public interface IUsers
    {
        User GetUserInfo(IEnumerable<Claim> claims);
        List<string> GetAllowedActions(IEnumerable<string> roles);
        Task<AccessToken> GetToken(string audience);
    }

    public class Users : IUsers
    {
        private readonly ILogger log;
        private readonly IServicesConfig config;
        private readonly IPolicies policies;
        private readonly string rolesKey;

        public Users(
            IServicesConfig config,
            ILogger log,
            IPolicies policies)
        {
            this.config = config;
            this.log = log;
            this.policies = policies;
            this.rolesKey = this.config.JwtRolesFrom.ToLowerInvariant();
        }

        public User GetUserInfo(IEnumerable<Claim> claims)
        {
            // Map all the claims into a dictionary
            var data = new Dictionary<string, string>();

            foreach (var c in claims)
            {
                data[c.Type.ToLowerInvariant()] = c.Value;
            }


            // Extract user information from the claims
            var id = this.config.JwtUserIdFrom
                .Select(key => key.ToLowerInvariant())
                .Where(k => data.ContainsKey(k))
                .Aggregate("", (current, k) => current + ((string)data[k] + ' '))
                .TrimEnd();
            var name = this.config.JwtNameFrom
                .Select(key => key.ToLowerInvariant())
                .Where(k => data.ContainsKey(k))
                .Aggregate("", (current, k) => current + ((string)data[k] + ' '))
                .TrimEnd();
            var email = this.config.JwtEmailFrom
                .Select(key => key.ToLowerInvariant())
                .Where(k => data.ContainsKey(k))
                .Aggregate("", (current, k) => current + ((string)data[k] + ' '))
                .TrimEnd();

            // Extract roles array from claims
            var roles = (from c in claims
                         where string.Equals(c.Type.ToLowerInvariant(), this.rolesKey, StringComparison.OrdinalIgnoreCase)
                         select c.Value).ToList();

            // Get allowed actions based on policy
            var allowedActions = this.GetAllowedActions(roles);

            if (string.IsNullOrEmpty(id)) id = "-unknown-";
            if (string.IsNullOrEmpty(name)) name = "user name unknown";
            if (string.IsNullOrEmpty(email)) email = "email address unknown";

            return new User
            {
                Id = id,
                Name = name,
                Email = email,
                AllowedActions = allowedActions,
                Roles = roles
            };
        }

        public List<string> GetAllowedActions(IEnumerable<string> roles)
        {
            // ensure only unique values are added to the allowed actions list
            // if duplicate actions are allowed in multiple roles
            var allowedActions = new HashSet<string>();
            foreach (var role in roles)
            {
                var policy = this.policies.GetByRole(role);
                allowedActions.UnionWith(policy.AllowedActions);
            }

            return allowedActions.ToList();
        }

        public async Task<AccessToken> GetToken(string audience)
        {
            // if no audiene is provided, use Azure Resource Manager endpoint url by default
            audience = string.IsNullOrEmpty(audience) ? this.config.ArmEndpointUrl : audience;

            if (string.IsNullOrEmpty(this.config.AadTenantId) ||
                string.IsNullOrEmpty(this.config.AadApplicationId) ||
                string.IsNullOrEmpty(this.config.AadApplicationSecret))
            {
                var message = $"Azure Active Directory properties '{nameof(this.config.AadEndpointUrl)}', '{nameof(this.config.AadTenantId)}'" +
                    $", '{nameof(this.config.AadApplicationId)}' or '{nameof(this.config.AadApplicationSecret)}' are not set.";
                this.log.Error(message, () => { });
                throw new InvalidConfigurationException(message);
            }

            string authorityUrl = this.config.AadEndpointUrl.EndsWith("/") ?
                $"{this.config.AadEndpointUrl}{this.config.AadTenantId}" :
                $"{this.config.AadEndpointUrl}/{this.config.AadTenantId}";

            var authenticationContext = new AuthenticationContext(authorityUrl, TokenCache.DefaultShared);
            try
            {
                AuthenticationResult authenticationResult = await authenticationContext.AcquireTokenAsync(
                    resource: audience,
                    clientCredential: new ClientCredential(
                        clientId: this.config.AadApplicationId,
                        clientSecret: this.config.AadApplicationSecret));
                return new AccessToken(audience, authenticationResult);
            }
            catch (Exception e)
            {
                var message = $"Unable to retrieve token with Azure Active Directory properties '{nameof(this.config.AadEndpointUrl)}', " +
                    $"'{nameof(this.config.AadTenantId)}', '{nameof(this.config.AadApplicationId)}' or '{nameof(this.config.AadApplicationSecret)}'.";
                this.log.Error(message, () => { });
                throw new InvalidConfigurationException(message, e);
            }
        }
    }
}
