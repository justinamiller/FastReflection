using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using FastReflection.Implementation;

[assembly: InternalsVisibleTo("FastReflection.Test")]
namespace FastReflection
{
    public static class Accessor
    {
        /// <summary>
        /// clear any cached accessor information
        /// </summary>
        public static void ClearCache()
        {
            TypeAccessor.ClearCache();
        }
        /// <summary>
        /// Wraps an individual object, allowing by-name access to that instance
        /// </summary>
        public static IObjectAccessor Create(object target)
        {
            return ObjectAccessor.Create(target, false);
        }
        /// <summary>
        /// Wraps an individual object, allowing by-name access to that instance
        /// </summary>
        public static IObjectAccessor Create(object target, bool allowNonPublicAccessors)
        {
            return ObjectAccessor.Create(target, allowNonPublicAccessors);
        }

        /// <summary>
        /// Provides a type-specific accessor, allowing by-name access for all objects of that type
        /// </summary>
        /// <remarks>The accessor is cached internally; a pre-existing accessor may be returned</remarks>
        public static ITypeAccessor Create(Type type)
        {
            return TypeAccessor.Create(type, false);
        }

        /// <summary>
        /// Provides a type-specific accessor, allowing by-name access for all objects of that type
        /// </summary>
        /// <remarks>The accessor is cached internally; a pre-existing accessor may be returned</remarks>
        public static ITypeAccessor Create(Type type, bool allowNonPublicAccessors)
        {
            return TypeAccessor.Create(type, allowNonPublicAccessors);
        }
    }
}
