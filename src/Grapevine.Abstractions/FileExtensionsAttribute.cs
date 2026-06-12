using System.Diagnostics.CodeAnalysis;

namespace Grapevine;

/// <summary>
/// Specifies the file extensions associated with a <see cref="ContentType"/> static field,
/// used during static initialization to populate the file extension registry.
/// </summary>
/// <remarks>
/// This attribute is intended for use on public static fields of <see cref="ContentType"/>
/// only. Extensions should be provided without a leading dot and in lowercase.
/// </remarks>
[ExcludeFromCodeCoverage]
[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
internal sealed class FileExtensionsAttribute : Attribute
{
    /// <summary>
    /// Gets the file extensions associated with the <see cref="ContentType"/>.
    /// </summary>
    public string[] Extensions { get; }

    /// <summary>
    /// Initializes a new instance of <see cref="FileExtensionsAttribute"/> with
    /// the specified file extensions.
    /// </summary>
    /// <param name="extensions">
    /// One or more file extensions to associate with the <see cref="ContentType"/>,
    /// without a leading dot and in lowercase (e.g. <c>"html"</c>, <c>"htm"</c>).
    /// </param>
    public FileExtensionsAttribute(params string[] extensions)
    {
        Extensions = extensions;
    }
}