namespace EasyCaching.UnitTests.Infrastructure
{
    using EasyCaching.Core.Internal;
    using System;

    public interface ICastleExampleService
    {
        string GetCurrentUTC();

        long GetCurrentUTCTick();

        string PutTest(int num,string str = "123");

        string EvictTest();

        string EvictAllTest();
    }

    public class CastleExampleService : ICastleExampleService, IEasyCaching
    {
        [EasyCachingEvict(CacheKeyPrefix = "CastleExample",IsAll = true)]
        public string EvictAllTest()
        {
            return "EvictAllTest";
        }

        [EasyCachingEvict(CacheKeyPrefix = "CastleExample")]
        public string EvictTest()
        {
            return "EvictTest";
        }

        [EasyCachingAble(Expiration = 1)]
        public string GetCurrentUTC()
        {
            return DateTime.UtcNow.ToString();
        }

        public long GetCurrentUTCTick()
        {
            return DateTime.UtcNow.Ticks;
        }

        [EasyCachingPut(CacheKeyPrefix = "CastleExample")]
        public string PutTest(int num, string str)
        {
            return $"PutTest-{num}";
        }
    }

    public interface IAspectCoreExampleService : IEasyCaching
    {
        [EasyCachingAble(Expiration = 1)]
        string GetCurrentUTC();

        long GetCurrentUTCTick();

        [EasyCachingEvict(CacheKeyPrefix = "AspectCoreExample")]
        string EvictTest();

        [EasyCachingEvict(CacheKeyPrefix = "AspectCoreExample",IsAll = true)]
        string EvictAllTest();

        [EasyCachingPut(CacheKeyPrefix = "AspectCoreExample")]
        string PutTest(int num , string str="123");
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

        public string PutTest(int num , string str)
        {
            return $"PutTest-{num}";
        }
    }
}