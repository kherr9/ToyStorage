using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Xunit;

namespace ToyStorage.UnitTests
{
    public class MiddlewareDocumentCollectionTests
    {
        private readonly MiddlewareDocumentCollection _documentCollection;

        public MiddlewareDocumentCollectionTests()
        {
            var client = CloudStorageAccountHelper.CreateCloudBlobClient();
            var container = client.GetContainerReference(nameof(DocumentCollectionTests).ToLowerInvariant());
            container.CreateIfNotExistsAsync().Wait();

            var middleware = new Middleware();
            middleware.Use<JsonFormaterMiddleware>();
            middleware.Use<GZipMiddleware>();
            middleware.Use<BlobStorageMiddleware>();

            _documentCollection = new MiddlewareDocumentCollection(container, middleware);
        }

        [Fact]
        public async Task TestGetAsync()
        {
            // Arrange
            var entity = GenerateEntity();

            await _documentCollection.StoreAsync(entity, entity.Id);
            var entityClone = await _documentCollection.GetAsync<Entity>(entity.Id);
            await _documentCollection.DeleteAsync(entity.Id);

            Assert.Equal(entity, entityClone);
            Assert.NotSame(entity, entityClone);
        }

        private Entity GenerateEntity()
        {
            return new Entity()
            {
                Id = Guid.NewGuid().ToString(),
                Name = Guid.NewGuid().ToString()
            };
        }

        [SuppressMessage("ReSharper", "NonReadonlyMemberInGetHashCode")]
        [SuppressMessage("ReSharper", "MemberCanBePrivate.Local")]
        class Entity
        {
            public string Id { get; set; }

            public string Name { get; set; }

            public override bool Equals(object obj)
            {
                if (!(obj is Entity other))
                {
                    return false;
                }

                return Equals(other);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return ((Id != null ? Id.GetHashCode() : 0) * 397) ^ (Name != null ? Name.GetHashCode() : 0);
                }
            }

            private bool Equals(Entity other)
            {
                return string.Equals(Id, other.Id) && string.Equals(Name, other.Name);
            }
        }
    }
}
