﻿using System;
using System.Collections;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using FastReflection.Infrastructure;

namespace FastReflection.Implementation
{
    /// <summary>
    /// Provides by-name member-access to objects of a given type
    /// </summary>
    public abstract class TypeAccessor : ITypeAccessor
    {
        // hash-table has better read-without-locking semantics than dictionary
        private static readonly Hashtable _publicAccessorsOnly = new Hashtable();
        private static readonly Hashtable _nonPublicAccessors = new Hashtable();

        internal static void ClearCache()
        {
            _publicAccessorsOnly.Clear();
            _nonPublicAccessors.Clear();
        }

        /// <summary>
        /// Does this type support new instances via a parameterless constructor?
        /// </summary>
        public virtual bool CreateNewSupported { get { return false; } }
        /// <summary>
        /// Create a new instance of this type
        /// </summary>
        public virtual object CreateNew() { throw new NotSupportedException(); }

        /// <summary>
        /// Can this type be queried for member availability?
        /// </summary>
        public virtual bool GetMembersSupported { get { return false; } }
        /// <summary>
        /// Query the members available for this type
        /// </summary>
        public virtual IMemberSet GetMembers() { throw new NotSupportedException(); }

        /// <summary>
        /// Provides a type-specific accessor, allowing by-name access for all objects of that type
        /// </summary>
        /// <remarks>The accessor is cached internally; a pre-existing accessor may be returned</remarks>
        public static TypeAccessor Create(Type type)
        {
            return Create(type, false);
        }

        /// <summary>
        /// Provides a type-specific accessor, allowing by-name access for all objects of that type
        /// </summary>
        /// <remarks>The accessor is cached internally; a pre-existing accessor may be returned</remarks>
        public static TypeAccessor Create(Type type, bool allowNonPublicAccessors)
        {
            if (type == null) throw new ArgumentNullException("type");
            var lookup = allowNonPublicAccessors ? _nonPublicAccessors : _publicAccessorsOnly;
            TypeAccessor obj = (TypeAccessor)lookup[type];
            if (obj != null) return obj;

            lock (lookup)
            {
                // double-check
                obj = (TypeAccessor)lookup[type];
                if (obj != null) return obj;

                obj = CreateNew(type, allowNonPublicAccessors);

                lookup[type] = obj;
                return obj;
            }
        }
        sealed class DynamicAccessor : TypeAccessor
        {
            public static readonly DynamicAccessor Singleton = new DynamicAccessor();
            private DynamicAccessor() { }
            public override object this[object target, string name]
            {
                get { return CallSiteCache.GetValue(name, target); }
                set { CallSiteCache.SetValue(name, target, value); }
            }
        }

        static TypeAccessor()
        {
            // note this region is synchronized; only one is being created at a time so we don't need to stress about the builders
            var name = "FastReflection_dynamic";
            AssemblyName assemblyName = new AssemblyName(name);
            var assembly = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
            _module = assembly.DefineDynamicModule(name);
        }

        private readonly static ModuleBuilder _module;
        private static int _counter;

        private static int GetNextCounterValue()
        {
            return Interlocked.Increment(ref _counter);
        }

