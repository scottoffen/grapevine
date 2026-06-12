using System.Collections.Concurrent;
using System.Reflection;

namespace Grapevine;

/// <summary>
/// Represents an HTTP content type (MIME type), providing a strongly-typed wrapper
/// around a MIME type string with charset, boundary, and binary/text encoding information.
/// </summary>
/// <remarks>
/// <para>
/// A set of well-known content types is provided as static readonly fields (e.g.
/// <see cref="Html"/>, <see cref="Json"/>, <see cref="Png"/>). These are automatically
/// registered on first use and can be looked up by MIME type string via
/// <see cref="FromMimeType"/> or by file extension via <see cref="FromExtension"/>.
/// </para>
/// <para>
/// Custom content types can be registered via <see cref="Register(ContentType, string[])"/>
/// or one of its overloads, and will then participate in the same lookup mechanisms.
/// </para>
/// <para>
/// Equality is based solely on <see cref="Value"/> (the MIME type portion), ignoring
/// charset, boundary, and other parameters. This means <c>"text/html; charset=utf-8"</c>
/// and <c>"text/html; charset=utf-16"</c> are considered equal. This is intentional and
/// supports the common pattern of comparing an incoming request header against a
/// well-known content type instance regardless of charset.
/// </para>
/// <para>
/// For multipart content types, use <see cref="ForMultipart"/> or
/// <see cref="MultipartFormData"/> to construct outgoing instances with a lazily
/// generated boundary, and <see cref="FromMimeType"/> to parse incoming request
/// headers. See <see cref="Boundary"/> for boundary access.
/// </para>
/// </remarks>
public class ContentType
{
    #region Static Fields

    /// <summary>Generic binary content. Use when the content type is unknown or unspecified.</summary>
    [FileExtensions("bin")]
    public static readonly ContentType Binary = new ContentType("application/octet-stream", ContentMode.Binary);

    /// <summary>Bitmap image format.</summary>
    [FileExtensions("bmp")]
    public static readonly ContentType Bmp = new ContentType("image/bmp", ContentMode.Binary);

    /// <summary>Cascading Style Sheets.</summary>
    [FileExtensions("css")]
    public static readonly ContentType Css = new ContentType("text/css", ContentMode.Text, "UTF-8");

    /// <summary>Comma-separated values.</summary>
    [FileExtensions("csv")]
    public static readonly ContentType Csv = new ContentType("text/csv", ContentMode.Text);

    /// <summary>HTML form data encoded as URL query parameters.</summary>
    [FileExtensions("form")]
    public static readonly ContentType FormUrlEncoded = new ContentType("application/x-www-form-urlencoded", ContentMode.Text);

    /// <summary>Graphics Interchange Format image.</summary>
    [FileExtensions("gif")]
    public static readonly ContentType Gif = new ContentType("image/gif", ContentMode.Binary);

    /// <summary>GZip compressed archive.</summary>
    [FileExtensions("gz", "gzip")]
    public static readonly ContentType GZip = new ContentType("application/gzip", ContentMode.Binary);

    /// <summary>HyperText Markup Language.</summary>
    [FileExtensions("html", "htm")]
    public static readonly ContentType Html = new ContentType("text/html", ContentMode.Text, "UTF-8");

    /// <summary>
    /// Icon image using the IANA-registered MIME type. Incoming requests using the
    /// legacy <c>image/x-icon</c> MIME type are automatically resolved to this instance.
    /// </summary>
    [FileExtensions("ico")]
    public static readonly ContentType Icon = new ContentType("image/vnd.microsoft.icon", ContentMode.Binary);

    /// <summary>JavaScript source code.</summary>
    [FileExtensions("js")]
    public static readonly ContentType JavaScript = new ContentType("application/javascript", ContentMode.Text, "UTF-8");

    /// <summary>JavaScript Object Notation.</summary>
    [FileExtensions("json")]
    public static readonly ContentType Json = new ContentType("application/json", ContentMode.Text, "UTF-8");

    /// <summary>JPEG image.</summary>
    [FileExtensions("jpg", "jpeg")]
    public static readonly ContentType Jpg = new ContentType("image/jpeg", ContentMode.Binary);

