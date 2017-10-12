using System;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Blob;

namespace ToyStorage
{
    public class DocumentCollection : IDocumentCollection
    {
        private readonly CloudBlobContainer _container;
        private readonly Middleware _middleware;

        public DocumentCollection(CloudBlobContainer container, Middleware middleware)
        {
            _container = container ?? throw new ArgumentNullException(nameof(container));
            _middleware = middleware ?? throw new ArgumentNullException(nameof(middleware));
        }

        public async Task<TEntity> GetAsync<TEntity>(string id)
        {
            if (id == null) throw new ArgumentNullException(nameof(id));

            var context = new RequestContext()
            {
                RequestMethod = RequestMethods.Get,
                Entity = null,
                EntityType = typeof(TEntity),
                Content = null,
                CloudBlockBlob = GetBlob(id),
            };

            await _middleware.Run(context);

            return (TEntity)context.Entity;
        }

        public Task PutAsync(object entity, string id)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            if (id == null) throw new ArgumentNullException(nameof(id));

            var context = new RequestContext()
            {
                RequestMethod = RequestMethods.Put,
                Entity = entity,
                EntityType = entity.GetType(),
                Content = null,
                CloudBlockBlob = GetBlob(id)
            };

            return _middleware.Run(context);
        }

        public Task DeleteAsync(string id)
        {
            if (id == null) throw new ArgumentNullException(nameof(id));

            var context = new RequestContext()
            {
                RequestMethod = RequestMethods.Delete,
                Entity = null,
                EntityType = null,
                Content = null,
                CloudBlockBlob = GetBlob(id)
            };

            return _middleware.Run(context);
        }

        private CloudBlockBlob GetBlob(string id)
        {
            return _container.GetBlockBlobReference(id);
        }
    }
}