using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;

namespace ToyStorage
{
    public class DocumentCollection : IDocumentCollection
    {
        private readonly CloudBlobContainer _container;
        private readonly JsonSerializer _serializer;

        public DocumentCollection(CloudBlobContainer container, JsonSerializer serializer)
        {
            _container = container ?? throw new ArgumentNullException(nameof(container));
            _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
        }

        public async Task<TEntity> GetAsync<TEntity>(string id)
        {
            if (id == null) throw new ArgumentNullException(nameof(id));

            var blob = GetBlob(id);

            using (var reader = new JsonTextReader(new StreamReader(await blob.OpenReadAsync())))
            {
                return _serializer.Deserialize<TEntity>(reader);
            }
        }

        public async Task StoreAsync(object entity, string id)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            if (id == null) throw new ArgumentNullException(nameof(id));

            var blob = GetBlob(id);

            using (var writer = new JsonTextWriter(new StreamWriter(await blob.OpenWriteAsync())))
            {
                _serializer.Serialize(writer, entity);
            }
        }

        public Task DeleteAsync(string id)
        {
            if (id == null) throw new ArgumentNullException(nameof(id));

            var blob = GetBlob(id);

            return blob.DeleteAsync();
        }

        private CloudBlockBlob GetBlob(string id)
        {
            return _container.GetBlockBlobReference(id);
        }
    }
}