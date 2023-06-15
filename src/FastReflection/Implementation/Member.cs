using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace FastReflection.Implementation
{
    /// <summary>
    /// Represents an abstracted view of an individual member defined for a type
    /// </summary>
    internal sealed class Member:IMember
    {
        private readonly MemberInfo _member;
        internal Member(MemberInfo member)
        {
            this._member = member;
        }
       
        /// <summary>
        /// The name of this member
        /// </summary>
        public string Name { get { return _member.Name; } }
        /// <summary>
        /// The type of value stored in this member
        /// </summary>
        public Type Type
        {
            get
            {
                if (_member is FieldInfo) return ((FieldInfo)_member).FieldType;
                if (_member is PropertyInfo) return ((PropertyInfo)_member).PropertyType;
                throw new NotSupportedException(_member.GetType().Name);
            }
        }

        /// <summary>
        /// Is the attribute specified defined on this type
        /// </summary>
        public bool IsDefined(Type attributeType)
        {
            if (attributeType == null) throw new ArgumentNullException(nameof(attributeType));
            return Attribute.IsDefined(_member, attributeType);
        }

        /// <summary>
        /// Getting Attribute Type
        /// </summary>
        public Attribute GetAttribute(Type attributeType, bool inherit)
            => Attribute.GetCustomAttribute(_member, attributeType, inherit);

        /// <summary>
        /// Property Can Write
        /// </summary>
        public bool CanWrite
        {
            get
            {
                switch (_member.MemberType)
                {
                    case MemberTypes.Property: return ((PropertyInfo)_member).CanWrite;
                    default: throw new NotSupportedException(_member.MemberType.ToString());
                }
            }
        }

        /// <summary>
        /// Property Can Read
        /// </summary>
        public bool CanRead
        {
            get
            {
                switch (_member.MemberType)
                {
                    case MemberTypes.Property: return ((PropertyInfo)_member).CanRead;
                    default: throw new NotSupportedException(_member.MemberType.ToString());
                }
            }
        }
    }
}
