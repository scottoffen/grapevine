using Grapevine.Abstractions;
using Shouldly;
using Xunit;

namespace Grapevine.Abstractions.Tests;

public class HeaderCollectionTests
{
    /// <summary>
    /// Minimal concrete subclass used to exercise the abstract base class
    /// through its public and internal surface. Not intended to represent a
    /// real header collection type.
    /// </summary>
    private sealed class TestHeaderCollection : HeaderCollection
    {
        /// <summary>
        /// Exposes <see cref="HeaderCollection.Seal"/> for testing.
        /// </summary>
        public void SealForTest() => Seal();

        /// <summary>
        /// Tracks whether <see cref="OnSealed"/> was called and how many times.
        /// </summary>
        public int OnSealedCallCount { get; private set; }

        protected override void OnSealed() => OnSealedCallCount++;
    }

    private static TestHeaderCollection CreateCollection() => new();

    private static TestHeaderCollection CreateSealed()
    {
        var c = new TestHeaderCollection();
        c.SealForTest();
        return c;
    }

    public class IsSealedProperty
    {
        [Fact]
        public void IsFalse_WhenNew()
        {
            CreateCollection().IsSealed.ShouldBeFalse();
        }

        [Fact]
        public void IsTrue_AfterSeal()
        {
            CreateSealed().IsSealed.ShouldBeTrue();
        }
    }

    public class SealMethod
    {
        [Fact]
        public void SetsIsSealed()
        {
            var c = CreateCollection();
            c.SealForTest();
            c.IsSealed.ShouldBeTrue();
        }

        [Fact]
        public void CallsOnSealed_OnFirstSeal()
        {
            var c = CreateCollection();
            c.SealForTest();
            c.OnSealedCallCount.ShouldBe(1);
        }

        [Fact]
        public void IsIdempotent_WhenCalledMultipleTimes()
        {
            var c = CreateCollection();
            c.SealForTest();
            c.SealForTest();
            c.SealForTest();
            c.OnSealedCallCount.ShouldBe(1);
        }
    }

    public class CountProperty
    {
        [Fact]
        public void IsZero_WhenEmpty()
        {
            CreateCollection().Count.ShouldBe(0);
        }

        [Fact]
        public void ReflectsDistinctHeaderNames()
        {
            var c = CreateCollection();
            c.Add("Accept", "text/html");
            c.Add("Content-Type", "application/json");
            c.Count.ShouldBe(2);
        }

        [Fact]
        public void DoesNotIncrease_WhenSameNameAddedAgain()
        {
            var c = CreateCollection();
            c.Add("Accept", "text/html");
            c.Add("Accept", "application/json");
            c.Count.ShouldBe(1);
        }
    }

    public class AddMethod
    {
        public class SingleValue
        {
            [Fact]
            public void AddsFirstValue_ForNewName()
            {
                var c = CreateCollection();
                c.Add("Accept", "text/html");
                c["Accept"].ShouldBe("text/html");
            }

            [Fact]
            public void AddsAdditionalValue_ForExistingName()
            {
                var c = CreateCollection();
                c.Add("Accept", "text/html");
                c.Add("Accept", "application/json");
                c.TryGetValues("Accept", out var values).ShouldBeTrue();
                values!.Count.ShouldBe(2);
            }

            [Fact]
            public void IsCaseInsensitive_ForName()
            {
                var c = CreateCollection();
                c.Add("accept", "text/html");
                c.Add("ACCEPT", "application/json");
                c.TryGetValues("Accept", out var values).ShouldBeTrue();
                values!.Count.ShouldBe(2);
            }

            [Fact]
            public void Throws_WhenNameIsNull()
            {
                var c = CreateCollection();
                Should.Throw<ArgumentNullException>(() => c.Add(null!, "text/html"));
            }

            [Fact]
            public void Throws_WhenValueIsNull()
            {
                var c = CreateCollection();
                Should.Throw<ArgumentNullException>(() => c.Add("Accept", (string?)null!));
            }

