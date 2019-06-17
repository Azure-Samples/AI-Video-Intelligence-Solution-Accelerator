// Copyright (c) Microsoft. All rights reserved.

using Microsoft.Azure.IoTSolutions.IotHubManager.Services.Diagnostics;
using Microsoft.Azure.IoTSolutions.IotHubManager.Services.Http;
using Microsoft.Azure.IoTSolutions.IotHubManager.Services.Models;
using Microsoft.Azure.IoTSolutions.IotHubManager.Services.Runtime;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace Microsoft.Azure.IoTSolutions.IotHubManager.Services
{
    class StatusService : IStatusService
    {
        private const bool ALLOW_INSECURE_SSL_SERVER = true;
        private readonly int timeoutMS = 10000;

        private readonly IDevices devices;
        private readonly IHttpClient httpClient;
        private readonly ILogger log;
        private readonly IServicesConfig servicesConfig;

        public StatusService(
            ILogger logger,
            IHttpClient httpClient,
            IDevices devices,
            IServicesConfig servicesConfig
            )
        {
            this.log = logger;
            this.httpClient = httpClient;
            this.devices = devices;
            this.servicesConfig = servicesConfig;
        }

        public async Task<StatusServiceModel> GetStatusAsync(bool authRequired)
        {
            var result = new StatusServiceModel(true, "Alive and well!");
            var errors = new List<string>();

            string storageAdapterName = "StorageAdapter";
            string authName = "Auth";

            // Check access to StorageAdapter
            var storageAdapterResult = await this.PingServiceAsync(
                storageAdapterName,
                this.servicesConfig.StorageAdapterApiUrl);
            SetServiceStatus(storageAdapterName, storageAdapterResult, result, errors);

            if (authRequired)
            {
                // Check access to Auth
                var authResult = await this.PingServiceAsync(
                    authName,
                    this.servicesConfig.UserManagementApiUrl);
                SetServiceStatus(authName, authResult, result, errors);
                result.Properties.Add("UserManagementApiUrl", this.servicesConfig?.UserManagementApiUrl);
            }

            // Preprovisioned IoT hub status
            var isHubPreprovisioned = this.IsHubConnectionStringConfigured();

            if (isHubPreprovisioned)
            {
                var ioTHubResult = await this.devices.PingRegistryAsync();
                SetServiceStatus("IoTHub", ioTHubResult, result, errors);
            }

            if (errors.Count > 0)
            {
                result.Status.Message = string.Join("; ", errors);
            }
            
            result.Properties.Add("StorageAdapterApiUrl", this.servicesConfig?.StorageAdapterApiUrl);

            this.log.Info(
                "Service status request",
                () => new
                {
                    Healthy = result.Status.IsHealthy,
                    result.Status.Message
                });

            return result;
        }

        private void SetServiceStatus(
            string dependencyName,
            StatusResultServiceModel serviceResult,
            StatusServiceModel result,
            List<string> errors
            )
        {
            if (!serviceResult.IsHealthy)
            {
                errors.Add(dependencyName + " check failed");
                result.Status.IsHealthy = false;
            }

            result.Dependencies.Add(dependencyName, serviceResult);
        }
        
        private async Task<StatusResultServiceModel> PingServiceAsync(string serviceName, string serviceURL)
        {
            var result = new StatusResultServiceModel(false, $"{serviceName} check failed");
            try
            {
                var response = await this.httpClient.GetAsync(this.PrepareRequest($"{serviceURL}/status"));
                if (!response.IsSuccessStatusCode)
                {
                    result.Message = $"Status code: {response.StatusCode}; Response: {response.Content}";
                }
                else
                {
                    var data = JsonConvert.DeserializeObject<StatusServiceModel>(response.Content);
                    result = data.Status;
                }
            }
            catch (Exception e)
            {
                this.log.Error(result.Message, () => new { e });
            }

            return result;
        }

        private HttpRequest PrepareRequest(string path)
        {
            var request = new HttpRequest();
            request.AddHeader(HttpRequestHeader.Accept.ToString(), "application/json");
            request.AddHeader(HttpRequestHeader.CacheControl.ToString(), "no-cache");
            request.AddHeader(HttpRequestHeader.Referer.ToString(), "IoTHubManager " + this.GetType().FullName);
            request.SetUriFromString(path);
            request.Options.EnsureSuccess = false;
            request.Options.Timeout = this.timeoutMS;
            if (path.ToLowerInvariant().StartsWith("https:"))
            {
                request.Options.AllowInsecureSSLServer = ALLOW_INSECURE_SSL_SERVER;
            }

            this.log.Debug("Prepare Request", () => new { request });

            return request;
        }

        // Check whether the configuration contains a connection string
        private bool IsHubConnectionStringConfigured()
        {
            var cs = this.servicesConfig?.IoTHubConnString?.ToLowerInvariant().Trim();
            return (!string.IsNullOrEmpty(cs)
                    && cs.Contains("hostname=")
                    && cs.Contains("sharedaccesskeyname=")
                    && cs.Contains("sharedaccesskey="));
        }
    }
}
