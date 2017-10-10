using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ToyStorage
{
    /// <summary>
    /// Middleware component that formats object to binary to be used downstream for persistence.
    /// </summary>
    public class JsonFormaterMiddleware : IMiddleware
    {
        public async Task Invoke(RequestContext context, RequestDelegate next)
        {
            if (context.IsWrite())
            {
                context.Content = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(context.Entity));
                context.CloudBlockBlob.Properties.ContentType = "application/json";
            }

            await next(context);

            if (context.IsRead())
            {
                context.Entity = JsonConvert.DeserializeObject(Encoding.UTF8.GetString(context.Content), context.EntityType);
            }
        }
    }
}