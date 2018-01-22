namespace EasyCaching.UnitTests
{    
    using EasyCaching.Core;
    using EasyCaching.Core.Internal;
    using EasyCaching.HybridCache;
    using EasyCaching.InMemory;
    using EasyCaching.Redis;
    using FakeItEasy;
    using Microsoft.Extensions.Caching.Memory;
    using System;
    using System.Threading.Tasks;
    using Xunit;
    using Microsoft.Extensions.Options;

    public class HybridCachingTest : BaseCachingProviderTest
    {        
        public HybridCachingTest()
        {            
            //IServiceCollection services = new ServiceCollection();
            //services.AddDefaultHybridCache();
            //services.AddDefaultRedisCacheForHybrid(option =>
            //{
            //    option.Endpoints.Add(new ServerEndPoint("127.0.0.1", 6379));
            //});
            //services.AddDefaultInMemoryCacheForHybrid();

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

            A.CallTo(() => serviceAccessor(HybridCachingKeyType.LocalKey)).Returns(new InMemoryCachingProvider(new MemoryCache(new MemoryCacheOptions())));
            A.CallTo(() => serviceAccessor(HybridCachingKeyType.DistributedKey)).Returns(new DefaultRedisCachingProvider(fakeDbProvider,serializer));

            _provider = new HybridCachingProvider(serviceAccessor);
            _defaultTs = TimeSpan.FromSeconds(50);
        }
    }
}
