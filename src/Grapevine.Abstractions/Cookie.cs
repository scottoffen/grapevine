using System.Diagnostics;

namespace Grapevine;

/// <summary>
/// Represents an HTTP response cookie, combining the cookie name and value
/// with its associated <see cref="CookieOptions"/> attributes.
/// </summary>
/// <remarks>
/// <para>
/// This type models a <c>Set-Cookie</c> response header entry. It is not used
/// to represent incoming request cookies, which arrive as flat <c>name=value</c>
/// pairs with no attributes and are accessible via
/// <see cref="RequestCookieCollection"/>.
/// </para>
/// <para>
/// Cookie attributes (path, domain, expiry, flags) are stored on the
/// <see cref="Options"/> property. Convenience pass-through properties are
/// provided on <see cref="Cookie"/> itself so that common configurations can be
/// set without referencing <see cref="Options"/> directly.
/// </para>
/// <para>
/// The name must be a valid RFC 6265 cookie-name token: non-empty, containing
/// no control characters, spaces, or separator characters. The value must not
/// contain the semicolon character.
/// </para>
/// </remarks>
[DebuggerDisplay("{Name}={Value}")]
public class Cookie
{
    /// <summary>
    /// The error message used when <see cref="Name"/> is set to null, empty,
    /// or whitespace.
    /// </summary>
    internal static readonly string InvalidNameMessage =
        "Cookie name cannot be null, empty, or whitespace.";

    /// <summary>
    /// The error message used when <see cref="Name"/> is set to a value
    /// containing invalid token characters.
    /// </summary>
    internal static readonly string InvalidNameTokenMessage =
        "Cookie name contains invalid characters.";

    /// <summary>
    /// The error message used when <see cref="Value"/> is set to null.
    /// </summary>
    internal static readonly string NullValueMessage =
        "Cookie value cannot be null.";

    /// <summary>
    /// The error message used when <see cref="Value"/> contains a semicolon.
    /// </summary>
    internal static readonly string InvalidValueMessage =
        "Cookie value cannot contain the ';' character.";

    private string _name = string.Empty;
    private string _value = string.Empty;

