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
        private const string JsonContentType = "application/json";

        public async Task Invoke(RequestContext context, RequestDelegate next)
        {
            if (context.IsWrite())
            {
                context.Content = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(context.Entity));
                context.CloudBlockBlob.Properties.ContentType = JsonContentType;
            }

            await next(context);

            if (context.IsRead() && context.CloudBlockBlob.Properties.ContentType == JsonContentType)
            {
                context.Entity = JsonConvert.DeserializeObject(Encoding.UTF8.GetString(context.Content), context.EntityType);
            }
        }
    }
}