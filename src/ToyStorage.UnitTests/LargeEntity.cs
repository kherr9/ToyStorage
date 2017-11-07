using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace ToyStorage.UnitTests
{
    [SuppressMessage("ReSharper", "NonReadonlyMemberInGetHashCode")]
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Local")]
    public class LargeEntity
    {
        public string Id { get; set; }

        public string[] Values { get; set; }

        public override bool Equals(object obj)
        {
            if (!(obj is LargeEntity other))
            {
                return false;
            }

            return Equals(other);
        }

        protected bool Equals(LargeEntity other)
        {
            return string.Equals(Id, other.Id) && Equals(Values, other.Values);
        }

        protected bool Equals(string[] values, string[] otherValues)
        {
            if (ReferenceEquals(values, otherValues))
            {
                return true;
            }

            // todo null checks
            if (values.Length == otherValues.Length)
            {
                return values.Zip(otherValues, Tuple.Create)
                    .All(x => string.Equals(x.Item1, x.Item2));
            }

            return false;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                // todo calc of Values
                return ((Id != null ? Id.GetHashCode() : 0) * 397) ^ (Values != null ? Values.GetHashCode() : 0);
            }
        }

        public static LargeEntity GenerateLargeEntity()
        {
            const int numberOfBytesInMegaByte = 1000000;
            const int numberOfBytesInGuidString = 36;
            const int numberOfMegaBytesDesired = 1;

            var count = (numberOfBytesInMegaByte / numberOfBytesInGuidString) * numberOfMegaBytesDesired;

            return new LargeEntity()
            {
                Id = Guid.NewGuid().ToString(),
                Values = Enumerable.Range(0, count)
                    .Select(x => Guid.NewGuid().ToString())
                    .ToArray()
            };
        }
    }
}