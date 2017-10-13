namespace ToyStorage
{
    /// <summary>
    /// Cache contract for <see cref="InMemoryResponseCacheMiddleware"/>
    /// </summary>
    /// <remarks>
    /// Define own interface for Cache since we use multiple implementations of caching for different targeted runtime.
    /// </remarks>
    public interface ICache
    {
        bool TryGetValue<TItem>(string key, out TItem value);

        void Set<TItem>(string key, TItem value);

        void Remove(string key);
    }
}