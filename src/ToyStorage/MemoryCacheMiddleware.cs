using System;
using System.Threading.Tasks;

#if NET45
using System.Runtime.Caching;
#else
using Microsoft.Extensions.Caching.Memory;
#endif

namespace ToyStorage
{
    public class MemoryCacheMiddleware : IMiddleware
    {
#if NET45
        private readonly ObjectCache _cache;
#else
        private readonly IMemoryCache _cache;
#endif
        public Task Invoke(RequestContext context, RequestDelegate next)
        {
            throw new NotImplementedException();
        }
    }
}
