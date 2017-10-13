using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;

namespace ToyStorage
{
    public class GZipMiddleware : IMiddleware
    {
        private const string GZipContentEncoding = "gzip";

        public async Task Invoke(RequestContext context, RequestDelegate next)
        {
            if (context.IsWrite())
            {
                context.Content = Compress(context.Content);
                context.CloudBlockBlob.Properties.ContentEncoding = GZipContentEncoding;
            }

            await next();

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