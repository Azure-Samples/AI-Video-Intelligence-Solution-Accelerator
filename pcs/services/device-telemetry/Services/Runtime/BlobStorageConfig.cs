using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Runtime
{
    public interface IBlobStorageConfig
    {
        // Azure blob credentials
        string BlobStorageConnectionString { get; }
        string BlobStorageWebUiDirectAccessPolicy { get; }
        int BlobStorageWebUiDirectAccessExpiryMinutes { get; }
    }

    public class BlobStorageConfig : IBlobStorageConfig
    {
        public string BlobStorageConnectionString { get; set; }
        public string BlobStorageWebUiDirectAccessPolicy { get; set; }
        public int BlobStorageWebUiDirectAccessExpiryMinutes { get; set; }
    }
}
