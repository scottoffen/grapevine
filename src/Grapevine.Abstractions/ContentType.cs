using System.Collections.Concurrent;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Grapevine;

/// <summary>
/// Represents an HTTP content type (MIME type), providing structured access to the
/// type, subtype, charset, boundary, and additional parameters of a content type header.
/// </summary>
/// <remarks>
/// <para>
/// A set of well-known content types is provided as static readonly fields (e.g.
/// <see cref="Html"/>, <see cref="Json"/>, <see cref="Png"/>). These are automatically
/// registered at startup and can be looked up by MIME type string via
/// <see cref="FromMimeType"/> or by file extension via <see cref="FromExtension"/>.
/// </para>
/// <para>
/// Equality is based solely on <see cref="Type"/> and <see cref="SubType"/>, ignoring
/// charset, boundary, and other parameters. This supports the common pattern of
/// comparing an incoming request header against a well-known content type instance
/// regardless of charset.
/// </para>
/// <para>
/// For multipart content types, use <see cref="ForMultipart"/> or
/// <see cref="MultipartFormData"/> to construct outgoing instances with a lazily
/// generated boundary, and <see cref="FromMimeType"/> to parse incoming request
/// headers. See <see cref="Boundary"/> for boundary access.
/// </para>
/// </remarks>
[DebuggerDisplay("{ToString()}")]
public partial class ContentType : IEquatable<ContentType>
{
    private static readonly string[] _textKeywords =
    [
        "form", "json", "xml", "javascript", "html", "css", "txt"
    ];

    private readonly Lazy<string> _boundary;

    /// <summary>
    /// Gets the primary type of the content type, e.g. <c>text</c> or <c>application</c>.
    /// </summary>
    public string Type { get; }

    /// <summary>
    /// Gets the subtype of the content type, e.g. <c>html</c> or <c>json</c>.
    /// </summary>
    public string SubType { get; }

    /// <summary>
    /// Gets the character set of the content type, or <see langword="null"/> if not specified.
    /// </summary>
    public string? Charset { get; }

    /// <summary>
    /// Gets the <see cref="ContentMode"/> indicating whether this content type
    /// represents binary or text content.
    /// </summary>
    public ContentMode Mode { get; }

    /// <summary>
    /// Gets a value indicating whether this content type represents binary content.
    /// When <see langword="true"/>, the content should be read from or written to
    /// a stream as raw bytes rather than as encoded text.
    /// </summary>
    public bool IsBinary => Mode == ContentMode.Binary;

    /// <summary>
    /// Gets a value indicating whether this content type represents a multipart
    /// content type.
    /// </summary>
    /// <seealso cref="IsMultipartContent"/>
    public bool IsMultipart { get; }

    /// <summary>
    /// Gets the boundary parameter for multipart content types. For outgoing responses,
    /// the boundary is generated lazily on first access. For incoming requests parsed via
    /// <see cref="FromMimeType"/>, the boundary is taken from the header value. Throws
    /// <see cref="InvalidOperationException"/> if accessed on a non-multipart content type.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown when accessed on a content type whose <see cref="Type"/> is not
    /// <c>multipart</c>.
    /// </exception>
    /// <seealso cref="ForMultipart"/>
    /// <seealso cref="FromMimeType"/>
    public string Boundary => _boundary.Value;

