﻿using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Xunit;

namespace ToyStorage.UnitTests
{
    public class DocumentCollectionTests
    {
        private readonly DocumentCollection _documentCollection;

        public DocumentCollectionTests()
        {
            var client = CloudStorageAccountHelper.CreateCloudBlobClient();
            var container = client.GetContainerReference(this.GetType().Name.ToLowerInvariant());
            container.CreateIfNotExistsAsync().Wait();

            var middleware = new Middleware();
            middleware.Use<ValidationMiddleware>();
            middleware.UseJson();
            middleware.Use<GZipMiddleware>();
            middleware.Use<BlobStorageMiddleware>();

            _documentCollection = new DocumentCollection(container, middleware);
        }

        [Fact]
        public async Task TestStoreGetDelete()
        {
            // Arrange
            var entity = GenerateEntity();

            // Act
            await _documentCollection.StoreAsync(entity, entity.Id);
            var entityClone = await _documentCollection.GetAsync<Entity>(entity.Id);
            await _documentCollection.StoreAsync(entity, entity.Id);
            await _documentCollection.DeleteAsync(entity.Id);

            // Assert
            Assert.Equal(entity, entityClone);
            Assert.NotSame(entity, entityClone);
        }

        [Fact]
        public async Task TestStoreWithValidationError()
        {
            // Arrange
            var entity = GenerateEntity();
            entity.Name = null;

            // Act
            var exception = await Assert.ThrowsAsync<ValidationException>(async () => await _documentCollection.StoreAsync(entity, entity.Id));

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
