using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Xunit;

namespace ToyStorage.UnitTests
{
    public class MemoryCacheMiddlewareTests
    {
        private readonly DocumentCollection _documentCollection;

        public MemoryCacheMiddlewareTests()
        {
            _documentCollection = CreateDocumentCollection();
        }

        [Fact]
        public async Task TestSomething()
        {
            // Arrange
            var entity = GenerateEntity();
            await _documentCollection.PutAsync(entity, entity.Id);
            var entity1 = await _documentCollection.GetAsync<Entity>(entity.Id);
            Assert.Equal(entity, entity1);

            // Act
            var entity2 = await _documentCollection.GetAsync<Entity>(entity.Id);
            Assert.Equal(entity, entity2);

            // Assert
        }

        private DocumentCollection CreateDocumentCollection()
        {
            var client = CloudStorageAccountHelper.CreateCloudBlobClient();
            var container = client.GetContainerReference(GetType().Name.ToLowerInvariant());
            container.CreateIfNotExistsAsync().Wait();

            var middleware = new Middleware();
            middleware.UseJsonFormatter();
            middleware.Use<MemoryCacheMiddleware>();
            middleware.Use<BlobStorageMiddleware>();

            return new DocumentCollection(container, middleware);
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
            [Required]
            public string Id { get; set; }

            [Required]
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
