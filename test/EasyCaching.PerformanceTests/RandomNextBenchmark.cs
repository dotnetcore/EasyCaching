namespace EasyCaching.PerformanceTests
{
    using BenchmarkDotNet.Attributes;

    [MemoryDiagnoser]
    [AllStatisticsColumn]
    public class RandomNextBenchmark
    {
        private static System.Random _random = new System.Random();

        [Benchmark]
        public int RandomRecreate() => new System.Random().Next(1, 10000);

        [Benchmark]
        public int RandomReuse() => _random.Next(1, 10000);

        [Benchmark]
        public int RandomGenerator() => System.Security.Cryptography.RandomNumberGenerator.GetInt32(1, 10000);

        [Benchmark]
        public int RandomShared() => System.Random.Shared.Next(1, 10000);
    }
}