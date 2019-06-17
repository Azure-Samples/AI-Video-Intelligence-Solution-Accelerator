// Copyright (c) Microsoft. All rights reserved.

using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.IoTSolutions.AsaManager.Services;
using Microsoft.Azure.IoTSolutions.AsaManager.WebService.Runtime;
using Microsoft.Azure.IoTSolutions.AsaManager.WebService.v1.Filters;
using Microsoft.Azure.IoTSolutions.AsaManager.WebService.v1.Models;

namespace Microsoft.Azure.IoTSolutions.AsaManager.WebService.v1.Controllers
{
    /*
     * StatusController is not accessible on deployed services because it is an internal service running in
     * the background. The below status API responds only on local machine. 
     */
    [Route(Version.PATH + "/[controller]"), ExceptionsFilter]
    public sealed class StatusController : Controller
    {
        private readonly IConfig config;
        private readonly IStatusService statusService;

        public StatusController(IConfig config, IStatusService statusService)
        {
            this.config = config;
            this.statusService = statusService;
        }

        public async Task<StatusApiModel> GetAsync()
        {
            var result = new StatusApiModel(await this.statusService.GetStatusAsync());

            result.Properties.Add("Port", this.config.Port.ToString());
            return result;
        }
    }
}

