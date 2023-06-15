using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FastReflection
{
    public interface IObjectAccessor
    {
        /// <summary>
        /// Get the value of a named member for the underlying object
        /// </summary>
        public object Get(string name);

        /// <summary>
        /// Set the value of a named member for the underlying object
        /// </summary>
        public void Set(string name, object value);

        /// <summary>
        /// The object represented by this instance
        /// </summary>
        public object Target { get; }
    }
}
