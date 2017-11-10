using System;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace ToyStorage.IntegrationTests
{
    public class CloudStorageFixture : IDisposable
    {
        public CloudStorageFixture()
        {
            var cloudStorageAccount = CloudStorageAccount.Parse(GetConnectionString());

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

        private string GetConnectionString()
        {
            var connectionString = Environment.GetEnvironmentVariable("ToyStorage.IntegrationTests.AzureStorageAccount");

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                connectionString = "UseDevelopmentStorage=true;";
            }

            return connectionString;
        }
    }
}
