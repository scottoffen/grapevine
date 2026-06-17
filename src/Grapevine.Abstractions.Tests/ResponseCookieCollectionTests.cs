namespace Grapevine.Abstractions.Tests;

public class ResponseCookieCollectionTests
{
    private static ResponseCookieCollection Create() => new();

    private static ResponseCookieCollection CreateSealed()
    {
        var c = new ResponseCookieCollection();
        c.Seal();
        return c;
    }

    private static Cookie MakeCookie(string? name = null) =>
        new(name ?? Guid.NewGuid().ToString("N"), Guid.NewGuid().ToString("N"));

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

    public class IsReadOnlyProperty
    {
        [Fact]
        public void IsFalse_WhenNotSealed()
        {
            Create().IsReadOnly.ShouldBeFalse();
        }

        [Fact]
        public void IsTrue_WhenSealed()
        {
            CreateSealed().IsReadOnly.ShouldBeTrue();
        }
    }

    public class SealMethod
    {
        [Fact]
        public void SetsIsSealed()
        {
            var c = Create();
            c.Seal();
            c.IsSealed.ShouldBeTrue();
        }

        [Fact]
        public void IsIdempotent_WhenCalledMultipleTimes()
        {
            var c = Create();
            c.Seal();
            c.Seal();
            c.IsSealed.ShouldBeTrue();
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
        public void ReflectsNumberOfDistinctNames()
        {
            var c = Create();
            c.Add(MakeCookie("session"));
            c.Add(MakeCookie("theme"));
            c.Count.ShouldBe(2);
        }

        [Fact]
        public void DoesNotIncrease_WhenSameNameAddedAgain()
        {
            var c = Create();
            c.Add(MakeCookie("session"));
            c.Add(MakeCookie("session"));
            c.Count.ShouldBe(1);
        }
    }

    public class AddMethod
    {
        [Fact]
        public void AddsCookie()
        {
            var c = Create();
            var cookie = MakeCookie("session");
            c.Add(cookie);
            c.Contains("session").ShouldBeTrue();
        }

        [Fact]
        public void ReplacesExistingCookie_WhenSameNameAdded()
        {
            var c = Create();
            var first = new Cookie("session", Guid.NewGuid().ToString("N"));
            var second = new Cookie("session", Guid.NewGuid().ToString("N"));
            c.Add(first);
            c.Add(second);
            c["session"].ShouldBeSameAs(second);
        }

        [Fact]
        public void NameComparisonIsCaseInsensitive()
        {
            var c = Create();
            c.Add(new Cookie("session", Guid.NewGuid().ToString("N")));
            c.Add(new Cookie("SESSION", Guid.NewGuid().ToString("N")));
            c.Count.ShouldBe(1);
        }

        [Fact]
        public void Throws_WhenCookieIsNull()
        {
            Should.Throw<ArgumentNullException>(() => Create().Add(null!));
        }

        [Fact]
        public void Throws_WhenSealed()
        {
            var ex = Should.Throw<InvalidOperationException>(() =>
                CreateSealed().Add(MakeCookie()));
            ex.Message.ShouldBe(ResponseCookieCollection.SealedCollectionMessage);
        }
    }

    public class AddRangeMethod
    {
        [Fact]
        public void AddsAllCookies()
        {
            var c = Create();
            c.AddRange([MakeCookie("a"), MakeCookie("b"), MakeCookie("c")]);
            c.Count.ShouldBe(3);
        }

        [Fact]
        public void ReplacesExisting_WhenDuplicateNamesInRange()
        {
            var c = Create();
            var first = new Cookie("session", Guid.NewGuid().ToString("N"));
            var second = new Cookie("session", Guid.NewGuid().ToString("N"));
            c.AddRange([first, second]);
            c.Count.ShouldBe(1);
            c["session"].ShouldBeSameAs(second);
        }

        [Fact]
        public void Throws_WhenCookiesIsNull()
        {
            Should.Throw<ArgumentNullException>(() => Create().AddRange(null!));
        }

        [Fact]
        public void Throws_WhenSealed()
        {
            var ex = Should.Throw<InvalidOperationException>(() =>
                CreateSealed().AddRange([MakeCookie()]));
            ex.Message.ShouldBe(ResponseCookieCollection.SealedCollectionMessage);
        }
    }

