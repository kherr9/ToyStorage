using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Xunit;

namespace ToyStorage.UnitTests
{
    public class DocumentCollectionTests
    {
        private readonly DocumentCollection _documentCollection;

        public DocumentCollectionTests()
        {
            var client = CloudStorageAccountHelper.CreateCloudBlobClient();
            var container = client.GetContainerReference(nameof(DocumentCollectionTests).ToLowerInvariant());
            container.CreateIfNotExistsAsync().Wait();
            _documentCollection = new DocumentCollection(container, new JsonSerializer());
        }

        [Fact]
        public async Task TestGetAsync()
        {
            // Arrange
            var entity = GenerateEntity();
            await _documentCollection.StoreAsync(entity, entity.Id);

            // Assert
            var entityClone = await _documentCollection.GetAsync<Entity>(entity.Id);

            Assert.Equal(entity, entityClone);
            Assert.NotSame(entity, entityClone);
        }

        private Entity GenerateEntity()
        {
            return new Entity()
            {
                Id = Guid.NewGuid().ToString(),
                Name = Guid.NewGuid().ToString()
            };
        }

        class Entity
        {
            public string Id { get; set; }

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
