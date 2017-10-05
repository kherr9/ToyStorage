﻿using System;
using System.Threading.Tasks;

namespace ToyStorage
{
    public sealed class BlobStorageMiddleware : IMiddleware
    {
        public Task Invoke(RequestContext context, RequestDelegate next)
        {
            switch (context.RequestMethod)
            {
                case RequestMethods.Get:
                    return OnGetAsync(context);
                case RequestMethods.Put:
                    return OnPutAsync(context);
                case RequestMethods.Delete:
                    return OnDeleteAsync(context);
                default:
                    throw new InvalidOperationException($"Unknown {nameof(context.RequestMethod)} '{context.RequestMethod}'");
            }
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