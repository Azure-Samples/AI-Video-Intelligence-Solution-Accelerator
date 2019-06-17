// Copyright (c) Microsoft. All rights reserved.

using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Storage
{
    // Wrapper to help with testability of cloud storage code
    public interface ICloudStorageWrapper
    {
        CloudStorageAccount Parse(string connectionString);
        CloudBlobClient CreateCloudBlobClient(CloudStorageAccount account);
        CloudBlobContainer GetContainerReference(CloudBlobClient client, string containerName);
    }

    public class CloudStoragWrapper : ICloudStorageWrapper
    {
        public CloudBlobClient CreateCloudBlobClient(CloudStorageAccount account)
        {
            return account.CreateCloudBlobClient();
        }

        public CloudBlobContainer GetContainerReference(CloudBlobClient client, string containerName)
        {
            return client.GetContainerReference(containerName);
        }

        public CloudStorageAccount Parse(string connectionString)
        {
            return CloudStorageAccount.Parse(connectionString);
        }
    }
}
