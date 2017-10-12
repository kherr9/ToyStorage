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

            AddOrRemoveETagFromCache(context);
        }

        private void SetIfMatchConditionIfETagExists(RequestContext context)
        {
            var name = context.CloudBlockBlob.Name;

            if (_etags.TryGetValue(name, out var etag))
            {
                context.AccessCondition = AccessCondition.GenerateIfMatchCondition(etag);
            }
        }

        private void AddOrRemoveETagFromCache(RequestContext context)
        {
            var name = context.CloudBlockBlob.Name;
            
            if (context.IsDelete())
            {
                // there is no ETag after a blob has been deleted
                _etags.TryRemove(name, out _);
            }
            else
            {
                var etag = context.CloudBlockBlob.Properties.ETag;

                _etags.AddOrUpdate(name, etag, (key, oldValue) => etag);
            }
        }
    }
}
