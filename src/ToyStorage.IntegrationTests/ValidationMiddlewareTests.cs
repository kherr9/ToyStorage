using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace ToyStorage.IntegrationTests
{
    public class ValidationMiddlewareTests : IClassFixture<CloudStorageFixture>
    {
        private readonly CloudStorageFixture _cloudStorageFixture;
        private readonly DocumentCollection _documentCollection;

        public ValidationMiddlewareTests(CloudStorageFixture cloudStorageFixture)
        {
            _cloudStorageFixture = cloudStorageFixture;
            _documentCollection = CreateDocumentCollection();
        }

        private DocumentCollection CreateDocumentCollection()
        {
            var middleware = new Middleware();
            middleware.UseJsonFormatter();
            middleware.Use<ValidationMiddleware>();
            middleware.Use<BlobStorageMiddleware>();

            return new DocumentCollection(_cloudStorageFixture.CloudBlobContainer, middleware);
        }

        [Fact]
        public async Task PutSimpleEntityWithNoValidationErrors()
        {
            // Arrange
            var entity = SimpleEntity.GenerateValidEntity();

            // Act
            await _documentCollection.PutAsync(entity, entity.Id);

            // Assert (no error)
        }

        [Fact]
        public async Task PutSimpleEntityWithValidationErrors()
        {
            // Arrange
            var entity = SimpleEntity.GenerateInvalidEntity();

            // Act
            var exception = await Assert.ThrowsAsync<ValidationException>(() => _documentCollection.PutAsync(entity, entity.Id));

            // Assert (no error)
            Assert.Single(exception.ValidationResult.MemberNames);
            Assert.Equal("Name", exception.ValidationResult.MemberNames.Single());
        }

        [SuppressMessage("ReSharper", "NonReadonlyMemberInGetHashCode")]
        [SuppressMessage("ReSharper", "MemberCanBePrivate.Local")]
        private class SimpleEntity
        {
            [Required]
            public string Id { get; set; }

            [Required]
            public string Name { get; set; }

            public static SimpleEntity GenerateValidEntity()
            {
                return new SimpleEntity()
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = "foo"
                };
            }

            public static SimpleEntity GenerateInvalidEntity()
            {
                return new SimpleEntity()
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = null
                };
            }
        }
    }
}
