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
            RedisCacheOptions options = new RedisCacheOptions()
            {
                //Password = ""
            };

            options.Endpoints.Add(new ServerEndPoint("127.0.0.1", 6379));

            var fakeOption = A.Fake<IOptions<RedisCacheOptions>>();

            A.CallTo(() => fakeOption.Value).Returns(options);

            var fakeDbProvider = A.Fake<RedisDatabaseProvider>(option => option.WithArgumentsForConstructor(new object[] { fakeOption }));

            var serializer = new DefaultBinaryFormatterSerializer();

            var serviceAccessor = A.Fake<Func<string, IEasyCachingProvider>>();

            A.CallTo(() => serviceAccessor(HybridCachingKeyType.LocalKey)).Returns(new DefaultInMemoryCachingProvider(new MemoryCache(new MemoryCacheOptions())));
            A.CallTo(() => serviceAccessor(HybridCachingKeyType.DistributedKey)).Returns(new DefaultRedisCachingProvider(fakeDbProvider, serializer));

            _provider = new HybridCachingProvider(serviceAccessor);
            _defaultTs = TimeSpan.FromSeconds(30);
        }

        //[Fact]
        //protected override void SetAll_Should_Succeed()
        //{

        //}

        //[Fact]
        //protected override async Task SetAllAsync_Should_Succeed()
        //{
        //    await Task.FromResult(1);
        //}

        //[Fact]
        //protected override void GetAll_Should_Succeed()
        //{

        //}

        //[Fact]
        //protected override async Task GetAllAsync_Should_Succeed()
        //{
        //    await Task.FromResult(1);
        //}

        //[Fact]
        //protected override void GetByPrefix_Should_Succeed()
        //{

        //}

        //[Fact]
        //protected override async Task GetByPrefixAsync_Should_Succeed()
        //{
        //    await Task.FromResult(1);
        //}

        //[Fact]
        //protected override void RemoveAll_Should_Succeed()
        //{

        //}

        //[Fact]
        //protected override async Task RemoveAllAsync_Should_Succeed()
        //{
        //    await Task.FromResult(1);
        //}


    }
}
