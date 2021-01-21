using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace Grapevine.Client
{
    public static class HttpResponseMessageExtensions
    {
        public static async Task<string> GetResponseStringAsync(this Task<HttpResponseMessage> taskResponseMessage)
        {
            var response = await taskResponseMessage;
            return (response.IsSuccessStatusCode)
                ? await response.Content.ReadAsStringAsync().ConfigureAwait(false)
                : default;
        }

        public static async Task<Stream> GetResponseStreamAsync(this Task<HttpResponseMessage> taskResponseMessage)
        {
            var response = await taskResponseMessage;
            return (response.IsSuccessStatusCode)
                ? await response.Content.ReadAsStreamAsync().ConfigureAwait(false)
                : default;
        }

        public static async Task<byte[]> GetResponseBytesAsync(this Task<HttpResponseMessage> taskResponseMessage)
        {
            var response = await taskResponseMessage;
            return (response.IsSuccessStatusCode)
                ? await response.Content.ReadAsByteArrayAsync().ConfigureAwait(false)
                : default;
        }
    }
}