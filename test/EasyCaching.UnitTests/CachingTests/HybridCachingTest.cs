namespace EasyCaching.UnitTests
{
    using Core.Configurations;
    using Core.Decoration;
    using EasyCaching.Core;
    using EasyCaching.Core.Bus;
    using Microsoft.Extensions.DependencyInjection;
    using System;
    using System.Threading.Tasks;
    using Xunit;
    using FakeItEasy;
    using System.Threading;
    using static ServiceBuilders;

    public class HybridCachingTest //: BaseCachingProviderTest
    {
        private const string LocalCacheProviderName = "m1";
        private const string DistributedCacheProviderName = "myredis";
        private static readonly TimeSpan Expiration = TimeSpan.FromSeconds(30);
        
        private readonly string _nameSpace= "hybrid";

        private void UseHybrid(EasyCachingOptions x)
        {
            x.UseHybrid(options =>
            {
                options.EnableLogging = false;
                options.TopicName = "test_topic";
                options.LocalCacheProviderName = LocalCacheProviderName;
                options.DistributedCacheProviderName = DistributedCacheProviderName;
                options.BusRetryCount = 1;
                options.DefaultExpirationForTtlFailed = 60;
            });
        }

        private IHybridCachingProvider CreateCachingProvider() => CreateService<IHybridCachingProvider>(services =>
        {
            services.AddEasyCaching(x =>
            {
                x.UseInMemory(LocalCacheProviderName);

                x.UseRedis(options =>
                {
                    options.DBConfig.Endpoints.Add(new Core.Configurations.ServerEndPoint("127.0.0.1", 6379));
                    options.DBConfig.Database = 5;
                },
                DistributedCacheProviderName);

                x.WithRedisBus(options =>
                {
                    options.Endpoints.Add(new Core.Configurations.ServerEndPoint("127.0.0.1", 6379));
                    options.Database = 6;
                });

                UseHybrid(x);
            });
        });
        
        private (IHybridCachingProvider HybridProvider, IEasyCachingBus Bus, IEasyCachingProvider DistributedProvider) CreateFakeCachingProvider()
        {
            var fakeBus = A.Fake<IEasyCachingBus>();
            
            var fakeDistributedProvider = A.Fake<IEasyCachingProvider>();
            A.CallTo(() => fakeDistributedProvider.IsDistributedCache).Returns(true);
            A.CallTo(() => fakeDistributedProvider.Name).Returns(DistributedCacheProviderName);

            var hybridProvider = CreateService<IHybridCachingProvider>(services =>
            {
                services.AddSingleton(fakeBus);

                services.AddEasyCaching(x =>
                {
                    x.UseInMemory(LocalCacheProviderName);

                    x.UseFake(
                        options =>
                        {
                            options.ProviderFactory = () => fakeDistributedProvider;

                            var circuitBreakerParameters = new CircuitBreakerParameters(
                                exceptionsAllowedBeforeBreaking: 1,
                                durationOfBreak: TimeSpan.FromMinutes(1));
                
                            options.DecorateWithCircuitBreaker(
                                exception => exception is InvalidOperationException,
                                initParameters: circuitBreakerParameters,
                                executeParameters: circuitBreakerParameters);
                        },
                        DistributedCacheProviderName);

                    UseHybrid(x);
                });
            });
            
            return (hybridProvider, fakeBus, fakeDistributedProvider);
        }

        [Fact]
        public void Set_And_Get_Should_Succeed()
        {
            var hybridProvider = CreateCachingProvider();
            var cacheKey = GetUniqueCacheKey();

            hybridProvider.Set(cacheKey, "val", Expiration);

            var res = hybridProvider.Get<string>(cacheKey);
            Assert.Equal("val", res.Value);
        }


        [Fact]
        public async Task SetAsync_And_ExistsAsync_Should_Succeed()
        {
            var hybridProvider = CreateCachingProvider();
            var cacheKey = GetUniqueCacheKey();

            await hybridProvider.SetAsync(cacheKey, "val", Expiration);

            var res = await hybridProvider.ExistsAsync(cacheKey);
            Assert.True(res);
        }

        [Fact]
        public void Set_And_Remove_Should_Succeed()
        {
            var hybridProvider = CreateCachingProvider();
            var cacheKey = GetUniqueCacheKey();

            hybridProvider.Set(cacheKey, "val", Expiration);
            hybridProvider.Remove(cacheKey);

            var res = hybridProvider.Exists(cacheKey);
            Assert.False(res);
        }

        [Fact]
        public async Task SetAsync_And_RemoveAsync_Should_Succeed()
        {
            var hybridProvider = CreateCachingProvider();
            var cacheKey = GetUniqueCacheKey();

            await hybridProvider.SetAsync(cacheKey, "val", Expiration);

            await hybridProvider.RemoveAsync(cacheKey);

            var res = await hybridProvider.ExistsAsync(cacheKey);

            Assert.False(res);
        }

        [Fact(Skip = "Delay")]
        public void Second_Client_Set_Same_Key_Should_Get_New_Value()
        {
            var hybridProvider = CreateCachingProvider();
            var cacheKey = GetUniqueCacheKey();

            hybridProvider.Set(cacheKey, "val", Expiration);

            //hybridCaching_2.Set(cacheKey, "value", Expiration);

            //System.Threading.Thread.Sleep(5000);

            var res = hybridProvider.Get<string>(cacheKey);

            Assert.Equal("value", res.Value);
        }

        [Fact]
        public void Send_Msg_Throw_Exception_Should_Not_Break()
        {
            var (hybridProvider, bus, _) = CreateFakeCachingProvider();
            A.CallTo(() => bus.Publish("test_topic", A<EasyCachingMessage>._)).Throws((arg) => new InvalidOperationException());

            hybridProvider.Remove("fake-remove");

            Assert.True(true);
        }

        [Fact]
        public async Task Send_Msg_Async_Throw_Exception_Should_Not_Break()
        {
            var (hybridProvider, bus, _) = CreateFakeCachingProvider();
            var token = new CancellationToken();
            A.CallTo(() => bus.PublishAsync("test_topic", A<EasyCachingMessage>._, token)).ThrowsAsync((arg) => new InvalidOperationException());

            await hybridProvider.RemoveAsync("fake-remove");

            Assert.True(true);
        }
              
        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        public void Distributed_Remove_Throw_Exception_Should_Not_Break(int attemptsCount)
        {
            var (hybridProvider, _, distributedProvider) = CreateFakeCachingProvider();
            A.CallTo(() => distributedProvider.Remove("fake-remove-key")).Throws(new InvalidOperationException());


            for (int i = 0; i < attemptsCount; i++)
            {
                hybridProvider.Remove("fake-remove-key");
            }
            

            Assert.True(true);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        public async Task Distributed_Remove_Async_Throw_Exception_Should_Not_Break(int attemptsCount)
        {
            var (hybridProvider, _, distributedProvider) = CreateFakeCachingProvider();
            A.CallTo(() => distributedProvider.RemoveAsync("fake-remove-key")).ThrowsAsync(new InvalidOperationException());


            for (int i = 0; i < attemptsCount; i++)
            {
                await hybridProvider.RemoveAsync("fake-remove-key");
            }

            Assert.True(true);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        public void Distributed_Set_Throw_Exception_Should_Not_Break(int attemptsCount)
        {
            var (hybridProvider, _, distributedProvider) = CreateFakeCachingProvider();
            A.CallTo(() => distributedProvider.Set(A<string>.Ignored, A<string>.Ignored, A<TimeSpan>.Ignored)).Throws(new InvalidOperationException());

            var key = GetUniqueCacheKey();
            
            
            for (int i = 0; i < attemptsCount; i++)
            {
                hybridProvider.Set(key, "123", Expiration);
            }
            

            var res = hybridProvider.Get<string>(key);

            Assert.True(res.HasValue);
            Assert.Equal("123", res.Value);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        public async Task Distributed_Set_Async_Throw_Exception_Should_Not_Break(int attemptsCount)
        {
            var (hybridProvider, _, distributedProvider) = CreateFakeCachingProvider();
            A.CallTo(() => distributedProvider.SetAsync(A<string>.Ignored, A<string>.Ignored, A<TimeSpan>.Ignored)).ThrowsAsync(new InvalidOperationException());

            var key = GetUniqueCacheKey();
            
            
            for (int i = 0; i < attemptsCount; i++)
            {
                await hybridProvider.SetAsync(key, "123", Expiration);
            }
            

            var res = await hybridProvider.GetAsync<string>(key);

            Assert.True(res.HasValue);
            Assert.Equal("123", res.Value);
        }
        
        private string GetUniqueCacheKey() => $"{_nameSpace}{Guid.NewGuid().ToString()}";
    }
}