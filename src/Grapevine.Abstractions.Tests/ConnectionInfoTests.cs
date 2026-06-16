using Shouldly;
using Xunit;

namespace Grapevine.Abstractions.Tests;

public class ConnectionInfoTests
{
    // Common valid test values shared across nested test classes.
    private static readonly string ValidAddress = "192.168.1.1";
    private static readonly int ValidPort = 8080;

    public class Constructor
    {
        [Fact]
        public void AssignsAddress_WhenArgumentsAreValid()
        {
            var sut = new ConnectionInfo(ValidAddress, ValidPort);
            sut.Address.ShouldBe(ValidAddress);
        }

        [Fact]
        public void AssignsPort_WhenArgumentsAreValid()
        {
            var sut = new ConnectionInfo(ValidAddress, ValidPort);
            sut.Port.ShouldBe(ValidPort);
        }

        [Fact]
        public void ThrowsArgumentException_WhenAddressIsNull()
        {
            var ex = Should.Throw<ArgumentException>(() => new ConnectionInfo(null!, ValidPort));
            ex.Message.ShouldContain(ConnectionInfo.AddressNullOrEmptyMessage);
            ex.ParamName.ShouldBe("address");
        }

        [Fact]
        public void ThrowsArgumentException_WhenAddressIsEmpty()
        {
            var ex = Should.Throw<ArgumentException>(() => new ConnectionInfo(string.Empty, ValidPort));
            ex.Message.ShouldContain(ConnectionInfo.AddressNullOrEmptyMessage);
            ex.ParamName.ShouldBe("address");
        }

        [Fact]
        public void ThrowsArgumentException_WhenAddressIsWhitespace()
        {
            var ex = Should.Throw<ArgumentException>(() => new ConnectionInfo("   ", ValidPort));
            ex.Message.ShouldContain(ConnectionInfo.AddressNullOrEmptyMessage);
            ex.ParamName.ShouldBe("address");
        }

        [Fact]
        public void ThrowsArgumentException_WhenAddressContainsInvalidCharacters()
        {
            var ex = Should.Throw<ArgumentException>(() => new ConnectionInfo("192.168.1.1 bad!", ValidPort));
            ex.Message.ShouldContain(ConnectionInfo.AddressInvalidCharactersMessage);
            ex.ParamName.ShouldBe("address");
        }

        [Fact]
        public void ThrowsArgumentOutOfRangeException_WhenPortIsBelowMinPort()
        {
            var ex = Should.Throw<ArgumentOutOfRangeException>(() => new ConnectionInfo(ValidAddress, ConnectionInfo.MinPort - 1));
            ex.Message.ShouldContain(ConnectionInfo.PortOutOfRangeMessage);
            ex.ParamName.ShouldBe("port");
        }

        [Fact]
        public void ThrowsArgumentOutOfRangeException_WhenPortIsAboveMaxPort()
        {
            var ex = Should.Throw<ArgumentOutOfRangeException>(() => new ConnectionInfo(ValidAddress, ConnectionInfo.MaxPort + 1));
            ex.Message.ShouldContain(ConnectionInfo.PortOutOfRangeMessage);
            ex.ParamName.ShouldBe("port");
        }

        [Fact]
        public void AcceptsMinPort()
        {
            var sut = new ConnectionInfo(ValidAddress, ConnectionInfo.MinPort);
            sut.Port.ShouldBe(ConnectionInfo.MinPort);
        }

        [Fact]
        public void AcceptsMaxPort()
        {
            var sut = new ConnectionInfo(ValidAddress, ConnectionInfo.MaxPort);
            sut.Port.ShouldBe(ConnectionInfo.MaxPort);
        }

        [Theory]
        [InlineData("192.168.1.1")]
        [InlineData("::1")]
        [InlineData("[::1]")]
        [InlineData("localhost")]
        [InlineData("my-host.example.com")]
        [InlineData("my_host")]
        public void AcceptsValidAddressFormats(string address)
        {
            var sut = new ConnectionInfo(address, ValidPort);
            sut.Address.ShouldBe(address);
        }
    }

