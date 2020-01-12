using EasyCaching.Core;

namespace EasyCaching.PerformanceTests
{
    using BenchmarkDotNet.Attributes;
    using EasyCaching.InMemory;
    using Microsoft.Extensions.Caching.Memory;

    [MemoryDiagnoser]
    [AllStatisticsColumn]
    public class SetBenchmark
    {
        private readonly MemoryCache _msCacheWithoutLimit = new MemoryCache(new MemoryCacheOptions() { });

        private readonly MemoryCache _msCacheWithLimit = new MemoryCache(new MemoryCacheOptions() { SizeLimit = 10000, CompactionPercentage = 0.9 });

        private readonly InMemoryCaching _cacheWithoutLimit =
            new InMemoryCaching("b1", new InMemoryCachingOptions());

        private readonly InMemoryCaching _cacheWithLimit =
            new InMemoryCaching("b2", new InMemoryCachingOptions() { SizeLimit = 10000 });

        private readonly MemoryCacheEntryOptions _mOptions = new MemoryCacheEntryOptions()
                .SetSize(1)
                .SetAbsoluteExpiration(System.TimeSpan.FromSeconds(5));


        [Benchmark]
        public void MS_WithoutLimit()
        {
            for (int i = 0; i < 15000; i++)
            {
                _msCacheWithoutLimit.Set($"ms-key-out-{i}", "ms-value", System.TimeSpan.FromSeconds(5));
            }
        }

        [Benchmark]
        public void MS_WithLimit()
        {
            for (int i = 0; i < 15000; i++)
            {
                _msCacheWithLimit.Set($"ms-key-{i}", "ms-value", _mOptions);
            }
        }

        [Benchmark]
        public void EC_WithoutLimit()
        {
            for (int i = 0; i < 15000; i++)
            {
                _cacheWithoutLimit.Set($"ec-key-out-{i}", "ec-value", System.TimeSpan.FromSeconds(5));
            }
        }

        [Benchmark]
        public void EC_WithLimit()
        {
            for (int i = 0; i < 15000; i++)
            {
                _cacheWithLimit.Set($"ec-key-{i}", "ec-value", System.TimeSpan.FromSeconds(5));
            }
        }
    }

    [MemoryDiagnoser]
    [AllStatisticsColumn]
    public class GetBenchmark
    {
        private readonly MemoryCache _msCache = new MemoryCache(new MemoryCacheOptions());

        private readonly InMemoryCaching _cache =
            new InMemoryCaching(EasyCachingConstValue.DefaultInMemoryName, new InMemoryCachingOptions());

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