    /// <summary>MPEG-4 audio.</summary>
    [FileExtensions("m4a")]
    public static readonly ContentType M4a = new ContentType("audio/mp4", ContentMode.Binary);

    /// <summary>MPEG audio (MP3).</summary>
    [FileExtensions("mp3")]
    public static readonly ContentType Mp3 = new ContentType("audio/mpeg", ContentMode.Binary);

    /// <summary>MPEG-4 video.</summary>
    [FileExtensions("mp4")]
    public static readonly ContentType Mp4 = new ContentType("video/mp4", ContentMode.Binary);

    /// <summary>MPEG video.</summary>
    [FileExtensions("mpeg", "mpg")]
    public static readonly ContentType Mpeg = new ContentType("video/mpeg", ContentMode.Binary);

    /// <summary>Ogg Vorbis audio.</summary>
    [FileExtensions("ogg")]
    public static readonly ContentType Ogg = new ContentType("audio/ogg", ContentMode.Binary);

    /// <summary>OpenType font.</summary>
    [FileExtensions("otf")]
    public static readonly ContentType Otf = new ContentType("font/otf", ContentMode.Binary);

    /// <summary>Portable Document Format.</summary>
    [FileExtensions("pdf")]
    public static readonly ContentType Pdf = new ContentType("application/pdf", ContentMode.Binary);

    /// <summary>Portable Network Graphics image.</summary>
    [FileExtensions("png")]
    public static readonly ContentType Png = new ContentType("image/png", ContentMode.Binary);

    /// <summary>Scalable Vector Graphics.</summary>
    [FileExtensions("svg")]
    public static readonly ContentType Svg = new ContentType("image/svg+xml", ContentMode.Text, "UTF-8");

    /// <summary>Tape Archive compressed file.</summary>
    [FileExtensions("tar")]
    public static readonly ContentType Tar = new ContentType("application/x-tar", ContentMode.Binary);

    /// <summary>Plain text.</summary>
    [FileExtensions("txt")]
    public static readonly ContentType Text = new ContentType("text/plain", ContentMode.Text, "UTF-8");

    /// <summary>Tagged Image File Format.</summary>
    [FileExtensions("tiff", "tif")]
    public static readonly ContentType Tiff = new ContentType("image/tiff", ContentMode.Binary);

    /// <summary>TrueType font.</summary>
    [FileExtensions("ttf")]
    public static readonly ContentType Ttf = new ContentType("font/ttf", ContentMode.Binary);

    /// <summary>WebAssembly binary format.</summary>
    [FileExtensions("wasm")]
    public static readonly ContentType Wasm = new ContentType("application/wasm", ContentMode.Binary);

    /// <summary>WebM video.</summary>
    [FileExtensions("webm")]
    public static readonly ContentType WebM = new ContentType("video/webm", ContentMode.Binary);

    /// <summary>WebP image.</summary>
    [FileExtensions("webp")]
    public static readonly ContentType WebP = new ContentType("image/webp", ContentMode.Binary);

    /// <summary>Web Open Font Format.</summary>
    [FileExtensions("woff")]
    public static readonly ContentType Woff = new ContentType("font/woff", ContentMode.Binary);

    /// <summary>Web Open Font Format 2.</summary>
    [FileExtensions("woff2")]
    public static readonly ContentType Woff2 = new ContentType("font/woff2", ContentMode.Binary);

    /// <summary>Extensible Markup Language.</summary>
    [FileExtensions("xml")]
    public static readonly ContentType Xml = new ContentType("application/xml", ContentMode.Text, "UTF-8");

    /// <summary>YAML Ain't Markup Language. Commonly used for configuration files.</summary>
    [FileExtensions("yaml", "yml")]
    public static readonly ContentType Yaml = new ContentType("text/yaml", ContentMode.Text);

    /// <summary>ZIP compressed archive.</summary>
    [FileExtensions("zip")]
    public static readonly ContentType Zip = new ContentType("application/zip", ContentMode.Binary);

    #endregion

    #region Static Initialization

