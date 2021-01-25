using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;

namespace Grapevine
{
    public delegate Task<byte[]> AsyncCompressionDelegate(byte[] contents);

    public partial class CompressionProvider
    {
        public string ContentEncoding { get; } = string.Empty;

        public AsyncCompressionDelegate CompressAsync { get; } = DefaultAsyncCompressionDelegate;

        public CompressionProvider(IList<string> acceptedEncodings, bool identityForbidden = false)
        {
            if (acceptedEncodings.Count != 0)
            {
                CompressAsync = GetCompressionDelegate(acceptedEncodings, identityForbidden, out string contentEncoding);
                ContentEncoding = contentEncoding;
            }
        }
    }

    public partial class CompressionProvider
    {
        public const int CompressIfContentLengthGreaterThan = 2048;

        public static IDictionary<string, AsyncCompressionDelegate> CompressionDelegates { get; } = new Dictionary<string, AsyncCompressionDelegate>()
        {
            {"gzip", CompressionProvider.GzipAsyncCompressionDelegate}
        };

        public static AsyncCompressionDelegate GetCompressionDelegate(IList<string> encodings, bool identityForbidden, out string contentEncoding)
        {
            contentEncoding = string.Empty;

            foreach (var encoding in encodings)
            {
                if (CompressionDelegates.ContainsKey(encoding))
                {
                    contentEncoding = encoding;
                    return CompressionDelegates[encoding];
                }
            }

            if (identityForbidden) return NotAcceptableCompressionDelegate;
            return DefaultAsyncCompressionDelegate;
        }

        public static async Task<byte[]> GzipAsyncCompressionDelegate(byte[] contents)
        {
            using (var ms = new MemoryStream())
            using (var gzip = new GZipStream(ms, CompressionLevel.Fastest))
            {
                await gzip.WriteAsync(contents, 0, contents.Length);
                gzip.Close();
                return ms.ToArray();
            }
        }

        public static async Task<byte[]> DefaultAsyncCompressionDelegate(byte[] contents)
        {
            await Task.CompletedTask;
            return contents;
        }

        public static async Task<byte[]> NotAcceptableCompressionDelegate(byte[] contents)
        {
            // This delegate should only be used when the identity value (meaning no encoding)
            // is explicitly forbidden, and no acceptable encoding delegate is available.
            await Task.CompletedTask;
            throw new StatusCodeException(HttpStatusCode.NotAcceptable);
        }
    }
}