using System.Data;
using FastReflection.Implementation;

namespace FastReflection.Test
{
    [TestClass]
    public class BasicTests
    {
        [TestMethod]
        public void BasicReadTest_PropsOnClass()
        {
            var now = DateTime.Now;

            var obj = new PropsOnClass() { A = 123, B = "abc", C = now, D = null };

            var access = TypeAccessor.Create(typeof(PropsOnClass));

            Assert.AreEqual(123, access[obj, "A"]);
            Assert.AreEqual("abc", access[obj, "B"]);
            Assert.AreEqual(now, access[obj, "C"]);
            Assert.IsNull(access[obj, "D"]);
        }

        [TestMethod]
        public void BasicWriteTest_PropsOnClass()
        {
            var now = DateTime.Now;

            var obj = new PropsOnClass();

            var access = TypeAccessor.Create(typeof(PropsOnClass));

            access[obj, "A"] = 123;
            access[obj, "B"] = "abc";
            access[obj, "C"] = now;
            access[obj, "D"] = null;

            Assert.AreEqual(123, obj.A);
            Assert.AreEqual("abc", obj.B);
            Assert.AreEqual(now, obj.C);
            Assert.IsNull(obj.D);
        }

        [TestMethod]
        public void Getmembers()
        {
            var access = TypeAccessor.Create(typeof(PropsOnClass));
            Assert.IsTrue(access.GetMembersSupported);
            var members = access.GetMembers();
            Assert.AreEqual(4, members.Count);
            Assert.AreEqual("A", members[0].Name);
            Assert.AreEqual("B", members[1].Name);
            Assert.AreEqual("C", members[2].Name);
            Assert.AreEqual("D", members[3].Name);
        }

        [TestMethod]
        public void BasicReadTest_PropsOnClass_ViaWrapper()
        {
            var now = DateTime.Now;

            var obj = new PropsOnClass() { A = 123, B = "abc", C = now, D = null };

            var wrapper = ObjectAccessor.Create(obj);

            Assert.AreEqual(123, wrapper["A"]);
            Assert.AreEqual("abc", wrapper["B"]);
            Assert.AreEqual(now, wrapper["C"]);
            Assert.IsNull(wrapper["D"]);
        }

        [TestMethod]
        public void BasicWriteTest_PropsOnClass_ViaWrapper()
        {
            var now = DateTime.Now;

            var obj = new PropsOnClass();

            var wrapper = ObjectAccessor.Create(obj);

            wrapper["A"] = 123;
            wrapper["B"] = "abc";
            wrapper["C"] = now;
            wrapper["D"] = null;

            Assert.AreEqual(123, obj.A);
            Assert.AreEqual("abc", obj.B);
            Assert.AreEqual(now, obj.C);
            Assert.IsNull(obj.D);
        }

        [TestMethod]
        public void BasicReadTest_FieldsOnClass()
        {
            var now = DateTime.Now;

            var obj = new FieldsOnClass() { A = 123, B = "abc", C = now, D = null };

            var access = TypeAccessor.Create(typeof(FieldsOnClass));

            Assert.AreEqual(123, access[obj, "A"]);
            Assert.AreEqual("abc", access[obj, "B"]);
            Assert.AreEqual(now, access[obj, "C"]);
            Assert.IsNull(access[obj, "D"]);
        }

        [TestMethod]
        public void BasicWriteTest_FieldsOnClass()
        {
            var now = DateTime.Now;

            var obj = new FieldsOnClass();

            var access = TypeAccessor.Create(typeof(FieldsOnClass));

            access[obj, "A"] = 123;
            access[obj, "B"] = "abc";
            access[obj, "C"] = now;
            access[obj, "D"] = null;

            Assert.AreEqual(123, obj.A);
            Assert.AreEqual("abc", obj.B);
            Assert.AreEqual(now, obj.C);
            Assert.IsNull(obj.D);
        }