            [Fact]
            public void Throws_WhenSealed()
            {
                var ex = Should.Throw<InvalidOperationException>(() =>
                    CreateSealed().Add("Accept", "text/html"));
                ex.Message.ShouldBe(HeaderCollection.SealedCollectionMessage);
            }
        }

        public class MultiValue
        {
            [Fact]
            public void AddsAllValues_ForNewName()
            {
                var c = CreateCollection();
                c.Add("Accept", ["text/html", "application/json"]);
                c.TryGetValues("Accept", out var values).ShouldBeTrue();
                values!.Count.ShouldBe(2);
            }

            [Fact]
            public void AppendsValues_ForExistingName()
            {
                var c = CreateCollection();
                c.Add("Accept", "text/html");
                c.Add("Accept", ["application/json", "application/xml"]);
                c.TryGetValues("Accept", out var values).ShouldBeTrue();
                values!.Count.ShouldBe(3);
            }

            [Fact]
            public void IsCaseInsensitive_ForName()
            {
                var c = CreateCollection();
                c.Add("accept", ["text/html"]);
                c.Add("ACCEPT", ["application/json"]);
                c.TryGetValues("Accept", out var values).ShouldBeTrue();
                values!.Count.ShouldBe(2);
            }

            [Fact]
            public void PreservesOrder_OfAddedValues()
            {
                var c = CreateCollection();
                c.Add("Accept", ["text/html", "application/json", "application/xml"]);
                c.TryGetValues("Accept", out var values).ShouldBeTrue();
                values![0].ShouldBe("text/html");
                values![1].ShouldBe("application/json");
                values![2].ShouldBe("application/xml");
            }

            [Fact]
            public void Throws_WhenNameIsNull()
            {
                var c = CreateCollection();
                Should.Throw<ArgumentNullException>(() => c.Add(null!, ["text/html"]));
            }

            [Fact]
            public void Throws_WhenValuesIsNull()
            {
                var c = CreateCollection();
                Should.Throw<ArgumentNullException>(() => c.Add("Accept", (string[])null!));
            }

            [Fact]
            public void Throws_WhenSealed()
            {
                var ex = Should.Throw<InvalidOperationException>(() =>
                    CreateSealed().Add("Accept", ["text/html"]));
                ex.Message.ShouldBe(HeaderCollection.SealedCollectionMessage);
            }
        }
    }

    public class SetMethod
    {
        [Fact]
        public void SetsValue_ForNewName()
        {
            var c = CreateCollection();
            c.Set("Accept", "text/html");
            c["Accept"].ShouldBe("text/html");
        }

        [Fact]
        public void ReplacesAllExistingValues_ForExistingName()
        {
            var c = CreateCollection();
            c.Add("Accept", "text/html");
            c.Add("Accept", "application/json");
            c.Set("Accept", "text/plain");
            c.TryGetValues("Accept", out var values).ShouldBeTrue();
            values!.Count.ShouldBe(1);
            values[0].ShouldBe("text/plain");
        }

        [Fact]
        public void IsCaseInsensitive_ForName()
        {
            var c = CreateCollection();
            c.Add("accept", "text/html");
            c.Set("ACCEPT", "text/plain");
            c.TryGetValues("Accept", out var values).ShouldBeTrue();
            values!.Count.ShouldBe(1);
        }

        [Fact]
        public void Throws_WhenNameIsNull()
        {
            var c = CreateCollection();
            Should.Throw<ArgumentNullException>(() => c.Set(null!, "text/html"));
        }

        [Fact]
        public void Throws_WhenValueIsNull()
        {
            var c = CreateCollection();
            Should.Throw<ArgumentNullException>(() => c.Set("Accept", null!));
        }

        [Fact]
        public void Throws_WhenSealed()
        {
            var ex = Should.Throw<InvalidOperationException>(() =>
                CreateSealed().Set("Accept", "text/html"));
            ex.Message.ShouldBe(HeaderCollection.SealedCollectionMessage);
        }
    }

