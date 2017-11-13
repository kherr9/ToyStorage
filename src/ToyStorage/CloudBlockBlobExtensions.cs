using System.Threading;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Blob;

namespace ToyStorage
{
    public static class CloudBlockBlobExtensions
    {
#if NETSTANDARD1_3
        public static Task FetchAttributesAsync(this CloudBlockBlob cloudBlockBlob, CancellationToken cancellationToken)
        {
            return cloudBlockBlob.FetchAttributesAsync(null, null, null, cancellationToken);
        }
#endif
    }
}