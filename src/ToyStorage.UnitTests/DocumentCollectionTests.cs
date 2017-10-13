using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
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
            var entity = GenerateEntity();

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
            var entity = GenerateEntity();
            entity.Name = null;

            // Act
            var exception = await Assert.ThrowsAsync<ValidationException>(async () => await _documentCollection.PutAsync(entity, entity.Id));

            // Assert
            Assert.NotNull(exception);
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
