using System.Text;

namespace Grapevine;

/// <summary>
/// Carries the attributes of an HTTP cookie that appear in the <c>Set-Cookie</c>
/// response header, independently of any specific cookie name or value.
/// </summary>
/// <remarks>
/// <para>
/// An instance of <see cref="CookieOptions"/> can be constructed once and reused
/// across multiple <see cref="Cookie"/> instances, which is useful when a
/// consistent security policy (e.g. <see cref="Secure"/>, <see cref="HttpOnly"/>,
/// <see cref="SameSite"/>) should apply to every cookie in a response.
/// </para>
/// <para>
/// The <see cref="Cookie"/> class exposes pass-through convenience properties for
/// all attributes on this type, so direct construction of <see cref="CookieOptions"/>
/// is not required for simple use cases.
/// </para>
/// <para>
/// Path behaviour is controlled by <see cref="AlwaysEmitPath"/>. When
/// <see langword="true"/> (the default), the <c>Path=/</c> attribute is always
/// included in the serialised cookie, even when <see cref="Path"/> has not been
/// explicitly set. This is the recommended default for embedded HTTP servers
/// where the cookie should be scoped to the entire domain. Set
/// <see cref="AlwaysEmitPath"/> to <see langword="false"/> only when you need
/// the client to apply its own default path scoping per RFC 6265.
/// </para>
/// <para>
/// When both <see cref="Expires"/> and <see cref="MaxAge"/> are set,
/// <see cref="MaxAge"/> takes precedence per RFC 6265. Both are emitted in
/// the serialised output for compatibility with older clients that do not
/// support <c>Max-Age</c>.
/// </para>
/// </remarks>
public class CookieOptions
{
    /// <summary>
    /// The error message used when <see cref="Domain"/> is set to null, empty,
    /// or whitespace.
    /// </summary>
    internal static readonly string InvalidDomainMessage =
        "Cookie domain cannot be null, empty, or whitespace.";

    /// <summary>
    /// The error message used when <see cref="Domain"/> is set to a value that
    /// is not a recognised host name format.
    /// </summary>
    internal static readonly string InvalidDomainFormatMessage =
        "Cookie domain is not a valid host name format.";

    /// <summary>
    /// The error message used when <see cref="Path"/> is set to null, empty,
    /// or whitespace.
    /// </summary>
    internal static readonly string InvalidPathMessage =
        "Cookie path cannot be null, empty, or whitespace.";

    /// <summary>
    /// The error message used when <see cref="Path"/> is set to a value that
    /// does not start with a forward slash.
    /// </summary>
    internal static readonly string InvalidPathFormatMessage =
        "Cookie path must start with '/'.";

    private string? _domain;
    private string? _path;

    /// <summary>
    /// Gets or sets the domain associated with the cookie.
    /// </summary>
    /// <remarks>
    /// When set, restricts the cookie to the specified domain and its subdomains.
    /// A leading dot is not required. Leave <see langword="null"/> to omit the
    /// <c>Domain</c> attribute, which scopes the cookie to the exact host that
    /// set it.
    /// </remarks>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="value"/> is null, empty, whitespace, or not
    /// a valid host name format.
    /// </exception>
    public string? Domain
    {
        get => _domain;
        set
        {
            if (value is null)
            {
                _domain = null;
                return;
            }

            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException(InvalidDomainMessage, nameof(value));

            if (Uri.CheckHostName(value) == UriHostNameType.Unknown)
                throw new ArgumentException(InvalidDomainFormatMessage, nameof(value));

            _domain = value;
        }
    }

    /// <summary>
    /// Gets or sets the path associated with the cookie.
    /// </summary>
    /// <remarks>
    /// <para>
    /// When set, restricts the cookie to requests whose URL path begins with
    /// this value. Must start with a forward slash (e.g. <c>/account</c>).
    /// </para>
    /// <para>
    /// When <see langword="null"/> and <see cref="AlwaysEmitPath"/> is
    /// <see langword="true"/> (the default), <c>Path=/</c> is emitted
    /// automatically. Set to <see langword="null"/> and set
    /// <see cref="AlwaysEmitPath"/> to <see langword="false"/> to suppress
    /// the path attribute entirely.
    /// </para>
    /// </remarks>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="value"/> is empty, whitespace, or does not
    /// start with a forward slash. Pass <see langword="null"/> to clear the path.
    /// </exception>
    public string? Path
    {
        get => _path;
        set
        {
            if (value is null)
            {
                _path = null;
                return;
            }

            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException(InvalidPathMessage, nameof(value));

            if (!value.StartsWith("/", StringComparison.Ordinal))
                throw new ArgumentException(InvalidPathFormatMessage, nameof(value));

            _path = value;
        }
    }

