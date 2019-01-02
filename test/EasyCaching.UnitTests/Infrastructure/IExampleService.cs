namespace EasyCaching.UnitTests.Infrastructure
{
    using EasyCaching.Core.Internal;
    using System;
    using System.Threading.Tasks;

    public interface ICastleExampleService
    {
        [EasyCachingAble(Expiration = 1)]
        string GetCurrentUTC();

        long GetCurrentUTCTick();

        [EasyCachingPut(CacheKeyPrefix = "CastleExample")]
        string PutTest(int num, string str = "123");

        [EasyCachingEvict(CacheKeyPrefix = "CastleExample")]
        string EvictTest();

        [EasyCachingEvict(CacheKeyPrefix = "CastleExample", IsAll = true)]
        string EvictAllTest();

        [EasyCachingAble(Expiration = 1)]
        Task<string> AbleTestAsync();

        [EasyCachingEvict(CacheKeyPrefix = "CastleExample")]
        Task EvictTestAsync();

        [EasyCachingPut(CacheKeyPrefix = "CastleExample")]
        Task<string> PutTestAsync(int num, string str = "123");

    }

    public class CastleExampleService : ICastleExampleService//, IEasyCaching
    {   
        public string EvictAllTest()
        {
            return "EvictAllTest";
        }

        public string EvictTest()
        {
            return "EvictTest";
        }

        public string GetCurrentUTC()
        {
            return DateTime.UtcNow.ToString();
        }

        public long GetCurrentUTCTick()
        {
            return DateTime.UtcNow.Ticks;
        }

        public string PutTest(int num, string str)
        {
            return $"PutTest-{num}";
        }

        public async Task<string> AbleTestAsync()
        {
            return await Task.FromResult("I am task");
        }

        public async Task EvictTestAsync()
        {
            await Task.CompletedTask;
        }

        public async Task<string> PutTestAsync(int num, string str)
        {
            return await Task.FromResult($"PutTestAsync-{num}");
        }
    }

    public interface IAspectCoreExampleService //: IEasyCaching
    {
        [EasyCachingAble(Expiration = 1)]
        string GetCurrentUTC();

        long GetCurrentUTCTick();

        [EasyCachingEvict(CacheKeyPrefix = "AspectCoreExample")]
        string EvictTest();

        [EasyCachingEvict(CacheKeyPrefix = "AspectCoreExample", IsAll = true)]
        string EvictAllTest();

        [EasyCachingPut(CacheKeyPrefix = "AspectCoreExample")]
        string PutTest(int num, string str = "123");

        [EasyCachingAble(Expiration = 1)]
        Task<string> AbleTestAsync();

        [EasyCachingEvict(CacheKeyPrefix = "AspectCoreExample")]
        Task EvictTestAsync();

        [EasyCachingPut(CacheKeyPrefix = "AspectCoreExample")]
        Task<string> PutTestAsync(int num, string str = "123");
    }

    public class AspectCoreExampleService : IAspectCoreExampleService
    {
        public string EvictAllTest()
        {
            return "EvictAllTest";
        }

        public string EvictTest()
        {
            return "EvictTest";
        }

        public string GetCurrentUTC()
        {
            return DateTime.UtcNow.ToString();
        }

        public long GetCurrentUTCTick()
        {
            return DateTime.UtcNow.Ticks;
        }

        public string PutTest(int num, string str)
        {
            return $"PutTest-{num}";
        }

        public async Task<string> AbleTestAsync()
        {
            return await Task.FromResult("I am task");
        }

        public async Task EvictTestAsync()
        {
            await Task.CompletedTask;
        }

        public async Task<string> PutTestAsync(int num, string str)
        {
            return await Task.FromResult($"PutTestAsync-{num}");
        }
    }
}