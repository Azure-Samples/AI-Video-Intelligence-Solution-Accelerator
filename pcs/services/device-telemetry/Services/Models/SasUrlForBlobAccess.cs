using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Models
{
    public class SasUrlForBlobAccess
    {
        public string Url { get; set; }

        public SasUrlForBlobAccess(string url)
        {
            this.Url = url;
        }
    }
}
