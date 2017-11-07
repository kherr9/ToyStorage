using System.Net;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Xunit;

namespace ToyStorage.IntegrationTests
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
        public async Task PutThenDelete()
        {
            // Arrange
            var entity = Entity.GenerateEntity();

            // Act
            await _documentCollection.PutAsync(entity, entity.Id);
            await _documentCollection.DeleteAsync(entity.Id);

            // Assert
            var exception = await Assert.ThrowsAsync<StorageException>(() => _documentCollection.GetAsync<Entity>(entity.Id));
            Assert.Equal((int)HttpStatusCode.NotFound, exception.RequestInformation.HttpStatusCode);
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

        [Fact]
        public async Task GetReadsUncompressedBlob()
        {
            // Arrange
            var entity = Entity.GenerateEntity();

            await PutWithoutCompressionAsync(entity, entity.Id);

            // Arrange
            var getEntity = await _documentCollection.GetAsync<Entity>(entity.Id);

            // Assert
            Assert.Equal(entity, getEntity); // should be able to read entity, even though blob isn't gzip-d
        }

        [Fact]
        public async Task ExceptionsNotCaughtInMiddleware()
        {
            // Act
            var exception = await Assert.ThrowsAsync<StorageException>(() => _documentCollection.GetAsync<Entity>("unknown_id"));

            // Arrange
            Assert.Equal((int)HttpStatusCode.NotFound, exception.RequestInformation.HttpStatusCode);
        }

        [Fact]
        public async Task GZipIsSmallerThanNonCompressed()
        {
            // Arrange
            var largeEntity = LargeEntity.GenerateLargeEntity();

            // Act
            await _documentCollection.PutAsync(largeEntity, "gzip");
            await PutWithoutCompressionAsync(largeEntity, "uncompressed");

            // Assert
            var gzipBlob = _cloudStorageFixture.CloudBlobContainer.GetBlockBlobReference("gzip");
            var uncompressedBlob = _cloudStorageFixture.CloudBlobContainer.GetBlockBlobReference("uncompressed");

            await gzipBlob.FetchAttributesAsync();
            await uncompressedBlob.FetchAttributesAsync();

            Assert.True(gzipBlob.Properties.Length < uncompressedBlob.Properties.Length);
        }

        private Task PutWithoutCompressionAsync(object entity, string id)
        {
            return CreateDocumentCollectionWithoutGZip().PutAsync(entity, id);
        }

        private DocumentCollection CreateDocumentCollection()
        {
            var middleware = new Middleware();
            middleware.UseJsonFormatter();
            middleware.Use<GZipMiddleware>();
            middleware.Use<BlobStorageMiddleware>();

            return new DocumentCollection(_cloudStorageFixture.CloudBlobContainer, middleware);
        }

        private DocumentCollection CreateDocumentCollectionWithoutGZip()
        {
            var middleware = new Middleware();
            middleware.UseJsonFormatter();
            middleware.Use<BlobStorageMiddleware>();

            return new DocumentCollection(_cloudStorageFixture.CloudBlobContainer, middleware);
        }
    }
}
