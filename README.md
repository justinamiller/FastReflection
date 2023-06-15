# Fast Reflection
A library for fast access to .net fields/properties.

In .NET reflection is slow, if you need access to the members of an arbitrary type, with the type and member-names known only at runtime, then it is frankly hard, especially for dynamic types. This library makes such access easy and fast.

## Usage
[![NuGet Badge](https://buildstats.info/nuget/FastReflection.NET)](https://www.nuget.org/packages/FastReflection.NET/)

## Examples
How to update value for a known member from type
````
  var accessor = Accessor.Create(typeof(SimpleObject));
  string name = // something known only at runtime
  //update value from type
  accessor.Set(simpleObj, "Name", "abc");
````

How to get a list of fields and properties
````
    var accessor = Accessor.Create(typeof(SimpleObject));
    var members = accessor.GetMembers();
    //can you update value for property index 0
    if (members[0].CanWrite)
    {
        accessor.Set(simpleObj, members[0].Name, null);
    }
````

How to create a new instance of type
````
  var accessor = Accessor.Create(typeof(SimpleObject));
  SimpleObject obj= accessor.CreateNew();
````

Represents an individual object, allowing access to members by-name
````
    var wrapped = Accessor.Create(simpleObj);
    //update value of property
    wrapped.Set("Name", "abc");
    //get value of property
    var abc = (string)wrapped.Get("Name");
````


 ## Benchmarks
 ``` ini
BenchmarkDotNet=v0.13.5, OS=Windows 11 (10.0.22621.1848/22H2/2022Update/SunValley2)
12th Gen Intel Core i9-12900HK, 1 CPU, 20 logical and 14 physical cores
.NET SDK=7.0.203
  [Host]     : .NET 6.0.16 (6.0.1623.17311), X64 RyuJIT AVX2
  Job-NKDHWV : .NET 6.0.16 (6.0.1623.17311), X64 RyuJIT AVX2
```

|                        Method |       Mean |     Error |    StdDev |     Median |     Rank |
|------------------------------ |-----------:|----------:|----------:|-----------:|---------:|
|                    Static C#' |   1.743 ns | 0.4016 ns |  1.022 ns |   1.250 ns |        * |
|                     c# new()' |  10.125 ns | 1.1979 ns |  3.027 ns |  10.625 ns |       ** |
|       TypeAccessor.CreateNew' |  11.500 ns | 0.8477 ns |  2.218 ns |  11.875 ns |      *** |
|                   Dynamic C#' |  11.661 ns | 1.0003 ns |  2.600 ns |  11.250 ns |      *** |
|     Activator.CreateInstance' |  20.188 ns | 1.1199 ns |  2.930 ns |  18.750 ns |     **** |

|                        Method |       Mean |     Error |    StdDev |     Median |     Rank |
|------------------------------ |-----------:|----------:|----------:|-----------:|---------:|
|          TypeAccessor.Create' |  48.158 ns | 1.3213 ns |  3.363 ns |  47.500 ns |        * |
|        ObjectAccessor.Create' |  51.154 ns | 1.1983 ns |  3.093 ns |  51.250 ns |       ** |
|                 PropertyInfo' | 107.864 ns | 4.4341 ns | 11.525 ns | 110.000 ns |      *** |
|           PropertyDescriptor' | 229.234 ns | 6.6791 ns | 17.478 ns | 222.500 ns |     **** |




