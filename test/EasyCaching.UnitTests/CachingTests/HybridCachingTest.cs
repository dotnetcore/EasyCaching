namespace EasyCaching.UnitTests
{
    using EasyCaching.Bus.Redis;
    using EasyCaching.Core;
    using EasyCaching.Core.Bus;
    using EasyCaching.Core.Configurations;
    using EasyCaching.Decoration.Polly;
    using EasyCaching.Redis;
    using Microsoft.Extensions.DependencyInjection;
    using System;
    using System.Threading.Tasks;
    using Xunit;
    using FakeItEasy;
    using System.Threading;
    using static ServiceBuilders;

    public class HybridCachingTest //: BaseCachingProviderTest
    {
        private const string AvailableRedis = "127.0.0.1:6379,abortConnect=true";
        private const string UnavailableRedis = "127.0.0.1:9999,abortConnect=true,connectTimeout=1";
        private const string UnavailableRedisWithAbortConnect = "127.0.0.1:9999,abortConnect=false,connectTimeout=1";
        
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
                    options.DBConfig.Configuration = AvailableRedis;
                },
                DistributedCacheProviderName);

                x.WithRedisBus(options =>
                {
                    options.Configuration = AvailableRedis;
                    
                    options
                        .DecorateWithRetry(1, RedisBusOptionsExtensions.RedisExceptionFilter)
                        .DecorateWithPublishFallback(RedisBusOptionsExtensions.RedisExceptionFilter);
                });

                UseHybrid(x);
            });
        });

        private IHybridCachingProvider CreateCachingProviderWithCircuitBreakerAndFallback(string connectionString) => 
            CreateService<IHybridCachingProvider>(services =>
            {
                services.AddEasyCaching(x =>
                {
                    x.UseInMemory(LocalCacheProviderName);

                    var circuitBreakerParameters = new CircuitBreakerParameters(
                        exceptionsAllowedBeforeBreaking: 1,
                        durationOfBreak: TimeSpan.FromMinutes(1));
                
                    x.UseRedis(options =>
                        {
                            options.DBConfig.Configuration = connectionString;
                
                            options.DecorateWithCircuitBreaker(
                                initParameters: circuitBreakerParameters,
                                executeParameters: circuitBreakerParameters,
                                exceptionFilter: RedisOptionsExtensions.RedisExceptionFilter);
                        },
                        DistributedCacheProviderName);

                    x.WithRedisBus(options =>
                    {
                        options.Configuration = connectionString;
                    
                        options
                            .DecorateWithCircuitBreaker(
                                initParameters: circuitBreakerParameters,
                                executeParameters: circuitBreakerParameters,
                                subscribeRetryInterval: TimeSpan.FromMinutes(1),
                                exceptionFilter: RedisOptionsExtensions.RedisExceptionFilter)
                            .DecorateWithRetry(1, RedisOptionsExtensions.RedisExceptionFilter)
                            .DecorateWithPublishFallback(RedisOptionsExtensions.RedisExceptionFilter);
                    });

                    UseHybrid(x);
                });
            });
        
        private IHybridCachingProvider CreateFakeCachingProvider(
            Action<FakeBusOptions> decorateFakeBus = null,
            Action<IEasyCachingProvider> setupFakeDistributedProvider = null,
            Action<IEasyCachingBus> setupFakeBus = null)
        {
            var fakeBus = A.Fake<IEasyCachingBus>();
            setupFakeBus?.Invoke(fakeBus);
            
            var fakeDistributedProvider = A.Fake<IEasyCachingProvider>();
            A.CallTo(() => fakeDistributedProvider.Name).Returns(DistributedCacheProviderName);
            setupFakeDistributedProvider?.Invoke(fakeDistributedProvider);

            return CreateService<IHybridCachingProvider>(services =>
            {
                services.AddEasyCaching(x =>
                {
                    x.UseInMemory(LocalCacheProviderName);

                    x.UseFakeProvider(
                        options =>
                        {
                            options.ProviderFactory = () => fakeDistributedProvider;

                            var circuitBreakerParameters = new CircuitBreakerParameters(
                                exceptionsAllowedBeforeBreaking: 1,
                                durationOfBreak: TimeSpan.FromMinutes(1));
                
                            options.DecorateWithCircuitBreaker(
                                initParameters: circuitBreakerParameters,
                                executeParameters: circuitBreakerParameters,
                                exception => exception is InvalidOperationException);
                        },
                        DistributedCacheProviderName);

                    x.WithFakeBus(options =>
                    {
                        options.BusFactory = () => fakeBus;

                        if (decorateFakeBus != null)
                        {
                            decorateFakeBus(options);
                        }
                        else
                        {
                            options
                                .DecorateWithRetry(retryCount: 1, exceptionFilter: null)
                                .DecorateWithPublishFallback(exceptionFilter: null);
                        };
                    });

                    UseHybrid(x);
                });
            });
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
            var hybridProvider = CreateFakeCachingProvider(
                setupFakeBus: bus => 
                    A.CallTo(() => bus.Publish("test_topic", A<EasyCachingMessage>._)).Throws(new InvalidOperationException()));

            hybridProvider.Remove("fake-remove");

            Assert.True(true);
        }

        [Fact]
        public async Task Send_Msg_Async_Throw_Exception_Should_Not_Break()
        {
            var token = new CancellationToken();
            var hybridProvider = CreateFakeCachingProvider(
                setupFakeBus: bus => 
                    A.CallTo(() => bus.PublishAsync("test_topic", A<EasyCachingMessage>._, token)).ThrowsAsync(new InvalidOperationException()));

            await hybridProvider.RemoveAsync("fake-remove");

            Assert.True(true);
        }

        [Fact]
        public void Subscribe_With_Circuit_Breaker_Subscribe_Exception_Should_Not_Break()
        {
            var circuitBreakerParameters = new CircuitBreakerParameters(
                exceptionsAllowedBeforeBreaking: 1,
                durationOfBreak: TimeSpan.FromMinutes(1));
            var hybridProvider = CreateFakeCachingProvider(
                decorateFakeBus: options => options.DecorateWithCircuitBreaker(
                    initParameters: circuitBreakerParameters,
                    executeParameters: circuitBreakerParameters,
                    subscribeRetryInterval: TimeSpan.FromMinutes(1),
                    exceptionFilter: exception => exception is InvalidOperationException),
                setupFakeBus: bus => 
                    A.CallTo(() => bus.Subscribe("test_topic", A<Action<EasyCachingMessage>>._)).Throws(new InvalidOperationException())
            );

            hybridProvider.RemoveAsync("fake-remove");

            Assert.True(true);
        }
              
        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        public void Distributed_Remove_Throw_Exception_Should_Not_Break(int attemptsCount)
        {
            var hybridProvider = CreateFakeCachingProvider(
                    setupFakeDistributedProvider: distributedProvider => 
                        A.CallTo(() => distributedProvider.Remove("fake-remove-key")).Throws(new InvalidOperationException()));


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
            var hybridProvider = CreateFakeCachingProvider(
                setupFakeDistributedProvider: distributedProvider => 
                    A.CallTo(() => distributedProvider.RemoveAsync("fake-remove-key")).ThrowsAsync(new InvalidOperationException()));


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
            var hybridProvider = CreateFakeCachingProvider(
                setupFakeDistributedProvider: distributedProvider => 
                    A.CallTo(() => distributedProvider.Set(A<string>.Ignored, A<string>.Ignored, A<TimeSpan>.Ignored)).Throws(new InvalidOperationException()));

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
            var hybridProvider = CreateFakeCachingProvider(
                setupFakeDistributedProvider: distributedProvider => 
                    A.CallTo(() => distributedProvider.SetAsync(A<string>.Ignored, A<string>.Ignored, A<TimeSpan>.Ignored)).ThrowsAsync(new InvalidOperationException()));

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

        [Theory]
        [InlineData(AvailableRedis, 1)]
        [InlineData(AvailableRedis, 2)]
        [InlineData(UnavailableRedis, 1)]
        [InlineData(UnavailableRedis, 2)]
        [InlineData(UnavailableRedisWithAbortConnect, 1)]
        [InlineData(UnavailableRedisWithAbortConnect, 2)]
        public void Distributed_Set_RedisWithCircuitBreakerAndFallback_Should_Not_Break(string connectionString, int attemptsCount)
        {
            var hybridProvider = CreateCachingProviderWithCircuitBreakerAndFallback(connectionString);

            var key = GetUniqueCacheKey();
            
            
            for (int i = 0; i < attemptsCount; i++)
            {
                hybridProvider.Set(key, "123", Expiration);
            }
            

            var res = hybridProvider.Get<string>(key);

            Assert.True(res.HasValue);
            Assert.Equal("123", res.Value);
        }
    }
}