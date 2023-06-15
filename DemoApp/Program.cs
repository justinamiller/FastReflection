// See https://aka.ms/new-console-template for more information

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using DemoApp;
using FastReflection;
using FastReflection.Implementation;
using static FastReflection.Benchmark.Program;

//var accessor = Accessor.Create(typeof(FastReflectionPerformance));

var simpleObj = SimpleObject.Get();

////var result=FastReflection.Accessor.Create(simpleObj, true);
////var i = result.Target;
////var ii = result.Get("Name");
//var result2 = Accessor.Create(typeof(SimpleObject), true);
//var result3 = Accessor.Create(typeof(SimpleObject));
////result.ToString();

//simpleObj.ToString();

for (var i = 0; i < 1000; i++)
{
    Accessor.ClearCache();
    var accessor = Accessor.Create(typeof(SimpleObject));
    accessor.Set(simpleObj, "Name", "abc");
    var abc = (string)accessor.Get(simpleObj, "Name");
    Accessor.ClearCache();
    var wrapped = Accessor.Create(simpleObj);
    wrapped.Set("Name", "abc");
    abc = (string)wrapped.Get("Name");


    Accessor.ClearCache();
    dynamic dyn = simpleObj;

    var accessor1 = Accessor.Create(dyn.GetType());
    accessor1.Set(dyn, "Name", "abc");
    abc = (string)accessor.Get(dyn, "Name");


}
Console.WriteLine("DONE");
