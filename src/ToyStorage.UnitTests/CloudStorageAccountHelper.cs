using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace ToyStorage.UnitTests
{
    public static class CloudStorageAccountHelper
    {
        public static CloudStorageAccount CreateCloudStorageAccount()
        {
            return CloudStorageAccount.Parse("UseDevelopmentStorage=true");
        }

        public static CloudBlobClient CreateCloudBlobClient()
        {
            return CreateCloudStorageAccount().CreateCloudBlobClient();
        }
    }
}