        static readonly MethodInfo tryGetValue = typeof(Dictionary<string, int>).GetMethod("TryGetValue");
        private static void WriteMapImpl(ILGenerator il, Type type, List<MemberInfo> members, FieldBuilder mapField, bool allowNonPublicAccessors, bool isGet)
        {
            OpCode obj, index, value;

            Label fail = il.DefineLabel();
            if (mapField == null)
            {
                index = OpCodes.Ldarg_0;
                obj = OpCodes.Ldarg_1;
                value = OpCodes.Ldarg_2;
            }
            else
            {
                il.DeclareLocal(typeof(int));
                index = OpCodes.Ldloc_0;
                obj = OpCodes.Ldarg_1;
                value = OpCodes.Ldarg_3;

                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldfld, mapField);
                il.Emit(OpCodes.Ldarg_2);
                il.Emit(OpCodes.Ldloca_S, (byte)0);
                il.EmitCall(OpCodes.Callvirt, tryGetValue, null);
                il.Emit(OpCodes.Brfalse, fail);
            }
            Label[] labels = new Label[members.Count];
            for (int i = 0; i < labels.Length; i++)
            {
                labels[i] = il.DefineLabel();
            }
            il.Emit(index);
            il.Emit(OpCodes.Switch, labels);
            il.MarkLabel(fail);
            il.Emit(OpCodes.Ldstr, "name");
            il.Emit(OpCodes.Newobj, typeof(ArgumentOutOfRangeException).GetConstructor(new Type[] { typeof(string) }));
            il.Emit(OpCodes.Throw);
            for (int i = 0; i < labels.Length; i++)
            {
                il.MarkLabel(labels[i]);
                var member = members[i];
                bool isFail = true;

                void WriteField(FieldInfo fieldToWrite)
                {
                    if (!fieldToWrite.FieldType.IsByRef)
                    {
                        il.Emit(obj);
                        Cast(il, type, true);
                        if (isGet)
                        {
                            il.Emit(OpCodes.Ldfld, fieldToWrite);
                            if (fieldToWrite.FieldType.IsValueType)
                                il.Emit(OpCodes.Box, fieldToWrite.FieldType);
                        }
                        else
                        {
                            il.Emit(value);
                            Cast(il, fieldToWrite.FieldType, false);
                            il.Emit(OpCodes.Stfld, fieldToWrite);
                        }
                        il.Emit(OpCodes.Ret);
                        isFail = false;
                    }
                }
                if (member is FieldInfo field)
                {
                    WriteField(field);
                }
                else if (member is PropertyInfo prop)
                {
                    var propType = prop.PropertyType;
                    bool isByRef = propType.IsByRef, isValid = true;
                    if (isByRef)
                    {
                        if (!isGet && prop.CustomAttributes.Any(x => x.AttributeType.FullName == "System.Runtime.CompilerServices.IsReadOnlyAttribute"))
                        {
                            isValid = false; // can't assign indirectly to ref-readonly
                        }
                        propType = propType.GetElementType(); // from "ref Foo" to "Foo"
                    }

                    var accessor = (isGet | isByRef) ? prop.GetGetMethod(allowNonPublicAccessors) : prop.GetSetMethod(allowNonPublicAccessors);
                    if (accessor == null && allowNonPublicAccessors && !isByRef)
                    {
                        // No getter/setter, use backing field instead if it exists
                        var backingField = $"<{prop.Name}>k__BackingField";
                        field = prop.DeclaringType?.GetField(backingField, BindingFlags.Instance | BindingFlags.NonPublic);

                        if (field != null)
                        {
                            WriteField(field);
                        }
                    }
                    else if (isValid && prop.CanRead && accessor != null)
                    {
                        il.Emit(obj);
                        Cast(il, type, true); // cast the input object to the right target type

                        if (isGet)
                        {
                            il.EmitCall(type.IsValueType ? OpCodes.Call : OpCodes.Callvirt, accessor, null);
                            if (isByRef)
                                il.Emit(OpCodes.Ldobj, propType); // defererence if needed
                            if (propType.IsValueType)
                                il.Emit(OpCodes.Box, propType); // box the value if needed
                        }
                        else
                        {
                            // when by-ref, we get the target managed pointer *first*, i.e. put obj.TheRef on the stack
                            if (isByRef)
                                il.EmitCall(type.IsValueType ? OpCodes.Call : OpCodes.Callvirt, accessor, null);

                            // load the new value, and type it
                            il.Emit(value);
                            Cast(il, propType, false);

                            if (isByRef)
                            {   // assign to the managed pointer
                                il.Emit(OpCodes.Stobj, propType);
                            }
                            else
                            {   // call the setter
                                il.EmitCall(type.IsValueType ? OpCodes.Call : OpCodes.Callvirt, accessor, null);
                            }
                        }
                        il.Emit(OpCodes.Ret);
                        isFail = false;
                    }
                }
                if (isFail)
                    il.Emit(OpCodes.Br, fail);
            }
        }

        /// <summary>
        /// A TypeAccessor based on a Type implementation, with available member metadata
        /// </summary>
        protected abstract class RuntimeTypeAccessor : TypeAccessor
        {
            /// <summary>
            /// Returns the Type represented by this accessor
            /// </summary>
            protected abstract Type Type { get; }