        [TestMethod]
        public void BasicReadTest_PropsOnStruct()
        {
            var now = DateTime.Now;

            var obj = new PropsOnStruct() { A = 123, B = "abc", C = now, D = null };

            var access = TypeAccessor.Create(typeof(PropsOnStruct));

            Assert.AreEqual(123, access[obj, "A"]);
            Assert.AreEqual("abc", access[obj, "B"]);
            Assert.AreEqual(now, access[obj, "C"]);
            Assert.IsNull(access[obj, "D"]);
        }

        [TestMethod]
        public void BasicWriteTest_PropsOnStruct()
        {
            var now = DateTime.Now;

            object obj = new PropsOnStruct { A = 1 };

            var access = TypeAccessor.Create(typeof(PropsOnStruct));

            access[obj, "A"] = 123;

            Assert.AreEqual(123, ((PropsOnStruct)obj).A);
        }

        [TestMethod]
        public void BasicReadTest_FieldsOnStruct()
        {
            var now = DateTime.Now;

            var obj = new FieldsOnStruct() { A = 123, B = "abc", C = now, D = null };

            var access = TypeAccessor.Create(typeof(FieldsOnStruct));

            Assert.AreEqual(123, access[obj, "A"]);
            Assert.AreEqual("abc", access[obj, "B"]);
            Assert.AreEqual(now, access[obj, "C"]);
            Assert.IsNull(access[obj, "D"]);
        }

        [TestMethod]
        public void BasicWriteTest_FieldsOnStruct()
        {
            var now = DateTime.Now;

            object obj = new FieldsOnStruct();

            var access = TypeAccessor.Create(typeof(FieldsOnStruct));

            access[obj, "A"] = 123;
            Assert.AreEqual(123, ((FieldsOnStruct)obj).A);
        }

        [TestMethod]
        public void WriteInvalidMember()
        {
            Assert.ThrowsException<ArgumentOutOfRangeException>(() =>
            {
                var access = TypeAccessor.Create(typeof(PropsOnClass));
                var obj = new PropsOnClass();
                access[obj, "doesnotexist"] = "abc";
            });
        }

        [TestMethod]
        public void ReadInvalidMember()
        {
            Assert.ThrowsException<ArgumentOutOfRangeException>(() =>
            {
                var access = TypeAccessor.Create(typeof(PropsOnClass));
                var obj = new PropsOnClass();
                object value = access[obj, "doesnotexist"];
            });
        }

        [TestMethod]
        public void GetSameAccessor()
        {
            var x = TypeAccessor.Create(typeof(PropsOnClass));
            var y = TypeAccessor.Create(typeof(PropsOnClass));
            Assert.AreSame(x, y);
        }

        public class PropsOnClass
        {
            public int A { get; set; }
            public string B { get; set; }
            public DateTime? C { get; set; }
            public decimal? D { get; set; }
        }
        public class FieldsOnClass
        {
            public int A;
            public string B;
            public DateTime? C;
            public decimal? D;
        }
        public struct PropsOnStruct
        {
            public int A { get; set; }
            public string B { get; set; }
            public DateTime? C { get; set; }
            public decimal? D { get; set; }
        }
        public struct FieldsOnStruct
        {
            public int A;
            public string B;
            public DateTime? C;
            public decimal? D;
        }

        public interface IPropsOnInterfaceBase
        {
            int A { get; set; }
            string B { get; set; }
            DateTime? C { get; set; }
            decimal? D { get; set; }
        }
        public interface IPropsOnInterfaceBase2
        {
            int E { get; set; }
            string F { get; set; }
            DateTime? G { get; set; }
            decimal? H { get; set; }
        }
        public interface IPropsOnInheritedInterface : IPropsOnInterfaceBase
        {
            int I { get; set; }
            string J { get; set; }
            DateTime? K { get; set; }
            decimal? L { get; set; }
        }
        public interface IPropseOnComposedInterface : IPropsOnInterfaceBase, IPropsOnInterfaceBase2
        {
            int M { get; set; }
            string N { get; set; }
            DateTime? O { get; set; }
            decimal? P { get; set; }
        }
        public interface IPropseOnInheritedComposedInterface : IPropsOnInheritedInterface, IPropsOnInterfaceBase2
        {
            int M { get; set; }
            string N { get; set; }
            DateTime? O { get; set; }
            decimal? P { get; set; }
        }


