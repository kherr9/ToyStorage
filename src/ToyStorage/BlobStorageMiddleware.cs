using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Blob;

namespace ToyStorage
{
    public class BlobStorageMiddleware : IMiddlewareComponent
    {
        public async Task Invoke(RequestContext context, RequestDelegate next)
        {
            switch (context.RequestMethod)
            {
                case RequestMethods.Get:
                    await OnGetAsync(context).ConfigureAwait(false);
                    break;
                case RequestMethods.Put:
                    await OnPutAsync(context).ConfigureAwait(false);
                    break;
                case RequestMethods.Delete:
                    await OnDeleteAsync(context).ConfigureAwait(false);
                    break;
                default:
                    throw new InvalidOperationException($"Unknown {nameof(context.RequestMethod)} '{context.RequestMethod}'");
            }

            await next();
        }

        private async Task OnGetAsync(RequestContext context)
        {
            using (var memoryStream = new MemoryStream())
            {
                await context.CloudBlockBlob.DownloadToStreamAsync(memoryStream, context.AccessCondition, null, null).ConfigureAwait(false);

                context.Content = memoryStream.ToArray();
            }
        }

        private Task OnPutAsync(RequestContext context)
        {
            return context.CloudBlockBlob.UploadFromByteArrayAsync(context.Content, 0, context.Content.Length, context.AccessCondition, null, null);
        }

        private Task OnDeleteAsync(RequestContext context)
        {
            return context.CloudBlockBlob.DeleteAsync(DeleteSnapshotsOption.None, context.AccessCondition, null, null);
        }
    }
}