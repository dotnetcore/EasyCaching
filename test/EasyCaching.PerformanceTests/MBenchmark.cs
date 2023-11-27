namespace EasyCaching.PerformanceTests
{
    using BenchmarkDotNet.Attributes;
    using Microsoft.Extensions.Caching.Memory;
    using Microsoft.Extensions.DependencyInjection;
    using System;
    using System.Diagnostics;

    [MemoryDiagnoser]
    [AllStatisticsColumn]
    public class MBenchmark
    {
        private Microsoft.Extensions.Caching.Memory.IMemoryCache mc;
        private EasyCaching.Core.IEasyCachingProvider ec;

        [GlobalSetup]
        public void Setup()
        {
            var sc = new Microsoft.Extensions.DependencyInjection.ServiceCollection();
            sc.AddMemoryCache();
            sc.AddEasyCaching(x => x.UseInMemory("b1"));

            var sp = sc.BuildServiceProvider();
            mc = sp.GetRequiredService<Microsoft.Extensions.Caching.Memory.IMemoryCache>();
            ec = sp.GetRequiredService<EasyCaching.Core.IEasyCachingProvider>();
        }

        [Benchmark]
        public void MC_SET()
        {
            for (int i = 0; i < 1000000; i++)
            {
                mc.Set($"ms-key-out-{i}", "ms-value", System.TimeSpan.FromSeconds(5));
            }
        }

        [Benchmark]
        public void EC_SET()
        {
            for (int i = 0; i < 1000000; i++)
            {
                ec.Set($"ec-key-out-{i}", "ec-value", System.TimeSpan.FromSeconds(5));
            }
        }

        public void Test()
        { 
            Setup();

            Stopwatch sw = new Stopwatch();
            sw.Start();
            MC_SET();
            sw.Stop();
            Console.WriteLine($"MC_SET: {sw.ElapsedMilliseconds}");

            sw.Restart();
            EC_SET();
            sw.Stop();
            Console.WriteLine($"EC_SET: {sw.ElapsedMilliseconds}");
        }
    }
}