    /// <summary>
    /// Gets the additional parameters of the content type, excluding charset and boundary.
    /// </summary>
    public Dictionary<string, string> Parameters { get; } = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Initializes a new instance of <see cref="ContentType"/> with the specified
    /// type, subtype, optional charset, optional boundary, and optional content mode.
    /// </summary>
    /// <param name="type">The primary MIME type, e.g. <c>text</c>.</param>
    /// <param name="subtype">The MIME subtype, e.g. <c>html</c>.</param>
    /// <param name="charset">
    /// The character set, e.g. <c>UTF-8</c>. Pass <see langword="null"/> if not applicable.
    /// </param>
    /// <param name="boundary">
    /// An explicit boundary string for multipart content types. If <see langword="null"/>
    /// and the content type is multipart, a boundary is generated lazily on first access
    /// of <see cref="Boundary"/>. Ignored for non-multipart content types.
    /// </param>
    /// <param name="mode">
    /// The content mode. If <see langword="null"/>, the mode is auto-detected from the
    /// type, subtype, and charset.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="type"/> is <see langword="null"/>.
    /// </exception>
    public ContentType(string type, string subtype, string? charset = null, string? boundary = null, ContentMode? mode = null)
    {
        Type = type ?? throw new ArgumentNullException(nameof(type));
        SubType = subtype ?? string.Empty;
        Charset = string.IsNullOrWhiteSpace(charset) ? null : charset;
        IsMultipart = string.Equals(Type, "multipart", StringComparison.OrdinalIgnoreCase);
        Mode = mode ?? DetectMode(Type, SubType, Charset);
        _boundary = new Lazy<string>(() =>
        {
            if (!IsMultipart)
                throw new InvalidOperationException($"Boundary is not applicable for content type '{Type}/{SubType}'.");
            return boundary ?? GenerateBoundary();
        });
    }

    /// <summary>
    /// Returns the fully formatted <c>Content-Type</c> header value, including charset,
    /// boundary, and any additional parameters.
    /// </summary>
    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.Append(Type);
        if (!string.IsNullOrEmpty(SubType))
        {
            sb.Append('/');
            sb.Append(SubType);
        }

        if (!string.IsNullOrWhiteSpace(Charset))
        {
            sb.Append("; charset=");
            sb.Append(Charset);
        }

        if (IsMultipart)
        {
            sb.Append("; boundary=");
            sb.Append(_boundary.Value);
        }

        foreach (var param in Parameters)
        {
            sb.Append("; ");
            sb.Append(param.Key);
            sb.Append('=');
            sb.Append(FormatParameterValue(param.Value));
        }

