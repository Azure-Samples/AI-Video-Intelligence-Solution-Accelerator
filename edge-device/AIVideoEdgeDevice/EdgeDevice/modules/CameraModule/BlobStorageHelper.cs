using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Threading.Tasks;

namespace BlobStorage
{
    public class BlobStorageHelper
    {
        const int ticksPerMillisecond = 10000;
        readonly string connectionString;
        bool isInitialized = false;
        CloudStorageAccount storageAccount;
        CloudBlobClient cloudBlobClient;
        CloudBlobContainer cloudBlobContainer;

        public BlobStorageHelper(string containerName, string connectionString)
        {
            ContainerName = containerName;
            this.connectionString = connectionString;
        }

        public string ContainerName { get; private set; }

        /// <summary>
        /// Returns a UTC time with its sub-millisecond part truncated. This gives
        /// better behaved file names for BLOB storage. Uses UtcNow if no time is
        /// supplied.
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public static DateTime GetImageUtcTime(DateTime? time = null)
        {
            if (time == null)
            {
                time = DateTime.UtcNow;
            }
            return new DateTime((time.Value.Ticks / ticksPerMillisecond) * ticksPerMillisecond, DateTimeKind.Utc);
        }

        /// <summary>
        /// Returns a JSON time truncated to milliseconds: 2009-06-15T13:45:30.000Z
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public static string FormatImageUtcTime(DateTime time)
        {
            return time.ToString("O").Substring(0, 23) + "Z";
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cameraId">Semantic camera Id; looks like bld666room2117/grid01x04look27</param>
        /// <param name="formattedImageUtcTime">From the BlobStorageHelper.FormatImageUtcTime call, looks like 2009-06-15T13:45:30.000Z</param>
        /// <param name="fileExtension">The file extension with no '.', e.g. "jpg" or "png"</param>
        /// <param name="data"></param>
        /// <returns></returns>
        public Task UploadBlobAsync(string cameraId, string formattedImageUtcTime, string fileExtension, byte[] data)
        {
            Initialize();
            string name = cameraId + "/" + formattedImageUtcTime + "." + fileExtension;
            CloudBlockBlob blob = this.cloudBlobContainer.GetBlockBlobReference(name);
            Task task = blob.UploadFromByteArrayAsync(data, 0, data.Length);
            return task;
        }

        private void Initialize()
        {
            if (!this.isInitialized)
            {
                this.storageAccount = CloudStorageAccount.Parse(this.connectionString);
                this.cloudBlobClient = storageAccount.CreateCloudBlobClient();
                this.cloudBlobContainer = cloudBlobClient.GetContainerReference(ContainerName);
                this.isInitialized = true;
            }
        }
    }
}
