using System.IO;
using System.Net.Http;
using System.Text;
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
                : response.StatusCode.ToString();
        }

        public static async Task<Stream> GetResponseStreamAsync(this Task<HttpResponseMessage> taskResponseMessage)
        {
            var response = await taskResponseMessage;
            return (response.IsSuccessStatusCode)
                ? await response.Content.ReadAsStreamAsync().ConfigureAwait(false)
                : new MemoryStream(Encoding.UTF8.GetBytes(response.StatusCode.ToString()));
        }

        public static async Task<byte[]> GetResponseBytesAsync(this Task<HttpResponseMessage> taskResponseMessage)
        {
            var response = await taskResponseMessage;
            return (response.IsSuccessStatusCode)
                ? await response.Content.ReadAsByteArrayAsync().ConfigureAwait(false)
                : Encoding.UTF8.GetBytes(response.StatusCode.ToString());
        }
    }
}