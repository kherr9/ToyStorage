using System.Threading.Tasks;
using Newtonsoft.Json.Serialization;
using Xunit;

namespace ToyStorage.IntegrationTests
{
    public class DocumentCollectionTests : IClassFixture<CloudStorageFixture>
    {
        private readonly DocumentCollection _documentCollection;

        public DocumentCollectionTests(CloudStorageFixture cloudStorageFixture)
        {
            var pipeline = new MiddlewarePipelineBuilder()
            .UseJsonFormatter(opt =>
            {
                opt.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            })
            .Use<GZipMiddleware>()
            .Use<BlobStorageMiddleware>()
            .Build();

            _documentCollection = new DocumentCollection(cloudStorageFixture.CloudBlobContainer, pipeline);
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