        public class HasDefaultCtor { }
        public class HasNoDefaultCtor { public HasNoDefaultCtor(string s) { } }
        public abstract class IsAbstract { }

        [TestMethod]
        public void TestCtor()
        {
            var accessor = TypeAccessor.Create(typeof(HasNoDefaultCtor));
            Assert.IsFalse(accessor.CreateNewSupported);

            accessor = TypeAccessor.Create(typeof(IsAbstract));
            Assert.IsFalse(accessor.CreateNewSupported);

            Assert.AreNotEqual("DynamicAccessor", accessor.GetType().Name);
            Assert.AreNotEqual("DelegateAccessor", accessor.GetType().Name);

            accessor = TypeAccessor.Create(typeof(HasDefaultCtor));
            Assert.IsTrue(accessor.CreateNewSupported);
            object obj = accessor.CreateNew();
            Assert.IsInstanceOfType(obj, typeof(HasDefaultCtor));
        }

        public class HasGetterNoSetter
        {
            public int Foo { get { return 5; } }
        }
        [TestMethod]
        public void TestHasGetterNoSetter()
        {
            var obj = new HasGetterNoSetter();
            var acc = TypeAccessor.Create(typeof(HasGetterNoSetter));
            Assert.AreEqual(5, acc[obj, "Foo"]);
        }
        public class HasGetterPrivateSetter
        {
            public int Foo { get; private set; }
            public HasGetterPrivateSetter(int value) { Foo = value; }
        }
        [TestMethod]
        public void TestHasGetterPrivateSetter()
        {
            var obj = new HasGetterPrivateSetter(5);
            var acc = TypeAccessor.Create(typeof(HasGetterPrivateSetter));
            Assert.AreEqual(5, acc[obj, "Foo"]);
        }

        public class MixedAccess
        {
            public MixedAccess()
            {
                Foo = Bar = Alpha = Beta = 2;
            }
            public int Foo { get; private set; }
            public int Bar { private get; set; }
            public readonly int Alpha;
            public int Beta { get; }
            public int Theta { get { return 5; } }
        }

        [TestMethod]
        public void TestMixedAccess()
        {
            TypeAccessor acc0 = TypeAccessor.Create(typeof(MixedAccess)),
                         acc1 = TypeAccessor.Create(typeof(MixedAccess), false),
                         acc2 = TypeAccessor.Create(typeof(MixedAccess), true);

            Assert.AreSame(acc0, acc1);
            Assert.AreNotSame(acc0, acc2);

            var obj = new MixedAccess();
            Assert.AreEqual(2, acc0[obj, "Foo"]);
            Assert.AreEqual(2, acc2[obj, "Foo"]);
            Assert.AreEqual(2, acc2[obj, "Bar"]);

            acc0[obj, "Bar"] = 3;
            Assert.AreEqual(3, acc2[obj, "Bar"]);
            acc2[obj, "Bar"] = 4;
            Assert.AreEqual(4, acc2[obj, "Bar"]);
            acc2[obj, "Foo"] = 5;
            Assert.AreEqual(5, acc0[obj, "Foo"]);
            acc2[obj, "Alpha"] = 6;
            Assert.AreEqual(6, acc2[obj, "Alpha"]);
            acc2[obj, "Beta"] = 7;
            Assert.AreEqual(7, acc2[obj, "Beta"]);

            Assert.ThrowsException<ArgumentOutOfRangeException>(() =>
            {
                int i = (int)acc0[obj, "Bar"];
            });
            Assert.ThrowsException<ArgumentOutOfRangeException>(() =>
            {
                acc0[obj, "Foo"] = 6;
            });
            Assert.ThrowsException<ArgumentOutOfRangeException>(() =>
            {
                acc0[obj, "Beta"] = 7;
            });
            Assert.ThrowsException<ArgumentOutOfRangeException>(() =>
            {
                acc0[obj, "Theta"] = 8;
            });
        }

