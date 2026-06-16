namespace Grapevine.Abstractions.Tests;

public class ConverterRegistryTests
{
    /// <summary>
    /// A unique struct used to test custom registration without conflicting
    /// with built-in converters or other tests.
    /// </summary>
    private readonly struct TestValue
    {
        public string Raw { get; }
        public TestValue(string raw) => Raw = raw;
    }

    /// <summary>
    /// A second unique struct used to test unregistered type behaviour.
    /// Never registered in any test.
    /// </summary>
    private readonly struct UnregisteredValue { }

    public class IsRegisteredMethod
    {
        [Fact]
        public void ReturnsTrue_ForBuiltInString()
        {
            ConverterRegistry.IsRegistered<string>().ShouldBeTrue();
        }

        [Fact]
        public void ReturnsTrue_ForBuiltInNullableInt()
        {
            ConverterRegistry.IsRegistered<int?>().ShouldBeTrue();
        }

        [Fact]
        public void ReturnsTrue_ForBuiltInNullableLong()
        {
            ConverterRegistry.IsRegistered<long?>().ShouldBeTrue();
        }

        [Fact]
        public void ReturnsTrue_ForBuiltInNullableDouble()
        {
            ConverterRegistry.IsRegistered<double?>().ShouldBeTrue();
        }

        [Fact]
        public void ReturnsTrue_ForBuiltInNullableDecimal()
        {
            ConverterRegistry.IsRegistered<decimal?>().ShouldBeTrue();
        }

        [Fact]
        public void ReturnsTrue_ForBuiltInNullableBool()
        {
            ConverterRegistry.IsRegistered<bool?>().ShouldBeTrue();
        }

        [Fact]
        public void ReturnsTrue_ForBuiltInNullableGuid()
        {
            ConverterRegistry.IsRegistered<Guid?>().ShouldBeTrue();
        }

        [Fact]
        public void ReturnsTrue_ForBuiltInNullableDateTimeOffset()
        {
            ConverterRegistry.IsRegistered<DateTimeOffset?>().ShouldBeTrue();
        }

        [Fact]
        public void ReturnsFalse_ForNonNullableInt()
        {
            // Built-ins are registered as nullable; non-nullable int is not registered.
            ConverterRegistry.IsRegistered<int>().ShouldBeFalse();
        }

        [Fact]
        public void ReturnsFalse_ForUnregisteredType()
        {
            ConverterRegistry.IsRegistered<UnregisteredValue>().ShouldBeFalse();
        }

        [Fact]
        public void ReturnsTrue_AfterCustomRegistration()
        {
            ConverterRegistry.Register<TestValue>(new ValueConverter<TestValue>(s => new TestValue(s)));
            ConverterRegistry.IsRegistered<TestValue>().ShouldBeTrue();
        }
    }

    public class RegisterMethod
    {
        [Fact]
        public void Throws_WhenConverterIsNull()
        {
            Should.Throw<ArgumentNullException>(() =>
                ConverterRegistry.Register<TestValue>(null!));
        }

        [Fact]
        public void RegistersConverter_WhenValid()
        {
            ConverterRegistry.Register<TestValue>(new ValueConverter<TestValue>(s => new TestValue(s)));
            ConverterRegistry.IsRegistered<TestValue>().ShouldBeTrue();
        }

        [Fact]
        public void ReplacesExistingConverter_WhenRegisteredAgain()
        {
            var firstCalled = false;
            var secondCalled = false;

            ConverterRegistry.Register<TestValue>(new ValueConverter<TestValue>(s =>
            {
                firstCalled = true;
                return new TestValue(s);
            }));

            ConverterRegistry.Register<TestValue>(new ValueConverter<TestValue>(s =>
            {
                secondCalled = true;
                return new TestValue(s);
            }));

            ConverterRegistry.TryConvert<TestValue>(Guid.NewGuid().ToString(), out _);

            firstCalled.ShouldBeFalse();
            secondCalled.ShouldBeTrue();
        }
    }

    public class TryConvertMethod
    {
        [Fact]
        public void Throws_WhenNoConverterRegistered()
        {
            var ex = Should.Throw<InvalidOperationException>(() =>
                ConverterRegistry.TryConvert<UnregisteredValue>(Guid.NewGuid().ToString(), out _));
            ex.Message.ShouldContain(ConverterRegistry.NoConverterMessage
                .Replace("{0}", typeof(UnregisteredValue).FullName));
        }

