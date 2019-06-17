// Copyright (c) Microsoft. All rights reserved.

using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.IoTSolutions.UIConfig.Services.Diagnostics;
using Microsoft.Azure.IoTSolutions.UIConfig.Services.Exceptions;
using Microsoft.Azure.IoTSolutions.UIConfig.Services.Http;
using Microsoft.Azure.IoTSolutions.UIConfig.Services.Runtime;
using Newtonsoft.Json;

namespace Microsoft.Azure.IoTSolutions.UIConfig.Services.External
{
    public interface IUserManagementClient
    {
        Task<IEnumerable<string>> GetAllowedActionsAsync(string userObjectId, IEnumerable<string> roles);

        Task<string> GetTokenAsync();
    }

    public class UserManagementClient : IUserManagementClient
    {
        private readonly IHttpClient httpClient;
        private readonly ILogger log;
        private readonly string serviceUri;
        private const string DEFAULT_USER_ID = "default";

        public UserManagementClient(
            IHttpClient httpClient,
            IServicesConfig config,
            ILogger logger)
        {
            this.httpClient = httpClient;
            this.log = logger;
            this.serviceUri = config.UserManagementApiUrl;
        }

        public async Task<IEnumerable<string>> GetAllowedActionsAsync(string userObjectId, IEnumerable<string> roles)
        {
            var request = this.CreateRequest($"users/{userObjectId}/allowedActions", roles);
            var response = await this.httpClient.PostAsync(request);
            this.CheckStatusCode(response, request);

            return JsonConvert.DeserializeObject<IEnumerable<string>>(response.Content);
        }

        public async Task<string> GetTokenAsync()
        {
            // Note: The DEFAULT_USER_ID is set to any value. The user management service doesn't 
            // currently use the user ID information, but if this API is updated in the future, we 
            // will need to grab the user ID from the request JWT token and pass in here.
            var request = this.CreateRequest($"users/{DEFAULT_USER_ID}/token");

            var response = await this.httpClient.GetAsync(request);
            this.CheckStatusCode(response, request);

            var tokenResponse = JsonConvert.DeserializeObject<TokenApiModel>(response.Content);
            return tokenResponse.AccessToken;
        }

        private HttpRequest CreateRequest(string path, IEnumerable<string> content = null)
        {
            var request = new HttpRequest();
            request.SetUriFromString($"{this.serviceUri}/{path}");
            if (this.serviceUri.ToLowerInvariant().StartsWith("https:"))
            {
                request.Options.AllowInsecureSSLServer = true;
            }

            if (content != null)
            {
                request.SetContent(content);
            }

            return request;
        }

        private void CheckStatusCode(IHttpResponse response, IHttpRequest request)
        {
            if (response.IsSuccessStatusCode)
            {
                return;
            }

            this.log.Info($"Auth service returns {response.StatusCode} for request {request.Uri}", () => new
            {
                request.Uri,
                response.StatusCode,
                response.Content
            });

            switch (response.StatusCode)
            {
                case HttpStatusCode.NotFound:
                    throw new ResourceNotFoundException($"{response.Content}, request URL = {request.Uri}");
                case HttpStatusCode.Forbidden:
                    throw new NotAuthorizedException("The user or the application is not authorized to make the " +
                                                     $"request to the user management service, content = {response.Content}, " +
                                                     $"request URL = {request.Uri}");
                default:
                    throw new HttpRequestException($"Http request failed, status code = {response.StatusCode}, " +
                                                   "content = {response.Content}, request URL = {request.Uri}");
            }
        }
    }
}