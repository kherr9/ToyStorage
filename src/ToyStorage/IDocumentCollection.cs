using System.Threading;
using System.Threading.Tasks;

namespace ToyStorage
{
    public interface IDocumentCollection
    {
        Task<TEntity> GetAsync<TEntity>(string id);

        Task<TEntity> GetAsync<TEntity>(string id, CancellationToken cancellationToken);

        Task PutAsync(object entity, string id);

        Task PutAsync(object entity, string id, CancellationToken cancellationToken);

        Task DeleteAsync(string id);

        Task DeleteAsync(string id, CancellationToken cancellationToken);
    }
}
