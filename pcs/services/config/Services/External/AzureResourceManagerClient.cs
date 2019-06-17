// Copyright (c) Microsoft. All rights reserved.

using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Azure.IoTSolutions.UIConfig.Services.Exceptions;
using Microsoft.Azure.IoTSolutions.UIConfig.Services.Http;
using Microsoft.Azure.IoTSolutions.UIConfig.Services.Runtime;

namespace Microsoft.Azure.IoTSolutions.UIConfig.Services.External
{
    public interface IAzureResourceManagerClient
    {
        Task<bool> IsOffice365EnabledAsync();
    }

    public class AzureResourceManagerClient : IAzureResourceManagerClient
    {
        private readonly IHttpClient httpClient;
        private readonly IUserManagementClient userManagementClient;
        private readonly IServicesConfig config;

        public AzureResourceManagerClient(
            IHttpClient httpClient,
            IServicesConfig config,
            IUserManagementClient userManagementClient)
        {
            this.httpClient = httpClient;
            this.userManagementClient = userManagementClient;
            this.config = config;
        }

        public async Task<bool> IsOffice365EnabledAsync()
        {
            if (string.IsNullOrEmpty(this.config.SubscriptionId) ||
                string.IsNullOrEmpty(this.config.ResourceGroup) ||
                string.IsNullOrEmpty(this.config.ArmEndpointUrl))
            {
                throw new InvalidConfigurationException("Subscription Id, Resource Group, and Arm Endpoint Url must be specified" +
                                                        "in the environment variable configuration for this " +
                                                        "solution in order to use this API.");
            }

            var logicAppTestConnectionUri = this.config.ArmEndpointUrl +
                                               $"/subscriptions/{this.config.SubscriptionId}/" +
                                               $"resourceGroups/{this.config.ResourceGroup}/" +
                                               "providers/Microsoft.Web/connections/" +
                                               "office365-connector/extensions/proxy/testconnection?" +
                                               $"api-version={this.config.ManagementApiVersion}";

            var request = await this.CreateRequest(logicAppTestConnectionUri);

            var response = await this.httpClient.GetAsync(request);

            if (response.StatusCode == HttpStatusCode.Forbidden)
            {
                throw new NotAuthorizedException("The application is not authorized and has not been " +
                                                 "assigned contributor permissions for the subscription. Go to the Azure portal and " +
                                                 "assign the application as a contributor in order to retrieve the token.");
            }

            return response.IsSuccessStatusCode;
        }

        private async Task<HttpRequest> CreateRequest(string uri, IEnumerable<string> content = null)
        {
            var request = new HttpRequest();
            request.SetUriFromString(uri);
            if (uri.ToLowerInvariant().StartsWith("https:"))
            {
                request.Options.AllowInsecureSSLServer = true;
            }

            if (content != null)
            {
                request.SetContent(content);
            }

            var token = await this.userManagementClient.GetTokenAsync();
            request.Headers.Add("Authorization", "Bearer " + token);

            return request;
        }
    }
}
