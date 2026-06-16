namespace Grapevine.Abstractions.Tests;

public class StringParamsExtensionsTests
{
    private static IStringParams Parse(string raw) => QueryParams.Parse(raw);

    public class TryGetValueMethod
    {
        [Fact]
        public void Throws_WhenQueryParamsIsNull()
        {
            Should.Throw<ArgumentNullException>(() =>
                ((IStringParams)null!).TryGetValue<string>("key", out _));
        }

        [Fact]
        public void Throws_WhenNoConverterRegistered()
        {
            // Use a type that is definitely not registered.
            Should.Throw<InvalidOperationException>(() =>
                Parse($"key={Guid.NewGuid()}").TryGetValue<int>("key", out _));
        }

        [Fact]
        public void ReturnsFalse_WhenKeyDoesNotExist()
        {
            Parse($"other={Guid.NewGuid()}")
                .TryGetValue<string>("missing", out _).ShouldBeFalse();
        }

        [Fact]
        public void SetsValueToDefault_WhenKeyDoesNotExist()
        {
            Parse($"other={Guid.NewGuid()}")
                .TryGetValue<string>("missing", out var value);
            value.ShouldBeNull();
        }

        [Fact]
        public void ReturnsFalse_WhenConversionFails()
        {
            Parse("page=not-a-number")
                .TryGetValue<int?>("page", out _).ShouldBeFalse();
        }

        [Fact]
        public void SetsValueToDefault_WhenConversionFails()
        {
            Parse("page=not-a-number")
                .TryGetValue<int?>("page", out var value);
            value.ShouldBeNull();
        }

        [Fact]
        public void ReturnsTrue_WhenKeyExistsAndConversionSucceeds()
        {
            Parse("page=3").TryGetValue<int?>("page", out _).ShouldBeTrue();
        }

        [Fact]
        public void SetsValue_WhenKeyExistsAndConversionSucceeds()
        {
            Parse("page=3").TryGetValue<int?>("page", out var value);
            value.ShouldBe(3);
        }

        [Fact]
        public void ConvertsString()
        {
            var expected = Guid.NewGuid().ToString();
            Parse($"key={expected}").TryGetValue<string>("key", out var value);
            value.ShouldBe(expected);
        }

        [Fact]
        public void ConvertsNullableInt()
        {
            Parse("count=42").TryGetValue<int?>("count", out var value);
            value.ShouldBe(42);
        }

        [Fact]
        public void ConvertsNullableGuid()
        {
            var guid = Guid.NewGuid();
            Parse($"id={guid}").TryGetValue<Guid?>("id", out var value);
            value.ShouldBe(guid);
        }

        [Fact]
        public void ConvertsNullableBool()
        {
            Parse("enabled=true").TryGetValue<bool?>("enabled", out var value);
            value.ShouldBe(true);
        }

        [Fact]
        public void UsesFirstValue_WhenMultipleValuesExist()
        {
            Parse("page=1&page=2").TryGetValue<int?>("page", out var value);
            value.ShouldBe(1);
        }

        [Fact]
        public void IsCaseInsensitive_ForKey()
        {
            Parse("Page=5").TryGetValue<int?>("page", out var value);
            value.ShouldBe(5);
        }
    }

    public class GetValueMethod
    {
        [Fact]
        public void Throws_WhenQueryParamsIsNull()
        {
            Should.Throw<ArgumentNullException>(() =>
                ((IStringParams)null!).GetValue<string>("key"));
        }

        [Fact]
        public void Throws_WhenNoConverterRegistered()
        {
            Should.Throw<InvalidOperationException>(() =>
                Parse($"key={Guid.NewGuid()}").GetValue<int>("key"));
        }

        [Fact]
        public void ReturnsDefault_WhenKeyDoesNotExist()
        {
            Parse($"other={Guid.NewGuid()}")
                .GetValue<string>("missing").ShouldBeNull();
        }

        [Fact]
        public void ReturnsDefaultValue_WhenKeyDoesNotExist()
        {
            var fallback = Guid.NewGuid().ToString();
            Parse($"other={Guid.NewGuid()}")
                .GetValue<string>("missing", fallback).ShouldBe(fallback);
        }

        [Fact]
        public void ReturnsDefaultValue_WhenConversionFails()
        {
            Parse("page=not-a-number")
                .GetValue<int?>("page", -1).ShouldBe(-1);
        }

        [Fact]
        public void ReturnsConvertedValue_WhenSuccessful()
        {
            Parse("page=7").GetValue<int?>("page").ShouldBe(7);
        }

        [Fact]
        public void ReturnsConvertedValue_IgnoringDefaultValue_WhenSuccessful()
        {
            // The defaultValue should not be returned when conversion succeeds.
            Parse("page=7").GetValue<int?>("page", -1).ShouldBe(7);
        }

        [Fact]
        public void ReturnsNull_WhenNoDefaultProvided_AndKeyAbsent()
        {
            Parse($"other={Guid.NewGuid()}")
                .GetValue<int?>("missing").ShouldBeNull();
        }

        [Fact]
        public void ConvertsString()
        {
            var expected = Guid.NewGuid().ToString();
            Parse($"key={expected}").GetValue<string>("key").ShouldBe(expected);
        }

        [Fact]
        public void IsCaseInsensitive_ForKey()
        {
            Parse("Count=10").GetValue<int?>("count").ShouldBe(10);
        }
    }
}