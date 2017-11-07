﻿using System.Threading.Tasks;
using Newtonsoft.Json.Serialization;
using Xunit;

namespace ToyStorage.IntegrationTests
{
    public class DocumentCollectionTests : IClassFixture<CloudStorageFixture>
    {
        private readonly DocumentCollection _documentCollection;

        public DocumentCollectionTests(CloudStorageFixture cloudStorageFixture)
        {
            var middleware = new Middleware();
            middleware.UseJsonFormatter(opt =>
            {
                opt.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            });
            middleware.Use<GZipMiddleware>();
            middleware.Use<BlobStorageMiddleware>();

            _documentCollection = new DocumentCollection(cloudStorageFixture.CloudBlobContainer, middleware);
        }

        [Fact]
        public async Task TestPutGetDelete()
        {
            // Arrange
            var entity = Entity.GenerateEntity();

            // Act
            await _documentCollection.PutAsync(entity, entity.Id);
            var entityClone = await _documentCollection.GetAsync<Entity>(entity.Id);
            await _documentCollection.PutAsync(entity, entity.Id);
            await _documentCollection.DeleteAsync(entity.Id);

            // Assert
            Assert.Equal(entity, entityClone);
            Assert.NotSame(entity, entityClone);
        }
    }
}
