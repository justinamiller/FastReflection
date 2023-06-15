using FastReflection.Implementation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FastReflection
{
    public interface IMemberSet : IEnumerable<IMember>, IList<IMember>
    {
        /// <summary>
        /// Get a member by index
        /// </summary>
        IMember this[int index] { get; }
        /// <summary>
        /// The number of members defined for this type
        /// </summary>
        int Count { get; }

    }
}
