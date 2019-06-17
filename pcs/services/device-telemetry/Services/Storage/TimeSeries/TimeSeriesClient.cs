// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Diagnostics;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Exceptions;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Http;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Models;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Runtime;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Storage.TimeSeries
{
    public interface ITimeSeriesClient
    {
        Task<StatusResultServiceModel> PingAsync();

        Task<MessageList> QueryEventsAsync(
            DateTimeOffset? from,
            DateTimeOffset? to,
            string order,
            int skip,
            int limit,
            string[] deviceIds);
    }

    public class TimeSeriesClient : ITimeSeriesClient
    {
        private readonly IHttpClient httpClient;
        private readonly ILogger log;

        private AuthenticationResult token;

        private readonly string authority;
        private readonly string applicationId;
        private readonly string applicationSecret;
        private readonly string tenant;
        private readonly string fqdn;
        private readonly string host;
        private readonly string apiVersion;
        private readonly string timeout;

        private const string TSI_DATE_FORMAT = "yyyy-MM-ddTHH:mm:ssZ";

        private const string TIME_SERIES_API_VERSION_PREFIX = "api-version";
        private const string TIME_SERIES_TIMEOUT_PREFIX = "timeout";
        private const string EVENTS_KEY = "events";
        private const string AVAILABILITY_KEY = "availability";
        private const string SEARCH_SPAN_KEY = "searchSpan";
        private const string PREDICATE_KEY = "predicate";
        private const string PREDICATE_STRING_KEY = "predicateString";
        private const string TOP_KEY = "top";
        private const string SORT_KEY = "sort";
        private const string SORT_INPUT_KEY = "input";
        private const string BUILT_IN_PROP_KEY = "builtInProperty";
        private const string BUILT_IN_PROP_VALUE = "$ts";
        private const string SORT_ORDER_KEY = "order";
        private const string COUNT_KEY = "count";
        private const string FROM_KEY = "from";
        private const string TO_KEY = "to";
        private const int CLOCK_CALIBRATION_IN_SECONDS = 5;

        private const string DEVICE_ID_KEY = "iothub-connection-device-id";

        private const string AAD_CLIENT_ID_KEY = "ApplicationClientId";
        private const string AAD_CLIENT_SECRET_KEY = "ApplicationClientSecret";
        private const string AAD_TENANT_KEY = "Tenant";

        public TimeSeriesClient(
            IHttpClient httpClient,
            IServicesConfig config,
            ILogger log)
        {
            this.httpClient = httpClient;
            this.log = log;
            this.authority = config.TimeSeriesAuthority;
            this.applicationId = config.ActiveDirectoryAppId;
            this.applicationSecret = config.ActiveDirectoryAppSecret;
            this.tenant = config.ActiveDirectoryTenant;
            this.fqdn = config.TimeSeriesFqdn;
            this.host = config.TimeSeriesAudience;
            this.apiVersion = config.TimeSertiesApiVersion;
            this.timeout = config.TimeSeriesTimeout;
        }

        /// <summary>
        /// Makes a request to the environment availability API to verify
        /// that the fqdn provided can reach Time Series Insights.
        /// Returns a tuple with the status [bool isAvailable, string message].
        /// </summary>
        public async Task<StatusResultServiceModel> PingAsync()
        {
            var result = new StatusResultServiceModel(false, "TimeSeries check failed");

            // Acquire an access token.
            string accessToken = "";
            try
            {
                accessToken = await this.AcquireAccessTokenAsync();

                // Prepare request
                HttpRequest request = this.PrepareRequest(
                    AVAILABILITY_KEY,
                    accessToken,
                    new[] { TIME_SERIES_TIMEOUT_PREFIX + "=" + this.timeout });

                var response = await this.httpClient.GetAsync(request);

                // Return status
                if (!response.IsError)
                {
                    result.IsHealthy = true;
                    result.Message = "Alive and well!";
                }
                else
                {
                    result.Message = $"Status code: {response.StatusCode}; Response: {response.Content}";
                }

            }
            catch (Exception e)
            {
                this.log.Error(result.Message, () => new { e });
            }
            return result;
        }

        public async Task<MessageList> QueryEventsAsync(
            DateTimeOffset? from,
            DateTimeOffset? to,
            string order, int skip,
            int limit,
            string[] deviceIds)
        {
            // Acquire an access token.
            string accessToken = await this.AcquireAccessTokenAsync();

            // Prepare request
            HttpRequest request = this.PrepareRequest(
                EVENTS_KEY,
                accessToken,
                new[] { TIME_SERIES_TIMEOUT_PREFIX + "=" + this.timeout });

            request.SetContent(
                this.PrepareInput(from, to, order, skip, limit, deviceIds));

            var msg = "Making Query to Time Series: Uri" + request.Uri + " Body: " + request.Content;
            this.log.Info(msg, () => new { request.Uri, request.Content });

            var response = await this.httpClient.PostAsync(request);
            var messages = JsonConvert.DeserializeObject<ValueListApiModel>(response.Content);

            return messages.ToMessageList(skip);
        }

        private async Task<string> AcquireAccessTokenAsync()
        {
            // Return existing token unless it is near expiry or null
            if (this.token != null)
            {
                // Add buffer time to renew token, built in buffer for AAD is 5 mins
                if (DateTimeOffset.UtcNow.AddSeconds(CLOCK_CALIBRATION_IN_SECONDS) < this.token.ExpiresOn)
                {
                    return this.token.AccessToken;
                }
            }

            if (string.IsNullOrEmpty(this.applicationId) ||
                string.IsNullOrEmpty(this.applicationSecret) ||
                string.IsNullOrEmpty(this.tenant))
            {
                throw new InvalidConfigurationException(
                    $"Active Directory properties '{AAD_CLIENT_ID_KEY}', '{AAD_CLIENT_SECRET_KEY}' " +
                    $"and '{AAD_TENANT_KEY}' are not set.");
            }

            var authenticationContext = new AuthenticationContext(
                this.authority + this.tenant,
                TokenCache.DefaultShared);

            try
            {
                AuthenticationResult tokenResponse = await authenticationContext.AcquireTokenAsync(
                    resource: this.host,
                    clientCredential: new ClientCredential(
                        clientId: this.applicationId,
                        clientSecret: this.applicationSecret));

                this.token = tokenResponse;

                return this.token.AccessToken;
            }
            catch (Exception e)
            {
                var msg = "Unable to retrieve token with Active Directory properties" +
                          $"'{AAD_CLIENT_ID_KEY}', '{AAD_CLIENT_SECRET_KEY}' and '{AAD_TENANT_KEY}'.";
                throw new InvalidConfigurationException(msg, e);
            }
        }

        /// <summary>
        /// Creates the request body for the Time Series get events API 
        /// </summary>
        private JObject PrepareInput(
            DateTimeOffset? from,
            DateTimeOffset? to,
            string order,
            int skip,
            int limit,
            string[] deviceIds)
        {
            var result = new JObject();

            // Add the search span clause
            // End of the interval is exclusive
            if (!to.HasValue) to = DateTimeOffset.UtcNow;
            if (!from.HasValue) from = DateTimeOffset.MinValue;

            result.Add(SEARCH_SPAN_KEY, new JObject(
                new JProperty(FROM_KEY, from.Value.ToString(TSI_DATE_FORMAT)),
                new JProperty(TO_KEY, to.Value.ToString(TSI_DATE_FORMAT))));

            // Add the predicate for devices
            if (deviceIds != null && deviceIds.Length > 0)
            {
                var devicePredicates = new List<string>();
                foreach (var deviceId in deviceIds)
                {
                    devicePredicates.Add($"[{DEVICE_ID_KEY}].String='{deviceId}'");
                }

                var predicateStringObject = new JObject
                {
                    new JProperty(PREDICATE_STRING_KEY, string.Join(" OR ", devicePredicates))
                };
                result.Add(PREDICATE_KEY, predicateStringObject);
            }

            // Add the limit top clause
            JObject builtInPropObject = new JObject(new JProperty(BUILT_IN_PROP_KEY, BUILT_IN_PROP_VALUE));
            JArray sortArray = new JArray(new JObject
                {
                    { SORT_INPUT_KEY, builtInPropObject },
                    { SORT_ORDER_KEY, order }
                }
            );

            JObject topObject = new JObject
            {
                { SORT_KEY, sortArray },
                { COUNT_KEY, skip + limit }
            };

            result.Add(TOP_KEY, topObject);

            return result;
        }

        /// <summary>
        /// Creates an HttpRequest for Time Series Insights with the required headers and tokens.
        /// </summary>
        private HttpRequest PrepareRequest(
            string path,
            string accessToken,
            string[] queryArgs = null)
        {
            string args = TIME_SERIES_API_VERSION_PREFIX + "=" + this.apiVersion;
            if (queryArgs != null && queryArgs.Any())
            {
                args += "&" + String.Join("&", queryArgs);
            }

            Uri uri = new UriBuilder("https", this.fqdn)
            {
                Path = path,
                Query = args
            }.Uri;
            HttpRequest request = new HttpRequest(uri);
            request.Headers.Add("x-ms-client-application-name", this.applicationId);
            request.Headers.Add("Authorization", "Bearer " + accessToken);

            return request;
        }
    }

}
