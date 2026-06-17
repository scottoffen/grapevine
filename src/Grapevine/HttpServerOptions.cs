using System.ComponentModel.DataAnnotations;
using System.Net;

namespace Grapevine;

/// <summary>
/// Provides configuration options for an <see cref="HttpServer"/> instance,
/// extending the base options with settings specific to the
/// <see cref="HttpListener"/> transport.
/// </summary>
/// <remarks>
/// <para>
/// Pass an instance of this class to the <see cref="HttpServer"/> constructor
/// to configure the underlying <see cref="HttpListener"/> and the
/// request processing channel. When using dependency injection, register this
/// class with <c>IServiceCollection.AddOptions</c> to enable automatic
/// validation at startup.
/// </para>
/// </remarks>
public class HttpServerOptions : Abstractions.HttpServerOptions
{
    /// <summary>
    /// The default shutdown timeout used when <see cref="ShutdownTimeout"/> is
    /// not explicitly set.
    /// </summary>
    public static readonly TimeSpan DefaultShutdownTimeout = TimeSpan.FromSeconds(5);

    /// <summary>
    /// The exception message used when <see cref="Abstractions.HttpServerOptions.ChannelCapacity"/>
    /// is less than or equal to zero.
    /// </summary>
    internal static readonly string InvalidChannelCapacityMessage =
        "ChannelCapacity must be greater than zero.";

    /// <summary>
    /// The exception message used when <see cref="ShutdownTimeout"/> is negative.
    /// </summary>
    internal static readonly string InvalidShutdownTimeoutMessage =
        "ShutdownTimeout must be a non-negative duration.";

    /// <summary>
    /// The exception message used when <see cref="Realm"/> is null or empty
    /// but <see cref="AuthenticationSchemes"/> requires it.
    /// </summary>
    internal static readonly string RealmRequiredMessage =
        "Realm must be provided when AuthenticationSchemes includes Basic or Digest.";

    /// <summary>
    /// Gets or sets the authentication schemes the server will challenge
    /// clients with. Defaults to <see cref="AuthenticationSchemes.Anonymous"/>.
    /// </summary>
    public AuthenticationSchemes AuthenticationSchemes { get; set; }
        = AuthenticationSchemes.Anonymous;

    /// <summary>
    /// Gets or sets a delegate that selects the authentication scheme for
    /// each individual request, overriding <see cref="AuthenticationSchemes"/>
    /// when set.
    /// </summary>
    /// <remarks>
    /// Use this when different routes require different authentication schemes.
    /// When <see langword="null"/>, <see cref="AuthenticationSchemes"/> applies
    /// to all requests.
    /// </remarks>
    public AuthenticationSchemeSelector? AuthenticationSchemeSelectorDelegate { get; set; }

    /// <summary>
    /// Gets or sets the realm sent to clients in Basic and Digest
    /// authentication challenges.
    /// </summary>
    /// <remarks>
    /// Required when <see cref="AuthenticationSchemes"/> includes
    /// <see cref="AuthenticationSchemes.Basic"/> or
    /// <see cref="AuthenticationSchemes.Digest"/>. Ignored for all other
    /// authentication schemes.
    /// </remarks>
    public string? Realm { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether NTLM authentication credentials
    /// are reused across multiple requests on the same connection.
    /// </summary>
    /// <remarks>
    /// Enabling this improves NTLM performance but reduces security by
    /// allowing connection-level credential reuse. Only relevant when
    /// <see cref="AuthenticationSchemes"/> includes
    /// <see cref="AuthenticationSchemes.Ntlm"/>. Defaults to
    /// <see langword="false"/>.
    /// </remarks>
    public bool UnsafeConnectionNtlmAuthentication { get; set; } = false;

    /// <summary>
    /// Gets or sets the maximum time to wait for in-flight requests to complete
    /// when the server is stopping. Defaults to <see cref="DefaultShutdownTimeout"/>.
    /// </summary>
    /// <remarks>
    /// After this duration elapses, the server stops regardless of any remaining
    /// in-flight requests. Set to <see cref="System.Threading.Timeout.InfiniteTimeSpan"/>
    /// to wait indefinitely.
    /// </remarks>
    public TimeSpan ShutdownTimeout { get; set; } = DefaultShutdownTimeout;

    /// <inheritdoc/>
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (ChannelCapacity <= 0)
            yield return new ValidationResult(
                InvalidChannelCapacityMessage,
                [nameof(ChannelCapacity)]);

        if (ShutdownTimeout < TimeSpan.Zero)
            yield return new ValidationResult(
                InvalidShutdownTimeoutMessage,
                [nameof(ShutdownTimeout)]);

        var requiresRealm =
            AuthenticationSchemes.HasFlag(AuthenticationSchemes.Basic) ||
            AuthenticationSchemes.HasFlag(AuthenticationSchemes.Digest);

        if (requiresRealm && string.IsNullOrWhiteSpace(Realm))
            yield return new ValidationResult(
                RealmRequiredMessage,
                [nameof(Realm)]);
    }
}