        public class ObjectReaderType
        {
            public int A { get; set; }
            public string B { get; set; }
            public byte C { get; set; }
            public int? D { get; set; }
        }

        public class HazStaticProperty
        {
            public int Foo { get; set; }
            public static int Bar { get; set; }

            public int Foo2 => 2;
            public static int Bar2 => 4;
        }

        [TestMethod]
        public void IgnoresStaticProperty()
        {
            var obj = new HazStaticProperty();
            var acc = TypeAccessor.Create(typeof(HazStaticProperty));
            var memberNames = string.Join(",", acc.GetMembers().Select(x => x.Name).OrderBy(_ => _));
            Assert.AreEqual("Foo,Foo2", memberNames);
        }

        [TestMethod]
        public void TestGetMembersOnInterface()
        {
            var access = TypeAccessor.Create(typeof(IPropsOnInterfaceBase));
            Assert.IsTrue(access.GetMembersSupported);
            var members = access.GetMembers();
            Assert.AreEqual(4, members.Count);
            Assert.AreEqual("A", members[0].Name);
            Assert.AreEqual("B", members[1].Name);
            Assert.AreEqual("C", members[2].Name);
            Assert.AreEqual("D", members[3].Name);
        }

        [TestMethod]
        public void TestGetMembersOnInheritedInterface()
        {
            var access = TypeAccessor.Create(typeof(IPropsOnInheritedInterface));
            Assert.IsTrue(access.GetMembersSupported);
            var members = access.GetMembers();
            Assert.AreEqual(8, members.Count);
            Assert.AreEqual("A", members[0].Name);
            Assert.AreEqual("B", members[1].Name);
            Assert.AreEqual("C", members[2].Name);
            Assert.AreEqual("D", members[3].Name);
            Assert.AreEqual("I", members[4].Name);
            Assert.AreEqual("J", members[5].Name);
            Assert.AreEqual("K", members[6].Name);
            Assert.AreEqual("L", members[7].Name);
        }

        [TestMethod]
        public void TestGetMembersOnComposedInterface()
        {
            var access = TypeAccessor.Create(typeof(IPropseOnComposedInterface));
            Assert.IsTrue(access.GetMembersSupported);
            var members = access.GetMembers();
            Assert.AreEqual(12, members.Count);
            Assert.AreEqual("A", members[0].Name);
            Assert.AreEqual("B", members[1].Name);
            Assert.AreEqual("C", members[2].Name);
            Assert.AreEqual("D", members[3].Name);
            Assert.AreEqual("E", members[4].Name);
            Assert.AreEqual("F", members[5].Name);
            Assert.AreEqual("G", members[6].Name);
            Assert.AreEqual("H", members[7].Name);
            Assert.AreEqual("M", members[8].Name);
            Assert.AreEqual("N", members[9].Name);
            Assert.AreEqual("O", members[10].Name);
            Assert.AreEqual("P", members[11].Name);
        }

        [TestMethod]
        public void TestGetMembersOnInheritedComposedInterface()
        {
            var access = TypeAccessor.Create(typeof(IPropseOnInheritedComposedInterface));
            Assert.IsTrue(access.GetMembersSupported);
            var members = access.GetMembers();
            Assert.AreEqual(16, members.Count);
            Assert.AreEqual("A", members[0].Name);
            Assert.AreEqual("B", members[1].Name);
            Assert.AreEqual("C", members[2].Name);
            Assert.AreEqual("D", members[3].Name);
            Assert.AreEqual("E", members[4].Name);
            Assert.AreEqual("F", members[5].Name);
            Assert.AreEqual("G", members[6].Name);
            Assert.AreEqual("H", members[7].Name);
            Assert.AreEqual("I", members[8].Name);
            Assert.AreEqual("J", members[9].Name);
            Assert.AreEqual("K", members[10].Name);
            Assert.AreEqual("L", members[11].Name);
            Assert.AreEqual("M", members[12].Name);
            Assert.AreEqual("N", members[13].Name);
            Assert.AreEqual("O", members[14].Name);
            Assert.AreEqual("P", members[15].Name);
        }

    }
}