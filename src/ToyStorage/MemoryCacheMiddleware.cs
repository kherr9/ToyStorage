using System.Net;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;

namespace ToyStorage
{
    /// <summary>
    /// Caches the response body and uses conditional gets the validate cache is valid.
    /// </summary>
    /// <remarks>
    /// Must be placed after formatter, so that the formatter can deserialize the cached response body.
    /// </remarks>
    public class MemoryCacheMiddleware : IMiddleware
    {
        private readonly ICache _cache;

        public MemoryCacheMiddleware()
        {
            _cache = Cache.CreateCache();
        }

        public async Task Invoke(RequestContext context, RequestDelegate next)
        {
            // Request
            CacheEntry cacheEntry = null;
            if (context.IsRead() && _cache.TryGetValue(context.CloudBlockBlob.Name, out cacheEntry) && cacheEntry != null)
            {
                // add If-None-Match for conditional GET
                context.AccessCondition = AccessCondition.GenerateIfNoneMatchCondition(cacheEntry.ETag);
            }

            // Next
            bool notModified = false;
            try
            {
                await next();
            }
            catch (StorageException ex)
            {
                if (ex.RequestInformation.HttpStatusCode == (int)HttpStatusCode.NotModified)
                {
                    // resource not modified, so we can use cache copy
                    notModified = true;
                }
                else
                {
                    throw;
                }
            }

            // Response
            if (notModified)
            {
                // content has not been modified, so use cache
                // ReSharper disable once PossibleNullReferenceException
                context.Content = cacheEntry.Content;
            }
            else if (context.IsRead() || context.IsWrite())
            {
                // either resource was not in cache or resource has been modified
                WriteToCache(context);
            }
            else if (context.IsDelete())
            {
                // remove from cache if the resource has been deleted
                DeleteCacheIfExists(context);
            }
        }

        private void WriteToCache(RequestContext context)
        {
            var cacheEntry = new CacheEntry(context.CloudBlockBlob.Properties.ETag, context.Content);

            _cache.Set(context.CloudBlockBlob.Name, cacheEntry);
        }

        private void DeleteCacheIfExists(RequestContext context)
        {
            _cache.Remove(context.CloudBlockBlob.Name);
        }

        private sealed class CacheEntry
        {
            public CacheEntry(string etag, byte[] content)
            {
                ETag = etag;
                Content = content;
            }

            public string ETag { get; }

            public byte[] Content { get; }
        }
    }
}
