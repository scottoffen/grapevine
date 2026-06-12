namespace Grapevine;

/// <summary>
/// Specifies the subtype of a multipart <c>Content-Type</c> header.
/// </summary>
public enum Multipart
{
    /// <summary>
    /// A mixed set of body parts with no particular relationship between them.
    /// The default multipart subtype when no other applies.
    /// </summary>
    Mixed,

    /// <summary>
    /// Multiple representations of the same content in different formats,
    /// such as plain text and HTML versions of the same email body.
    /// </summary>
    Alternative,

    /// <summary>
    /// A digest of multiple messages, typically used to bundle email messages together.
    /// Defaults each part to <c>message/rfc822</c> rather than <c>text/plain</c>.
    /// </summary>
    Digest,

    /// <summary>
    /// An encrypted body part, used in conjunction with a security protocol
    /// such as S/MIME or PGP.
    /// </summary>
    Encrypted,

    /// <summary>
    /// Form data submitted via an HTML form with <c>enctype="multipart/form-data"</c>,
    /// typically containing file uploads and field values as separate parts.
    /// </summary>
    FormData,

    /// <summary>
    /// Body parts that are related to one another, such as an HTML document and the
    /// inline images it references. The root part is typically the first part.
    /// </summary>
    Related,

    /// <summary>
    /// A digitally signed body part, used in conjunction with a security protocol
    /// such as S/MIME or PGP.
    /// </summary>
    Signed,

    /// <summary>
    /// Body parts intended to be processed in parallel rather than sequentially.
    /// Rarely used in practice.
    /// </summary>
    Parallel
}