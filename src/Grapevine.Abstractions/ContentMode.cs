namespace Grapevine;

/// <summary>
/// Specifies how the content payload should be read from or written to a stream.
/// </summary>
public enum ContentMode
{
    /// <summary>
    /// The content is treated as raw bytes and read from or written to the stream
    /// as a binary payload. Use for images, audio, video, compressed files, and
    /// any other non-text content.
    /// </summary>
    Binary,

    /// <summary>
    /// The content is treated as human-readable text and read from or written to
    /// the stream using a text encoding such as UTF-8. Use for HTML, JSON, XML,
    /// CSS, JavaScript, and other text-based formats.
    /// </summary>
    Text
}