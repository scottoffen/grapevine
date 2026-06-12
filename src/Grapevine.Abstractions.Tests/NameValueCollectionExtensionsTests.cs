using System.Collections.Specialized;
using Grapevine;
using Shouldly;
using Xunit;

namespace Grapeseed.Tests;

public class NameValueCollectionExtensionsTests
{
    public class TryGetValueMethod
    {
        [Fact]
        public void ReturnsTrueAndValue_WhenKeyExists()
        {
            var collection = new NameValueCollection { { "key", "value" } };
            collection.TryGetValue("key", out var value).ShouldBeTrue();
            value.ShouldBe("value");
        }

        [Fact]
        public void ReturnsFalse_WhenKeyDoesNotExist()
        {
            var collection = new NameValueCollection();
            collection.TryGetValue("missing", out var value).ShouldBeFalse();
            value.ShouldBeNull();
        }

        [Fact]
        public void Throws_WhenCollectionIsNull()
        {
            NameValueCollection? collection = null;
            Should.Throw<ArgumentNullException>(() => collection!.TryGetValue("key", out _));
        }

        [Fact]
        public void Throws_WhenKeyIsNull()
        {
            var collection = new NameValueCollection();
            Should.Throw<ArgumentNullException>(() => collection.TryGetValue(null!, out _));
        }

        [Fact]
        public void Throws_WhenKeyIsWhitespace()
        {
            var collection = new NameValueCollection();
            Should.Throw<ArgumentNullException>(() => collection.TryGetValue("   ", out _));
        }
    }

    public class GetValue_ThrowingOverload
    {
        [Fact]
        public void ReturnsStringValue_WhenKeyExists()
        {
            var collection = new NameValueCollection { { "key", "value" } };
            collection.GetValue<string>("key").ShouldBe("value");
        }

        [Fact]
        public void ConvertsToInt_WhenKeyExists()
        {
            var collection = new NameValueCollection { { "key", "42" } };
            collection.GetValue<int>("key").ShouldBe(42);
        }

        [Fact]
        public void ConvertsToDecimal_WhenKeyExists()
        {
            var collection = new NameValueCollection { { "key", "3.14" } };
            collection.GetValue<decimal>("key").ShouldBe(3.14m);
        }

        [Fact]
        public void ConvertsToBool_WhenKeyExists()
        {
            var collection = new NameValueCollection { { "key", "true" } };
            collection.GetValue<bool>("key").ShouldBeTrue();
        }

        [Fact]
        public void ConvertsToGuid_WhenKeyExists()
        {
            var guid = Guid.NewGuid();
            var collection = new NameValueCollection { { "key", guid.ToString() } };
            collection.GetValue<Guid>("key").ShouldBe(guid);
        }

        [Fact]
        public void Throws_WhenCollectionIsNull()
        {
            NameValueCollection? collection = null;
            Should.Throw<ArgumentNullException>(() => collection!.GetValue<string>("key"));
        }

        [Fact]
        public void Throws_WhenKeyIsNull()
        {
            var collection = new NameValueCollection();
            Should.Throw<ArgumentNullException>(() => collection.GetValue<string>(null!));
        }

        [Fact]
        public void Throws_WhenKeyIsWhitespace()
        {
            var collection = new NameValueCollection();
            Should.Throw<ArgumentNullException>(() => collection.GetValue<string>("   "));
        }

        [Fact]
        public void Throws_WhenKeyNotInCollection()
        {
            var collection = new NameValueCollection();
            Should.Throw<ArgumentOutOfRangeException>(() => collection.GetValue<string>("missing"));
        }

        [Fact]
        public void Throws_WhenNoConverterExistsForType()
        {
            var collection = new NameValueCollection { { "key", "value" } };
            Should.Throw<InvalidOperationException>(() => collection.GetValue<NameValueCollection>("key"));
        }

        [Fact]
        public void ReturnsDefault_WhenValueCannotBeConverted()
        {
            var collection = new NameValueCollection { { "key", "not-an-int" } };
            collection.GetValue<int>("key").ShouldBe(default);
        }

        [Fact]
        public void ConverterIsCached_ForSameType()
        {
            var collection = new NameValueCollection { { "key", "42" } };
            collection.GetValue<int>("key").ShouldBe(42);
            collection.GetValue<int>("key").ShouldBe(42);
        }
    }

    public class GetValue_DefaultOverload
    {
        [Fact]
        public void ReturnsValue_WhenKeyExistsAndConversionSucceeds()
        {
            var collection = new NameValueCollection { { "key", "42" } };
            collection.GetValue<int>("key", 0).ShouldBe(42);
        }

        [Fact]
        public void ReturnsDefault_WhenKeyNotInCollection()
        {
            var collection = new NameValueCollection();
            collection.GetValue<string>("missing", "default").ShouldBe("default");
        }

        [Fact]
        public void ReturnsDefault_WhenValueCannotBeConverted()
        {
            var collection = new NameValueCollection { { "key", "not-an-int" } };
            collection.GetValue<int>("key", -1).ShouldBe(-1);
        }

        [Fact]
        public void Throws_WhenCollectionIsNull()
        {
            NameValueCollection? collection = null;
            Should.Throw<ArgumentNullException>(() => collection!.GetValue<string>("key", "default"));
        }

        [Fact]
        public void Throws_WhenKeyIsNull()
        {
            var collection = new NameValueCollection();
            Should.Throw<ArgumentNullException>(() => collection.GetValue<string>(null!, "default"));
        }

        [Fact]
        public void Throws_WhenKeyIsWhitespace()
        {
            var collection = new NameValueCollection();
            Should.Throw<ArgumentNullException>(() => collection.GetValue<string>("   ", "default"));
        }
    }

    public class SortQualityValuesMethod
    {
        [Fact]
        public void ReturnsEmptyArray_WhenKeyNotInCollection()
        {
            var collection = new NameValueCollection();
            collection.SortQualityValues("missing").ShouldBeEmpty();
        }

        [Fact]
        public void ReturnsEmptyArray_WhenValueIsEmpty()
        {
            var collection = new NameValueCollection { { "key", "" } };
            collection.SortQualityValues("key").ShouldBeEmpty();
        }

        [Fact]
        public void ReturnsEmptyArray_WhenValueIsWhitespace()
        {
            var collection = new NameValueCollection { { "key", "   " } };
            collection.SortQualityValues("key").ShouldBeEmpty();
        }

        [Fact]
        public void ReturnsSingleElement_ForSingleValue()
        {
            var collection = new NameValueCollection { { "key", "text/html" } };
            var result = collection.SortQualityValues("key");
            result.Count.ShouldBe(1);
            result[0].ShouldBe("text/html");
        }

        [Fact]
        public void ReturnsQualityOrderedList_ForMultipleValues()
        {
            var collection = new NameValueCollection
            {
                { "Accept", "text/plain;q=0.5, text/html;q=0.9, application/json;q=0.8" }
            };
            var result = collection.SortQualityValues("Accept");
            result[0].ShouldBe("text/html");
            result[1].ShouldBe("application/json");
            result[2].ShouldBe("text/plain");
        }

        [Fact]
        public void ReturnsSpecificityOrderedList_WithinSameQualityFactor()
        {
            var collection = new NameValueCollection
            {
                { "Accept", "*/*, text/html, text/*" }
            };
            var result = collection.SortQualityValues("Accept");
            result[0].ShouldBe("text/html");
            result[1].ShouldBe("text/*");
            result[2].ShouldBe("*/*");
        }
    }
}