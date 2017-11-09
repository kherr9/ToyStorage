using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Xunit;

namespace ToyStorage.IntegrationTests
{
    public class IfMatchConditionOnChangeMiddlewareTests : IClassFixture<CloudStorageFixture>
    {
        private readonly CloudStorageFixture _cloudStorageFixture;
        private readonly DocumentCollection _documentCollection;

        public IfMatchConditionOnChangeMiddlewareTests(CloudStorageFixture cloudStorageFixture)
        {
            _cloudStorageFixture = cloudStorageFixture;
            _documentCollection = CreateDocumentCollection();
        }

        [Fact]
        public async Task TestPutGetDelete()
        {
            // Arrange
            var entity = Entity.GenerateEntity();

            // Act
            // create
            await _documentCollection.PutAsync(entity, entity.Id);

            // get
            entity = await _documentCollection.GetAsync<Entity>(entity.Id);

            // update
            entity.Name = Guid.NewGuid().ToString();
            await _documentCollection.PutAsync(entity, entity.Id);

            // delete
            await _documentCollection.DeleteAsync(entity.Id);
        }

        [Fact]
        public async Task TestPutWhenResourceHasChangedBetweenGetAndPut()
        {
            // Assert
            var entity = Entity.GenerateEntity();

            await _documentCollection.PutAsync(entity, entity.Id);

            await ModifyEntityInDifferentDocumentCollectionAsync(entity.Id);

            // Act
            var exception = await Assert.ThrowsAsync<StorageException>(async () => await _documentCollection.PutAsync(entity, entity.Id));

            // Assert
            Assert.Equal((int)HttpStatusCode.PreconditionFailed, exception.RequestInformation.HttpStatusCode);
        }

        [Fact]
        public async Task TestDeleteWhenResourceHasChangedBetweenGetAndDelete()
        {
            // Assert
            var entity = Entity.GenerateEntity();

            await _documentCollection.PutAsync(entity, entity.Id);

            await ModifyEntityInDifferentDocumentCollectionAsync(entity.Id);

            // Act
            var exception = await Assert.ThrowsAsync<StorageException>(async () => await _documentCollection.DeleteAsync(entity.Id));

            // Assert
            Assert.Equal((int)HttpStatusCode.PreconditionFailed, exception.RequestInformation.HttpStatusCode);
        }

        [Fact]
        public async Task TestGetWhenResourceHasChangedBetweenGetAndGet()
        {
            // Assert
            var entity = Entity.GenerateEntity();

            await _documentCollection.PutAsync(entity, entity.Id);

            await ModifyEntityInDifferentDocumentCollectionAsync(entity.Id);

            // Act
            // should not throw exception
            var otherEntity = await _documentCollection.GetAsync<Entity>(entity.Id);

            // Assert
            Assert.NotEqual(entity, otherEntity);
        }

        private DocumentCollection CreateDocumentCollection()
        {
            var pipeline = new MiddlewarePipeline()
                .Use<IfMatchConditionOnChangeMiddleware>()
                .UseJsonFormatter()
                .Use<BlobStorageMiddleware>();

            return new DocumentCollection(_cloudStorageFixture.CloudBlobContainer, pipeline);
        }

        private async Task ModifyEntityInDifferentDocumentCollectionAsync(string entityId)
        {
            var documentCollection = CreateDocumentCollection();
            var entity = await documentCollection.GetAsync<Entity>(entityId);
            entity.Name = "foo";
            await documentCollection.PutAsync(entity, entity.Id);
        }
    }
}
