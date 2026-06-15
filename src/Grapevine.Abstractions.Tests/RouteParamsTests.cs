using Grapevine;
using Grapevine.Abstractions;
using Shouldly;
using Xunit;

namespace Grapevine.Abstractions.Tests;

public class RouteParamsTests
{
    private static RouteParams Create() => new();

    private static RouteParams CreateSealed()
    {
        var r = new RouteParams();
        r.Seal();
        return r;
    }

    private static RouteParams CreateWith(string key, string value)
    {
        var r = new RouteParams();
        r.Add(key, value);
        r.Seal();
        return r;
    }

    public class EmptyField
    {
        [Fact]
        public void HasCountOfZero()
        {
            RouteParams.Empty.Count.ShouldBe(0);
        }

        [Fact]
        public void IsSealed()
        {
            RouteParams.Empty.IsSealed.ShouldBeTrue();
        }

        [Fact]
        public void IsSameInstanceOnEveryAccess()
        {
            RouteParams.Empty.ShouldBeSameAs(RouteParams.Empty);
        }
    }

    public class IsSealedProperty
    {
        [Fact]
        public void IsFalse_WhenNew()
        {
            Create().IsSealed.ShouldBeFalse();
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
            var r = Create();
            r.Seal();
            r.IsSealed.ShouldBeTrue();
        }

        [Fact]
        public void IsIdempotent_WhenCalledMultipleTimes()
        {
            var r = Create();
            r.Seal();
            r.Seal();
            r.IsSealed.ShouldBeTrue();
        }
    }

    public class AddMethod
    {
        [Fact]
        public void AddsParam()
        {
            var value = Guid.NewGuid().ToString();
            var r = Create();
            r.Add("id", value);
            r["id"].ShouldBe(value);
        }

        [Fact]
        public void ReplacesExistingValue_ForSameKey()
        {
            var r = Create();
            r.Add("id", Guid.NewGuid().ToString());
            var second = Guid.NewGuid().ToString();
            r.Add("id", second);
            r["id"].ShouldBe(second);
        }

        [Fact]
        public void IsCaseInsensitive_ForKey()
        {
            var value = Guid.NewGuid().ToString();
            var r = Create();
            r.Add("Id", value);
            r["id"].ShouldBe(value);
            r["ID"].ShouldBe(value);
        }

        [Fact]
        public void Throws_WhenSealed()
        {
            var ex = Should.Throw<InvalidOperationException>(() =>
                CreateSealed().Add("id", Guid.NewGuid().ToString()));
            ex.Message.ShouldBe(RouteParams.SealedCollectionMessage);
        }
    }

    public class CountProperty
    {
        [Fact]
        public void IsZero_WhenEmpty()
        {
            Create().Count.ShouldBe(0);
        }

        [Fact]
        public void ReflectsNumberOfDistinctKeys()
        {
            var r = Create();
            r.Add("id", Guid.NewGuid().ToString());
            r.Add("slug", Guid.NewGuid().ToString());
            r.Count.ShouldBe(2);
        }

        [Fact]
        public void DoesNotIncrease_WhenSameKeyAddedAgain()
        {
            var r = Create();
            r.Add("id", Guid.NewGuid().ToString());
            r.Add("id", Guid.NewGuid().ToString());
            r.Count.ShouldBe(1);
        }
    }

    public class Indexer
    {
        [Fact]
        public void ReturnsValue_WhenKeyExists()
        {
            var value = Guid.NewGuid().ToString();
            CreateWith("id", value)["id"].ShouldBe(value);
        }

        [Fact]
        public void ReturnsNull_WhenKeyDoesNotExist()
        {
            RouteParams.Empty["id"].ShouldBeNull();
        }

        [Fact]
        public void IsCaseInsensitive()
        {
            var value = Guid.NewGuid().ToString();
            var r = CreateWith("Id", value);
            r["id"].ShouldBe(value);
            r["ID"].ShouldBe(value);
        }
    }

