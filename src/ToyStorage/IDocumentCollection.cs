using System.Threading.Tasks;

namespace ToyStorage
{
    public interface IDocumentCollection
    {
        Task<TEntity> GetAsync<TEntity>(string id);

        Task PutAsync(object entity, string id);

        Task DeleteAsync(string id);
    }
}
