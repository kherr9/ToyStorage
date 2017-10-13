using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Newtonsoft.Json.Serialization;
using Xunit;

namespace ToyStorage.UnitTests
{
    public class DocumentCollectionTests : IClassFixture<CloudStorageFixture>
    {
        private readonly DocumentCollection _documentCollection;

        public DocumentCollectionTests(CloudStorageFixture cloudStorageFixture)
        {
            var middleware = new Middleware();
            middleware.Use<ValidationMiddleware>();
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

        [Fact]
        public async Task TestPutWithValidationError()
        {
            // Arrange
            var entity = Entity.GenerateEntity();
            entity.Name = null;

            // Act
            var exception = await Assert.ThrowsAsync<ValidationException>(async () => await _documentCollection.PutAsync(entity, entity.Id));

            // Assert
            Assert.NotNull(exception);
        }
    }
}
