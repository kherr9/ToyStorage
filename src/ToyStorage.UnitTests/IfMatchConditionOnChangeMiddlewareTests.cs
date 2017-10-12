using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Xunit;

namespace ToyStorage.UnitTests
{
    public class IfMatchConditionOnChangeMiddlewareTests
    {
        private readonly DocumentCollection _documentCollection;

        public IfMatchConditionOnChangeMiddlewareTests()
        {
            var client = CloudStorageAccountHelper.CreateCloudBlobClient();
            var container = client.GetContainerReference(GetType().Name.ToLowerInvariant());
            container.CreateIfNotExistsAsync().Wait();

            var middleware = new Middleware();
            middleware.Use<IfMatchConditionOnChangeMiddleware>();
            middleware.UseJsonFormatter();
            middleware.Use<BlobStorageMiddleware>();

            _documentCollection = new DocumentCollection(container, middleware);
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
