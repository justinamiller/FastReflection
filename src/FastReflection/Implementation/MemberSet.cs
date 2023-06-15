using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FastReflection.Infrastructure;

namespace FastReflection.Implementation
{
    /// <summary>
    /// Represents an abstracted view of the members defined for a type
    /// </summary>
    public sealed class MemberSet : IMemberSet
    {
        private readonly IMember[] _members;
        internal MemberSet(Type type)
        {
            const BindingFlags PublicInstance = BindingFlags.Public | BindingFlags.Instance;
            _members = type.GetTypeAndInterfaceProperties(PublicInstance).Cast<MemberInfo>().Concat(type.GetFields(PublicInstance).Cast<MemberInfo>()).OrderBy(x => x.Name)
                .Select(member => new Member(member)).ToArray();
        }
        /// <summary>
        /// Return a sequence of all defined members
        /// </summary>
        public IEnumerator<IMember> GetEnumerator()
        {
            foreach (var member in _members) yield return member;
        }
        /// <summary>
        /// Get a member by index
        /// </summary>
        public IMember this[int index]
        {
            get { return _members[index]; }
        }
        /// <summary>
        /// The number of members defined for this type
        /// </summary>
        public int Count { get { return _members.Length; } }
        IMember IList<IMember>.this[int index]
        {
            get { return _members[index]; }
            set { throw new NotSupportedException(); }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() { return GetEnumerator(); }
        bool ICollection<IMember>.Remove(IMember item) { throw new NotSupportedException(); }
        void ICollection<IMember>.Add(IMember item) { throw new NotSupportedException(); }
        void ICollection<IMember>.Clear() { throw new NotSupportedException(); }
        void IList<IMember>.RemoveAt(int index) { throw new NotSupportedException(); }
        void IList<IMember>.Insert(int index, IMember item) { throw new NotSupportedException(); }

        bool ICollection<IMember>.Contains(IMember item) => _members.Contains(item);
        void ICollection<IMember>.CopyTo(IMember[] array, int arrayIndex) { _members.CopyTo(array, arrayIndex); }
        bool ICollection<IMember>.IsReadOnly { get { return true; } }
        int IList<IMember>.IndexOf(IMember member) { return Array.IndexOf(_members, member); }

    }
}
