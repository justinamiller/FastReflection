using System.Collections;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.CSharp.RuntimeBinder;

namespace FastReflection.Implementation
{
    internal static class CallSiteCache
    {
        private static readonly Hashtable _getters = new Hashtable();
        private static readonly Hashtable _setters = new Hashtable();

        internal static object GetValue(string name, object target)
        {
            CallSite<Func<CallSite, object, object>> callSite = (CallSite<Func<CallSite, object, object>>)_getters[name];
            if (callSite == null)
            {
                CallSite<Func<CallSite, object, object>> newSite = CallSite<Func<CallSite, object, object>>.Create(Microsoft.CSharp.RuntimeBinder.Binder.GetMember(CSharpBinderFlags.None, name, typeof(CallSiteCache), new CSharpArgumentInfo[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) }));
                lock (_getters)
                {
                    callSite = (CallSite<Func<CallSite, object, object>>)_getters[name];
                    if (callSite == null)
                    {
                        _getters[name] = callSite = newSite;
                    }
                }
            }

            return callSite.Target(callSite, target);
        }
        internal static void SetValue(string name, object target, object value)
        {
            CallSite<Func<CallSite, object, object, object>> callSite = (CallSite<Func<CallSite, object, object, object>>)_setters[name];
            if (callSite == null)
            {
                CallSite<Func<CallSite, object, object, object>> newSite = CallSite<Func<CallSite, object, object, object>>.Create(Microsoft.CSharp.RuntimeBinder.Binder.SetMember(CSharpBinderFlags.None, name, typeof(CallSiteCache), new CSharpArgumentInfo[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null), CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType, null) }));
                lock (_setters)
                {
                    callSite = (CallSite<Func<CallSite, object, object, object>>)_setters[name];
                    if (callSite == null)
                    {
                        _setters[name] = callSite = newSite;
                    }
                }
            }
            callSite.Target(callSite, target, value);
        }

    }
}