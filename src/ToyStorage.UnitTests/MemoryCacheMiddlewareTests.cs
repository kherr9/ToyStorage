using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Xunit;

namespace ToyStorage.UnitTests
{
    public class MemoryCacheMiddlewareTests
    {
        [Fact]
        public async Task TestGetGet()
        {
            // Arrange
            var entity = GenerateEntity();
            await PutEntityAsync(entity);

            var documentCollection = CreateDocumentCollection();

            // Act
            var notCachedEntity = await documentCollection.GetAsync<Entity>(entity.Id);
            var cachedEntity = await documentCollection.GetAsync<Entity>(entity.Id);

            // Assert
            Assert.Equal(entity, notCachedEntity);
            Assert.Equal(entity, cachedEntity);
            Assert.NotSame(notCachedEntity, cachedEntity);
        }

        [Fact]
        public async Task TestGetPutGet()
        {
            // Arrange
            var entity = GenerateEntity();
            await PutEntityAsync(entity);

            var documentCollection = CreateDocumentCollection();

            // Act
            var entity1 = await documentCollection.GetAsync<Entity>(entity.Id);
            entity1.Name = "foo";
            await documentCollection.PutAsync(entity1, entity1.Id);
            var entity2 = await documentCollection.GetAsync<Entity>(entity.Id);

            // Assert
            Assert.NotEqual(entity, entity1);
            Assert.Equal(entity1, entity2);
        }

        [Fact]
        public async Task TestGetDelete()
        {
            // Arrange
            var entity = GenerateEntity();
            await PutEntityAsync(entity);

            var documentCollection = CreateDocumentCollection();

            // Act
            var entity1 = await documentCollection.GetAsync<Entity>(entity.Id);
            await documentCollection.DeleteAsync(entity1.Id);

            // Assert
            // no error
        }

        [Fact]
        public async Task TestPutGet()
        {
            // Arrange
            var entity = GenerateEntity();

            var documentCollection = CreateDocumentCollection();

            // Act
            await documentCollection.PutAsync(entity, entity.Id);
            var cachedEntity = await documentCollection.GetAsync<Entity>(entity.Id);

            // Assert
            Assert.Equal(entity, cachedEntity);
            Assert.NotSame(entity, cachedEntity);
        }

        [Fact]
        public async Task TestPutDelete()
        {
            // Arrange
            var entity = GenerateEntity();

            var documentCollection = CreateDocumentCollection();

            // Act
            await documentCollection.PutAsync(entity, entity.Id);
            await documentCollection.DeleteAsync(entity.Id);

            // Assert
            // no error
        }

        private Task PutEntityAsync(Entity entity)
        {
            var documentCollection = CreateDocumentCollection();

            return documentCollection.PutAsync(entity, entity.Id);
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
