namespace EasyCaching.UnitTests
{
    using EasyCaching.Core;
    using EasyCaching.Core.Internal;
    using EasyCaching.HybridCache;
    using EasyCaching.InMemory;
    using EasyCaching.Redis;
    using FakeItEasy;
    using Microsoft.Extensions.Caching.Memory;
    using Microsoft.Extensions.Options;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Xunit;

    public class HybridCachingTest : BaseCachingProviderTest
    {
        public HybridCachingTest()
        {
            RedisOptions options = new RedisOptions();
            options.DBConfig.AllowAdmin = true;
            options.DBConfig.Endpoints.Add(new ServerEndPoint("127.0.0.1", 6379));
            options.DBConfig.Database = 6;

            var fakeOption = A.Fake<IOptionsMonitor<RedisOptions>>();

            A.CallTo(() => fakeOption.CurrentValue).Returns(options);

            var fakeDbProvider = A.Fake<RedisDatabaseProvider>(option => option.WithArgumentsForConstructor(new object[] { fakeOption }));

            var serializer = new DefaultBinaryFormatterSerializer();

            var providers = new List<IEasyCachingProvider>
            {
                new DefaultInMemoryCachingProvider(new MemoryCache(new MemoryCacheOptions()), new TestOptionMonitorWrapper<InMemoryOptions>(new InMemoryOptions())),
                new DefaultRedisCachingProvider(fakeDbProvider, serializer, new TestOptionMonitorWrapper<RedisOptions>(new RedisOptions()))
            };

            _provider = new HybridCachingProvider(providers);
            _defaultTs = TimeSpan.FromSeconds(30);
        }

        [Fact]
        protected override void Get_Count_Without_Prefix_Should_Succeed()
        {

        }

        [Fact]
        protected override void Get_Count_With_Prefix_Should_Succeed()
        {

        }

        [Fact]
        protected override void OnHit_Should_Return_One_And_OnMiss_Should_Return_Zero()
        {
            
        }

        [Fact]
        protected override void OnHit_Should_Return_Zero_And_OnMiss_Should_Return_One()
        {

        }

        [Fact]
        protected override void TrySet_Value_And_Get_Cached_Value_Should_Succeed()
        {
            
        }

        [Fact]
        protected override async Task TrySet_Value_And_Get_Cached_Value_Async_Should_Succeed()
        {
            await Task.FromResult(1);
        }
    }
}