    public class ClearMethod
    {
        [Fact]
        public void RemovesAllCookies()
        {
            var c = Create();
            c.Add(MakeCookie("a"));
            c.Add(MakeCookie("b"));
            c.Clear();
            c.Count.ShouldBe(0);
        }

        [Fact]
        public void HasNoEffect_WhenAlreadyEmpty()
        {
            var c = Create();
            c.Clear();
            c.Count.ShouldBe(0);
        }

        [Fact]
        public void Throws_WhenSealed()
        {
            var ex = Should.Throw<InvalidOperationException>(() => CreateSealed().Clear());
            ex.Message.ShouldBe(ResponseCookieCollection.SealedCollectionMessage);
        }
    }

    public class ContainsMethod
    {
        [Fact]
        public void ReturnsTrueForCookie_WhenNameExists()
        {
            var cookie = MakeCookie("session");
            var c = Create();
            c.Add(cookie);
            c.Contains(cookie).ShouldBeTrue();
        }

        [Fact]
        public void ReturnsFalseForCookie_WhenNameDoesNotExist()
        {
            Create().Contains(MakeCookie("session")).ShouldBeFalse();
        }

        [Fact]
        public void ReturnsTrueForString_WhenNameExists()
        {
            var c = Create();
            c.Add(MakeCookie("session"));
            c.Contains("session").ShouldBeTrue();
        }

        [Fact]
        public void ReturnsFalseForString_WhenNameDoesNotExist()
        {
            Create().Contains("session").ShouldBeFalse();
        }

        [Fact]
        public void IsCaseInsensitive_ForStringOverload()
        {
            var c = Create();
            c.Add(MakeCookie("session"));
            c.Contains("SESSION").ShouldBeTrue();
        }

        [Fact]
        public void IsCaseInsensitive_ForCookieOverload()
        {
            var c = Create();
            c.Add(new Cookie("session", Guid.NewGuid().ToString("N")));
            c.Contains(new Cookie("SESSION", Guid.NewGuid().ToString("N"))).ShouldBeTrue();
        }
    }

    public class RemoveMethod
    {
        [Fact]
        public void ReturnsTrueForCookie_WhenFound()
        {
            var cookie = MakeCookie("session");
            var c = Create();
            c.Add(cookie);
            c.Remove(cookie).ShouldBeTrue();
        }

        [Fact]
        public void ReturnsFalseForCookie_WhenNotFound()
        {
            Create().Remove(MakeCookie("session")).ShouldBeFalse();
        }

        [Fact]
        public void ReturnsTrueForString_WhenFound()
        {
            var c = Create();
            c.Add(MakeCookie("session"));
            c.Remove("session").ShouldBeTrue();
        }

        [Fact]
        public void ReturnsFalseForString_WhenNotFound()
        {
            Create().Remove("session").ShouldBeFalse();
        }

        [Fact]
        public void RemovesCookie_WhenFound()
        {
            var c = Create();
            c.Add(MakeCookie("session"));
            c.Remove("session");
            c.Contains("session").ShouldBeFalse();
        }

        [Fact]
        public void IsCaseInsensitive()
        {
            var c = Create();
            c.Add(MakeCookie("session"));
            c.Remove("SESSION").ShouldBeTrue();
            c.Contains("session").ShouldBeFalse();
        }

        [Fact]
        public void Throws_WhenSealed()
        {
            var ex = Should.Throw<InvalidOperationException>(() =>
                CreateSealed().Remove("session"));
            ex.Message.ShouldBe(ResponseCookieCollection.SealedCollectionMessage);
        }
    }

    public class CopyToMethod
    {
        [Fact]
        public void CopiesCookiesToArray()
        {
            var c = Create();
            c.Add(MakeCookie("a"));
            c.Add(MakeCookie("b"));
            var array = new Cookie[2];
            c.CopyTo(array, 0);
            array.ShouldAllBe(x => x != null);
        }

        [Fact]
        public void RespectsArrayIndex()
        {
            var c = Create();
            c.Add(MakeCookie("a"));
            var array = new Cookie[3];
            c.CopyTo(array, 2);
            array[0].ShouldBeNull();
            array[1].ShouldBeNull();
            array[2].ShouldNotBeNull();
        }

        [Fact]
        public void Throws_WhenArrayIsNull()
        {
            Should.Throw<ArgumentNullException>(() => Create().CopyTo(null!, 0));
        }

