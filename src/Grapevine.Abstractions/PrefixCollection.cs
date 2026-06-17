using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace Grapevine;

/// <summary>
/// Represents a collection of URI prefixes that an HTTP server listens on.
/// </summary>
/// <remarks>
/// <para>
/// Prefixes are stored in a case-insensitive set, so <c>http://localhost:8080/</c>
/// and <c>http://Localhost:8080/</c> are treated as the same prefix.
/// </para>
/// <para>
/// Each prefix is validated by <see cref="IsValidPrefix"/> before being added.
/// A valid prefix must begin with <c>http://</c> or <c>https://</c> and end
/// with a forward slash, matching the requirements of <see cref="System.Net.HttpListener"/>.
/// Use <see cref="TryAdd"/> to add a prefix without risking an exception on
/// invalid input.
/// </para>
/// <para>
/// The collection is sealed by the server infrastructure when the server starts
/// and unsealed when the server stops. While sealed, all mutation operations
/// throw <see cref="InvalidOperationException"/>. <see cref="Seal"/> and
/// <see cref="Unseal"/> are intended to be called by <see cref="IHttpServer"/>
/// implementations only.
/// </para>
/// </remarks>
[DebuggerDisplay("Count = {Count}, IsSealed = {IsSealed}")]
public class PrefixCollection : ICollection<string>
{
    private static readonly Regex PrefixRegex = new(
        @"^https?://[^/]+(/.*)?/$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase
    );

    /// <summary>
    /// The exception message used when a prefix fails validation.
    /// </summary>
    internal static readonly string InvalidPrefixMessage =
        "The prefix '{0}' is not a valid HttpListener prefix. Prefixes must begin with http:// or https:// and end with a forward slash.";

    /// <summary>
    /// The exception message used when a mutation is attempted on a sealed collection.
    /// </summary>
    internal static readonly string SealedCollectionMessage =
        "The prefix collection has been sealed and cannot be modified while the server is running.";

    private readonly HashSet<string> _prefixes =
        new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Gets a value indicating whether the collection has been sealed.
    /// </summary>
    /// <remarks>
    /// When <see langword="true"/>, all mutation operations throw
    /// <see cref="InvalidOperationException"/>. The collection is sealed by
    /// the server when it starts and unsealed when it stops.
    /// </remarks>
    public bool IsSealed { get; private set; }

    /// <summary>
    /// Gets the number of prefixes in the collection.
    /// </summary>
    public int Count => _prefixes.Count;

    /// <summary>
    /// Gets a value indicating whether the collection is read-only.
    /// </summary>
    /// <remarks>
    /// Returns <see langword="true"/> when the collection is sealed,
    /// satisfying the <see cref="ICollection{T}"/> contract.
    /// </remarks>
    public bool IsReadOnly => IsSealed;

    /// <summary>
    /// Seals the collection, preventing any further modifications until
    /// <see cref="Unseal"/> is called.
    /// </summary>
    /// <remarks>
    /// Intended to be called by <see cref="IHttpServer"/> implementations
    /// when the server starts. Calling <see cref="Seal"/> more than once
    /// has no effect.
    /// </remarks>
    public void Seal()
    {
        IsSealed = true;
    }

    /// <summary>
    /// Unseals the collection, allowing modifications to be made.
    /// </summary>
    /// <remarks>
    /// Intended to be called by <see cref="IHttpServer"/> implementations
    /// when the server stops. Calling <see cref="Unseal"/> more than once
    /// has no effect.
    /// </remarks>
    public void Unseal()
    {
        IsSealed = false;
    }

    /// <summary>
    /// Adds a prefix to the collection after validating it against
    /// <see cref="IsValidPrefix"/>.
    /// </summary>
    /// <param name="prefix">The URI prefix to add.</param>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="prefix"/> is not a valid
    /// <see cref="System.Net.HttpListener"/> prefix.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the collection is sealed.
    /// </exception>
    public void Add(string prefix)
    {
        ThrowIfSealed();

        if (!IsValidPrefix(prefix))
            throw new ArgumentException(
                string.Format(InvalidPrefixMessage, prefix), nameof(prefix));

        _prefixes.Add(prefix);
    }

    /// <summary>
    /// Removes all prefixes from the collection.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the collection is sealed.
    /// </exception>
    public void Clear()
    {
        ThrowIfSealed();
        _prefixes.Clear();
    }

    /// <summary>
    /// Determines whether the collection contains the specified prefix.
    /// Comparison is case-insensitive.
    /// </summary>
    /// <param name="prefix">The prefix to locate.</param>
    /// <returns>
    /// <see langword="true"/> if the prefix is found; otherwise
    /// <see langword="false"/>.
    /// </returns>
    public bool Contains(string prefix) => _prefixes.Contains(prefix);

    /// <summary>
    /// Copies the prefixes in the collection to an array, starting at the
    /// specified array index.
    /// </summary>
    /// <param name="array">The destination array.</param>
    /// <param name="arrayIndex">
    /// The zero-based index in <paramref name="array"/> at which copying begins.
    /// </param>
    public void CopyTo(string[] array, int arrayIndex) =>
        _prefixes.CopyTo(array, arrayIndex);

    /// <summary>
    /// Returns an enumerator that iterates through the prefixes in the collection.
    /// </summary>
    public IEnumerator<string> GetEnumerator() => _prefixes.GetEnumerator();

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <summary>
    /// Removes the specified prefix from the collection.
    /// </summary>
    /// <param name="prefix">The prefix to remove.</param>
    /// <returns>
    /// <see langword="true"/> if the prefix was found and removed; otherwise
    /// <see langword="false"/>.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the collection is sealed.
    /// </exception>
    public bool Remove(string prefix)
    {
        ThrowIfSealed();
        return _prefixes.Remove(prefix);
    }

    /// <summary>
    /// Attempts to add a prefix to the collection without throwing on invalid
    /// input.
    /// </summary>
    /// <param name="prefix">The URI prefix to add.</param>
    /// <returns>
    /// <see langword="true"/> if the prefix is valid and was added; otherwise
    /// <see langword="false"/>.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the collection is sealed.
    /// </exception>
    public bool TryAdd(string prefix)
    {
        ThrowIfSealed();
        if (!IsValidPrefix(prefix)) return false;
        _prefixes.Add(prefix);
        return true;
    }

    /// <summary>
    /// Determines whether the specified prefix is valid for use with
    /// <see cref="System.Net.HttpListener"/>.
    /// </summary>
    /// <remarks>
    /// A valid prefix must not be null or empty, must begin with
    /// <c>http://</c> or <c>https://</c>, and must end with a forward slash.
    /// </remarks>
    /// <param name="prefix">The prefix to validate.</param>
    /// <returns>
    /// <see langword="true"/> if the prefix is a well-formed
    /// <see cref="System.Net.HttpListener"/> prefix; otherwise
    /// <see langword="false"/>.
    /// </returns>
    public static bool IsValidPrefix(string prefix)
    {
        return !string.IsNullOrWhiteSpace(prefix) && PrefixRegex.IsMatch(prefix);
    }

    /// <summary>
    /// Throws <see cref="InvalidOperationException"/> if the collection is sealed.
    /// </summary>
    private void ThrowIfSealed()
    {
        if (IsSealed)
            throw new InvalidOperationException(SealedCollectionMessage);
    }
}