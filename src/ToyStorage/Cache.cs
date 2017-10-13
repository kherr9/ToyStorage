namespace ToyStorage
{
#if NET45
    using System.Runtime.Caching;

    internal class Cache : ICache
    {
        private readonly ObjectCache _objectCache;

        public Cache(ObjectCache objectCache)
        {
            _objectCache = objectCache;
        }

        public bool TryGetValue<TItem>(string key, out TItem value)
        {
            value = (TItem)_objectCache.Get(key);
            return value != null;
        }

        public void Set<TItem>(string key, TItem value)
        {
            _objectCache.Set(key, value, new CacheItemPolicy());
        }

        public static Cache CreateCache()
        {
            var memoryCache = MemoryCache.Default;
            return new Cache(memoryCache);
        }
    }
#endif

#if NETSTANDARD1_3
    using Microsoft.Extensions.Caching.Memory;

    internal class Cache : ICache
    {
        private readonly IMemoryCache _memoryCache;

        public Cache(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
        }

        public bool TryGetValue<TItem>(string key, out TItem value)
        {
            return _memoryCache.TryGetValue(key, out value);
        }

        public void Set<TItem>(string key, TItem value)
        {
            _memoryCache.Set(key, value);
        }

        public static Cache CreateCache()
        {
            var memoryCache = new MemoryCache(new MemoryCacheOptions());
            return new Cache(memoryCache);
        }
    }
#endif
}