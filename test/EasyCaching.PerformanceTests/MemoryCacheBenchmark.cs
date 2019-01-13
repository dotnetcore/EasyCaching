namespace EasyCaching.PerformanceTests
{
    using BenchmarkDotNet.Attributes;
    using EasyCaching.InMemory;
    using Microsoft.Extensions.Caching.Memory;

    [MemoryDiagnoser]
    [AllStatisticsColumn]
    public class SetBenchmark
    {
        private readonly MemoryCache _msCache = new MemoryCache(new MemoryCacheOptions());
        private readonly InMemoryCaching _cache = new InMemoryCaching(new InMemoryCachingOptions());

        [Benchmark]
        public void MS()
        {
            _msCache.Set("ms-key", "ms-value", System.TimeSpan.FromSeconds(5));
        }

        [Benchmark]
        public void EC()
        {
            _cache.Set("ec-key", "ec-value", System.TimeSpan.FromSeconds(5));
        }
    }

    [MemoryDiagnoser]
    [AllStatisticsColumn]
    public class GetBenchmark
    {
        private readonly MemoryCache _msCache = new MemoryCache(new MemoryCacheOptions());
        private readonly InMemoryCaching _cache = new InMemoryCaching(new InMemoryCachingOptions());

        [GlobalSetup]
        public void Setup()
        {
            for (int i = 0; i < 1000; i++)
            {
                _msCache.Set($"ms-key-{i}", $"ms-value-{i}", System.TimeSpan.FromSeconds(5));
                _cache.Set($"ec-key-{i}", $"ec-value-{i}", System.TimeSpan.FromSeconds(5));
            }
        }

        [Benchmark]
        public void MS()
        {
            var i = new System.Random().Next(0, 1000);
            _msCache.Get($"ms-key-{i}");
        }

        [Benchmark]
        public void EC()
        {
            var i = new System.Random().Next(0, 1000);
            _cache.Get<string>($"ec-key-{i}");
        }
    }
}
