namespace EasyCaching.UnitTests
{
    using EasyCaching.Bus.Redis;
    using EasyCaching.Core;
    using EasyCaching.Core.Bus;
    using EasyCaching.HybridCache;
    using EasyCaching.InMemory;
    using EasyCaching.Redis;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Options;
    using System;
    using System.Threading.Tasks;
    using Xunit;
    using FakeItEasy;
    using System.Threading;

    public class HybridCachingTest //: BaseCachingProviderTest
    {
        private string _namespace;
        private IHybridCachingProvider hybridCaching_1;
        private IEasyCachingProviderFactory factory;

        private HybridCachingProvider fakeHybrid;
        private IEasyCachingProviderFactory fakeFactory;
        private IEasyCachingBus fakeBus;
        private FakeDistributedCachingProvider fakeDisProvider;

        public HybridCachingTest()
        {
            _namespace = "hybrid";

            var options = new HybridCachingOptions
            {
                EnableLogging = false,
                TopicName = "test_topic",
                LocalCacheProviderName = "m1",
                DistributedCacheProviderName = "myredis",
                BusRetryCount = 1,
                DefaultExpirationForTtlFailed = 60
            };

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

                option.UseHybrid(config =>
                {
                    config.EnableLogging = false;
                    config.TopicName = "test_topic";
                    config.LocalCacheProviderName = "m1";
                    config.DistributedCacheProviderName = "myredis";
                });

                option.WithRedisBus(config =>
                {
                    config.Endpoints.Add(new Core.Configurations.ServerEndPoint("127.0.0.1", 6379));
                    config.Database = 6;
                });
            });

            IServiceProvider serviceProvider = services.BuildServiceProvider();
            factory = serviceProvider.GetService<IEasyCachingProviderFactory>();

            var bus = serviceProvider.GetService<IEasyCachingBus>();

            hybridCaching_1 = serviceProvider.GetService<IHybridCachingProvider>();

            fakeBus = A.Fake<IEasyCachingBus>();
            fakeFactory = A.Fake<IEasyCachingProviderFactory>();
            fakeDisProvider = A.Fake<FakeDistributedCachingProvider>();
            var myOptions = Options.Create(options);
            FakeCreatProvider();
            fakeHybrid = new HybridCachingProvider("h1", myOptions.Value, fakeFactory, fakeBus);
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

            //hybridCaching_2.Set(cacheKey, "value", TimeSpan.FromSeconds(30));

            //System.Threading.Thread.Sleep(5000);

            var res = hybridCaching_1.Get<string>(cacheKey);

            Assert.Equal("value", res.Value);
        }

        [Fact]
        public void Send_Msg_Throw_Exception_Should_Not_Break()
        {
            A.CallTo(() => fakeBus.Publish("test_topic", A<EasyCachingMessage>._)).Throws((arg) => new Exception());

            fakeHybrid.Remove("fake-remove");

            Assert.True(true);
        }

        [Fact]
        public async Task Send_Msg_Async_Throw_Exception_Should_Not_Break()
        {
            CancellationToken token = new CancellationToken();
            A.CallTo(() => fakeBus.PublishAsync("test_topic", A<EasyCachingMessage>._, token)).ThrowsAsync((arg) => new Exception());

            await fakeHybrid.RemoveAsync("fake-remove");

            Assert.True(true);
        }
              
        [Fact]
        public void Distributed_Remove_Throw_Exception_Should_Not_Break()
        {
            A.CallTo(() => fakeDisProvider.Remove("fake-remove-key")).Throws(new Exception());

            fakeHybrid.Remove("fake-remove-key");

            Assert.True(true);
        }

        [Fact]
        public async Task Distributed_Remove_Async_Throw_Exception_Should_Not_Break()
        {
            A.CallTo(() => fakeDisProvider.RemoveAsync("fake-remove-key")).ThrowsAsync(new Exception());

            await fakeHybrid.RemoveAsync("fake-remove-key");

            Assert.True(true);
        }

        [Fact]
        public void Distributed_Set_Throw_Exception_Should_Not_Break()
        {
            A.CallTo(() => fakeDisProvider.Set(A<string>.Ignored, A<string>.Ignored, A<TimeSpan>.Ignored)).Throws(new Exception());

            var key = $"d-set-{Guid.NewGuid().ToString()}";

            fakeHybrid.Set(key, "123", TimeSpan.FromSeconds(30));

            var res = fakeHybrid.Get<string>(key);

            Assert.True(res.HasValue);
            Assert.Equal(default(string), res.Value);
        }

        [Fact]
        public async Task Distributed_Set_Async_Throw_Exception_Should_Not_Break()
        {
            A.CallTo(() => fakeDisProvider.SetAsync(A<string>.Ignored, A<string>.Ignored, A<TimeSpan>.Ignored)).ThrowsAsync(new Exception());

            var key = $"d-set-{Guid.NewGuid().ToString()}";

            await fakeHybrid.SetAsync(key, "123", TimeSpan.FromSeconds(30));

            var res = await fakeHybrid.GetAsync<string>(key);

            Assert.True(res.HasValue);
            Assert.Equal(default(string), res.Value);
        }

        private void FakeCreatProvider()
        {
            A.CallTo(() => fakeFactory.GetCachingProvider("m1")).Returns(new FakeLocalCachingProvider());
            A.CallTo(() => fakeFactory.GetCachingProvider("myredis")).Returns(fakeDisProvider);
        }
    }
}