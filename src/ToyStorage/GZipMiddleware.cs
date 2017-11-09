using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;

namespace ToyStorage
{
    /// <summary>
    /// Middleware component for gzip compression of content body.
    /// </summary>
    /// <remarks>
    /// Insert after formatter middleware component, so that it compresses the
    /// the formatted content on writes and decompresses content before formatter reads content.
    /// </remarks>
    public class GZipMiddleware : IMiddlewareComponent
    {
        private const string GZipContentEncoding = "gzip";

        public async Task Invoke(RequestContext context, RequestDelegate next)
        {
            if (context.IsWrite())
            {
                context.Content = Compress(context.Content);
                context.CloudBlockBlob.Properties.ContentEncoding = GZipContentEncoding;
            }

            await next().ConfigureAwait(false);

            if (context.IsRead())
            {
                await context.CloudBlockBlob.FetchAttributesAsync();

                if (context.CloudBlockBlob.Properties.ContentEncoding == GZipContentEncoding)
                {
                    context.Content = Decompress(context.Content);
                }
            }
        }

        private byte[] Compress(byte[] raw)
        {
            using (var memory = new MemoryStream())
            {
                using (GZipStream gzip = new GZipStream(memory, CompressionMode.Compress, true))
                {
                    gzip.Write(raw, 0, raw.Length);
                }

                return memory.ToArray();
            }
        }

        private byte[] Decompress(byte[] raw)
        {
            using (var compressedMs = new MemoryStream(raw))
            {
                using (var decompressedMs = new MemoryStream())
                {
                    using (var gzs = new GZipStream(compressedMs, CompressionMode.Decompress))
                    {
                        gzs.CopyTo(decompressedMs);
                    }

                    return decompressedMs.ToArray();
                }
            }
        }
    }
}