            /// <summary>
            /// Can this type be queried for member availability?
            /// </summary>
            public override bool GetMembersSupported { get { return true; } }
            private MemberSet _members;
            /// <summary>
            /// Query the members available for this type
            /// </summary>
            public override MemberSet GetMembers()
            {
                return _members ?? (_members = new MemberSet(Type));
            }
        }
        sealed class DelegateAccessor : RuntimeTypeAccessor
        {
            private readonly Dictionary<string, int> _map;
            private readonly Func<int, object, object> _getter;
            private readonly Action<int, object, object> _setter;
            private readonly Func<object> _ctor;
            private readonly Type _type;
            protected override Type Type
            {
                get { return _type; }
            }
            public DelegateAccessor(Dictionary<string, int> map, Func<int, object, object> getter, Action<int, object, object> setter, Func<object> ctor, Type type)
            {
                this._map = map;
                this._getter = getter;
                this._setter = setter;
                this._ctor = ctor;
                this._type = type;
            }
            public override bool CreateNewSupported { get { return _ctor != null; } }
            public override object CreateNew()
            {
                return _ctor != null ? _ctor() : base.CreateNew();
            }
            public override object this[object target, string name]
            {
                get
                {
                    int index;
                    if (_map.TryGetValue(name, out index))
                        return _getter(index, target);

                    //else
                    throw new ArgumentOutOfRangeException("name");
                }
                set
                {
                    int index;
                    if (_map.TryGetValue(name, out index))
                        _setter(index, target, value);

                    //else
                    throw new ArgumentOutOfRangeException("name");
                }
            }
        }

        private static bool IsFullyPublic(Type type, PropertyInfo[] props, FieldInfo[] fields, bool allowNonPublicAccessors)
        {
            while (type.IsNestedPublic) type = type.DeclaringType;
            if (!type.IsPublic) return false;

            if (allowNonPublicAccessors)
            {
                for (int i = 0; i < props.Length; i++)
                {
                    var prop = props[i];

                    if (prop.GetGetMethod(true) != null && prop.GetGetMethod(false) == null)
                        return false; // non-public getter
                    if (prop.GetSetMethod(true) != null && prop.GetSetMethod(false) == null)
                        return false; // non-public setter
                }
                for (int i = 0; i < fields.Length; i++)
                {
                    if (!fields[i].IsPublic)
                        return false;
                }
            }

            return true;
        }
        static TypeAccessor CreateNew(Type type, bool allowNonPublicAccessors)
        {
            if (typeof(IDynamicMetaObjectProvider).IsAssignableFrom(type))
            {
                return DynamicAccessor.Singleton;
            }

            PropertyInfo[] props = type.GetTypeAndInterfaceProperties(BindingFlags.Public | BindingFlags.Instance);
            FieldInfo[] fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);
            Dictionary<string, int> map = new Dictionary<string, int>();
            List<MemberInfo> members = new List<MemberInfo>(props.Length + fields.Length);
            int i = 0;
            //get properties
            for (var x = 0; x < props.Length; x++)
            {
                var prop = props[x];
                var name = prop.Name;
                if (!map.ContainsKey(name) && prop.GetIndexParameters().Length == 0)
                {
                    map.Add(name, i++);
                    members.Add(prop);
                }
            }

            //get fields
            for (var x = 0; x < fields.Length; x++)
            {
                var field = fields[x];
                var name = field.Name;
                if (!map.ContainsKey(name))
                {
                    map.Add(name, i++);
                    members.Add(field);
                }
            }

            ConstructorInfo ctor = null;
            if (type.IsClass && !type.IsAbstract)
            {
                ctor = type.GetConstructor(Type.EmptyTypes);
            }
            ILGenerator il;
            if (!IsFullyPublic(type, props, fields, allowNonPublicAccessors))
            {
                DynamicMethod dynGetter = new DynamicMethod(type.FullName + "_get", typeof(object), new Type[] { typeof(int), typeof(object) }, type, true);
                DynamicMethod dynSetter = new DynamicMethod(type.FullName + "_set", null, new Type[] { typeof(int), typeof(object), typeof(object) }, type, true);
                WriteMapImpl(dynGetter.GetILGenerator(), type, members, null, allowNonPublicAccessors, true);
                WriteMapImpl(dynSetter.GetILGenerator(), type, members, null, allowNonPublicAccessors, false);
                DynamicMethod dynCtor = null;
                if (ctor != null)
                {
                    dynCtor = new DynamicMethod(type.FullName + "_ctor", typeof(object), Type.EmptyTypes, type, true);
                    il = dynCtor.GetILGenerator();
                    il.Emit(OpCodes.Newobj, ctor);
                    il.Emit(OpCodes.Ret);
                }
                return new DelegateAccessor(
                    map,
                    (Func<int, object, object>)dynGetter.CreateDelegate(typeof(Func<int, object, object>)),
                    (Action<int, object, object>)dynSetter.CreateDelegate(typeof(Action<int, object, object>)),
                    dynCtor == null ? null : (Func<object>)dynCtor.CreateDelegate(typeof(Func<object>)), type);
            }