    public class ContainsKeyMethod
    {
        [Fact]
        public void ReturnsTrue_WhenKeyExists()
        {
            CreateWith("id", Guid.NewGuid().ToString()).ContainsKey("id").ShouldBeTrue();
        }

        [Fact]
        public void ReturnsFalse_WhenKeyDoesNotExist()
        {
            RouteParams.Empty.ContainsKey("id").ShouldBeFalse();
        }

        [Fact]
        public void IsCaseInsensitive()
        {
            var r = CreateWith("Id", Guid.NewGuid().ToString());
            r.ContainsKey("id").ShouldBeTrue();
            r.ContainsKey("ID").ShouldBeTrue();
        }
    }

    public class TryGetValueMethod
    {
        [Fact]
        public void ReturnsTrue_WhenKeyExists()
        {
            CreateWith("id", Guid.NewGuid().ToString())
                .TryGetValue("id", out _).ShouldBeTrue();
        }

        [Fact]
        public void SetsValue_WhenKeyExists()
        {
            var expected = Guid.NewGuid().ToString();
            CreateWith("id", expected).TryGetValue("id", out var value);
            value.ShouldBe(expected);
        }

        [Fact]
        public void ReturnsFalse_WhenKeyDoesNotExist()
        {
            RouteParams.Empty.TryGetValue("id", out _).ShouldBeFalse();
        }

        [Fact]
        public void SetsValueToNull_WhenKeyDoesNotExist()
        {
            RouteParams.Empty.TryGetValue("id", out var value);
            value.ShouldBeNull();
        }

        [Fact]
        public void IsCaseInsensitive()
        {
            var expected = Guid.NewGuid().ToString();
            var r = CreateWith("Id", expected);
            r.TryGetValue("ID", out var value);
            value.ShouldBe(expected);
        }
    }

    public class GetValuesMethod
    {
        [Fact]
        public void ReturnsSingleElementList_WhenKeyExists()
        {
            var expected = Guid.NewGuid().ToString();
            var values = CreateWith("id", expected).GetValues("id");
            values.Count.ShouldBe(1);
            values[0].ShouldBe(expected);
        }

        [Fact]
        public void ReturnsEmptyList_WhenKeyDoesNotExist()
        {
            RouteParams.Empty.GetValues("id").ShouldBeEmpty();
        }
    }

    public class TryGetValuesMethod
    {
        [Fact]
        public void ReturnsTrue_WhenKeyExists()
        {
            CreateWith("id", Guid.NewGuid().ToString())
                .TryGetValues("id", out _).ShouldBeTrue();
        }

        [Fact]
        public void ReturnsSingleElementList_WhenKeyExists()
        {
            var expected = Guid.NewGuid().ToString();
            CreateWith("id", expected).TryGetValues("id", out var values);
            values!.Count.ShouldBe(1);
            values[0].ShouldBe(expected);
        }

        [Fact]
        public void ReturnsFalse_WhenKeyDoesNotExist()
        {
            RouteParams.Empty.TryGetValues("id", out _).ShouldBeFalse();
        }

        [Fact]
        public void SetsValuesToNull_WhenKeyDoesNotExist()
        {
            RouteParams.Empty.TryGetValues("id", out var values);
            values.ShouldBeNull();
        }
    }

    public class GetEnumerator
    {
        [Fact]
        public void EnumeratesAllParams()
        {
            var r = Create();
            r.Add("id", Guid.NewGuid().ToString());
            r.Add("slug", Guid.NewGuid().ToString());
            r.Seal();

            var count = 0;
            foreach (var _ in r) count++;
            count.ShouldBe(2);
        }

        [Fact]
        public void YieldsEmptySequence_WhenCollectionIsEmpty()
        {
            var count = 0;
            foreach (var _ in RouteParams.Empty) count++;
            count.ShouldBe(0);
        }

        [Fact]
        public void EachPairHasSingleElementValueList()
        {
            var r = Create();
            r.Add("id", Guid.NewGuid().ToString());
            r.Seal();

            foreach (var pair in r)
                pair.Value.Count.ShouldBe(1);
        }
    }
}