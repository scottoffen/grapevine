namespace Grapevine;

/// <summary>
/// Specifies the value of the <c>SameSite</c> attribute on a <c>Set-Cookie</c> header,
/// controlling whether the cookie is sent with cross-site requests.
/// </summary>
/// <remarks>
/// <para>
/// The <c>SameSite</c> attribute is a defence-in-depth measure against cross-site
/// request forgery (CSRF). Browser support and enforcement behaviour varies across
/// versions; consult the MDN documentation for current compatibility notes.
/// </para>
/// <para>
/// When <see cref="None"/> is used, the cookie must also have the <c>Secure</c>
/// attribute set, or modern browsers will reject it.
/// </para>
/// </remarks>
public enum SameSiteMode
{
    /// <summary>
    /// The cookie is sent with all requests, including cross-site requests.
    /// Requires the <c>Secure</c> attribute to be set or the cookie will be
    /// rejected by modern browsers.
    /// </summary>
    None,

    /// <summary>
    /// The cookie is sent with same-site requests and with cross-site top-level
    /// navigations (e.g. following a link). It is not sent with cross-site
    /// sub-requests such as images or frames.
    /// </summary>
    Lax,

    /// <summary>
    /// The cookie is sent only with same-site requests. It is never sent with
    /// cross-site requests regardless of the request type.
    /// </summary>
    Strict
}