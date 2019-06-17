// Copyright (c) Microsoft. All rights reserved.

using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.IoTSolutions.IotHubManager.Services;
using Microsoft.Azure.IoTSolutions.IotHubManager.Services.Exceptions;
using Microsoft.Azure.IoTSolutions.IotHubManager.WebService.v1.Filters;
using Microsoft.Azure.IoTSolutions.IotHubManager.WebService.v1.Models;

namespace Microsoft.Azure.IoTSolutions.IotHubManager.WebService.v1.Controllers
{
    [Route(Version.PATH + "/[controller]"), ExceptionsFilter]
    public class ModulesController : Controller
    {
        private const string CONTINUATION_TOKEN_NAME = "x-ms-continuation";
        private readonly IDevices devices;

        public ModulesController(IDevices devices)
        {
            this.devices = devices;
        }

        /// <summary>Retrieve module twin properties based on provided query</summary>
        /// <param name="query">Where clause of IoTHub query</param>
        /// <returns>List of module twins</returns>
        [HttpGet]
        [Authorize("ReadAll")]
        public async Task<TwinPropertiesListApiModel> GetModuleTwinsAsync([FromQuery] string query)
        {
            string continuationToken = string.Empty;
            if (this.Request.Headers.ContainsKey(CONTINUATION_TOKEN_NAME))
            {
                continuationToken = this.Request.Headers[CONTINUATION_TOKEN_NAME].FirstOrDefault();
            }

            return new TwinPropertiesListApiModel(
                await this.devices.GetModuleTwinsByQueryAsync(query, continuationToken));
        }

        /// <summary>Retrieve module twin properties. Query in body of post request</summary>
        /// <param name="query">Where clause of IoTHub query</param>
        /// <returns>List of module twins</returns>
        [HttpPost("query")]
        [Authorize("ReadAll")]
        public async Task<TwinPropertiesListApiModel> QueryModuleTwinsAsync([FromBody] string query)
        {
            return await this.GetModuleTwinsAsync(query);
        }

        /// <summary>Get module information for a device</summary>
        /// <param name="deviceId">Device Id</param>
        /// <param name="moduleId">Module Id</param>
        /// <returns>Device information</returns>
        [HttpGet("{deviceId}/{moduleId}")]
        [Authorize("ReadAll")]
        public async Task<TwinPropertiesApiModel> GetModuleTwinAsync(string deviceId, string moduleId)
        {
            if (string.IsNullOrWhiteSpace(deviceId))
            {
                throw new InvalidInputException("deviceId must be provided");
            }

            if (string.IsNullOrWhiteSpace(moduleId))
            {
                throw new InvalidInputException("moduleId must be provided");
            }

            var twin = await this.devices.GetModuleTwinAsync(deviceId, moduleId);
            return new TwinPropertiesApiModel(twin.DesiredProperties, twin.ReportedProperties,
                                              deviceId, moduleId);
        }
    }
}