        [Fact]
        public void ReturnsTrue_WhenConversionSucceeds()
        {
            ConverterRegistry.TryConvert<string>(Guid.NewGuid().ToString(), out _).ShouldBeTrue();
        }

        [Fact]
        public void SetsValue_WhenConversionSucceeds()
        {
            var raw = Guid.NewGuid().ToString();
            ConverterRegistry.TryConvert<string>(raw, out var value);
            value.ShouldBe(raw);
        }

        [Fact]
        public void ReturnsFalse_WhenConverterReturnsNull()
        {
            ConverterRegistry.TryConvert<int?>("not-a-number", out _).ShouldBeFalse();
        }

        [Fact]
        public void SetsValueToDefault_WhenConverterReturnsNull()
        {
            ConverterRegistry.TryConvert<int?>("not-a-number", out var value);
            value.ShouldBeNull();
        }

        [Fact]
        public void ConvertsString()
        {
            var raw = Guid.NewGuid().ToString();
            ConverterRegistry.TryConvert<string>(raw, out var value).ShouldBeTrue();
            value.ShouldBe(raw);
        }

        [Fact]
        public void ConvertsNullableInt()
        {
            ConverterRegistry.TryConvert<int?>("42", out var value).ShouldBeTrue();
            value.ShouldBe(42);
        }

        [Fact]
        public void ConvertsNullableLong()
        {
            ConverterRegistry.TryConvert<long?>("9999999999", out var value).ShouldBeTrue();
            value.ShouldBe(9999999999L);
        }

        [Fact]
        public void ConvertsNullableDouble()
        {
            ConverterRegistry.TryConvert<double?>("3.14", out var value).ShouldBeTrue();
            value.ShouldBe(3.14);
        }

        [Fact]
        public void ConvertsNullableDecimal()
        {
            ConverterRegistry.TryConvert<decimal?>("1.23", out var value).ShouldBeTrue();
            value.ShouldBe(1.23m);
        }

        [Fact]
        public void ConvertsNullableBool_True()
        {
            ConverterRegistry.TryConvert<bool?>("true", out var value).ShouldBeTrue();
            value.ShouldBe(true);
        }

        [Fact]
        public void ConvertsNullableBool_False()
        {
            ConverterRegistry.TryConvert<bool?>("false", out var value).ShouldBeTrue();
            value.ShouldBe(false);
        }

        [Fact]
        public void ConvertsNullableGuid()
        {
            var guid = Guid.NewGuid();
            ConverterRegistry.TryConvert<Guid?>(guid.ToString(), out var value).ShouldBeTrue();
            value.ShouldBe(guid);
        }

        [Fact]
        public void ConvertsNullableDateTimeOffset()
        {
            var dto = new DateTimeOffset(2030, 6, 15, 12, 0, 0, TimeSpan.Zero);
            ConverterRegistry.TryConvert<DateTimeOffset?>(
                dto.ToString("O"),
                out var value).ShouldBeTrue();
            value.ShouldBe(dto);
        }

        [Fact]
        public void ReturnsFalse_ForInvalidInt()
        {
            ConverterRegistry.TryConvert<int?>("not-a-number", out _).ShouldBeFalse();
        }

        [Fact]
        public void ReturnsFalse_ForInvalidGuid()
        {
            ConverterRegistry.TryConvert<Guid?>("not-a-guid", out _).ShouldBeFalse();
        }

        [Fact]
        public void ReturnsFalse_ForInvalidBool()
        {
            ConverterRegistry.TryConvert<bool?>("yes", out _).ShouldBeFalse();
        }

        [Fact]
        public void UsesInvariantCulture_ForDouble()
        {
            // Ensures decimal separator is always '.' regardless of system locale.
            ConverterRegistry.TryConvert<double?>("1.5", out var value).ShouldBeTrue();
            value.ShouldBe(1.5);
        }

        [Fact]
        public void UsesInvariantCulture_ForDecimal()
        {
            ConverterRegistry.TryConvert<decimal?>("1.5", out var value).ShouldBeTrue();
            value.ShouldBe(1.5m);
        }

        [Fact]
        public void InvokesCustomConverter_WhenRegistered()
        {
            var raw = Guid.NewGuid().ToString();
            ConverterRegistry.Register<TestValue>(new ValueConverter<TestValue>(s => new TestValue(s)));
            ConverterRegistry.TryConvert<TestValue>(raw, out var value).ShouldBeTrue();
            value.Raw.ShouldBe(raw);
        }
    }
}