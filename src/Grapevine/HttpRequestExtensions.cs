using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Grapevine
{
    public static class HttpRequestExtensions
    {
        private static readonly string _multipartContentType = "multipart/form-data; boundary=";
        private static readonly int _startIndex = _multipartContentType.Length;

        internal static string GetMultipartBoundary(this IHttpRequest request)
        {
            return (string.IsNullOrWhiteSpace(request.ContentType) || !request.ContentType.StartsWith(_multipartContentType))
                ? string.Empty
                : request.ContentType.Substring(_startIndex);
        }

        public static async Task<IDictionary<string, string>> ParseFormUrlEncodedData(this IHttpRequest request)
        {
            var data = new Dictionary<string, string>();

            using (var reader = new StreamReader(request.InputStream, request.ContentEncoding))
            {
                var payload = await reader.ReadToEndAsync();

                foreach (var kvp in payload.Split('&'))
                {
                    var pair = kvp.Split('=');
                    var key = pair[0];
                    var value = pair[1];

                    var decoded = string.Empty;
                    while((decoded = Uri.UnescapeDataString(value)) != value)
                    {
                        value = decoded;
                    }

                    data.Add(key, value);
                }
            }

            return data;
        }
    }
}