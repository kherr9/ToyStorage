using System.Collections.Concurrent;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;

namespace ToyStorage
{
    /// <summary>
    /// Optimistic concurreny implementation using etags for conditional put and delete
    /// </summary>
    public class IfMatchConditionOnChangeMiddleware : IMiddleware
    {
        private readonly ConcurrentDictionary<string, string> _etags = new ConcurrentDictionary<string, string>();

        public async Task Invoke(RequestContext context, RequestDelegate next)
        {
            if ((context.IsWrite() || context.IsDelete()) && context.AccessCondition == null)
            {
                SetIfMatchConditionIfETagExists(context);
            }

            await next();

            if (context.IsRead())
            {
                SaveETagFromResponse(context);
            }
        }

        private void SetIfMatchConditionIfETagExists(RequestContext context)
        {
            var name = context.CloudBlockBlob.Name;

            if (_etags.TryGetValue(name, out var etag))
            {
                context.AccessCondition = AccessCondition.GenerateIfMatchCondition(etag);
            }
        }

        private void SaveETagFromResponse(RequestContext context)
        {
            var name = context.CloudBlockBlob.Name;
            var etag = context.CloudBlockBlob.Properties.ETag;

            _etags.AddOrUpdate(name, etag, (key, oldValue) => etag);
        }
    }
}
