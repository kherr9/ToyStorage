namespace ToyStorage
{
    public interface ICache
    {
        bool TryGetValue<TItem>(string key, out TItem value);

        void Set<TItem>(string key, TItem value);
    }
}