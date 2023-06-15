using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Exporters.Csv;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Running;
using System.ComponentModel;
using System.Reflection;
using Iced.Intel;
using BenchmarkDotNet.Order;

namespace FastReflection.Benchmark
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var summary = BenchmarkRunner.Run<FastReflectionPerformance>(new Config());
            Console.WriteLine();
            //    summary.

            //    foreach (var report in summary.Reports.OrderBy(r => r.GenerateResult))
            //    {
            //        Console.WriteLine("{0}: {1:N2} ns", report.Benchmark.Target.MethodDisplayInfo, report.ResultStatistics.Median);
            //    }

            //    // Display a summary to match the output of the original Performance test
            //    foreach (var report in summary.Reports.OrderBy(r => r.Benchmark.Target.MethodDisplayInfo))
            //{
            //    Console.WriteLine("{0}: {1:N2} ns", report.Benchmark.Target.MethodDisplayInfo, report.ResultStatistics.Median);
            //}
            Console.WriteLine();
            Console.ReadLine();
        }


        public class FastReflectionPerformance
        {
            public string Value { get; set; }

            private FastReflectionPerformance obj;
            private dynamic dlr;
            private PropertyInfo prop;
            private PropertyDescriptor descriptor;

            private ITypeAccessor accessor;
            private IObjectAccessor wrapped;

            private ITypeAccessor accessordlr;
            private IObjectAccessor wrappeddlr;

            private Type type;

            [GlobalSetup]
            public void Setup()
            {
                obj = new FastReflectionPerformance();
                dlr = obj;
                prop = typeof(FastReflectionPerformance).GetProperty("Value");
                descriptor = TypeDescriptor.GetProperties(obj)["Value"];

                // FastMember specific code
                accessor = Accessor.Create(typeof(FastReflectionPerformance));
                wrapped = Accessor.Create(obj);

                accessordlr = Accessor.Create(dlr.GetType());
                wrappeddlr = Accessor.Create(dlr);

                type = typeof(FastReflectionPerformance);
            }

            [Benchmark(Description = "1. Static C#", Baseline = true)]
            public string StaticCSharp()
            {
                obj.Value = "abc";
                return obj.Value;
            }

            [Benchmark(Description = "2. Dynamic C#")]
            public string DynamicCSharp()
            {
                dlr.Value = "abc";
                return dlr.Value;
            }

            [Benchmark(Description = "3. PropertyInfo")]
            public string PropertyInfo()
            {
                prop.SetValue(obj, "abc", null);
                return (string)prop.GetValue(obj, null);
            }

            [Benchmark(Description = "4. PropertyDescriptor")]
            public string PropertyDescriptor()
            {
                descriptor.SetValue(obj, "abc");
                return (string)descriptor.GetValue(obj);
            }

            [Benchmark(Description = "5. TypeAccessor.Create")]
            public string TypeAccessor()
            {
                accessor.Set(obj, "Value", "abc");
                return (string)accessor.Get(obj, "Value");
            }

            [Benchmark(Description = "6. ObjectAccessor.Create")]
            public string ObjectAccessor()
            {
                wrapped.Set("Value", "abc");
                return (string)wrapped.Get("Value");
            }

            //[Benchmark(Description = "5a. TypeAccessor.Create dynamic")]
            //public string TypeAccessorDynamic()
            //{
            //    accessordlr.Set(obj, "Value", "abc");
            //    return (string)accessordlr.Get(obj, "Value");
            //}

            //[Benchmark(Description = "6a. ObjectAccessor.Create dynamic")]
            //public string ObjectAccessorDynamic()
            //{
            //    wrappeddlr.Set("Value", "abc");
            //    return (string)wrappeddlr.Get("Value");
            //}

            [Benchmark(Description = "7. c# new()")]
            public FastReflectionPerformance CSharpNew()
            {
                return new FastReflectionPerformance();
            }

            [Benchmark(Description = "8. Activator.CreateInstance")]
            public object ActivatorCreateInstance()
            {
                return Activator.CreateInstance(type);
            }

            [Benchmark(Description = "9. TypeAccessor.CreateNew")]
            public object TypeAccessorCreateNew()
            {
                return accessor.CreateNew();
            }
        }

        // BenchmarkDotNet settings (you can use the defaults, but these are tailored for this benchmark)
        private class Config : ManualConfig
        {
            public Config()
            {
                //       Add(Job.Default.WithLaunchCount(1));

                //     Add(StatisticColumn. StatisticColumn.Median, StatisticColumn.StdDev);
                this.AddColumn(TargetMethodColumn.Method, StatisticColumn.StdDev, StatisticColumn.Median, RankColumn.Stars);
            //    this.AddColumn(TargetMethodColumn.Method, StatisticColumn.Median, RankColumn.Stars);
                // Add(StatisticColumn.AllStatistics);
                Add(CsvExporter.Default, MarkdownExporter.Default, MarkdownExporter.GitHub);
                Add(new ConsoleLogger());

                Add(Job.Default.WithLaunchCount(1).WithWarmupCount(16).WithIterationCount(16*5).WithInvocationCount(16*5));
                //Add(Job.Default.WithWarmupCount(5));
                //Add(Job.Default.WithWarmupCount(5));
                //Add(Job.Default.WithLaunchCount(5));
            }
        }
    }
}