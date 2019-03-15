namespace EasyCaching.UnitTests
{
    using EasyCaching.Bus.Redis;
    using EasyCaching.Core;
    using EasyCaching.Core.Bus;
    using EasyCaching.HybridCache;
    using EasyCaching.InMemory;
    using EasyCaching.Redis;
    using Microsoft.Extensions.DependencyInjection;
    using System;
    using System.Threading.Tasks;
    using Xunit;

    public class HybridCachingTest //: BaseCachingProviderTest
    {
        private HybridCachingProvider hybridCaching_1;
        private HybridCachingProvider hybridCaching_2;
        private IEasyCachingProviderFactory factory;
        private string _namespace;

        public HybridCachingTest()
        {
            _namespace = "hybrid";
            IServiceCollection services = new ServiceCollection();
            services.AddEasyCaching(option =>
            {
                option.UseInMemory("m1");
                option.UseInMemory("m2");

                option.UseRedis(config =>
                {
                    config.DBConfig.Endpoints.Add(new Core.Configurations.ServerEndPoint("127.0.0.1", 6379));
                    config.DBConfig.Database = 5;
                }, "myredis");

                option.WithRedisBus(config =>
                {
                    config.Endpoints.Add(new Core.Configurations.ServerEndPoint("127.0.0.1", 6379));
                    config.Database = 6;
                });
            });

            IServiceProvider serviceProvider = services.BuildServiceProvider();
            factory = serviceProvider.GetService<IEasyCachingProviderFactory>();

            var bus = serviceProvider.GetService<IEasyCachingBus>();

            hybridCaching_1 = new HybridCachingProvider(new HybridCachingOptions
            {
                EnableLogging = false,
                TopicName = "test_topic",
                LocalCacheProviderName = "m1",
                DistributedCacheProviderName = "myredis"
            }, factory, bus);

            hybridCaching_2 = new HybridCachingProvider(new HybridCachingOptions
            {
                EnableLogging = false,
                TopicName = "test_topic",
                LocalCacheProviderName = "m2",
                DistributedCacheProviderName = "myredis"
            }, factory, bus);
        }

        [Fact]
        public void Set_And_Get_Should_Succeed()
        {
            var cacheKey = $"{_namespace}_{Guid.NewGuid().ToString()}";

            hybridCaching_1.Set(cacheKey, "val", TimeSpan.FromSeconds(30));

            var res = hybridCaching_1.Get<string>(cacheKey);

            Assert.Equal("val", res.Value);
        }


        [Fact]
        public async Task SetAsync_And_ExistsAsync_Should_Succeed()
        {
            var cacheKey = $"{_namespace}_{Guid.NewGuid().ToString()}";

            await hybridCaching_1.SetAsync(cacheKey, "val", TimeSpan.FromSeconds(30));

            var res = await hybridCaching_1.ExistsAsync(cacheKey);

            Assert.True(res);
        }

        [Fact]
        public void Set_And_Remove_Should_Succeed()
        {
            var cacheKey = $"{_namespace}_{Guid.NewGuid().ToString()}";

            hybridCaching_1.Set(cacheKey, "val", TimeSpan.FromSeconds(30));

            hybridCaching_1.Remove(cacheKey);

            var res = hybridCaching_1.Exists(cacheKey);

            Assert.False(res);
        }

        [Fact]
        public async Task SetAsync_And_RemoveAsync_Should_Succeed()
        {
            var cacheKey = $"{_namespace}_{Guid.NewGuid().ToString()}";

            await hybridCaching_1.SetAsync(cacheKey, "val", TimeSpan.FromSeconds(30));

            await hybridCaching_1.RemoveAsync(cacheKey);

            var res = await hybridCaching_1.ExistsAsync(cacheKey);

            Assert.False(res);
        }

        [Fact(Skip = "Delay")]
        public void Second_Client_Set_Same_Key_Should_Get_New_Value()
        {
            var cacheKey = $"{_namespace}_{Guid.NewGuid().ToString()}";

            hybridCaching_1.Set(cacheKey, "val", TimeSpan.FromSeconds(30));

            hybridCaching_2.Set(cacheKey, "value", TimeSpan.FromSeconds(30));

            //System.Threading.Thread.Sleep(5000);

            var res = hybridCaching_1.Get<string>(cacheKey);

            Assert.Equal("value", res.Value);
        }
    }
}