    /// <summary>
    /// Gets or sets the name of the cookie.
    /// </summary>
    /// <remarks>
    /// Must be a valid RFC 6265 cookie-name token: non-empty, no control
    /// characters, spaces, or any of the separator characters
    /// <c>()&lt;&gt;@,;:\"/[]?={}</c>.
    /// </remarks>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="value"/> is null, empty, whitespace, or
    /// contains invalid token characters.
    /// </exception>
    public string Name
    {
        get => _name;
        set
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException(InvalidNameMessage, nameof(value));

            if (!IsValidToken(value))
                throw new ArgumentException(InvalidNameTokenMessage, nameof(value));

            _name = value;
        }
    }

    /// <summary>
    /// Gets or sets the value of the cookie.
    /// </summary>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="value"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="value"/> contains the <c>;</c> character.
    /// </exception>
    public string Value
    {
        get => _value;
        set
        {
            if (value is null)
                throw new ArgumentNullException(nameof(value), NullValueMessage);

            if (value.Contains(';'))
                throw new ArgumentException(InvalidValueMessage, nameof(value));

            _value = value;
        }
    }

    /// <summary>
    /// Gets the <see cref="CookieOptions"/> instance that carries the cookie
    /// attributes for this cookie.
    /// </summary>
    public CookieOptions Options { get; }

    /// <summary>
    /// Initializes a new instance of <see cref="Cookie"/> with the specified
    /// name and value, using default <see cref="CookieOptions"/>.
    /// </summary>
    /// <param name="name">The cookie name.</param>
    /// <param name="value">The cookie value.</param>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="name"/> is null, empty, whitespace, or
    /// contains invalid token characters.
    /// </exception>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="value"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="value"/> contains the <c>;</c> character.
    /// </exception>
    public Cookie(string name, string value) : this(name, value, new CookieOptions())
    {
    }

    /// <summary>
    /// Initializes a new instance of <see cref="Cookie"/> with the specified
    /// name, value, and options.
    /// </summary>
    /// <param name="name">The cookie name.</param>
    /// <param name="value">The cookie value.</param>
    /// <param name="options">
    /// The cookie attribute options. Must not be <see langword="null"/>.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="options"/> is <see langword="null"/>.
    /// </exception>
    public Cookie(string name, string value, CookieOptions options)
    {
        Options = options ?? throw new ArgumentNullException(nameof(options));

        // Name and Value are set via properties to trigger validation.
        Name = name;
        Value = value;
    }

    // -------------------------------------------------------------------------
    // Pass-through convenience properties
    // -------------------------------------------------------------------------

    /// <summary>
    /// Gets or sets the domain attribute of the cookie.
    /// </summary>
    /// <seealso cref="CookieOptions.Domain"/>
    public string? Domain
    {
        get => Options.Domain;
        set => Options.Domain = value;
    }

    /// <summary>
    /// Gets or sets the path attribute of the cookie.
    /// </summary>
    /// <seealso cref="CookieOptions.Path"/>
    public string? Path
    {
        get => Options.Path;
        set => Options.Path = value;
    }

    /// <summary>
    /// Gets or sets the absolute expiration date and time of the cookie.
    /// </summary>
    /// <seealso cref="CookieOptions.Expires"/>
    public DateTimeOffset? Expires
    {
        get => Options.Expires;
        set => Options.Expires = value;
    }

    /// <summary>
    /// Gets or sets the maximum age of the cookie as a duration.
    /// </summary>
    /// <seealso cref="CookieOptions.MaxAge"/>
    public TimeSpan? MaxAge
    {
        get => Options.MaxAge;
        set => Options.MaxAge = value;
    }

    /// <summary>
    /// Gets or sets a value indicating whether the cookie is sent only over
    /// secure (HTTPS) connections.
    /// </summary>
    /// <seealso cref="CookieOptions.Secure"/>
    public bool Secure
    {
        get => Options.Secure;
        set => Options.Secure = value;
    }

    /// <summary>
    /// Gets or sets a value indicating whether the cookie is inaccessible to
    /// client-side scripts.
    /// </summary>
    /// <seealso cref="CookieOptions.HttpOnly"/>
    public bool HttpOnly
    {
        get => Options.HttpOnly;
        set => Options.HttpOnly = value;
    }

    /// <summary>
    /// Gets or sets the <c>SameSite</c> attribute of the cookie.
    /// </summary>
    /// <seealso cref="CookieOptions.SameSite"/>
    public SameSiteMode? SameSite
    {
        get => Options.SameSite;
        set => Options.SameSite = value;
    }

    /// <summary>
    /// Gets or sets a value indicating whether <c>Path=/</c> is always emitted
    /// in the serialized cookie even when <see cref="Path"/> is not explicitly set.
    /// </summary>
    /// <seealso cref="CookieOptions.AlwaysEmitPath"/>
    public bool AlwaysEmitPath
    {
        get => Options.AlwaysEmitPath;
        set => Options.AlwaysEmitPath = value;
    }

    /// <summary>
    /// Returns the fully formatted <c>Set-Cookie</c> header value for this cookie,
    /// including the name, value, and all applicable attributes from
    /// <see cref="Options"/>.
    /// </summary>
    public override string ToString() => $"{Name}={Value}{Options}";

    /// <summary>
    /// Determines whether a string is a valid RFC 6265 cookie-name token.
    /// </summary>
    /// <remarks>
    /// A valid token contains only visible ASCII characters excluding
    /// control characters, spaces, and the separator characters
    /// defined in RFC 2616: <c>()&lt;&gt;@,;:\"/[]?={}</c> and tab.
    /// </remarks>
    /// <param name="input">The string to validate.</param>
    /// <returns>
    /// <see langword="true"/> if the string is a valid token;
    /// otherwise <see langword="false"/>.
    /// </returns>
    private static bool IsValidToken(string input)
    {
        foreach (var c in input)
        {
            if (c <= 0x20 || c >= 0x7f || "()<>@,;:\\\"/[]?={} \t".IndexOf(c) >= 0)
                return false;
        }

        return true;
    }
}