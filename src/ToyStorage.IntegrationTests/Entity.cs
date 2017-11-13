using System;
using System.Diagnostics.CodeAnalysis;

namespace ToyStorage.IntegrationTests
{
    [SuppressMessage("ReSharper", "NonReadonlyMemberInGetHashCode")]
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Local")]
    public class Entity
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

        public static Entity GenerateEntity()
        {
            return new Entity()
            {
                Id = GenerateId(),
                Name = Guid.NewGuid().ToString()
            };
        }

        public static string GenerateId()
        {
            return Guid.NewGuid().ToString();
        }
    }
}