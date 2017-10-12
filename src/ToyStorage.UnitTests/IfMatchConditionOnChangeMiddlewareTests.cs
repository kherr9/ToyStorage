using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Xunit;

namespace ToyStorage.UnitTests
{
    public class IfMatchConditionOnChangeMiddlewareTests
    {
        private readonly DocumentCollection _documentCollection;

        public IfMatchConditionOnChangeMiddlewareTests()
        {
            _documentCollection = CreateDocumentCollection();
        }

        [Fact]
        public async Task TestPutGetDelete()
        {
            // Arrange
            var entity = GenerateEntity();

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
            var entity = GenerateEntity();

            await _documentCollection.PutAsync(entity, entity.Id);

            await ModifyEntityInDifferentDocumentCollectionAsync(entity.Id);

            // Act
            var exception = await Assert.ThrowsAsync<StorageException>(async () => await _documentCollection.PutAsync(entity, entity.Id));

            // Assert
            Assert.NotNull(exception);
            Assert.Equal((int)HttpStatusCode.PreconditionFailed, exception.RequestInformation.HttpStatusCode);
        }

        [Fact]
        public async Task TestDeleteWhenResourceHasChangedBetweenGetAndDelete()
        {
            // Assert
            var entity = GenerateEntity();

            await _documentCollection.PutAsync(entity, entity.Id);

            await ModifyEntityInDifferentDocumentCollectionAsync(entity.Id);

            // Act
            var exception = await Assert.ThrowsAsync<StorageException>(async () => await _documentCollection.DeleteAsync(entity.Id));

            // Assert
            Assert.NotNull(exception);
            Assert.Equal((int)HttpStatusCode.PreconditionFailed, exception.RequestInformation.HttpStatusCode);
        }
        
        [Fact]
        public async Task TestGetWhenResourceHasChangedBetweenGetAndGet()
        {
            // Assert
            var entity = GenerateEntity();

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
            var client = CloudStorageAccountHelper.CreateCloudBlobClient();
            var container = client.GetContainerReference(GetType().Name.ToLowerInvariant());
            container.CreateIfNotExistsAsync().Wait();

            var middleware = new Middleware();
            middleware.Use<IfMatchConditionOnChangeMiddleware>();
            middleware.UseJsonFormatter();
            middleware.Use<BlobStorageMiddleware>();

            return new DocumentCollection(container, middleware);
        }

        private Entity GenerateEntity()
        {
            return new Entity()
            {
                Id = Guid.NewGuid().ToString(),
                Name = Guid.NewGuid().ToString()
            };
        }

        private async Task ModifyEntityInDifferentDocumentCollectionAsync(string entityId)
        {
            var documentCollection = CreateDocumentCollection();
            var entity = await documentCollection.GetAsync<Entity>(entityId);
            entity.Name = "foo";
            await documentCollection.PutAsync(entity, entity.Id);
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
