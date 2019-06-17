// Copyright (c) Microsoft. All rights reserved.

using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.WebService.Runtime;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.WebService.v1.Filters;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.WebService.v1.Models;

namespace Microsoft.Azure.IoTSolutions.DeviceTelemetry.WebService.v1.Controllers
{
    [Route(Version.PATH + "/[controller]"), TypeFilter(typeof(ExceptionsFilterAttribute))]
    public sealed class StatusController : Controller
    {
        private readonly IConfig config;
        private readonly IStatusService statusService;

        public StatusController(IConfig config, IStatusService statusService)
        {
            this.statusService = statusService;
            this.config = config;
        }

        [HttpGet]
        [Authorize("ReadAll")]
        public async Task<StatusApiModel> GetAsync()
        {
            bool authRequired = this.config.ClientAuthConfig.AuthRequired;
            var serviceStatus = await this.statusService.GetStatusAsync(authRequired);
            var result = new StatusApiModel(serviceStatus);

            result.Properties.Add("AuthRequired", authRequired.ToString());
            result.Properties.Add("Port", this.config.Port.ToString());
            return result;
        }
    }
}
