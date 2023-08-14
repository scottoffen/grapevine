using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Grapevine
{
    public class ContentType
    {
        #region Static Implementations

        /// <summary>
        /// This is also the fallback
        /// </summary>
        public static ContentType Binary { get; } = new ContentType("application/octet-stream", "exe");

        public static ContentType Css { get; } = new ContentType("text/css", "css", false, "UTF-8");

        public static ContentType FormUrlEncoded { get; } = new ContentType("application/x-www-form-urlencoded", "");

        public static ContentType Gif { get; } = new ContentType("image/gif", "gif");

        public static ContentType Html { get; } = new ContentType("text/html", "html", false, "UTF-8");

        public static ContentType Htm { get; } = new ContentType("text/html", "htm", false, "UTF-8");

        public static ContentType Icon { get; } = new ContentType("image/x-icon", "ico");

        public static ContentType JavaScript { get; } = new ContentType("application/javascript", "js", false, "UTF-8");

        public static ContentType Json { get; } = new ContentType("application/json", "json", false, "UTF-8");

        public static ContentType Jpg { get; } = new ContentType("image/jpeg", "jpg");

        public static ContentType Mp3 { get; } = new ContentType("audio/mpeg", "mp3");

        public static ContentType Mp4 { get; } = new ContentType("video/mp4", "mp4");

        public static ContentType MultipartFormData { get; } = new ContentType("multipart/form-data", "");

        public static ContentType Pdf { get; } = new ContentType("application/pdf", "pdf");

        public static ContentType Png { get; } = new ContentType("image/png", "png");

        public static IProblemDetailsContentTypes ProblemDetails { get; } = new ProblemDetailsContentTypes();

        public static ContentType Svg { get; } = new ContentType("image/svg+xml", "svg", false, "UTF-8");

        public static ContentType Text { get; } = new ContentType("text/plain", "txt", false, "UTF-8");

        public static ContentType Xml { get; } = new ContentType("application/xml", "xml", false, "UTF-8");

        public static ContentType Zip { get; } = new ContentType("application/zip", "zip");

        #endregion

        #region Static Initialization

        private static readonly Dictionary<string, ContentType> _contentTypes;

        private static readonly Dictionary<string, ContentType> _extensions;

        static ContentType()
        {
            _contentTypes = new Dictionary<string, ContentType>();
            _extensions = new Dictionary<string, ContentType>();
            var contentTypes = new List<ContentType>()
            {
                Binary,
                Css,
                FormUrlEncoded,
                Gif,
                Html,
                Htm,
                Icon,
                JavaScript,
                Jpg,
                Json,
                Mp3,
                Mp4,
                Xml,
                Zip,
                Png,
                Pdf,
                Svg,
                Text,
                MultipartFormData,
            };

            foreach (var c in contentTypes)
            {
                if (!_contentTypes.ContainsKey(c.Value))
                {
                    _contentTypes.Add(c.Value, c); // Ignore if it's already there
                }

                if (!string.IsNullOrEmpty(c.Extension))
                {
                    // Some types cannot be derived from their extension
                    _extensions.Add(c.Extension, c);
                }
            }
        }

        #endregion

        private string _value;

        public string Value { get; }

        public bool IsBinary { get; }

        public string CharSet { get; }

        public string Boundary { get; protected set; }

        public string Extension { get; }

        /// <summary>
        /// Declares a mime type
        /// </summary>
        /// <param name="value">The mime type</param>
        /// <param name="extension">The extension to associate. Can be empty if this type has no defined extension</param>
        /// <param name="isBinary">True if this is a binary file</param>
        /// <param name="charSet">Character set of the data</param>
        public ContentType(string value, string extension, bool isBinary = true, string charSet = "")
        {
            Value = value;
            Extension = extension;
            IsBinary = isBinary;
            CharSet = charSet;
        }

        public void ResetBoundary(string newBoundary = null)
        {
            if (Value.StartsWith("multipart"))
            {
                _value = null;
                Boundary = MultiPartBoundary.Generate(newBoundary);
            }
        }

        public override string ToString()
        {
            if (string.IsNullOrWhiteSpace(_value))
            {
                var sb = new StringBuilder(Value);

                if (!string.IsNullOrWhiteSpace(Boundary))
                {
                    sb.Append($"; boundary={Boundary}");
                }
                else if (!string.IsNullOrWhiteSpace(CharSet))
                {
                    sb.Append($"; charset={CharSet}");
                }

                _value = sb.ToString();
            }

            return _value;
        }

        public static implicit operator ContentType(string value)
        {
            return Find(value);
        }

        public static implicit operator string(ContentType value)
        {
            return value.ToString();
        }

        public static ContentType Find(string value)
        {
            return Add(value);
        }

        public static ContentType FindKey(string key)
        {
            var k = key.ToLower();
            return _extensions.TryGetValue(k, out var extension)
                ? extension
                : ContentType.Binary;
        }

        public static ContentType Add(string value, bool isBinary = true, string charSet = "")
        {
            if (_contentTypes.TryGetValue(value, out ContentType result))
            {
                return result;
            }

            var key = (value.Contains(';') && string.IsNullOrWhiteSpace(charSet))
                ? value
                : string.IsNullOrWhiteSpace(charSet)
                    ? value
                    : $"{value}; charset={charSet}";

            if (_contentTypes.TryGetValue(key, out result))
            {
                return result;
            }

            if (value.Contains(';') && string.IsNullOrWhiteSpace(charSet))
            {
                var parts = value.Split(';');
                value = parts[0];
                charSet = parts[1]?.Replace("charset=", "").Trim();
            }

            if (_contentTypes.TryGetValue(value, out result))
            {
                return result;
            }

            var contentType = new ContentType(value, "", isBinary, charSet);
            _contentTypes.Add(contentType.Value, contentType);
            return contentType;
        }

        public static void Add(ContentType contentType)
        {
            _contentTypes.Add(contentType.Value, contentType);
            if (!string.IsNullOrWhiteSpace(contentType.Extension))
            {
                _extensions.Add(contentType.Extension, contentType);
            }
        }

        public static ContentType MultipartContent(Multipart multipart = default, string boundary = null)
        {
            if (string.IsNullOrWhiteSpace(boundary))
                boundary = MultiPartBoundary.Generate();

            return new ContentType($"multipart/{multipart.ToString().ToLower()}", "", false, "")
            {
                Boundary = boundary.Substring(0, 70).TrimEnd()
            };
        }
    }

    public static class MultiPartBoundary
    {
        public const int MAX_BOUNDARY_LENGTH = 70;
        public const int MIN_BOUNDARY_LENGTH = 30;

        public const string DEFAULT_BOUNDARY_PREFIX = "----=NextPart_";

        private static readonly char[] _multipartChars = "-_1234567890abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();

        private static readonly Random _random = new Random();

        public static string Generate(string prefix = DEFAULT_BOUNDARY_PREFIX)
        {
            prefix = (prefix?.Trim().Length > 0)
                ? prefix.Trim()
                : DEFAULT_BOUNDARY_PREFIX;

            if (prefix.Length >= MIN_BOUNDARY_LENGTH) return prefix;

            var sb = new StringBuilder(prefix);
            var init_size = prefix?.Length ?? 0;
            var end_size = _random.Next(MIN_BOUNDARY_LENGTH, MAX_BOUNDARY_LENGTH);

            for (int i = init_size; i <= end_size; i++)
            {
                sb.Append(_multipartChars[_random.Next(_multipartChars.Length)]);
            }

            return sb.ToString();
        }
    }

    public enum Multipart
    {
        Mixed,
        Alternative,
        Digest,
        Encrypted,
        FormData,
        Related,
        Signed,
        Parallel
    }
}
