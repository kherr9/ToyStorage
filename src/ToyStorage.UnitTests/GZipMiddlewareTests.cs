using System.Threading.Tasks;
using Xunit;

namespace ToyStorage.UnitTests
{
    public class GZipMiddlewareTests : IClassFixture<CloudStorageFixture>
    {
        private readonly CloudStorageFixture _cloudStorageFixture;
        private readonly DocumentCollection _documentCollection;

        public GZipMiddlewareTests(CloudStorageFixture cloudStorageFixture)
        {
            _cloudStorageFixture = cloudStorageFixture;
            _documentCollection = CreateDocumentCollection();
        }

        [Fact]
        public async Task PutSetsContentEncodingType()
        {
            // Arrange
            var entity = Entity.GenerateEntity();

            // Act
            await _documentCollection.PutAsync(entity, entity.Id);

            // Assert
            var blob = _cloudStorageFixture.CloudBlobContainer.GetBlockBlobReference(entity.Id);
            await blob.FetchAttributesAsync();

            Assert.Equal("gzip", blob.Properties.ContentEncoding);
        }

        [Fact]
        public async Task PutThenGet()
        {
            // Arrange
            var entity = Entity.GenerateEntity();

            // Act
            await _documentCollection.PutAsync(entity, entity.Id);
            var getEntity = await _documentCollection.GetAsync<Entity>(entity.Id);

            // Assert
            Assert.Equal(entity, getEntity);
        }

        [Fact]
        public async Task PutThenGetLargeEntity()
        {
            // Arrange
            var entity = LargeEntity.GenerateLargeEntity();

            // Act
            await _documentCollection.PutAsync(entity, entity.Id);
            var getEntity = await _documentCollection.GetAsync<LargeEntity>(entity.Id);

            // Assert
            Assert.Equal(entity, getEntity);
        }

        private DocumentCollection CreateDocumentCollection()
        {
            var middleware = new Middleware();
            middleware.UseJsonFormatter();
            middleware.Use<GZipMiddleware>();
            middleware.Use<BlobStorageMiddleware>();

            return new DocumentCollection(_cloudStorageFixture.CloudBlobContainer, middleware);
        }
    }
}