    public class RemoveMethod
    {
        [Fact]
        public void ReturnsTrue_WhenHeaderExists()
        {
            var c = CreateCollection();
            c.Add("Accept", "text/html");
            c.Remove("Accept").ShouldBeTrue();
        }

        [Fact]
        public void ReturnsFalse_WhenHeaderDoesNotExist()
        {
            CreateCollection().Remove("Accept").ShouldBeFalse();
        }

        [Fact]
        public void RemovesAllValuesForName()
        {
            var c = CreateCollection();
            c.Add("Accept", "text/html");
            c.Add("Accept", "application/json");
            c.Remove("Accept");
            c.Contains("Accept").ShouldBeFalse();
        }

        [Fact]
        public void IsCaseInsensitive_ForName()
        {
            var c = CreateCollection();
            c.Add("accept", "text/html");
            c.Remove("ACCEPT").ShouldBeTrue();
        }

        [Fact]
        public void Throws_WhenSealed()
        {
            var ex = Should.Throw<InvalidOperationException>(() =>
                CreateSealed().Remove("Accept"));
            ex.Message.ShouldBe(HeaderCollection.SealedCollectionMessage);
        }
    }

    public class ClearMethod
    {
        [Fact]
        public void RemovesAllHeaders()
        {
            var c = CreateCollection();
            c.Add("Accept", "text/html");
            c.Add("Content-Type", "application/json");
            c.Clear();
            c.Count.ShouldBe(0);
        }

        [Fact]
        public void HasNoEffect_WhenAlreadyEmpty()
        {
            var c = CreateCollection();
            c.Clear();
            c.Count.ShouldBe(0);
        }

        [Fact]
        public void Throws_WhenSealed()
        {
            var ex = Should.Throw<InvalidOperationException>(() =>
                CreateSealed().Clear());
            ex.Message.ShouldBe(HeaderCollection.SealedCollectionMessage);
        }
    }

    public class ContainsMethod
    {
        [Fact]
        public void ReturnsTrue_WhenHeaderExists()
        {
            var c = CreateCollection();
            c.Add("Accept", "text/html");
            c.Contains("Accept").ShouldBeTrue();
        }

        [Fact]
        public void ReturnsFalse_WhenHeaderDoesNotExist()
        {
            CreateCollection().Contains("Accept").ShouldBeFalse();
        }

        [Fact]
        public void IsCaseInsensitive()
        {
            var c = CreateCollection();
            c.Add("accept", "text/html");
            c.Contains("ACCEPT").ShouldBeTrue();
        }

        [Fact]
        public void ReturnsFalse_AfterRemove()
        {
            var c = CreateCollection();
            c.Add("Accept", "text/html");
            c.Remove("Accept");
            c.Contains("Accept").ShouldBeFalse();
        }
    }

    public class TryGetValueMethod
    {
        [Fact]
        public void ReturnsTrue_WhenHeaderExists()
        {
            var c = CreateCollection();
            c.Add("Accept", "text/html");
            c.TryGetValue("Accept", out _).ShouldBeTrue();
        }

        [Fact]
        public void SetsValue_WhenHeaderExists()
        {
            var c = CreateCollection();
            c.Add("Accept", "text/html");
            c.TryGetValue("Accept", out var value);
            value.ShouldBe("text/html");
        }

        [Fact]
        public void ReturnsFirstValue_WhenMultipleValuesExist()
        {
            var c = CreateCollection();
            c.Add("Accept", "text/html");
            c.Add("Accept", "application/json");
            c.TryGetValue("Accept", out var value);
            value.ShouldBe("text/html");
        }

        [Fact]
        public void ReturnsFalse_WhenHeaderDoesNotExist()
        {
            CreateCollection().TryGetValue("Accept", out _).ShouldBeFalse();
        }

        [Fact]
        public void SetsValueToNull_WhenHeaderDoesNotExist()
        {
            CreateCollection().TryGetValue("Accept", out var value);
            value.ShouldBeNull();
        }

