namespace EasyCaching.UnitTests.Infrastructure
{
    using EasyCaching.BaseTest.Infrastructure;
    using EasyCaching.Core.Interceptor;
    using EasyCaching.UnitTests.CustomInterceptors;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface ICastleExampleService
    {
        [EasyCachingAble(Expiration = 1)]
        string GetCurrentUTC();

        long GetCurrentUTCTick();

        [EasyCachingPut(CacheKeyPrefix = "CastleExample")]
        string PutTest(int num, string str = "123");

        [EasyCachingPut(CacheKeyPrefix = "CastleExample", CacheProviderName = "second")]
        string PutSwitchProviderTest(int num, string str = "123");

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

        [EasyCachingAble(Expiration = 1)]
        Task<string> AbleTestWithNullValueAsync();

        [CustomCachingAble]
        string CustomAbleAttributeTest();

        [CustomCachingPut]
        string CustomPutAttributeTest(int num);

        [CustomCachingEvict]
        string CustomEvictAttributeTest();
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
        public string PutSwitchProviderTest(int num, string str)
        {
            return PutTest(num, str);
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

        public Task<string> AbleTestWithNullValueAsync()
        {
            return Task.FromResult<string>(null);
        }

        public string CustomAbleAttributeTest()
        {
            return GetCurrentUTC();
        }

        public string CustomPutAttributeTest(int num)
        {
            return $"CustomPutTest-{num}";
        }

        public string CustomEvictAttributeTest()
        {
            return "CustomEvictTest";
        }
    }

    public interface IAspectCoreExampleService //: IEasyCaching
    {
        [EasyCachingAble(Expiration = 1)]
        string GetCurrentUTC();

        long GetCurrentUTCTick();

        [EasyCachingEvict(CacheKeyPrefix = "AspectCoreExample")]
        string EvictTest();

        [EasyCachingEvict(CacheKeyPrefix = "AspectCoreExample", CacheProviderName = "second")]
        string EvictSwitchProviderTest();

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

        [EasyCachingAble(Expiration = 1)]
        Task<IEnumerable<SerializerTestModel>> AbleIEnumerableTest();

        [EasyCachingAble(Expiration = 1)]
        Task<IList<SerializerTestModel>> AbleIListTest();

        [EasyCachingAble(Expiration = 1)]
        Task<List<SerializerTestModel>> AbleListTest();

        [EasyCachingAble(Expiration = 1)]
        Task<string> AbleTestWithNullValueAsync();

        [CustomCachingAble]
        string CustomAbleAttributeTest();

        [CustomCachingPut]
        string CustomPutAttributeTest(int num);

        [CustomCachingEvict]
        string CustomEvictAttributeTest();
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
        public string EvictSwitchProviderTest()
        {
            return EvictTest();
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

        public Task<IEnumerable<SerializerTestModel>> AbleIEnumerableTest()
        {
            IEnumerable<SerializerTestModel> list = new List<SerializerTestModel> { new SerializerTestModel { Prop = "prop" } };
            return Task.FromResult(list);
        }

        public Task<IList<SerializerTestModel>> AbleIListTest()
        {
            IList<SerializerTestModel> list = new List<SerializerTestModel> { new SerializerTestModel { Prop = "prop" } };
            return Task.FromResult(list);
        }

        public Task<List<SerializerTestModel>> AbleListTest()
        {
            var list = new List<SerializerTestModel> { new SerializerTestModel { Prop = "prop" } };
            return Task.FromResult(list);
        }

        public Task<string> AbleTestWithNullValueAsync()
        {
            return Task.FromResult<string>(null);
        }

        public string CustomAbleAttributeTest()
        {
            return GetCurrentUTC();
        }

        public string CustomPutAttributeTest(int num)
        {
            return $"CustomPutTest-{num}";
        }

        public string CustomEvictAttributeTest()
        {
            return "CustomEvictTest";
        }
    }
}