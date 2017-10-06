using System;
using System.Threading.Tasks;

namespace ToyStorage
{
    public class BlobStorageMiddleware : IMiddleware
    {
        public async Task Invoke(RequestContext context, RequestDelegate next)
        {
            switch (context.RequestMethod)
            {
                case RequestMethods.Get:
                    await OnGetAsync(context);
                    break;
                case RequestMethods.Put:
                    await OnPutAsync(context);
                    break;
                case RequestMethods.Delete:
                    await OnDeleteAsync(context);
                    break;
                default:
                    throw new InvalidOperationException($"Unknown {nameof(context.RequestMethod)} '{context.RequestMethod}'");
            }

            await next(context);
        }

        private async Task OnGetAsync(RequestContext context)
        {
            await context.CloudBlockBlob.FetchAttributesAsync();

            var buffer = new byte[context.CloudBlockBlob.Properties.Length];

            await context.CloudBlockBlob.DownloadToByteArrayAsync(buffer, 0);

            context.Content = buffer;
        }

        private async Task OnPutAsync(RequestContext context)
        {
            await context.CloudBlockBlob.UploadFromByteArrayAsync(context.Content, 0, context.Content.Length);
        }

        private Task OnDeleteAsync(RequestContext context)
        {
            return context.CloudBlockBlob.DeleteAsync();
        }
    }
}