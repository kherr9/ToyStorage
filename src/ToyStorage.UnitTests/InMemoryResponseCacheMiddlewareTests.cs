using System.Threading.Tasks;
using Xunit;

namespace ToyStorage.UnitTests
{
    public class InMemoryResponseCacheMiddlewareTests : IClassFixture<CloudStorageFixture>
    {
        private readonly CloudStorageFixture _cloudStorageFixture;

        public InMemoryResponseCacheMiddlewareTests(CloudStorageFixture cloudStorageFixture)
        {
            _cloudStorageFixture = cloudStorageFixture;
        }

        [Fact]
        public async Task TestGetGet()
        {
            // Arrange
            var entity = Entity.GenerateEntity();
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
            var entity = Entity.GenerateEntity();
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
            var entity = Entity.GenerateEntity();
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
            var entity = Entity.GenerateEntity();

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
            var entity = Entity.GenerateEntity();

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
            var middleware = new Middleware();
            middleware.UseJsonFormatter();
            middleware.Use<InMemoryResponseCacheMiddleware>();
            middleware.Use<BlobStorageMiddleware>();

            return new DocumentCollection(_cloudStorageFixture.CloudBlobContainer, middleware);
        }
    }
}