        return sb.ToString();
    }

    /// <summary>
    /// Determines whether this instance is equal to another <see cref="ContentType"/>.
    /// Equality is based solely on <see cref="Type"/> and <see cref="SubType"/>,
    /// ignoring charset, boundary, and other parameters.
    /// </summary>
    /// <param name="other">The <see cref="ContentType"/> to compare with this instance.</param>
    /// <returns>
    /// <see langword="true"/> if both instances have the same <see cref="Type"/> and
    /// <see cref="SubType"/>; otherwise <see langword="false"/>.
    /// </returns>
    public bool Equals(ContentType? other)
    {
        if (other is null) return false;
        return string.Equals(Type, other.Type, StringComparison.OrdinalIgnoreCase)
            && string.Equals(SubType, other.SubType, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Determines whether this instance is equal to another object. Equality is based
    /// solely on <see cref="Type"/> and <see cref="SubType"/>, ignoring charset,
    /// boundary, and other parameters.
    /// </summary>
    /// <param name="obj">The object to compare with this instance.</param>
    /// <returns>
    /// <see langword="true"/> if <paramref name="obj"/> is a <see cref="ContentType"/>
    /// with the same <see cref="Type"/> and <see cref="SubType"/>; otherwise
    /// <see langword="false"/>.
    /// </returns>
    public override bool Equals(object? obj) => Equals(obj as ContentType);

    /// <summary>
    /// Returns a hash code based on <see cref="Type"/> and <see cref="SubType"/>,
    /// consistent with the equality contract defined by <see cref="Equals(ContentType?)"/>.
    /// </summary>
    public override int GetHashCode()
        => StringComparer.OrdinalIgnoreCase.GetHashCode(Type)
            ^ StringComparer.OrdinalIgnoreCase.GetHashCode(SubType);

    /// <summary>
    /// Auto-detects the <see cref="ContentMode"/> from the type, subtype, and charset.
    /// Types beginning with <c>text/</c>, containing known text keywords, or having a
    /// charset specified are inferred as <see cref="ContentMode.Text"/>. All others
    /// default to <see cref="ContentMode.Binary"/>.
    /// </summary>
    private static ContentMode DetectMode(string type, string subtype, string? charset)
    {
        if (!string.IsNullOrWhiteSpace(charset)) return ContentMode.Text;
        var combined = $"{type}/{subtype}";
        if (combined.StartsWithAny("text/") || combined.ContainsAny(_textKeywords))
            return ContentMode.Text;
        return ContentMode.Binary;
    }

    /// <summary>
    /// Formats a parameter value for inclusion in a <c>Content-Type</c> header,
    /// quoting the value and escaping internal quotes if necessary.
    /// </summary>
    private static string FormatParameterValue(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return "\"\"";
        var needsQuoting = value.Any(c => char.IsWhiteSpace(c) || c == ';' || c == '=' || c == '"');
        var escaped = value.Replace("\"", "\\\"");
        return needsQuoting ? $"\"{escaped}\"" : escaped;
    }
}

/// <summary>
/// Provides equality and conversion operators for <see cref="ContentType"/>.
/// </summary>
public partial class ContentType
{
    /// <summary>Returns <see langword="true"/> if <paramref name="left"/> and <paramref name="right"/> have the same type and subtype.</summary>
    public static bool operator ==(ContentType? left, ContentType? right)
        => left?.Equals(right) ?? right is null;

    /// <summary>Returns <see langword="true"/> if <paramref name="left"/> and <paramref name="right"/> have different types or subtypes.</summary>
    public static bool operator !=(ContentType? left, ContentType? right)
        => !(left == right);

    /// <summary>
    /// Implicitly converts a <see cref="ContentType"/> to its formatted
    /// <see cref="string"/> representation.
    /// </summary>
    /// <param name="contentType">The <see cref="ContentType"/> instance to convert.</param>
    public static implicit operator string(ContentType contentType) => contentType.ToString();

    /// <summary>
    /// Implicitly converts a <see cref="string"/> to a <see cref="ContentType"/> by
    /// parsing the string.
    /// </summary>
    /// <param name="contentType">The content type string to parse.</param>
    public static implicit operator ContentType(string contentType) => Parse(contentType);
}

/// <summary>
/// Provides static registry, parsing, and lookup methods for <see cref="ContentType"/>.
/// </summary>
public partial class ContentType
{
    private static readonly ConcurrentDictionary<string, ContentType> _contentTypes
        = new(StringComparer.OrdinalIgnoreCase);

    private static readonly ConcurrentDictionary<string, ContentType> _extensions
        = new(StringComparer.OrdinalIgnoreCase);

    private const string CharsetTag = "charset";
    private const string BoundaryTag = "boundary";

#if NET6_0_OR_GREATER
    private static int NextRandom(int maxValue) => Random.Shared.Next(maxValue);
#else
    [ThreadStatic]
    private static Random? _random;
    private static int NextRandom(int maxValue) => (_random ??= new Random()).Next(maxValue);
#endif

    private static readonly char[] _boundaryChars =
        "-_1234567890abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();

    private const int MinBoundaryLength = 30;
    private const int MaxBoundaryLength = 70;
    private const string DefaultBoundaryPrefix = "----=NextPart_";

    /// <summary>
    /// Initializes the static registry by explicitly registering each well-known
    /// <see cref="ContentType"/> field with its associated file extensions.
    /// Also registers the legacy <c>image/x-icon</c> MIME type as an alias for
    /// <see cref="Icon"/>.
    /// </summary>
    /// <remarks>
    /// This replaces the previous reflection-based approach that used
    /// <c>GetFields</c> and <c>GetCustomAttribute</c> to discover registrations
    /// at runtime. The explicit registration is AOT-safe: no metadata preservation
    /// is required, and the trimmer can statically analyse all field references.
    /// </remarks>
    static ContentType()
    {
        Register(Binary,            "bin");
        Register(Bmp,               "bmp");
        Register(Css,               "css");
        Register(Csv,               "csv");
        Register(FormUrlEncoded,    "form");
        Register(Gif,               "gif");
        Register(GZip,              "gz", "gzip");
        Register(Html,              "html", "htm");
        Register(Icon,              "ico");
        Register(JavaScript,        "js");
        Register(Json,              "json");
        Register(Jpg,               "jpg", "jpeg");
        Register(M4a,               "m4a");
        Register(Mp3,               "mp3");
        Register(Mp4,               "mp4");
        Register(Mpeg,              "mpeg", "mpg");
        Register(Ogg,               "ogg");
        Register(Otf,               "otf");
        Register(Pdf,               "pdf");
        Register(Png,               "png");
        Register(ProblemDetailsJson);
        Register(ProblemDetailsXml);
        Register(Svg,               "svg");
        Register(Tar,               "tar");
        Register(Text,              "txt");
        Register(Tiff,              "tiff", "tif");
        Register(Ttf,               "ttf");
        Register(Wasm,              "wasm");
        Register(WebM,              "webm");
        Register(WebP,              "webp");
        Register(Woff,              "woff");
        Register(Woff2,             "woff2");
        Register(Xml,               "xml");
        Register(Yaml,              "yaml", "yml");
        Register(Zip,               "zip");

        // Register the legacy image/x-icon MIME type as an alias for Icon.
        _contentTypes.TryAdd("image/x-icon", Icon);
    }

    /// <summary>
    /// Parses a <c>Content-Type</c> header string into a <see cref="ContentType"/> instance.
    /// Always creates a new instance; use <see cref="FromMimeType"/> to retrieve a cached
    /// instance for well-known types.
    /// </summary>
    /// <param name="contentType">The content type string to parse.</param>
    /// <returns>A new <see cref="ContentType"/> instance.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="contentType"/> is null or whitespace.
    /// </exception>
    public static ContentType Parse(string contentType)
    {
        if (string.IsNullOrWhiteSpace(contentType))
            throw new ArgumentException("Missing or invalid content type.", nameof(contentType));

        var parts = contentType.Trim().Split(';');
        var typeParts = parts[0].Trim().Split('/');

        var type = typeParts[0].Trim();
        var subtype = typeParts.Length > 1 ? typeParts[1].Trim() : string.Empty;

        string? charset = null;
        string? boundary = null;
        var @params = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (var part in parts.Skip(1))
        {
            var paramParts = part.Split(['='], 2, StringSplitOptions.None);
            var key = paramParts[0].Trim().ToLowerInvariant();
            var value = paramParts.Length > 1
                ? paramParts[1].Trim().Trim('"')
                : string.Empty;

            switch (key)
            {
                case CharsetTag:
                    charset = value;
                    break;
                case BoundaryTag:
                    boundary = value;
                    break;
                default:
                    @params[key] = value;
                    break;
            }
        }

        var result = new ContentType(type, subtype, charset, boundary);
        foreach (var param in @params)
            result.Parameters[param.Key] = param.Value;

        return result;
    }

    /// <summary>
    /// Attempts to parse a <c>Content-Type</c> header string into a
    /// <see cref="ContentType"/> instance without throwing on failure.
    /// </summary>
    /// <param name="value">The content type string to parse.</param>
    /// <param name="contentType">
    /// When this method returns <see langword="true"/>, contains the parsed
    /// <see cref="ContentType"/> instance. When this method returns
    /// <see langword="false"/>, contains <see langword="null"/>.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if <paramref name="value"/> was successfully parsed;
    /// otherwise <see langword="false"/>.
    /// </returns>
    /// <remarks>
    /// This method is the non-throwing counterpart to <see cref="Parse"/>. It
    /// returns <see langword="false"/> for null, empty, or whitespace input, and
    /// for any input that <see cref="Parse"/> would reject with an exception.
    /// Use this method when parsing untrusted input such as incoming request
    /// <c>Content-Type</c> headers.
    /// </remarks>
    public static bool TryParse(string? value, [NotNullWhen(true)] out ContentType? contentType)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            contentType = null;
            return false;
        }
 
        try
        {
            contentType = Parse(value!);
            return true;
        }
        catch
        {
            contentType = null;
            return false;
        }
    }

    /// <summary>
    /// Looks up a <see cref="ContentType"/> by MIME type string. Returns a cached instance
    /// for well-known and previously registered types. For multipart types, always returns
    /// a new instance with the boundary parsed from the header value. For unknown types,
    /// creates, registers, and returns a new instance.
    /// </summary>
    /// <param name="mimeType">
    /// The MIME type string to look up, e.g. <c>"text/html"</c>,
    /// <c>"text/html; charset=utf-8"</c>, or
    /// <c>"multipart/form-data; boundary=abc123"</c>.
    /// </param>
    /// <returns>
    /// A cached <see cref="ContentType"/> instance for non-multipart types, or a new
    /// instance with the parsed boundary for multipart types.
    /// </returns>
    /// <seealso cref="IsMultipartContent"/>
    /// <seealso cref="Boundary"/>
    public static ContentType FromMimeType(string mimeType)
    {
        var parsed = Parse(mimeType);

        if (parsed.IsMultipart) return parsed;

        var key = $"{parsed.Type}/{parsed.SubType}";

        if (_contentTypes.TryGetValue(key, out var cached)) return cached;

        _contentTypes.TryAdd(key, parsed);
        return parsed;
    }

    /// <summary>
    /// Looks up a <see cref="ContentType"/> by file extension. The extension may be
    /// provided with or without a leading dot and is matched case-insensitively.
    /// Returns <see cref="Binary"/> if the extension is not registered.
    /// </summary>
    /// <param name="extension">
    /// The file extension to look up, e.g. <c>"html"</c> or <c>".HTML"</c>.
    /// </param>
    /// <returns>
    /// The registered <see cref="ContentType"/> for the given extension, or
    /// <see cref="Binary"/> if the extension is not registered.
    /// </returns>
    public static ContentType FromExtension(string extension)
    {
        var index = extension.IndexOf('.');
        var key = index < 0 ? extension : extension.Substring(index + 1);
        return _extensions.TryGetValue(key, out var contentType) ? contentType : Binary;
    }

    /// <summary>
    /// Registers a <see cref="ContentType"/> instance in the MIME type registry, and
    /// optionally registers one or more file extensions that map to it. Throws for
    /// multipart content types, which cannot be registered.
    /// </summary>
    /// <param name="contentType">The <see cref="ContentType"/> instance to register.</param>
    /// <param name="extensions">
    /// Zero or more file extensions to associate with this content type, without a
    /// leading dot (e.g. <c>"html"</c>, <c>"htm"</c>).
    /// </param>
    /// <exception cref="InvalidOperationException">
    /// Thrown if <paramref name="contentType"/> is a multipart content type.
    /// </exception>
    public static void Register(ContentType contentType, params string[] extensions)
    {
        if (contentType.IsMultipart)
            throw new InvalidOperationException("Multipart content types cannot be registered. Use ContentType.MultipartFormData or ContentType.ForMultipart instead.");

        var key = $"{contentType.Type}/{contentType.SubType}";
        _contentTypes.TryAdd(key, contentType);

        foreach (var ext in extensions)
            _extensions.TryAdd(ext, contentType);
    }

    /// <summary>
    /// Registers a new <see cref="ContentType"/> parsed from the specified string,
    /// and optionally registers file extensions. Throws for multipart content types.
    /// </summary>
    /// <param name="value">
    /// The MIME type string, optionally including a charset parameter, e.g.
    /// <c>"text/html; charset=utf-8"</c>.
    /// </param>
    /// <param name="extensions">
    /// Zero or more file extensions to associate with this content type.
    /// </param>
    /// <exception cref="InvalidOperationException">
    /// Thrown if <paramref name="value"/> represents a multipart content type.
    /// </exception>
    public static void Register(string value, params string[] extensions)
        => Register(Parse(value), extensions);

    /// <summary>
    /// Returns <see langword="true"/> if the given string represents a multipart content
    /// type. Use this for a quick check against a raw header string without constructing
    /// a full <see cref="ContentType"/> instance.
    /// </summary>
    /// <param name="value">The raw <c>Content-Type</c> header string to check.</param>
    /// <seealso cref="IsMultipart"/>
    public static bool IsMultipartContent(string value)
    {
        var typeEnd = value.IndexOf('/');
        if (typeEnd < 0) return false;
        return string.Equals(value.Substring(0, typeEnd).Trim(), "multipart", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Returns a new <see cref="ContentType"/> instance for the specified multipart
    /// subtype with a lazily generated boundary.
    /// </summary>
    /// <param name="multipart">The multipart subtype to use.</param>
    /// <seealso cref="MultipartFormData"/>
    /// <seealso cref="Boundary"/>
    public static ContentType ForMultipart(Multipart multipart)
        => new ContentType("multipart", ToMimeSubtype(multipart));

    /// <summary>
    /// Generates a random multipart boundary string between <c>30</c> and <c>70</c>
    /// characters in length, prefixed with <c>----=NextPart_</c>.
    /// </summary>
    private static string GenerateBoundary()
    {
        var sb = new StringBuilder(DefaultBoundaryPrefix);
        var endSize = NextRandom(MaxBoundaryLength - MinBoundaryLength + 1) + MinBoundaryLength;
        for (var i = DefaultBoundaryPrefix.Length; i < endSize; i++)
            sb.Append(_boundaryChars[NextRandom(_boundaryChars.Length)]);
        return sb.ToString();
    }

    /// <summary>
    /// Maps a <see cref="Multipart"/> enum value to its correct MIME subtype string.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="multipart"/> is not a recognized enum value.
    /// </exception>
    private static string ToMimeSubtype(Multipart multipart) => multipart switch
    {
        Multipart.Mixed       => "mixed",
        Multipart.Alternative => "alternative",
        Multipart.Digest      => "digest",
        Multipart.Encrypted   => "encrypted",
        Multipart.FormData    => "form-data",
        Multipart.Related     => "related",
        Multipart.Signed      => "signed",
        Multipart.Parallel    => "parallel",
        _ => throw new ArgumentOutOfRangeException(nameof(multipart), multipart, "Unknown multipart subtype.")
    };
}

/// <summary>
/// Provides the static well-known content type fields for <see cref="ContentType"/>.
/// </summary>
public partial class ContentType
{
    /// <summary>Generic binary content. Use when the content type is unknown or unspecified.</summary>
    public static readonly ContentType Binary = Parse("application/octet-stream");

    /// <summary>Bitmap image format.</summary>
    public static readonly ContentType Bmp = Parse("image/bmp");

    /// <summary>Cascading Style Sheets.</summary>
    public static readonly ContentType Css = Parse("text/css; charset=UTF-8");

    /// <summary>Comma-separated values.</summary>
    public static readonly ContentType Csv = Parse("text/csv");

    /// <summary>HTML form data encoded as URL query parameters.</summary>
    public static readonly ContentType FormUrlEncoded = Parse("application/x-www-form-urlencoded");

    /// <summary>Graphics Interchange Format image.</summary>
    public static readonly ContentType Gif = Parse("image/gif");

    /// <summary>GZip compressed archive.</summary>
    public static readonly ContentType GZip = Parse("application/gzip");

    /// <summary>HyperText Markup Language.</summary>
    public static readonly ContentType Html = Parse("text/html; charset=UTF-8");

    /// <summary>
    /// Icon image using the IANA-registered MIME type. Incoming requests using the
    /// legacy <c>image/x-icon</c> MIME type are automatically resolved to this instance.
    /// </summary>
    public static readonly ContentType Icon = Parse("image/vnd.microsoft.icon");

    /// <summary>JavaScript source code.</summary>
    public static readonly ContentType JavaScript = Parse("application/javascript; charset=UTF-8");

    /// <summary>JavaScript Object Notation.</summary>
    public static readonly ContentType Json = Parse("application/json; charset=UTF-8");

    /// <summary>JPEG image.</summary>
    public static readonly ContentType Jpg = Parse("image/jpeg");

    /// <summary>MPEG-4 audio.</summary>
    public static readonly ContentType M4a = Parse("audio/mp4");

    /// <summary>MPEG audio (MP3).</summary>
    public static readonly ContentType Mp3 = Parse("audio/mpeg");

    /// <summary>MPEG-4 video.</summary>
    public static readonly ContentType Mp4 = Parse("video/mp4");

    /// <summary>MPEG video.</summary>
    public static readonly ContentType Mpeg = Parse("video/mpeg");

    /// <summary>
    /// Returns a new <see cref="ContentType"/> instance for multipart form data with a
    /// lazily generated boundary. Each access produces a fresh instance suitable for use
    /// as an outgoing response <c>Content-Type</c> header.
    /// </summary>
    /// <seealso cref="ForMultipart"/>
    /// <seealso cref="Boundary"/>
    public static ContentType MultipartFormData => ForMultipart(Multipart.FormData);

    /// <summary>Ogg Vorbis audio.</summary>
    public static readonly ContentType Ogg = Parse("audio/ogg");

    /// <summary>OpenType font.</summary>
    public static readonly ContentType Otf = Parse("font/otf");

    /// <summary>Portable Document Format.</summary>
    public static readonly ContentType Pdf = Parse("application/pdf");

    /// <summary>Portable Network Graphics image.</summary>
    public static readonly ContentType Png = Parse("image/png");

    /// <summary>
    /// Problem Details JSON, as defined in RFC 7807. Used for structured error responses.
    /// </summary>
    public static readonly ContentType ProblemDetailsJson = Parse("application/problem+json; charset=UTF-8");

    /// <summary>
    /// Problem Details XML, as defined in RFC 7807. Used for structured error responses.
    /// </summary>
    public static readonly ContentType ProblemDetailsXml = Parse("application/problem+xml; charset=UTF-8");

    /// <summary>Scalable Vector Graphics.</summary>
    public static readonly ContentType Svg = Parse("image/svg+xml; charset=UTF-8");

    /// <summary>Tape Archive compressed file.</summary>
    public static readonly ContentType Tar = Parse("application/x-tar");

    /// <summary>Plain text.</summary>
    public static readonly ContentType Text = Parse("text/plain; charset=UTF-8");

    /// <summary>Tagged Image File Format.</summary>
    public static readonly ContentType Tiff = Parse("image/tiff");

    /// <summary>TrueType font.</summary>
    public static readonly ContentType Ttf = Parse("font/ttf");

    /// <summary>WebAssembly binary format.</summary>
    public static readonly ContentType Wasm = Parse("application/wasm");

    /// <summary>WebM video.</summary>
    public static readonly ContentType WebM = Parse("video/webm");

    /// <summary>WebP image.</summary>
    public static readonly ContentType WebP = Parse("image/webp");

    /// <summary>Web Open Font Format.</summary>
    public static readonly ContentType Woff = Parse("font/woff");

    /// <summary>Web Open Font Format 2.</summary>
    public static readonly ContentType Woff2 = Parse("font/woff2");

    /// <summary>Extensible Markup Language.</summary>
    public static readonly ContentType Xml = Parse("application/xml; charset=UTF-8");

    /// <summary>YAML Ain't Markup Language. Commonly used for configuration files.</summary>
    public static readonly ContentType Yaml = Parse("text/yaml");

    /// <summary>ZIP compressed archive.</summary>
    public static readonly ContentType Zip = Parse("application/zip");
}