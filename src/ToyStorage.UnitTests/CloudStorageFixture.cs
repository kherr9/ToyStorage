using System;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace ToyStorage.UnitTests
{
    public class CloudStorageFixture : IDisposable
    {
        public CloudStorageFixture()
        {
            var cloudStorageAccount = CloudStorageAccount.Parse("UseDevelopmentStorage=true;");

            var cloudBlobClient = cloudStorageAccount.CreateCloudBlobClient();

            var cloudBlobContainer = cloudBlobClient.GetContainerReference(Guid.NewGuid().ToString().ToLowerInvariant());

            cloudBlobContainer.CreateAsync().Wait();

            CloudBlobContainer = cloudBlobContainer;
        }

        public CloudBlobContainer CloudBlobContainer { get; }

        public void Dispose()
        {
            CloudBlobContainer.DeleteIfExistsAsync().Wait();
        }
    }
}