            TypeAttributes attribs = typeof(TypeAccessor).Attributes;
            TypeBuilder tb = _module.DefineType("FastReflection_dynamic." + type.Name + "_" + GetNextCounterValue(),
                (attribs | TypeAttributes.Sealed | TypeAttributes.Public) & ~(TypeAttributes.Abstract | TypeAttributes.NotPublic), typeof(RuntimeTypeAccessor));

            il = tb.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, new[] {
                typeof(Dictionary<string,int>)
            }).GetILGenerator();
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldarg_1);
            FieldBuilder mapField = tb.DefineField("_map", typeof(Dictionary<string, int>), FieldAttributes.InitOnly | FieldAttributes.Private);
            il.Emit(OpCodes.Stfld, mapField);
            il.Emit(OpCodes.Ret);


            PropertyInfo indexer = typeof(TypeAccessor).GetProperty("Item");
            MethodInfo baseGetter = indexer.GetGetMethod();
            MethodInfo baseSetter = indexer.GetSetMethod();
            MethodBuilder body = tb.DefineMethod(baseGetter.Name, baseGetter.Attributes & ~MethodAttributes.Abstract, typeof(object), new Type[] { typeof(object), typeof(string) });
            il = body.GetILGenerator();
            WriteMapImpl(il, type, members, mapField, allowNonPublicAccessors, true);
            tb.DefineMethodOverride(body, baseGetter);

            body = tb.DefineMethod(baseSetter.Name, baseSetter.Attributes & ~MethodAttributes.Abstract, null, new Type[] { typeof(object), typeof(string), typeof(object) });
            il = body.GetILGenerator();
            WriteMapImpl(il, type, members, mapField, allowNonPublicAccessors, false);
            tb.DefineMethodOverride(body, baseSetter);

            MethodInfo baseMethod;
            if (ctor != null)
            {
                baseMethod = typeof(TypeAccessor).GetProperty("CreateNewSupported").GetGetMethod();
                body = tb.DefineMethod(baseMethod.Name, baseMethod.Attributes, baseMethod.ReturnType, Type.EmptyTypes);
                il = body.GetILGenerator();
                il.Emit(OpCodes.Ldc_I4_1);
                il.Emit(OpCodes.Ret);
                tb.DefineMethodOverride(body, baseMethod);

                baseMethod = typeof(TypeAccessor).GetMethod("CreateNew");
                body = tb.DefineMethod(baseMethod.Name, baseMethod.Attributes, baseMethod.ReturnType, Type.EmptyTypes);
                il = body.GetILGenerator();
                il.Emit(OpCodes.Newobj, ctor);
                il.Emit(OpCodes.Ret);
                tb.DefineMethodOverride(body, baseMethod);
            }

            baseMethod = typeof(RuntimeTypeAccessor).GetProperty("Type", BindingFlags.NonPublic | BindingFlags.Instance).GetGetMethod(true);
            body = tb.DefineMethod(baseMethod.Name, baseMethod.Attributes & ~MethodAttributes.Abstract, baseMethod.ReturnType, Type.EmptyTypes);
            il = body.GetILGenerator();
            il.Emit(OpCodes.Ldtoken, type);
            il.Emit(OpCodes.Call, typeof(Type).GetMethod("GetTypeFromHandle"));
            il.Emit(OpCodes.Ret);
            tb.DefineMethodOverride(body, baseMethod);

            var accessor = (TypeAccessor)Activator.CreateInstance(tb.CreateTypeInfo().AsType(), map);
            return accessor;
        }

        private static void Cast(ILGenerator il, Type type, bool valueAsPointer)
        {
            if (type == typeof(object))
                return;
            if (type.IsValueType)
            {
                if (valueAsPointer)
                {
                    il.Emit(OpCodes.Unbox, type);
                }
                else
                {
                    il.Emit(OpCodes.Unbox_Any, type);
                }
            }
            else
            {
                il.Emit(OpCodes.Castclass, type);
            }
        }

        public object Get(object target, string name)
        {
            return this[target, name];
        }

        public void Set(object target, string name, object value)
        {
            this[target, name] = value;
        }

        /// <summary>
        /// Get or set the value of a named member on the target instance
        /// </summary>
        public abstract object this[object target, string name]
        {
            get;
            set;
        }
    }
}
