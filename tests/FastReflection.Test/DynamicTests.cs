using FastReflection.Implementation;
using Microsoft.CSharp.RuntimeBinder;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FastReflection.Test
{
    [TestClass]
    public class DynamicTests
    {
        [TestMethod]
        public void TestReadValid()
        {
            dynamic expando = new ExpandoObject();
            expando.A = 123;
            expando.B = "def";
            var wrap = ObjectAccessor.Create((object)expando);

            Assert.AreEqual(123, wrap["A"]);
            Assert.AreEqual("def", wrap["B"]);
        }
        [TestMethod]
        public void TestReadInvalid()
        {
            Assert.ThrowsException<RuntimeBinderException>(() =>
            {
                dynamic expando = new ExpandoObject();
                var wrap = ObjectAccessor.Create((object)expando);
                Assert.AreEqual(123, wrap["C"]);
            });
        }
        [TestMethod]
        public void TestWrite()
        {
            dynamic expando = new ExpandoObject();
            var wrap = ObjectAccessor.Create((object)expando);
            wrap["A"] = 123;
            wrap["B"] = "def";

            Assert.AreEqual(123, expando.A);
            Assert.AreEqual("def", expando.B);
        }

        [TestMethod]
        public void DynamicByTypeWrapper()
        {
            var obj = new ExpandoObject();
            ((dynamic)obj).Foo = "bar";
            var accessor = TypeAccessor.Create(obj.GetType());

            Assert.AreEqual("bar", accessor[obj, "Foo"]);
            accessor[obj, "Foo"] = "BAR";
            string result = ((dynamic)obj).Foo;
            Assert.AreEqual("BAR", result);
        }
    }
}
