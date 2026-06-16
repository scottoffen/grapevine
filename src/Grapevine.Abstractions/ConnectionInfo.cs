using System.Diagnostics;
using System.Globalization;

namespace Grapevine;

/// <summary>
/// Represents a network endpoint as an IP address or hostname and a port number.
/// </summary>
/// <remarks>
/// This type is used to represent both the local and remote endpoints of an HTTP
/// connection. Use <see cref="TryCreate"/> to construct an instance from untrusted
/// input without risking an exception on the hot path.
/// </remarks>
[DebuggerDisplay("{Address}:{Port}")]
public readonly record struct ConnectionInfo
{
    /// <summary>
    /// The minimum valid port number, as defined by the TCP/IP specification.
    /// </summary>
    public const int MinPort = 0;

    /// <summary>
    /// The maximum valid port number, as defined by the TCP/IP specification.
    /// </summary>
    public const int MaxPort = 65535;

    internal static readonly string AddressNullOrEmptyMessage = "Address cannot be null or empty.";
    internal static readonly string AddressInvalidCharactersMessage = "Address contains invalid characters.";
    internal static readonly string PortOutOfRangeMessage = $"Port must be between {MinPort} and {MaxPort}.";

    /// <summary>
    /// Gets the IP address or hostname of the endpoint.
    /// </summary>
    public string Address { get; init; }

    /// <summary>
    /// Gets the port number of the endpoint.
    /// </summary>
    public int Port { get; init; }

    /// <summary>
    /// Initializes a new instance of <see cref="ConnectionInfo"/> with the
    /// specified address and port.
    /// </summary>
    /// <param name="address">The IP address or hostname of the endpoint.</param>
    /// <param name="port">The port number of the endpoint.</param>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="address"/> is null, empty, or contains invalid
    /// characters.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="port"/> is less than <see cref="MinPort"/> or
    /// greater than <see cref="MaxPort"/>.
    /// </exception>
    public ConnectionInfo(string address, int port)
    {
        if (string.IsNullOrWhiteSpace(address))
            throw new ArgumentException(AddressNullOrEmptyMessage, nameof(address));

        if (!IsValidAddress(address))
            throw new ArgumentException(AddressInvalidCharactersMessage, nameof(address));

        if (port < MinPort || port > MaxPort)
            throw new ArgumentOutOfRangeException(nameof(port), PortOutOfRangeMessage);

        Address = address;
        Port = port;
    }

    /// <summary>
    /// Attempts to create a <see cref="ConnectionInfo"/> from the specified address
    /// and port without throwing on invalid input.
    /// </summary>
    /// <param name="address">The IP address or hostname of the endpoint.</param>
    /// <param name="port">The port number of the endpoint.</param>
    /// <param name="result">
    /// When this method returns <see langword="true"/>, contains the constructed
    /// <see cref="ConnectionInfo"/>. When this method returns <see langword="false"/>,
    /// contains the default value.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the address and port are valid and the instance was
    /// created successfully; otherwise, <see langword="false"/>.
    /// </returns>
    public static bool TryCreate(string address, int port, out ConnectionInfo result)
    {
        try
        {
            result = new ConnectionInfo(address, port);
            return true;
        }
        catch
        {
            result = default;
            return false;
        }
    }

    /// <summary>
    /// Returns a string representation of this endpoint in the form
    /// <c>address:port</c>.
    /// </summary>
    /// <returns>A string in the form <c>address:port</c>.</returns>
    public override string ToString()
    {
        return string.Format(CultureInfo.InvariantCulture, "{0}:{1}", Address, Port);
    }

    /// <summary>
    /// Returns a value indicating whether all characters in the specified address
    /// string are valid for an IP address or hostname.
    /// </summary>
    /// <remarks>
    /// Valid characters are letters, digits, hyphens, periods, underscores, colons
    /// (for IPv6 segments), and square brackets (for bracketed IPv6 literals such as
    /// <c>[::1]</c>).
    /// </remarks>
    /// <param name="address">The address string to validate.</param>
    /// <returns>
    /// <see langword="true"/> if all characters in <paramref name="address"/> are
    /// valid; otherwise, <see langword="false"/>.
    /// </returns>
    private static bool IsValidAddress(string address)
    {
        foreach (var c in address)
        {
            if (char.IsLetterOrDigit(c)) continue;

            // Allow punctuation valid in IPv4, IPv6, and hostnames:
            // hyphen and period for hostnames and IPv4
            // colon for IPv6 segments
            // square brackets for bracketed IPv6 literals e.g. [::1]
            // underscore for non-standard but commonly seen hostnames
            switch (c)
            {
                case '-':
                case '.':
                case '_':
                case ':':
                case '[':
                case ']':
                    continue;
            }

            return false;
        }

        return true;
    }
}