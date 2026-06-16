#if NETSTANDARD2_0

// These nullable flow analysis attributes were introduced in .NET Core 3.0 and
// netstandard2.1. On netstandard2.0 the types do not exist in the BCL, so we
// define them here to allow the same source to compile across all target frameworks.
// The compiler recognizes these by name and namespace regardless of which assembly
// they originate from.

namespace System.Diagnostics.CodeAnalysis;

/// <summary>
/// Specifies that when a method returns <see cref="ReturnValue"/>, the parameter
/// will not be <see langword="null"/> even if the corresponding type allows it.
/// </summary>
[AttributeUsage(AttributeTargets.Parameter, Inherited = false)]
internal sealed class NotNullWhenAttribute : Attribute
{
    /// <summary>
    /// Gets the return value condition. If the method returns this value, the
    /// associated parameter will not be <see langword="null"/>.
    /// </summary>
    public bool ReturnValue { get; }

    /// <summary>
    /// Initializes the attribute with the specified return value condition.
    /// </summary>
    /// <param name="returnValue">
    /// The return value condition. If the method returns this value, the
    /// associated parameter will not be <see langword="null"/>.
    /// </param>
    public NotNullWhenAttribute(bool returnValue) => ReturnValue = returnValue;
}

/// <summary>
/// Specifies that the output will be non-<see langword="null"/> if the named
/// parameter is non-<see langword="null"/>.
/// </summary>
[AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Property | AttributeTargets.ReturnValue, AllowMultiple = true, Inherited = false)]
internal sealed class NotNullIfNotNullAttribute : Attribute
{
    /// <summary>
    /// Gets the name of the parameter whose non-nullness determines the non-nullness
    /// of this output.
    /// </summary>
    public string ParameterName { get; }

    /// <summary>
    /// Initializes the attribute with the associated parameter name.
    /// </summary>
    /// <param name="parameterName">
    /// The parameter name. The output will be non-<see langword="null"/> if the
    /// argument to the associated parameter is non-<see langword="null"/>.
    /// </param>
    public NotNullIfNotNullAttribute(string parameterName) => ParameterName = parameterName;
}

/// <summary>
/// Specifies that the method will not return if the associated boolean parameter
/// is <see langword="false"/>.
/// </summary>
[AttributeUsage(AttributeTargets.Parameter, Inherited = false)]
internal sealed class DoesNotReturnIfAttribute : Attribute
{
    /// <summary>
    /// Gets the condition parameter value. Code after the method will be considered
    /// unreachable by diagnostics if the argument to the associated parameter matches
    /// this value.
    /// </summary>
    public bool ParameterValue { get; }

    /// <summary>
    /// Initializes the attribute with the specified parameter value.
    /// </summary>
    /// <param name="parameterValue">
    /// The condition parameter value. Code after the method will be considered
    /// unreachable by diagnostics if the argument to the associated parameter
    /// matches this value.
    /// </param>
    public DoesNotReturnIfAttribute(bool parameterValue) => ParameterValue = parameterValue;
}

/// <summary>
/// Specifies that the method never returns normally. Code after a call to a
/// method annotated with this attribute is considered unreachable.
/// </summary>
[AttributeUsage(AttributeTargets.Method, Inherited = false)]
internal sealed class DoesNotReturnAttribute : Attribute { }

/// <summary>
/// Specifies that the method or property will ensure that the listed field and
/// property members have non-<see langword="null"/> values when returning with
/// the specified return value condition.
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Property, Inherited = false, AllowMultiple = true)]
internal sealed class MemberNotNullWhenAttribute : Attribute
{
    /// <summary>
    /// Gets the return value condition.
    /// </summary>
    public bool ReturnValue { get; }

    /// <summary>
    /// Gets field or property member names.
    /// </summary>
    public string[] Members { get; }

    /// <summary>
    /// Initializes the attribute with the specified return value condition and member names.
    /// </summary>
    /// <param name="returnValue">
    /// The return value condition. If the method returns this value, the members
    /// listed will not be <see langword="null"/>.
    /// </param>
    /// <param name="members">
    /// The field or property member names that will be non-<see langword="null"/>
    /// if the method returns with the specified return value.
    /// </param>
    public MemberNotNullWhenAttribute(bool returnValue, params string[] members)
    {
        ReturnValue = returnValue;
        Members = members;
    }
}

#endif