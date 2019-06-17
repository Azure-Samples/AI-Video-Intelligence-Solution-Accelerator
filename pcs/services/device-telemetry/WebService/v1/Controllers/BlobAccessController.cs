using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Diagnostics;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Models;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Storage;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.WebService.v1.Filters;

namespace Microsoft.Azure.IoTSolutions.DeviceTelemetry.WebService.v1.Controllers
{
    /// <summary>
    /// Returns an URL with embedded SAS token for access to the BLOB in the supplied
    /// container / name. Note that BLOB names may look like paths: a/b/d.jpg
    /// </summary>
    [Route(Version.PATH + "/[controller]"), TypeFilter(typeof(ExceptionsFilterAttribute))]
    public sealed class BlobAccessController : Controller
    {
        private readonly IBlobStorageHelper bobStorageHelper;

        public BlobAccessController(IBlobStorageHelper bobStorageHelper)
        {
            this.bobStorageHelper = bobStorageHelper;
        }

        [HttpGet()]
        [Authorize("ReadAll")]
        public string Get([FromQuery] string container, [FromQuery] string name)
        {
            SasUrlForBlobAccess result = 
                this.bobStorageHelper.GetSasUrlForBlobAccess(container, name);
            return result.Url;
        }
    }
}
