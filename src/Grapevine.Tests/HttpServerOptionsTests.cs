using System.ComponentModel.DataAnnotations;
using System.Net;

namespace Grapevine.Tests;

public class HttpServerOptionsTests
{
    private static HttpServerOptions CreateOptions() => new();

    private static IList<ValidationResult> Validate(HttpServerOptions options)
    {
        var results = new List<ValidationResult>();
        var context = new ValidationContext(options);
        Validator.TryValidateObject(options, context, results, validateAllProperties: true);

        // IValidatableObject.Validate is not called by Validator.TryValidateObject
        // unless the object passes all data annotation validation first. Call it
        // explicitly to ensure all validation rules are exercised in tests.
        results.AddRange(options.Validate(context));
        return results;
    }

    public class Defaults
    {
        [Fact]
        public void ChannelCapacity_DefaultsToDefaultChannelCapacity()
        {
            var sut = CreateOptions();
            sut.ChannelCapacity.ShouldBe(Abstractions.HttpServerOptions.DefaultChannelCapacity);
        }

        [Fact]
        public void Prefixes_DefaultsToEmpty()
        {
            var sut = CreateOptions();
            sut.Prefixes.ShouldBeEmpty();
        }

        [Fact]
        public void IgnoreWriteExceptions_DefaultsToTrue()
        {
            var sut = CreateOptions();
            sut.IgnoreWriteExceptions.ShouldBeTrue();
        }

        [Fact]
        public void AuthenticationSchemes_DefaultsToAnonymous()
        {
            var sut = CreateOptions();
            sut.AuthenticationSchemes.ShouldBe(AuthenticationSchemes.Anonymous);
        }

        [Fact]
        public void AuthenticationSchemeSelectorDelegate_DefaultsToNull()
        {
            var sut = CreateOptions();
            sut.AuthenticationSchemeSelectorDelegate.ShouldBeNull();
        }

        [Fact]
        public void Realm_DefaultsToNull()
        {
            var sut = CreateOptions();
            sut.Realm.ShouldBeNull();
        }

        [Fact]
        public void UnsafeConnectionNtlmAuthentication_DefaultsToFalse()
        {
            var sut = CreateOptions();
            sut.UnsafeConnectionNtlmAuthentication.ShouldBeFalse();
        }

        [Fact]
        public void ShutdownTimeout_DefaultsToDefaultShutdownTimeout()
        {
            var sut = CreateOptions();
            sut.ShutdownTimeout.ShouldBe(HttpServerOptions.DefaultShutdownTimeout);
        }
    }

    public class ValidateMethod
    {
        [Fact]
        public void ReturnsNoErrors_WhenOptionsAreValid()
        {
            var sut = CreateOptions();
            Validate(sut).ShouldBeEmpty();
        }

        [Fact]
        public void ReturnsError_WhenChannelCapacityIsZero()
        {
            var sut = CreateOptions();
            sut.ChannelCapacity = 0;

            var results = Validate(sut);
            results.ShouldContain(r =>
                r.ErrorMessage == HttpServerOptions.InvalidChannelCapacityMessage &&
                r.MemberNames.Contains(nameof(HttpServerOptions.ChannelCapacity)));
        }

        [Fact]
        public void ReturnsError_WhenChannelCapacityIsNegative()
        {
            var sut = CreateOptions();
            sut.ChannelCapacity = -1;

            var results = Validate(sut);
            results.ShouldContain(r =>
                r.MemberNames.Contains(nameof(HttpServerOptions.ChannelCapacity)));
        }

        [Fact]
        public void ReturnsNoError_WhenChannelCapacityIsOne()
        {
            var sut = CreateOptions();
            sut.ChannelCapacity = 1;
            Validate(sut).ShouldBeEmpty();
        }

        [Fact]
        public void ReturnsError_WhenBasicAuthAndRealmIsNull()
        {
            var sut = CreateOptions();
            sut.AuthenticationSchemes = AuthenticationSchemes.Basic;
            sut.Realm = null;

            var results = Validate(sut);
            results.ShouldContain(r =>
                r.ErrorMessage == HttpServerOptions.RealmRequiredMessage &&
                r.MemberNames.Contains(nameof(HttpServerOptions.Realm)));
        }

        [Fact]
        public void ReturnsError_WhenDigestAuthAndRealmIsNull()
        {
            var sut = CreateOptions();
            sut.AuthenticationSchemes = AuthenticationSchemes.Digest;
            sut.Realm = null;

            var results = Validate(sut);
            results.ShouldContain(r =>
                r.ErrorMessage == HttpServerOptions.RealmRequiredMessage &&
                r.MemberNames.Contains(nameof(HttpServerOptions.Realm)));
        }

        [Fact]
        public void ReturnsError_WhenBasicAuthAndRealmIsEmpty()
        {
            var sut = CreateOptions();
            sut.AuthenticationSchemes = AuthenticationSchemes.Basic;
            sut.Realm = string.Empty;

            var results = Validate(sut);
            results.ShouldContain(r => r.ErrorMessage == HttpServerOptions.RealmRequiredMessage);
        }

        [Fact]
        public void ReturnsNoError_WhenBasicAuthAndRealmIsProvided()
        {
            var sut = CreateOptions();
            sut.AuthenticationSchemes = AuthenticationSchemes.Basic;
            sut.Realm = "MyRealm";

            Validate(sut).ShouldBeEmpty();
        }

        [Fact]
        public void ReturnsNoError_WhenDigestAuthAndRealmIsProvided()
        {
            var sut = CreateOptions();
            sut.AuthenticationSchemes = AuthenticationSchemes.Digest;
            sut.Realm = "MyRealm";

            Validate(sut).ShouldBeEmpty();
        }

        [Fact]
        public void ReturnsNoError_WhenNtlmAuthAndRealmIsNull()
        {
            var sut = CreateOptions();
            sut.AuthenticationSchemes = AuthenticationSchemes.Ntlm;
            sut.Realm = null;

            Validate(sut).ShouldBeEmpty();
        }

        [Fact]
        public void ReturnsNoError_WhenAnonymousAuthAndRealmIsNull()
        {
            var sut = CreateOptions();
            sut.AuthenticationSchemes = AuthenticationSchemes.Anonymous;
            sut.Realm = null;

            Validate(sut).ShouldBeEmpty();
        }

        [Fact]
        public void ReturnsNoError_WhenShutdownTimeoutIsZero()
        {
            var sut = CreateOptions();
            sut.ShutdownTimeout = TimeSpan.Zero;
            Validate(sut).ShouldBeEmpty();
        }

        [Fact]
        public void ReturnsError_WhenShutdownTimeoutIsNegative()
        {
            var sut = CreateOptions();
            sut.ShutdownTimeout = TimeSpan.FromSeconds(-1);

            var results = Validate(sut);
            results.ShouldContain(r =>
                r.ErrorMessage == HttpServerOptions.InvalidShutdownTimeoutMessage &&
                r.MemberNames.Contains(nameof(HttpServerOptions.ShutdownTimeout)));
        }

        [Fact]
        public void ReturnsBothErrors_WhenChannelCapacityAndRealmAreInvalid()
        {
            var sut = CreateOptions();
            sut.ChannelCapacity = 0;
            sut.AuthenticationSchemes = AuthenticationSchemes.Basic;
            sut.Realm = null;

            var results = Validate(sut);
            results.Count.ShouldBe(2);
        }
    }
}