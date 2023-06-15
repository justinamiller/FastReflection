using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FastReflection
{
    public interface IMember
    {
        /// <summary>
        /// The name of this member
        /// </summary>
        string Name { get; }

        /// <summary>
        /// The type of value stored in this member
        /// </summary>
        Type Type { get; }

        /// <summary>
        /// Is the attribute specified defined on this type
        /// </summary>
        bool IsDefined(Type attributeType);

        /// <summary>
        /// Getting Attribute Type
        /// </summary>
        Attribute GetAttribute(Type attributeType, bool inherit);

        /// <summary>
        /// Property Can Write
        /// </summary>
        bool CanWrite { get; }

        /// <summary>
        /// Property Can Read
        /// </summary>
        bool CanRead { get; }

    }
}
