using System;
using System.Linq;
using Shouldly;
using Xunit;

namespace Grapevine.Tests;

public class PrefixCollectionTests
{
    private static PrefixCollection CreateCollection() => new();

    private static readonly string ValidPrefix = "http://localhost:8080/";
    private static readonly string ValidPrefixUppercase = "http://Localhost:8080/";
    private static readonly string ValidHttpsPrefix = "https://localhost:8443/";

    public class SealMethod
    {
        [Fact]
        public void SetsIsSealed_ToTrue()
        {
            var sut = CreateCollection();
            sut.Seal();
            sut.IsSealed.ShouldBeTrue();
        }

        [Fact]
        public void IsIdempotent()
        {
            var sut = CreateCollection();
            sut.Seal();
            Should.NotThrow(() => sut.Seal());
            sut.IsSealed.ShouldBeTrue();
        }
    }

    public class UnsealMethod
    {
        [Fact]
        public void SetsIsSealed_ToFalse()
        {
            var sut = CreateCollection();
            sut.Seal();
            sut.Unseal();
            sut.IsSealed.ShouldBeFalse();
        }

        [Fact]
        public void IsIdempotent()
        {
            var sut = CreateCollection();
            Should.NotThrow(() => sut.Unseal());
            sut.IsSealed.ShouldBeFalse();
        }

        [Fact]
        public void AllowsMutation_AfterUnseal()
        {
            var sut = CreateCollection();
            sut.Seal();
            sut.Unseal();
            Should.NotThrow(() => sut.Add(ValidPrefix));
        }
    }

    public class IsReadOnlyProperty
    {
        [Fact]
        public void ReturnsFalse_WhenNotSealed()
        {
            var sut = CreateCollection();
            sut.IsReadOnly.ShouldBeFalse();
        }

        [Fact]
        public void ReturnsTrue_WhenSealed()
        {
            var sut = CreateCollection();
            sut.Seal();
            sut.IsReadOnly.ShouldBeTrue();
        }
    }

    public class AddMethod
    {
        [Fact]
        public void AddsPrefix_WhenValid()
        {
            var sut = CreateCollection();
            sut.Add(ValidPrefix);
            sut.Contains(ValidPrefix).ShouldBeTrue();
        }

        [Fact]
        public void AddsHttpsPrefix_WhenValid()
        {
            var sut = CreateCollection();
            sut.Add(ValidHttpsPrefix);
            sut.Contains(ValidHttpsPrefix).ShouldBeTrue();
        }

        [Fact]
        public void ThrowsArgumentException_WhenPrefixIsInvalid()
        {
            var sut = CreateCollection();
            var ex = Should.Throw<ArgumentException>(() => sut.Add("not-a-valid-prefix"));
            ex.ParamName.ShouldBe("prefix");
            ex.Message.ShouldContain(string.Format(PrefixCollection.InvalidPrefixMessage, "not-a-valid-prefix"));
        }

        [Fact]
        public void ThrowsArgumentException_WhenPrefixHasNoTrailingSlash()
        {
            var sut = CreateCollection();
            Should.Throw<ArgumentException>(() => sut.Add("http://localhost:8080"));
        }

        [Fact]
        public void ThrowsArgumentException_WhenPrefixIsNull()
        {
            var sut = CreateCollection();
            Should.Throw<ArgumentException>(() => sut.Add(null!));
        }

        [Fact]
        public void ThrowsArgumentException_WhenPrefixIsEmpty()
        {
            var sut = CreateCollection();
            Should.Throw<ArgumentException>(() => sut.Add(string.Empty));
        }

        [Fact]
        public void ThrowsInvalidOperationException_WhenSealed()
        {
            var sut = CreateCollection();
            sut.Seal();
            var ex = Should.Throw<InvalidOperationException>(() => sut.Add(ValidPrefix));
            ex.Message.ShouldBe(PrefixCollection.SealedCollectionMessage);
        }

        [Fact]
        public void IsCaseInsensitive()
        {
            var sut = CreateCollection();
            sut.Add(ValidPrefix);
            sut.Add(ValidPrefixUppercase);
            sut.Count.ShouldBe(1);
        }
    }

    public class ClearMethod
    {
        [Fact]
        public void RemovesAllPrefixes()
        {
            var sut = CreateCollection();
            sut.Add(ValidPrefix);
            sut.Add(ValidHttpsPrefix);
            sut.Clear();
            sut.Count.ShouldBe(0);
        }

        [Fact]
        public void ThrowsInvalidOperationException_WhenSealed()
        {
            var sut = CreateCollection();
            sut.Add(ValidPrefix);
            sut.Seal();
            var ex = Should.Throw<InvalidOperationException>(() => sut.Clear());
            ex.Message.ShouldBe(PrefixCollection.SealedCollectionMessage);
        }
    }