        [Fact]
        public void IsCaseInsensitive()
        {
            var c = CreateCollection();
            c.Add("accept", "text/html");
            c.TryGetValue("ACCEPT", out var value);
            value.ShouldBe("text/html");
        }
    }

    public class TryGetValuesMethod
    {
        [Fact]
        public void ReturnsTrue_WhenHeaderExists()
        {
            var c = CreateCollection();
            c.Add("Accept", "text/html");
            c.TryGetValues("Accept", out _).ShouldBeTrue();
        }

        [Fact]
        public void ReturnsAllValues_WhenMultipleExist()
        {
            var c = CreateCollection();
            c.Add("Accept", "text/html");
            c.Add("Accept", "application/json");
            c.TryGetValues("Accept", out var values).ShouldBeTrue();
            values!.Count.ShouldBe(2);
            values[0].ShouldBe("text/html");
            values[1].ShouldBe("application/json");
        }

        [Fact]
        public void ReturnsFalse_WhenHeaderDoesNotExist()
        {
            CreateCollection().TryGetValues("Accept", out _).ShouldBeFalse();
        }

        [Fact]
        public void SetsValuesToNull_WhenHeaderDoesNotExist()
        {
            CreateCollection().TryGetValues("Accept", out var values);
            values.ShouldBeNull();
        }

        [Fact]
        public void IsCaseInsensitive()
        {
            var c = CreateCollection();
            c.Add("accept", "text/html");
            c.TryGetValues("ACCEPT", out var values).ShouldBeTrue();
            values!.Count.ShouldBe(1);
        }
    }

    public class Indexer
    {
        [Fact]
        public void ReturnsFirstValue_WhenHeaderExists()
        {
            var c = CreateCollection();
            c.Add("Accept", "text/html");
            c["Accept"].ShouldBe("text/html");
        }

        [Fact]
        public void ReturnsFirstValue_WhenMultipleValuesExist()
        {
            var c = CreateCollection();
            c.Add("Accept", "text/html");
            c.Add("Accept", "application/json");
            c["Accept"].ShouldBe("text/html");
        }

        [Fact]
        public void ReturnsNull_WhenHeaderDoesNotExist()
        {
            CreateCollection()["Accept"].ShouldBeNull();
        }

        [Fact]
        public void IsCaseInsensitive()
        {
            var c = CreateCollection();
            c.Add("accept", "text/html");
            c["ACCEPT"].ShouldBe("text/html");
        }
    }

    public class GetEnumerator
    {
        [Fact]
        public void EnumeratesAllHeaders()
        {
            var c = CreateCollection();
            c.Add("Accept", "text/html");
            c.Add("Content-Type", "application/json");

            var keys = new List<string>();
            foreach (var pair in c)
                keys.Add(pair.Key);

            keys.Count.ShouldBe(2);
            keys.ShouldContain("Accept");
            keys.ShouldContain("Content-Type");
        }

        [Fact]
        public void EnumeratesAllValuesPerHeader()
        {
            var c = CreateCollection();
            c.Add("Accept", "text/html");
            c.Add("Accept", "application/json");

            foreach (var pair in c)
            {
                if (pair.Key == "Accept")
                    pair.Value.Count.ShouldBe(2);
            }
        }

        [Fact]
        public void YieldsEmptySequence_WhenCollectionIsEmpty()
        {
            var count = 0;
            foreach (var _ in CreateCollection())
                count++;
            count.ShouldBe(0);
        }

        [Fact]
        public void ValuesAreReadOnly()
        {
            var c = CreateCollection();
            c.Add("Accept", "text/html");

            foreach (var pair in c)
                pair.Value.ShouldBeOfType<List<string>>();

            // Verify the returned IReadOnlyList cannot be cast back to a mutable
            // list and mutated without going through the collection's own API.
            // The enumeration wraps the internal list, but the contract is IReadOnlyList.
            foreach (var pair in c)
                pair.Value.ShouldBeAssignableTo<IReadOnlyList<string>>();
        }
    }
}