    /// <summary>
    /// Gets or sets the absolute expiration date and time of the cookie.
    /// </summary>
    /// <remarks>
    /// <para>
    /// When both <see cref="Expires"/> and <see cref="MaxAge"/> are set,
    /// <see cref="MaxAge"/> takes precedence per RFC 6265. Both are emitted
    /// in the serialised output for compatibility with clients that do not
    /// support <c>Max-Age</c>.
    /// </para>
    /// <para>
    /// When neither is set, the cookie is a session cookie and is deleted
    /// when the browser session ends.
    /// </para>
    /// </remarks>
    public DateTimeOffset? Expires { get; set; }

    /// <summary>
    /// Gets or sets the maximum age of the cookie as a duration from the
    /// time the response is received.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <c>Max-Age</c> is the modern, preferred expiry mechanism per RFC 6265
    /// and takes precedence over <see cref="Expires"/> when both are present.
    /// </para>
    /// <para>
    /// A value of zero or any negative <see cref="TimeSpan"/> instructs the
    /// client to delete the cookie immediately, per RFC 6265 section 5.2.2.
    /// </para>
    /// </remarks>
    public TimeSpan? MaxAge { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the cookie should be sent only
    /// over secure (HTTPS) connections.
    /// </summary>
    public bool Secure { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the cookie is inaccessible to
    /// client-side scripts. When <see langword="true"/>, the cookie is sent
    /// only in HTTP requests and is not accessible via <c>document.cookie</c>.
    /// </summary>
    public bool HttpOnly { get; set; }

    /// <summary>
    /// Gets or sets the <c>SameSite</c> attribute of the cookie, controlling
    /// whether it is sent with cross-site requests.
    /// </summary>
    /// <seealso cref="SameSiteMode"/>
    public SameSiteMode? SameSite { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the <c>Path</c> attribute is
    /// always included in the serialised cookie, even when <see cref="Path"/>
    /// has not been explicitly set.
    /// </summary>
    /// <remarks>
    /// Defaults to <see langword="true"/>. When <see langword="true"/> and
    /// <see cref="Path"/> is <see langword="null"/>, <c>Path=/</c> is emitted,
    /// scoping the cookie to the entire domain. Set to <see langword="false"/>
    /// only when you need the client to apply its own default path scoping
    /// per RFC 6265.
    /// </remarks>
    public bool AlwaysEmitPath { get; set; } = true;

    /// <summary>
    /// Returns the serialised attribute suffix for this cookie options instance,
    /// suitable for appending after the <c>name=value</c> pair in a
    /// <c>Set-Cookie</c> header.
    /// </summary>
    /// <remarks>
    /// The returned string begins with <c>; </c> if any attributes are present,
    /// or is <see cref="string.Empty"/> if no attributes apply. The
    /// <c>name=value</c> pair itself is not included.
    /// </remarks>
    public override string ToString()
    {
        var sb = new StringBuilder(128);

        // Domain
        if (!string.IsNullOrEmpty(_domain))
        {
            sb.Append("; Domain=");
            sb.Append(_domain);
        }

        // Path: emit explicit value if set, or fall back to "/" when AlwaysEmitPath
        if (!string.IsNullOrEmpty(_path))
        {
            sb.Append("; Path=");
            sb.Append(_path);
        }
        else if (AlwaysEmitPath)
        {
            sb.Append("; Path=/");
        }

        // Max-Age before Expires: RFC 6265 specifies Max-Age takes precedence,
        // so emit it first. Both are included for compatibility with older clients.
        if (MaxAge.HasValue)
        {
            // Per RFC 6265, emit total seconds as an integer. Negative values
            // are valid and instruct the client to delete the cookie immediately.
            sb.Append("; Max-Age=");
            sb.Append((long)MaxAge.Value.TotalSeconds);
        }

        if (Expires.HasValue)
        {
            sb.Append("; Expires=");
            sb.Append(Expires.Value.ToUniversalTime().ToString("R"));
        }

        if (Secure)
            sb.Append("; Secure");

        if (HttpOnly)
            sb.Append("; HttpOnly");

        if (SameSite.HasValue)
        {
            sb.Append("; SameSite=");
            sb.Append(SameSite.Value);
        }

        return sb.ToString();
    }
}