    public class TryCreateMethod
    {
        [Fact]
        public void ReturnsTrue_WhenArgumentsAreValid()
        {
            var success = ConnectionInfo.TryCreate(ValidAddress, ValidPort, out _);
            success.ShouldBeTrue();
        }

        [Fact]
        public void SetsResult_WhenArgumentsAreValid()
        {
            ConnectionInfo.TryCreate(ValidAddress, ValidPort, out var result);
            result.Address.ShouldBe(ValidAddress);
            result.Port.ShouldBe(ValidPort);
        }

        [Fact]
        public void ReturnsFalse_WhenAddressIsNull()
        {
            var success = ConnectionInfo.TryCreate(null!, ValidPort, out _);
            success.ShouldBeFalse();
        }

        [Fact]
        public void SetsResultToDefault_WhenAddressIsNull()
        {
            ConnectionInfo.TryCreate(null!, ValidPort, out var result);
            result.ShouldBe(default(ConnectionInfo));
        }

        [Fact]
        public void ReturnsFalse_WhenAddressIsEmpty()
        {
            var success = ConnectionInfo.TryCreate(string.Empty, ValidPort, out _);
            success.ShouldBeFalse();
        }

        [Fact]
        public void ReturnsFalse_WhenAddressContainsInvalidCharacters()
        {
            var success = ConnectionInfo.TryCreate("bad address!", ValidPort, out _);
            success.ShouldBeFalse();
        }

        [Fact]
        public void ReturnsFalse_WhenPortIsBelowMinPort()
        {
            var success = ConnectionInfo.TryCreate(ValidAddress, ConnectionInfo.MinPort - 1, out _);
            success.ShouldBeFalse();
        }

        [Fact]
        public void ReturnsFalse_WhenPortIsAboveMaxPort()
        {
            var success = ConnectionInfo.TryCreate(ValidAddress, ConnectionInfo.MaxPort + 1, out _);
            success.ShouldBeFalse();
        }

        [Fact]
        public void DoesNotThrow_WhenArgumentsAreInvalid()
        {
            Should.NotThrow(() => ConnectionInfo.TryCreate(null!, -1, out _));
        }
    }

    public class ToStringMethod
    {
        [Fact]
        public void ReturnsAddressColonPort()
        {
            var sut = new ConnectionInfo(ValidAddress, ValidPort);
            sut.ToString().ShouldBe($"{ValidAddress}:{ValidPort}");
        }
    }

    public class ValueEquality
    {
        [Fact]
        public void TwoInstancesWithSameValues_AreEqual()
        {
            var a = new ConnectionInfo(ValidAddress, ValidPort);
            var b = new ConnectionInfo(ValidAddress, ValidPort);
            a.ShouldBe(b);
        }

        [Fact]
        public void TwoInstancesWithDifferentAddresses_AreNotEqual()
        {
            var a = new ConnectionInfo("10.0.0.1", ValidPort);
            var b = new ConnectionInfo("10.0.0.2", ValidPort);
            a.ShouldNotBe(b);
        }

        [Fact]
        public void TwoInstancesWithDifferentPorts_AreNotEqual()
        {
            var a = new ConnectionInfo(ValidAddress, 8080);
            var b = new ConnectionInfo(ValidAddress, 9090);
            a.ShouldNotBe(b);
        }

        [Fact]
        public void EqualityOperator_ReturnsTrueForEqualInstances()
        {
            var a = new ConnectionInfo(ValidAddress, ValidPort);
            var b = new ConnectionInfo(ValidAddress, ValidPort);
            (a == b).ShouldBeTrue();
        }

        [Fact]
        public void InequalityOperator_ReturnsTrueForDifferentInstances()
        {
            var a = new ConnectionInfo("10.0.0.1", ValidPort);
            var b = new ConnectionInfo("10.0.0.2", ValidPort);
            (a != b).ShouldBeTrue();
        }
    }
}