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
    using Xunit;

    public class HybridCachingTest : BaseCachingProviderTest
    {
        public HybridCachingTest()
        {
            RedisDBOptions options = new RedisDBOptions()
            {
                AllowAdmin = true,
                //Password = ""
            };

            options.Endpoints.Add(new ServerEndPoint("127.0.0.1", 6379));

            var fakeOption = A.Fake<IOptions<RedisDBOptions>>();

            A.CallTo(() => fakeOption.Value).Returns(options);

            var fakeDbProvider = A.Fake<RedisDatabaseProvider>(option => option.WithArgumentsForConstructor(new object[] { fakeOption }));

            var serializer = new DefaultBinaryFormatterSerializer();

            //var serviceAccessor = A.Fake<Func<string, IEasyCachingProvider>>();

            //A.CallTo(() => serviceAccessor(HybridCachingKeyType.LocalKey)).Returns(new DefaultInMemoryCachingProvider(new MemoryCache(new MemoryCacheOptions())));
            //A.CallTo(() => serviceAccessor(HybridCachingKeyType.DistributedKey)).Returns(new DefaultRedisCachingProvider(fakeDbProvider, serializer));

            var providers = new List<IEasyCachingProvider>
            {
                new DefaultInMemoryCachingProvider(new MemoryCache(new MemoryCacheOptions()), new InMemoryOptions()),
                new DefaultRedisCachingProvider(fakeDbProvider, serializer, new RedisOptions())
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
    }
}
