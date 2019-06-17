// Copyright (c) Microsoft. All rights reserved.

using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.IoTSolutions.UIConfig.Services;
using Microsoft.Azure.IoTSolutions.UIConfig.WebService.v1.Filters;
using Microsoft.Azure.IoTSolutions.UIConfig.WebService.v1.Models;

namespace Microsoft.Azure.IoTSolutions.UIConfig.WebService.v1.Controllers
{
    [Route(Version.PATH + "/configtypes"), TypeFilter(typeof(ExceptionsFilterAttribute))]
    public class ConfigTypesController
    {
        private readonly IStorage storage;

        public ConfigTypesController(IStorage storage)
        {
            this.storage = storage;
        }

        [HttpGet]
        [Authorize("ReadAll")]
        public async Task<ConfigTypeListApiModel> GetAllConfigTypesAsync()
        {
            return new ConfigTypeListApiModel(await this.storage.GetConfigTypesListAsync());
        }
    }
}
