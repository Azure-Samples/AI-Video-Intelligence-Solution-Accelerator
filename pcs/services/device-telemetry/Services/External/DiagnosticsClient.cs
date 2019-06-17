// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Diagnostics;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Http;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Runtime;
using Newtonsoft.Json;

namespace Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.External
{
    public interface IDiagnosticsClient
    {
        bool CanLogToDiagnostics { get; }

        Task LogEventAsync(string eventName);

        Task LogEventAsync(string eventName, Dictionary<string, object> eventProperties);

        Task<Tuple<bool, string>> PingAsync();
    }

    public class DiagnosticsClient : IDiagnosticsClient
    {
        public bool CanLogToDiagnostics { get; }

        private readonly IHttpClient httpClient;
        private readonly ILogger log;
        private readonly string serviceUrl;
        private readonly int maxRetries;
        private const int RETRY_SLEEP_MS = 500;

        public DiagnosticsClient(IHttpClient httpClient, IServicesConfig config, ILogger logger)
        {
            this.httpClient = httpClient;
            this.log = logger;
            this.serviceUrl = config.DiagnosticsApiUrl;
            this.maxRetries = config.DiagnosticsMaxLogRetries;
            if (string.IsNullOrEmpty(this.serviceUrl))
            {
                this.log.Error("Cannot log to diagnostics service, diagnostics url not provided", () => { });
                this.CanLogToDiagnostics = false;
            }
            else
            {
                this.CanLogToDiagnostics = true;
            }
        }

        /**
         * Logs event with given event name and empty event properties
         * to diagnostics event endpoint.
         */
        public async Task LogEventAsync(string eventName)
        {
            await this.LogEventAsync(eventName, new Dictionary<string, object>());
        }

        /**
         * Logs event with given event name and event properties
         * to diagnostics event endpoint.
         */
        public async Task LogEventAsync(string eventName, Dictionary<string, object> eventProperties)
        {
            var request = new HttpRequest();
            try
            {
                request.SetUriFromString($"{this.serviceUrl}/diagnosticsevents");
                DiagnosticsRequestModel model = new DiagnosticsRequestModel
                {
                    EventType = eventName,
                    EventProperties = eventProperties
                };
                request.SetContent(JsonConvert.SerializeObject(model));
                await this.PostHttpRequestWithRetryAsync(request);
            }
            catch (Exception e)
            {
                this.log.Warn("Cannot log to diagnostics service, diagnostics url not provided", () => new { e.Message });
            }
        }

        private async Task PostHttpRequestWithRetryAsync(HttpRequest request)
        {
            int retries = 0;
            bool requestSucceeded = false;
            while (!requestSucceeded && retries < this.maxRetries)
            {
                try
                {
                    IHttpResponse response = await this.httpClient.PostAsync(request);
                    if (response.StatusCode != HttpStatusCode.OK)
                    {
                        retries++;
                        this.LogAndSleepOnFailure(retries, response.Content);
                    }
                    else
                    {
                        requestSucceeded = true;
                    }
                }
                catch (Exception e)
                {
                    retries++;
                    this.LogAndSleepOnFailure(retries, e.Message);
                }
            }
        }

        private void LogAndSleepOnFailure(int retries, string errorMessage)
        {
            if (retries < this.maxRetries)
            {
                int retriesLeft = this.maxRetries - retries;
                string logString = $"Failed to log to diagnostics, {retriesLeft} retries remaining";
                this.log.Warn(logString, () => new { errorMessage });
                Thread.Sleep(RETRY_SLEEP_MS);
            }
            else
            {
                this.log.Error("Failed to log to diagnostics, reached max retries and will not log", () => new { errorMessage });
            }
        }

        public async Task<Tuple<bool, string>> PingAsync()
        {
            var isHealthy = false;
            var message = "Diagnostics check failed";
            var request = new HttpRequest();
            try
            {
                request.SetUriFromString($"{this.serviceUrl}/status");
                var response = await this.httpClient.GetAsync(request);

                if (response.IsError)
                {
                    message = "Status code: " + response.StatusCode + "; Response: " + response.Content;
                }
                else
                {
                    var data = JsonConvert.DeserializeObject<Dictionary<string, object>>(response.Content);
                    message = data["Message"].ToString();
                    isHealthy = Convert.ToBoolean(data["IsHealthy"]);
                }
            }
            catch (Exception e)
            {
                this.log.Error(message, () => new { e });
            }

            return new Tuple<bool, string>(isHealthy, message);
        }
    }
}