        [Fact]
        public void Throws_WhenArrayIndexIsNegative()
        {
            Should.Throw<ArgumentOutOfRangeException>(() =>
                Create().CopyTo(new Cookie[1], -1));
        }

        [Fact]
        public void Throws_WhenArrayIndexExceedsLength()
        {
            Should.Throw<ArgumentOutOfRangeException>(() =>
                Create().CopyTo(new Cookie[1], 2));
        }

        [Fact]
        public void Throws_WhenInsufficientSpace()
        {
            var c = Create();
            c.Add(MakeCookie("a"));
            c.Add(MakeCookie("b"));
            Should.Throw<ArgumentException>(() => c.CopyTo(new Cookie[1], 0));
        }
    }

    public class StringIndexer
    {
        [Fact]
        public void ReturnsCookie_WhenNameExists()
        {
            var cookie = MakeCookie("session");
            var c = Create();
            c.Add(cookie);
            c["session"].ShouldBeSameAs(cookie);
        }

        [Fact]
        public void ReturnsNull_WhenNameDoesNotExist()
        {
            Create()["session"].ShouldBeNull();
        }

        [Fact]
        public void IsCaseInsensitive_ForGet()
        {
            var cookie = MakeCookie("session");
            var c = Create();
            c.Add(cookie);
            c["SESSION"].ShouldBeSameAs(cookie);
        }

        [Fact]
        public void SetterStoresCookie()
        {
            var cookie = MakeCookie("session");
            var c = Create();
            c["session"] = cookie;
            c["session"].ShouldBeSameAs(cookie);
        }

        [Fact]
        public void SetterReplacesCookie_WhenNameAlreadyExists()
        {
            var first = new Cookie("session", Guid.NewGuid().ToString("N"));
            var second = new Cookie("session", Guid.NewGuid().ToString("N"));
            var c = Create();
            c["session"] = first;
            c["session"] = second;
            c["session"].ShouldBeSameAs(second);
            c.Count.ShouldBe(1);
        }

        [Fact]
        public void SetterIsCaseInsensitive_ForNameMatch()
        {
            var cookie = new Cookie("session", Guid.NewGuid().ToString("N"));
            var c = Create();
            c["SESSION"] = cookie;
            c.Contains("session").ShouldBeTrue();
        }

        [Fact]
        public void SetterThrows_WhenValueIsNull()
        {
            Should.Throw<ArgumentNullException>(() => Create()["session"] = null!);
        }

        [Fact]
        public void SetterThrows_WhenCookieNameDoesNotMatchKey()
        {
            var cookie = MakeCookie("other");
            Should.Throw<ArgumentException>(() => Create()["session"] = cookie);
        }

        [Fact]
        public void SetterThrows_WhenSealed()
        {
            var cookie = MakeCookie("session");
            var ex = Should.Throw<InvalidOperationException>(() =>
                CreateSealed()["session"] = cookie);
            ex.Message.ShouldBe(ResponseCookieCollection.SealedCollectionMessage);
        }
    }

    public class ToHeaderValuesMethod
    {
        [Fact]
        public void ReturnsOneStringPerCookie()
        {
            var c = Create();
            c.Add(MakeCookie("a"));
            c.Add(MakeCookie("b"));
            c.ToHeaderValues().Count().ShouldBe(2);
        }

        [Fact]
        public void ReturnsToStringForEachCookie()
        {
            var cookie = new Cookie("session", Guid.NewGuid().ToString("N"));
            var c = Create();
            c.Add(cookie);
            c.ToHeaderValues().Single().ShouldBe(cookie.ToString());
        }

        [Fact]
        public void ReturnsEmptySequence_WhenCollectionIsEmpty()
        {
            Create().ToHeaderValues().ShouldBeEmpty();
        }
    }

    public class GetEnumerator
    {
        [Fact]
        public void EnumeratesAllCookies()
        {
            var c = Create();
            c.Add(MakeCookie("a"));
            c.Add(MakeCookie("b"));
            c.Add(MakeCookie("c"));

            var count = 0;
            foreach (var _ in c) count++;
            count.ShouldBe(3);
        }

        [Fact]
        public void YieldsEmptySequence_WhenCollectionIsEmpty()
        {
            var count = 0;
            foreach (var _ in Create()) count++;
            count.ShouldBe(0);
        }
    }
}