using System;
using System.Net;
#if NET45
using System.Runtime.Caching;
#endif
using System.Threading.Tasks;
#if NETSTANDARD1_3
using Microsoft.Extensions.Caching.Memory;
#endif
using Microsoft.WindowsAzure.Storage;

namespace ToyStorage
{
    /// <summary>
    /// Caches the response body and uses conditional gets the validate cache is valid.
    /// </summary>
    /// <remarks>
    /// Must be placed after formatter, so that the formatter can deserialize the cached response body.
    /// </remarks>
    public class InMemoryResponseCacheMiddleware : IMiddleware
    {
        private readonly ICache _cache;

        public InMemoryResponseCacheMiddleware()
            : this(Cache.CreateCache())
        {
        }

#if NET45
        public InMemoryResponseCacheMiddleware(ObjectCache objectCache)
            : this(new Cache(objectCache))
        {
        }
#endif

#if NETSTANDARD1_3
        public InMemoryResponseCacheMiddleware(IMemoryCache memoryCache)
            : this(new Cache(memoryCache))
        {
        }
#endif

        private InMemoryResponseCacheMiddleware(ICache cache)
        {
            _cache = cache;
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
                ETag = etag ?? throw new ArgumentNullException(nameof(etag));
                Content = content ?? throw new ArgumentNullException(nameof(content));
            }

            public string ETag { get; }

            public byte[] Content { get; }
        }
    }
}
