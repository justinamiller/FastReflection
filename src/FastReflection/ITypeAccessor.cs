using FastReflection.Implementation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FastReflection
{
    public interface ITypeAccessor
    {

        /// <summary>
        /// Does this type support new instances via a parameterless constructor?
        /// </summary>
        public bool CreateNewSupported { get; }

        /// <summary>
        /// Create a new instance of this type
        /// </summary>
        public object CreateNew();

        /// <summary>
        /// Can this type be queried for member availability?
        /// </summary>
        public bool GetMembersSupported { get; }
        /// <summary>
        /// Query the members available for this type
        /// </summary>
        public IMemberSet GetMembers();

        /// <summary>
        /// Get the value of a named member on the target instance
        /// </summary>
        object Get(object target, string name);

        /// <summary>
        /// set the value of a named member on the target instance
        /// </summary>
        void Set(object target, string name, object value);
    }
}
