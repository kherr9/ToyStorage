using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;

namespace ToyStorage
{
    public class MemoryCacheMiddleware : IMiddleware
    {
        private readonly ICache _cache;

        public MemoryCacheMiddleware(ICache cache)
        {
            _cache = cache;
        }

        public async Task Invoke(RequestContext context, RequestDelegate next)
        {
            CacheEntry cacheEntry;
            if (context.IsRead() && _cache.TryGetValue(context.CloudBlockBlob.Name, out cacheEntry))
            {
                // add If-None-Match for conditional GET
                context.AccessCondition = AccessCondition.GenerateIfNoneMatchCondition(cacheEntry.RequestContext.CloudBlockBlob.Properties.ETag);
            }

            // catch precondition failed?
            await next();

            if (context.IsRead())
            {
                WriteToCache(context);
            }
        }

        private void WriteToCache(RequestContext context)
        {
            var cacheEntry = new CacheEntry(context);

            _cache.Set(context.CloudBlockBlob.Name, cacheEntry);
        }

        private sealed class CacheEntry
        {
            public CacheEntry(RequestContext requestContext)
            {
                RequestContext = requestContext;
            }

            public RequestContext RequestContext { get; }
        }
    }
}