    public class ContainsMethod
    {
        [Fact]
        public void ReturnsTrue_WhenPrefixExists()
        {
            var sut = CreateCollection();
            sut.Add(ValidPrefix);
            sut.Contains(ValidPrefix).ShouldBeTrue();
        }

        [Fact]
        public void ReturnsFalse_WhenPrefixDoesNotExist()
        {
            var sut = CreateCollection();
            sut.Contains(ValidPrefix).ShouldBeFalse();
        }

        [Fact]
        public void IsCaseInsensitive()
        {
            var sut = CreateCollection();
            sut.Add(ValidPrefix);
            sut.Contains(ValidPrefixUppercase).ShouldBeTrue();
        }
    }

    public class RemoveMethod
    {
        [Fact]
        public void ReturnsTrue_WhenPrefixExists()
        {
            var sut = CreateCollection();
            sut.Add(ValidPrefix);
            sut.Remove(ValidPrefix).ShouldBeTrue();
        }

        [Fact]
        public void ReturnsFalse_WhenPrefixDoesNotExist()
        {
            var sut = CreateCollection();
            sut.Remove(ValidPrefix).ShouldBeFalse();
        }

        [Fact]
        public void RemovesPrefix_FromCollection()
        {
            var sut = CreateCollection();
            sut.Add(ValidPrefix);
            sut.Remove(ValidPrefix);
            sut.Contains(ValidPrefix).ShouldBeFalse();
        }

        [Fact]
        public void ThrowsInvalidOperationException_WhenSealed()
        {
            var sut = CreateCollection();
            sut.Add(ValidPrefix);
            sut.Seal();
            var ex = Should.Throw<InvalidOperationException>(() => sut.Remove(ValidPrefix));
            ex.Message.ShouldBe(PrefixCollection.SealedCollectionMessage);
        }
    }

    public class TryAddMethod
    {
        [Fact]
        public void ReturnsTrue_WhenPrefixIsValid()
        {
            var sut = CreateCollection();
            sut.TryAdd(ValidPrefix).ShouldBeTrue();
        }

        [Fact]
        public void ReturnsFalse_WhenPrefixIsInvalid()
        {
            var sut = CreateCollection();
            sut.TryAdd("not-a-valid-prefix").ShouldBeFalse();
        }

        [Fact]
        public void AddsPrefix_WhenValid()
        {
            var sut = CreateCollection();
            sut.TryAdd(ValidPrefix);
            sut.Contains(ValidPrefix).ShouldBeTrue();
        }

        [Fact]
        public void DoesNotThrow_WhenPrefixIsInvalid()
        {
            var sut = CreateCollection();
            Should.NotThrow(() => sut.TryAdd("not-a-valid-prefix"));
        }

        [Fact]
        public void ThrowsInvalidOperationException_WhenSealed()
        {
            var sut = CreateCollection();
            sut.Seal();
            var ex = Should.Throw<InvalidOperationException>(() => sut.TryAdd(ValidPrefix));
            ex.Message.ShouldBe(PrefixCollection.SealedCollectionMessage);
        }
    }

    public class IsValidPrefixMethod
    {
        [Theory]
        [InlineData("http://localhost:8080/")]
        [InlineData("https://localhost:8443/")]
        [InlineData("http://localhost:8080/api/")]
        [InlineData("http://localhost:8080/api/v1/")]
        [InlineData("http://+:8080/")]
        [InlineData("http://*:8080/")]
        public void ReturnsTrue_ForValidPrefixes(string prefix)
        {
            PrefixCollection.IsValidPrefix(prefix).ShouldBeTrue();
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("http://localhost:8080")]
        [InlineData("ftp://localhost:8080/")]
        [InlineData("localhost:8080/")]
        [InlineData("http://")]
        public void ReturnsFalse_ForInvalidPrefixes(string prefix)
        {
            PrefixCollection.IsValidPrefix(prefix).ShouldBeFalse();
        }

        [Fact]
        public void ReturnsFalse_ForNullPrefix()
        {
            PrefixCollection.IsValidPrefix(null!).ShouldBeFalse();
        }
    }

    public class CopyToMethod
    {
        [Fact]
        public void CopiesPrefixes_ToArray()
        {
            var sut = CreateCollection();
            sut.Add(ValidPrefix);
            sut.Add(ValidHttpsPrefix);

            var array = new string[2];
            sut.CopyTo(array, 0);

            array.ShouldContain(ValidPrefix);
            array.ShouldContain(ValidHttpsPrefix);
        }
    }

    public class EnumeratorMethod
    {
        [Fact]
        public void EnumeratesAllPrefixes()
        {
            var sut = CreateCollection();
            sut.Add(ValidPrefix);
            sut.Add(ValidHttpsPrefix);

            var prefixes = sut.ToList();
            prefixes.Count.ShouldBe(2);
            prefixes.ShouldContain(ValidPrefix);
            prefixes.ShouldContain(ValidHttpsPrefix);
        }
    }
}