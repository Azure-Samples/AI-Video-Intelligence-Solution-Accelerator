// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Threading.Tasks;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Storage;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Diagnostics;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Models;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Runtime;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Shared.Protocol;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Exceptions;

namespace Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Storage
{
    public interface IBlobStorageHelper
    {
        SasUrlForBlobAccess GetSasUrlForBlobAccess(string container, string name);
        Task<StatusResultServiceModel> PingAsync();
    }

    public class BlobStorageHelper : IBlobStorageHelper
    {
        private readonly ICloudStorageWrapper cloudStorageWrapper;
        private readonly IBlobStorageConfig blobStorageConfig;
        private readonly ILogger logger;
        private CloudBlobClient cloudBlobClient;
        private bool isInitialized;

        public BlobStorageHelper(
            IBlobStorageConfig blobStorageConfig,
            ICloudStorageWrapper cloudStorageWrapper,
            ILogger logger)
        {
            this.logger = logger;
            this.blobStorageConfig = blobStorageConfig;
            this.cloudStorageWrapper = cloudStorageWrapper;
            this.isInitialized = false;
        }

        private void InitializeBlobStorage()
        {
            if (!this.isInitialized)
            {
                string storageConnectionString = this.blobStorageConfig.BlobStorageConnectionString;
                CloudStorageAccount account = this.cloudStorageWrapper.Parse(storageConnectionString);
                this.cloudBlobClient = this.cloudStorageWrapper.CreateCloudBlobClient(account);
                this.isInitialized = true;
            }
        }

        public SasUrlForBlobAccess GetSasUrlForBlobAccess(string container, string name)
        {
            this.InitializeBlobStorage();
            CloudBlobContainer blobContainer = this.cloudBlobClient.GetContainerReference(container);
            //Set the expiry time for the container.
            //In this case no start time is specified, so the shared access signature becomes valid immediately.
            SharedAccessBlobPolicy sasConstraints = new SharedAccessBlobPolicy();
            sasConstraints.SharedAccessExpiryTime = 
                DateTimeOffset.UtcNow.AddMinutes(blobStorageConfig.BlobStorageWebUiDirectAccessExpiryMinutes);
            //The sasConstraints.Permissions property must NOT be set or it will cause problems with the container's access policy

            string sas = blobContainer.GetSharedAccessSignature(sasConstraints, blobStorageConfig.BlobStorageWebUiDirectAccessPolicy);
            string result = blobContainer.StorageUri.PrimaryUri.ToString() + "/" + name + sas;

            return new SasUrlForBlobAccess(result);
        }


        public async Task<StatusResultServiceModel> PingAsync()
        {
            var result = new StatusResultServiceModel(false, "Blob helper in device telemetry check failed");

            try
            {
                this.InitializeBlobStorage();
                ServiceProperties response = await this.cloudBlobClient.GetServicePropertiesAsync();
                if (response != null)
                {
                    result.Message = "Alive and well!";
                    result.IsHealthy = true;
                }
            }
            catch (Exception e)
            {
                this.logger.Error(result.Message, () => new { e });
            }

            return result;
        }
    }
}
