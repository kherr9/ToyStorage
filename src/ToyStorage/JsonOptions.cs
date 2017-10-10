using System;
using Newtonsoft.Json;

namespace ToyStorage
{
    /// <summary>
    /// Configuration options for <see cref="JsonFormaterMiddleware"/> in <see cref="JsonFormaterMiddlewareExtensions"/>
    /// </summary>
    public class JsonOptions
    {
        public JsonOptions() : this(new JsonSerializerSettings())
        {
        }

        public JsonOptions(JsonSerializerSettings serializerSettings)
        {
            SerializerSettings = serializerSettings ?? throw new ArgumentNullException(nameof(serializerSettings));
        }

        public JsonSerializerSettings SerializerSettings { get; }
    }
}