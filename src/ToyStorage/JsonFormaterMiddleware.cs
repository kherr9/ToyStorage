using System;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ToyStorage
{
    /// <summary>
    /// Middleware component that formats object to binary (writes) and binary to object (reads).
    /// </summary>
    public class JsonFormaterMiddleware : IMiddleware
    {
        private readonly JsonSerializerSettings _serializerSettings;
        private const string JsonContentType = "application/json";

        public JsonFormaterMiddleware()
            : this(new JsonSerializerSettings())
        {

        }

        public JsonFormaterMiddleware(JsonSerializerSettings serializerSettings)
        {
            _serializerSettings = serializerSettings ?? throw new ArgumentNullException(nameof(serializerSettings));
        }

        public async Task Invoke(RequestContext context, RequestDelegate next)
        {
            if (context.IsWrite())
            {
                context.Content = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(context.Entity, _serializerSettings));
                context.CloudBlockBlob.Properties.ContentType = JsonContentType;
            }

            await next(context);

            if (context.IsRead() && context.CloudBlockBlob.Properties.ContentType == JsonContentType)
            {
                context.Entity = JsonConvert.DeserializeObject(GetString(context.Content), context.EntityType, _serializerSettings);
            }
        }

#if NETSTANDARD1_1
        private static string GetString(byte[] bytes)
        {
            return Encoding.UTF8.GetString(bytes, 0, bytes.Length);
        }
#else
        private static string GetString(byte[] bytes)
        {
            return Encoding.UTF8.GetString(bytes);
        }
#endif
    }
}