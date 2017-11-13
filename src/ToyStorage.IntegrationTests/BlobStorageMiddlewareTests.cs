using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Xunit;

namespace ToyStorage.IntegrationTests
{
    public class BlobStorageMiddlewareTests : IClassFixture<CloudStorageFixture>
    {
        private readonly CloudStorageFixture _cloudStorageFixture;
        private readonly DocumentCollection _documentCollection;

        public BlobStorageMiddlewareTests(CloudStorageFixture cloudStorageFixture)
        {
            _cloudStorageFixture = cloudStorageFixture;
            var pipeline = new MiddlewarePipeline()
                .Use<JsonFormaterMiddleware>()
                .Use<BlobStorageMiddleware>();

            _documentCollection = new DocumentCollection(cloudStorageFixture.CloudBlobContainer, pipeline);
        }

        #region Get

        [Fact]
        public async Task GetReturnsData()
        {
            // Arrange
            var entity = await PutEntityAsync();

            // Act
            var readEntity = _documentCollection.GetAsync<Entity>(entity.Id);

            // Assert
            Assert.NotNull(readEntity);
        }

        [Fact]
        public async Task GetThrowsExceptionWhenCancellationTokenSet()
        {
            // Arrange
            var entity = await PutEntityAsync();
            var token = GenerateCancelledCancellationToken();

            // Act
            await Assert.ThrowsAsync<TaskCanceledException>(() => _documentCollection.GetAsync<Entity>(entity.Id, token));
        }

        #endregion

        #region Put

        [Fact]
        public async Task PutCreatesBlob()
        {
            // Arrange
            var entity = Entity.GenerateEntity();

            // Act
            await _documentCollection.PutAsync(entity, entity.Id);

            // Assert
            var blob = _cloudStorageFixture.CloudBlobContainer.GetBlockBlobReference(entity.Id);
            Assert.True(await blob.ExistsAsync());
        }

        [Fact]
        public async Task PutOverwritesBlob()
        {
            // Arrange
            var entity = await PutEntityAsync();

            // Act
            entity.Name = "My Message Is Foo";
            await _documentCollection.PutAsync(entity, entity.Id);

            // Assert
            var readEntity = await _documentCollection.GetAsync<Entity>(entity.Id);
            Assert.Equal("My Message Is Foo", readEntity.Name);
        }
        
        [Fact]
        public async Task PutThrowsExceptionAndDoesNotCreateBlobWhenCancellationTokenSet()
        {
            // Arrange
            var entity = Entity.GenerateEntity();
            var token = GenerateCancelledCancellationToken();

            // Act
            await Assert.ThrowsAsync<TaskCanceledException>(() => _documentCollection.PutAsync(entity, entity.Id, token));

            // Arrange
            var blob = _cloudStorageFixture.CloudBlobContainer.GetBlockBlobReference(entity.Id);
            // ReSharper disable once MethodSupportsCancellation
            Assert.False(await blob.ExistsAsync());
        }

        #endregion

        #region Delete

        [Fact]
        public async Task DeleteDoesDeleteBlob()
        {
            // Arrange
            var entity = await PutEntityAsync();

            // Act
            await _documentCollection.DeleteAsync(entity.Id);

            // Assert
            var blob = _cloudStorageFixture.CloudBlobContainer.GetBlockBlobReference(entity.Id);
            Assert.False(await blob.ExistsAsync());
        }

        [Fact]
        public async Task DeleteThrowsExceptionWhenBlobDoesNotExist()
        {
            // Arrange
            var entityId = Entity.GenerateId();

            // Act
            var exception =
                await Assert.ThrowsAsync<StorageException>(
                    () => _documentCollection.DeleteAsync(entityId));

            // Assert
            Assert.Equal((int)HttpStatusCode.NotFound, exception.RequestInformation.HttpStatusCode);
        }
        
        [Fact]
        public async Task DeleteThrowsExceptionAndDoesNotDeleteBlobWhenCancellationTokenSet()
        {
            // Arrange
            var entity = await PutEntityAsync();
            var token = GenerateCancelledCancellationToken();

            // Act
            await Assert.ThrowsAsync<TaskCanceledException>(() => _documentCollection.DeleteAsync(entity.Id, token));

            // Arrange
            var blob = _cloudStorageFixture.CloudBlobContainer.GetBlockBlobReference(entity.Id);
            // ReSharper disable once MethodSupportsCancellation
            Assert.True(await blob.ExistsAsync());
        }

        #endregion

        private async Task<Entity> PutEntityAsync()
        {
            var entity = Entity.GenerateEntity();
            await _documentCollection.PutAsync(entity, entity.Id);
            return entity;
        }

        private CancellationToken GenerateCancelledCancellationToken()
        {
            var cts = new CancellationTokenSource();
            cts.Cancel();
            return cts.Token;
        }
    }
}