    private static readonly ConcurrentDictionary<string, ContentType> _contentTypes = new(StringComparer.OrdinalIgnoreCase);
    private static readonly ConcurrentDictionary<string, ContentType> _extensions = new(StringComparer.OrdinalIgnoreCase);

#if NET6_0_OR_GREATER
    private static int NextRandom(int maxValue) => Random.Shared.Next(maxValue);
#else
    [ThreadStatic]
    private static Random? _random;
    private static int NextRandom(int maxValue) => (_random ??= new Random()).Next(maxValue);
#endif

    private const int CharSetPrefixLength = 8;   // "charset=".Length
    private const int BoundaryPrefixLength = 9;  // "boundary=".Length

    private static readonly char[] _boundaryChars =
        "-_1234567890abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();

    private const int MinBoundaryLength = 30;
    private const int MaxBoundaryLength = 70;
    private const string DefaultBoundaryPrefix = "----=NextPart_";

    /// <summary>
    /// Initializes the static registry by reflecting over all public static fields of type
    /// <see cref="ContentType"/>, registering each with any file extensions declared via
    /// <see cref="FileExtensionsAttribute"/>. Also registers the legacy <c>image/x-icon</c>
    /// MIME type as an alias for <see cref="Icon"/>.
    /// </summary>
    static ContentType()
    {
        var fields = typeof(ContentType).GetFields(BindingFlags.Public | BindingFlags.Static);
        foreach (var field in fields)
        {
            if (field.GetValue(null) is ContentType contentType)
            {
                var extensions = field.GetCustomAttribute<FileExtensionsAttribute>()?.Extensions ?? Array.Empty<string>();
                Register(contentType, extensions);
            }
        }

        // Register legacy MIME type alias pointing to the canonical instance
        _contentTypes.TryAdd("image/x-icon", Icon);
    }

    #endregion

    #region Static Properties

    /// <summary>
    /// Returns a new <see cref="ContentType"/> instance for multipart form data with a
    /// lazily generated boundary. Each access produces a fresh instance suitable for use
    /// as an outgoing response <c>Content-Type</c> header. The boundary is generated on
    /// first access of <see cref="Boundary"/> and not before, to avoid unnecessary allocations.
    /// </summary>
    /// <seealso cref="ForMultipart"/>
    /// <seealso cref="Boundary"/>
    public static ContentType MultipartFormData => ForMultipart(Multipart.FormData);

    /// <summary>
    /// Returns a new <see cref="ContentType"/> instance for the specified multipart subtype
    /// with a lazily generated boundary. Each call produces a fresh instance suitable for
    /// use as an outgoing response <c>Content-Type</c> header.
    /// </summary>
    /// <param name="multipart">The multipart subtype to use.</param>
    /// <seealso cref="MultipartFormData"/>
    /// <seealso cref="Boundary"/>
    public static ContentType ForMultipart(Multipart multipart)
        => new ContentType($"multipart/{ToMimeSubtype(multipart)}", ContentMode.Binary);

    #endregion

    private readonly Lazy<string> _boundary;

