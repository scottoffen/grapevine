using Grapevine;

namespace Grapevine.Abstractions.Tests;

public class LocalsTests
{
    public class GetMethod
    {
        [Fact]
        public void ReturnsValue_WhenKeyExists()
        {
            var locals = new Locals();
            locals["key"] = "value";
            locals.Get("key").ShouldBe("value");
        }

        [Fact]
        public void ReturnsNull_WhenKeyDoesNotExist()
        {
            var locals = new Locals();
            locals.Get("missing").ShouldBeNull();
        }

        [Fact]
        public void WorksWithIntKey()
        {
            var locals = new Locals();
            locals[42] = "forty-two";
            locals.Get(42).ShouldBe("forty-two");
        }

        [Fact]
        public void WorksWithObjectKey()
        {
            var locals = new Locals();
            var key = new object();
            locals[key] = "value";
            locals.Get(key).ShouldBe("value");
        }
    }

    public class GetAsMethod
    {
        [Fact]
        public void ReturnsTypedValue_WhenKeyExists()
        {
            var locals = new Locals();
            locals["key"] = "value";
            locals.GetAs<string>("key").ShouldBe("value");
        }

        [Fact]
        public void ReturnsDefault_WhenKeyDoesNotExist_ReferenceType()
        {
            var locals = new Locals();
            locals.GetAs<string>("missing").ShouldBeNull();
        }

        [Fact]
        public void ReturnsDefault_WhenKeyDoesNotExist_ValueType()
        {
            var locals = new Locals();
            locals.GetAs<int>("missing").ShouldBe(default);
        }

        [Fact]
        public void ReturnsDefault_WhenKeyDoesNotExist_NullableValueType()
        {
            var locals = new Locals();
            locals.GetAs<int?>("missing").ShouldBeNull();
        }

        [Fact]
        public void ReturnsCastValue_ForComplexType()
        {
            var locals = new Locals();
            var list = new List<string> { "a", "b" };
            locals["list"] = list;
            locals.GetAs<List<string>>("list").ShouldBeSameAs(list);
        }
    }

    public class GetOrAddAs_ValueOverload
    {
        [Fact]
        public void ReturnsExistingValue_WhenKeyExists()
        {
            var locals = new Locals();
            locals["key"] = "existing";
            locals.GetOrAddAs<string>("key", "new").ShouldBe("existing");
        }

        [Fact]
        public void AddsAndReturnsNewValue_WhenKeyDoesNotExist()
        {
            var locals = new Locals();
            locals.GetOrAddAs<string>("key", "new").ShouldBe("new");
        }

        [Fact]
        public void ValueIsStoredAfterAdd()
        {
            var locals = new Locals();
            locals.GetOrAddAs<string>("key", "new");
            locals.Get("key").ShouldBe("new");
        }

        [Fact]
        public void WorksWithValueType()
        {
            var locals = new Locals();
            locals.GetOrAddAs<int>("key", 42).ShouldBe(42);
        }
    }

    public class GetOrAddAs_FactoryOverload
    {
        [Fact]
        public void ReturnsExistingValue_WhenKeyExists()
        {
            var locals = new Locals();
            locals["key"] = "existing";
            locals.GetOrAddAs<string>("key", _ => "new").ShouldBe("existing");
        }

        [Fact]
        public void AddsAndReturnsFactoryValue_WhenKeyDoesNotExist()
        {
            var locals = new Locals();
            locals.GetOrAddAs<string>("key", _ => "factory").ShouldBe("factory");
        }

        [Fact]
        public void FactoryNotCalled_WhenKeyExists()
        {
            var locals = new Locals();
            locals["key"] = "existing";
            var factoryCalled = false;
            locals.GetOrAddAs<string>("key", _ => { factoryCalled = true; return "new"; });
            factoryCalled.ShouldBeFalse();
        }

        [Fact]
        public void FactoryReceivesKey_AsArgument()
        {
            var locals = new Locals();
            object? receivedKey = null;
            locals.GetOrAddAs<string>("key", k => { receivedKey = k; return "value"; });
            receivedKey.ShouldBe("key");
        }

        [Fact]
        public void ValueIsStoredAfterAdd()
        {
            var locals = new Locals();
            locals.GetOrAddAs<string>("key", _ => "factory");
            locals.Get("key").ShouldBe("factory");
        }

        [Fact]
        public void WorksWithValueType()
        {
            var locals = new Locals();
            locals.GetOrAddAs<int>("key", _ => 42).ShouldBe(42);
        }
    }
}