    /// <summary>
    /// Gets the MIME type portion of this content type, e.g. <c>text/html</c>.
    /// </summary>
    public string Value { get; }

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
    /// Gets a value indicating whether this content type represents a multipart content
    /// type, i.e. whether <see cref="Value"/> begins with <c>multipart/</c>.
    /// </summary>
    /// <seealso cref="IsMultipartContent"/>
    public bool IsMultipart => Value.StartsWith("multipart/", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Gets the character set associated with this content type, e.g. <c>UTF-8</c>,
    /// or an empty string if no charset is specified.
    /// </summary>
    public string CharSet { get; }

    /// <summary>
    /// Gets the boundary parameter for multipart content types. For outgoing responses,
    /// the boundary is generated lazily on first access. For incoming requests parsed via
    /// <see cref="FromMimeType"/>, the boundary is taken from the header value. Throws
    /// <see cref="InvalidOperationException"/> if accessed on a non-multipart content type.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown when accessed on a content type whose <see cref="Value"/> does not begin
    /// with <c>multipart/</c>.
    /// </exception>
    /// <seealso cref="ForMultipart"/>
    /// <seealso cref="FromMimeType"/>
    public string Boundary => _boundary.Value;

    /// <summary>
    /// Initializes a new instance of <see cref="ContentType"/> with the specified
    /// MIME type, content mode, optional charset, and optional boundary.
    /// </summary>
    /// <param name="value">The MIME type string, e.g. <c>text/html</c>.</param>
    /// <param name="mode">
    /// Indicates whether the content is binary or text. Defaults to
    /// <see cref="ContentMode.Binary"/>.
    /// </param>
    /// <param name="charSet">
    /// The character set to associate with this content type, e.g. <c>UTF-8</c>.
    /// Pass an empty string if no charset applies.
    /// </param>
    /// <param name="boundary">
    /// An explicit boundary string for multipart content types. If <see langword="null"/>
    /// and the content type is multipart, a boundary is generated lazily on first access
    /// of <see cref="Boundary"/>. Ignored for non-multipart content types.
    /// </param>
    public ContentType(string value, ContentMode mode = ContentMode.Binary, string charSet = "", string? boundary = null)
    {
        Value = value;
        Mode = mode;
        CharSet = charSet;
        _boundary = new Lazy<string>(() =>
        {
            if (!value.StartsWith("multipart/", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException($"Boundary is not applicable for content type '{value}'.");
            return boundary ?? GenerateBoundary();
        });
    }

    /// <summary>
    /// Returns the fully formatted <c>Content-Type</c> header value. For multipart types,
    /// includes the boundary parameter (triggering lazy generation if not yet set).
    /// For other types with a charset, includes the charset parameter.
    /// </summary>
    public override string ToString()
    {
        if (IsMultipart)
            return $"{Value}; boundary={Boundary}";

        if (!string.IsNullOrWhiteSpace(CharSet))
            return $"{Value}; charset={CharSet}";

        return Value;
    }

    /// <summary>
    /// Determines whether this instance is equal to another object. Equality is based
    /// solely on <see cref="Value"/> using a case-insensitive ordinal comparison.
    /// </summary>
    /// <remarks>
    /// Charset, boundary, and other parameters are intentionally ignored during comparison.
    /// This means two instances with the same MIME type but different charsets or boundaries
    /// are considered equal. As a consequence, <see cref="GetHashCode"/> also hashes only
    /// <see cref="Value"/>, ensuring the equality/hash code contract is maintained.
    /// When comparing against a <see cref="string"/>, only the MIME type portion before
    /// any semicolon is used, so <c>"text/html; charset=utf-8"</c> compares equal to
    /// <c>ContentType.Html</c>.
    /// </remarks>
    /// <param name="obj">The object to compare with this instance.</param>
    /// <returns>
    /// <see langword="true"/> if <paramref name="obj"/> is a <see cref="ContentType"/>
    /// or <see cref="string"/> whose MIME type portion matches <see cref="Value"/>;
    /// otherwise <see langword="false"/>.
    /// </returns>
    public override bool Equals(object? obj)
    {
        return obj switch
        {
            ContentType ct => string.Equals(Value, ct.Value, StringComparison.OrdinalIgnoreCase),
            string s => string.Equals(Value, ParseContentType(s).mimeType, StringComparison.OrdinalIgnoreCase),
            _ => false
        };
    }

    /// <summary>
    /// Returns a hash code based solely on <see cref="Value"/>, consistent with the
    /// equality contract defined by <see cref="Equals"/>.
    /// </summary>
    public override int GetHashCode() => StringComparer.OrdinalIgnoreCase.GetHashCode(Value);

    /// <summary>Returns <see langword="true"/> if <paramref name="left"/> and <paramref name="right"/> have the same MIME type value.</summary>
    public static bool operator ==(ContentType left, ContentType right) => left.Equals(right);

    /// <summary>Returns <see langword="true"/> if <paramref name="left"/> and <paramref name="right"/> have different MIME type values.</summary>
    public static bool operator !=(ContentType left, ContentType right) => !left.Equals(right);

    /// <summary>Returns <see langword="true"/> if <paramref name="left"/> has the same MIME type value as the MIME type portion of <paramref name="right"/>.</summary>
    public static bool operator ==(ContentType left, string right) => left.Equals(right);

    /// <summary>Returns <see langword="true"/> if <paramref name="left"/> has a different MIME type value than the MIME type portion of <paramref name="right"/>.</summary>
    public static bool operator !=(ContentType left, string right) => !left.Equals(right);

    /// <summary>Returns <see langword="true"/> if the MIME type portion of <paramref name="left"/> matches the MIME type value of <paramref name="right"/>.</summary>
    public static bool operator ==(string left, ContentType right) => right.Equals(left);

    /// <summary>Returns <see langword="true"/> if the MIME type portion of <paramref name="left"/> does not match the MIME type value of <paramref name="right"/>.</summary>
    public static bool operator !=(string left, ContentType right) => !right.Equals(left);

    /// <summary>
    /// Implicitly converts a <see cref="ContentType"/> to its formatted
    /// <see cref="string"/> representation, e.g. <c>text/html; charset=UTF-8</c>.
    /// </summary>
    /// <param name="contentType">The <see cref="ContentType"/> instance to convert.</param>
    public static implicit operator string(ContentType contentType) => contentType.ToString();

    /// <summary>
    /// Registers a <see cref="ContentType"/> instance in the MIME type registry, and
    /// optionally registers one or more file extensions that map to it. Has no effect
    /// if the content type is already registered. Throws for multipart content types,
    /// which cannot be registered. For content types with a charset, both
    /// the full string (e.g. <c>text/html; charset=UTF-8</c>) and the bare MIME type
    /// (e.g. <c>text/html</c>) are registered, with the first registration winning for
    /// the bare MIME type key.
    /// </summary>
    /// <param name="contentType">The <see cref="ContentType"/> instance to register.</param>
    /// <param name="extensions">
    /// Zero or more file extensions to associate with this content type, without a
    /// leading dot and in lowercase (e.g. <c>"html"</c>, <c>"htm"</c>).
    /// </param>
    /// <exception cref="InvalidOperationException">
    /// Thrown if <paramref name="contentType"/> is a multipart content type. Use
    /// <see cref="MultipartFormData"/> or <see cref="FromMimeType"/> instead.
    /// </exception>
    public static void Register(ContentType contentType, params string[] extensions)
    {
        if (contentType.IsMultipart)
            throw new InvalidOperationException($"Multipart content types cannot be registered. Use ContentType.MultipartFormData or ContentType.FromMimeType to work with multipart content types.");

        _contentTypes.TryAdd(contentType.ToString(), contentType);
        if (!string.IsNullOrWhiteSpace(contentType.CharSet))
            _contentTypes.TryAdd(contentType.Value, contentType);

        foreach (var ext in extensions)
            _extensions.TryAdd(ext, contentType);
    }

    /// <summary>
    /// Registers a new <see cref="ContentType"/> by parsing the MIME type and optional
    /// charset from a single value string, and optionally registers file extensions.
    /// The value may include a charset parameter (e.g. <c>"text/html; charset=utf-8"</c>)
    /// or omit it (e.g. <c>"text/html"</c>).
    /// </summary>
    /// <param name="value">
    /// The MIME type string, optionally including a charset parameter separated by a
    /// semicolon, e.g. <c>"application/json"</c> or <c>"text/html; charset=utf-8"</c>.
    /// </param>
    /// <param name="mode">Indicates whether the content is binary or text.</param>
    /// <param name="extensions">
    /// Zero or more file extensions to associate with this content type, without a
    /// leading dot and in lowercase (e.g. <c>"html"</c>, <c>"htm"</c>).
    /// </param>
    /// <exception cref="InvalidOperationException">
    /// Thrown if <paramref name="value"/> represents a multipart content type. Use
    /// <see cref="MultipartFormData"/> or <see cref="FromMimeType"/> instead.
    /// </exception>
    public static void Register(string value, ContentMode mode, params string[] extensions)
    {
        var (mimeType, charSet, _) = ParseContentType(value);
        Register(new ContentType(mimeType, mode, charSet ?? string.Empty), extensions);
    }

    /// <summary>
    /// Registers a new <see cref="ContentType"/> with the MIME type and charset provided
    /// as separate arguments, and optionally registers file extensions. Use this overload
    /// when the MIME type and charset are known independently. To pass them as a single
    /// combined string, use <see cref="Register(string, ContentMode, string[])"/> instead.
    /// </summary>
    /// <param name="value">
    /// The bare MIME type string, without a charset parameter, e.g. <c>"text/html"</c>.
    /// </param>
    /// <param name="charSet">
    /// The character set to associate with this content type, e.g. <c>"utf-8"</c>.
    /// </param>
    /// <param name="mode">Indicates whether the content is binary or text.</param>
    /// <param name="extensions">
    /// Zero or more file extensions to associate with this content type, without a
    /// leading dot and in lowercase (e.g. <c>"html"</c>, <c>"htm"</c>).
    /// </param>
    /// <exception cref="InvalidOperationException">
    /// Thrown if <paramref name="value"/> represents a multipart content type. Use
    /// <see cref="MultipartFormData"/> or <see cref="FromMimeType"/> instead.
    /// </exception>
    public static void Register(string value, string charSet, ContentMode mode, params string[] extensions)
    {
        Register(new ContentType(value.Trim(), mode, charSet), extensions);
    }

    /// <summary>
    /// Returns <see langword="true"/> if the given string represents a multipart content
    /// type, i.e. if the MIME type portion begins with <c>multipart/</c>. Use this for a
    /// quick check against a raw header string without constructing a full
    /// <see cref="ContentType"/> instance.
    /// </summary>
    /// <param name="value">The raw <c>Content-Type</c> header string to check.</param>
    /// <seealso cref="IsMultipart"/>
    public static bool IsMultipartContent(string value)
    {
        var (mimeType, _, _) = ParseContentType(value);
        return mimeType.StartsWith("multipart/", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Looks up a <see cref="ContentType"/> by MIME type string. First attempts an exact
    /// match (including charset), then falls back to matching on the bare MIME type only.
    /// For multipart content types, always returns a new instance with the boundary parsed
    /// from the header value, since boundaries are unique per request or response.
    /// For non-multipart types, if no match is found a new instance is created with the
    /// content mode inferred from the MIME type pattern, registered, and returned.
    /// Unrecognized types default to <see cref="ContentMode.Binary"/>.
    /// </summary>
    /// <param name="mimeType">
    /// The MIME type string to look up, e.g. <c>"text/html"</c>,
    /// <c>"text/html; charset=utf-8"</c>, or
    /// <c>"multipart/form-data; boundary=----WebKitFormBoundary"</c>.
    /// </param>
    /// <returns>
    /// The registered <see cref="ContentType"/> instance for non-multipart types, or a
    /// new instance with the parsed boundary for multipart types.
    /// </returns>
    /// <seealso cref="IsMultipartContent"/>
    /// <seealso cref="Boundary"/>
    public static ContentType FromMimeType(string mimeType)
    {
        var (value, charSet, boundary) = ParseContentType(mimeType.Trim());

        if (value.StartsWith("multipart/", StringComparison.OrdinalIgnoreCase))
            return new ContentType(value, ContentMode.Binary, charSet ?? string.Empty, boundary);

        var key = mimeType.Trim();

        if (_contentTypes.TryGetValue(key, out var exact))
            return exact;

        if (_contentTypes.TryGetValue(value, out var partial))
            return partial;

        var mode = InferContentMode(value);
        var contentType = new ContentType(value, mode, charSet ?? string.Empty);
        Register(contentType);
        return contentType;
    }

    /// <summary>
    /// Looks up a <see cref="ContentType"/> by file extension. The extension may be
    /// provided with or without a leading dot (e.g. <c>"png"</c> or <c>".PNG"</c>)
    /// and is matched case-insensitively. Returns <see cref="Binary"/> if the extension
    /// is not registered, since treating unknown content as binary is safer than treating
    /// it as text.
    /// </summary>
    /// <param name="extension">
    /// The file extension to look up, with or without a leading dot, e.g.
    /// <c>"html"</c>, <c>".HTML"</c>.
    /// </param>
    /// <returns>
    /// The registered <see cref="ContentType"/> for the given extension, or
    /// <see cref="Binary"/> if the extension is not registered.
    /// </returns>
    public static ContentType FromExtension(string extension)
    {
        var index = extension.IndexOf('.');
        var key = index < 0 ? extension : extension.Substring(index + 1);

        return _extensions.TryGetValue(key, out var contentType)
            ? contentType
            : Binary;
    }

    /// <summary>
    /// Generates a random multipart boundary string between <c>30</c> and <c>70</c>
    /// characters in length, prefixed with <c>----=NextPart_</c>.
    /// </summary>
    private static string GenerateBoundary()
    {
        var sb = new System.Text.StringBuilder(DefaultBoundaryPrefix);
        var endSize = NextRandom(MaxBoundaryLength - MinBoundaryLength + 1) + MinBoundaryLength;
        for (var i = DefaultBoundaryPrefix.Length; i < endSize; i++)
            sb.Append(_boundaryChars[NextRandom(_boundaryChars.Length)]);
        return sb.ToString();
    }

    /// <summary>
    /// Maps a <see cref="Multipart"/> enum value to its correct MIME subtype string,
    /// handling cases like <see cref="Multipart.FormData"/> which requires a hyphen.
    /// </summary>
    /// <param name="multipart">The multipart subtype to map.</param>
    /// <returns>The lowercase MIME subtype string, e.g. <c>"form-data"</c>.</returns>
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

    /// <summary>
    /// Infers the <see cref="ContentMode"/> for an unrecognized MIME type based on
    /// known patterns. MIME types beginning with <c>text/</c> or ending with
    /// <c>+xml</c> or <c>+json</c> are inferred as <see cref="ContentMode.Text"/>.
    /// All other types default to <see cref="ContentMode.Binary"/>.
    /// </summary>
    /// <param name="mimeType">The bare MIME type string to evaluate.</param>
    /// <returns>The inferred <see cref="ContentMode"/>.</returns>
    private static ContentMode InferContentMode(string mimeType)
    {
        if (mimeType.StartsWith("text/", StringComparison.OrdinalIgnoreCase))
            return ContentMode.Text;

        if (mimeType.EndsWith("+xml", StringComparison.OrdinalIgnoreCase))
            return ContentMode.Text;

        if (mimeType.EndsWith("+json", StringComparison.OrdinalIgnoreCase))
            return ContentMode.Text;

        return ContentMode.Binary;
    }

    /// <summary>
    /// Parses a <c>Content-Type</c> header value into its MIME type, optional charset,
    /// and optional boundary components. For example,
    /// <c>"multipart/form-data; boundary=abc123"</c> returns
    /// <c>("multipart/form-data", null, "abc123")</c>, and
    /// <c>"text/html; charset=utf-8"</c> returns <c>("text/html", "utf-8", null)</c>.
    /// Parameter order is not significant.
    /// </summary>
    /// <param name="value">The raw content type string to parse.</param>
    /// <returns>
    /// A tuple containing the trimmed MIME type, the charset value if present, and the
    /// boundary value if present. Either or both of charset and boundary may be
    /// <see langword="null"/> if not found in the string.
    /// </returns>
    private static (string mimeType, string? charSet, string? boundary) ParseContentType(string value)
    {
        var index = value.IndexOf(';');
        if (index < 0) return (value.Trim(), null, null);

        var mimeType = value.Substring(0, index).Trim();
        string? charSet = null;
        string? boundary = null;

        var remaining = value.Substring(index + 1);
        while (remaining.Length > 0)
        {
            var next = remaining.IndexOf(';');
            var part = next < 0 ? remaining.Trim() : remaining.Substring(0, next).Trim();
            remaining = next < 0 ? string.Empty : remaining.Substring(next + 1);

            if (part.StartsWith("charset=", StringComparison.OrdinalIgnoreCase))
                charSet = part.Substring(CharSetPrefixLength).Trim();
            else if (part.StartsWith("boundary=", StringComparison.OrdinalIgnoreCase))
                boundary = part.Substring(BoundaryPrefixLength).Trim();
        }

        return (mimeType,
            string.IsNullOrWhiteSpace(charSet) ? null : charSet,
            string.IsNullOrWhiteSpace(boundary) ? null : boundary);
    }
}