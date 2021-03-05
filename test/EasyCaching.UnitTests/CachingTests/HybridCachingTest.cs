namespace EasyCaching.UnitTests
{
    using EasyCaching.Bus.Redis;
    using EasyCaching.Core;
    using EasyCaching.Core.Bus;
    using EasyCaching.Core.Configurations;
    using EasyCaching.Decoration.Polly;
    using EasyCaching.Redis;
    using EasyCaching.UnitTests.Fake;
    using Microsoft.Extensions.DependencyInjection;
    using System;
    using System.Threading.Tasks;
    using Xunit;
    using FakeItEasy;
    using System.Threading;
    using static ServiceBuilders;
    using static TestHelpers;

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

        private IHybridCachingProvider CreateCachingProvider(bool cacheNulls = false) => CreateService<IHybridCachingProvider>(services =>
        {
            services.AddEasyCaching(x =>
            {
                x.UseInMemory(
                    options =>
                    {
                        options.CacheNulls = cacheNulls;
                    },LocalCacheProviderName);

                x.UseRedis(options =>
                {
                    options.CacheNulls = cacheNulls;
                    options.DBConfig.Configuration = AvailableRedis;
                },
                DistributedCacheProviderName);

                x.WithRedisBus(options =>
                {
                    options.Configuration = AvailableRedis;
                    
                    options.DecorateWithRetry(1).DecorateWithPublishFallback();
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
                                subscribeRetryInterval: TimeSpan.FromMinutes(1))
                            .DecorateWithRetry(1)
                            .DecorateWithPublishFallback();
                    });

                    UseHybrid(x);
                });
            });
        
        private (IHybridCachingProvider HybridProvider, IEasyCachingProvider localCachingProvider, IEasyCachingProvider fakeDistributedProvider, IEasyCachingBus FakeBus) 
            CreateCachingProviderWithFakes(
            Action<FakeBusOptions> decorateFakeBus = null,
            Action<IEasyCachingProvider> setupFakeDistributedProvider = null,
            Action<IEasyCachingBus> setupFakeBus = null)
        {
            var fakeBus = A.Fake<IEasyCachingBus>();
            setupFakeBus?.Invoke(fakeBus);
            
            var fakeDistributedProvider = A.Fake<IEasyCachingProvider>();
            A.CallTo(() => fakeDistributedProvider.Name).Returns(DistributedCacheProviderName);
            setupFakeDistributedProvider?.Invoke(fakeDistributedProvider);

            var services = new ServiceCollection();
            services.AddEasyCaching(x =>
            {
                x.UseInMemory(LocalCacheProviderName);

                x.UseFakeProvider(
                    options =>
                    {
                        options.ProviderFactory = () => fakeDistributedProvider;

                        var circuitBreakerParameters = new CircuitBreakerParameters(
                            exceptionsAllowedBeforeBreaking: 2,
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
            
            var serviceProvider = services.BuildServiceProvider();

            return (
                serviceProvider.GetService<IHybridCachingProvider>(),
                serviceProvider.GetService<IEasyCachingProviderFactory>().GetCachingProvider(LocalCacheProviderName),
                fakeDistributedProvider, 
                fakeBus);
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
            var (hybridProvider, _, _, _) = CreateCachingProviderWithFakes(
                setupFakeBus: bus => 
                    A.CallTo(() => bus.Publish("test_topic", A<EasyCachingMessage>._)).Throws(new InvalidOperationException()));

            hybridProvider.Remove("fake-remove");

            Assert.True(true);
        }

        [Fact]
        public async Task Send_Msg_Async_Throw_Exception_Should_Not_Break()
        {
            var token = new CancellationToken();
            var (hybridProvider, _, _, _) = CreateCachingProviderWithFakes(
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
            var (hybridProvider, _, _, _) = CreateCachingProviderWithFakes(
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

        [Fact]
        public void SetNull_GetWithNoCachesNullProvider_ReturnsNoValue()
        {
            var hybridProvider = CreateCachingProvider();
            var hybridProviderWithCacheNulls = CreateCachingProvider(cacheNulls: true);
            var cacheKey = GetUniqueCacheKey();

            hybridProviderWithCacheNulls.Set<string>(cacheKey, null, Expiration);

            var res = hybridProvider.Get<string>(cacheKey);
            Assert.Equal(CacheValue<string>.NoValue, res);
        }
              
        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        public void Distributed_Remove_Throw_Exception_Should_Not_Break(int attemptsCount)
        {
            var (hybridProvider, _, _, _) = CreateCachingProviderWithFakes(
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
            var (hybridProvider, _, _, _) = CreateCachingProviderWithFakes(
                setupFakeDistributedProvider: distributedProvider => 
                    A.CallTo(() => distributedProvider.RemoveAsync("fake-remove-key")).ThrowsAsync(new InvalidOperationException()));


            for (int i = 0; i < attemptsCount; i++)
            {
                await hybridProvider.RemoveAsync("fake-remove-key");
            }

            Assert.True(true);
        }

        [Fact]
        public void GetWithDataRetriever_LocalCacheHasValue_DistributedCacheAndDataRetrieverShouldNotBeCalled()
        {
            var (hybridProvider, localProvider, fakeDistributedProvider, _) = CreateCachingProviderWithFakes();
            var dataRetriever = CreateFakeDataRetriever(result: "value");
            localProvider.Set("key", "cachedValue", Expiration);
            
            
            var res = hybridProvider.Get("key", dataRetriever, Expiration);
            
            
            Assert.True(res.HasValue);
            Assert.Equal("cachedValue", res.Value);
            
            var cachedValue = localProvider.Get<string>("key");
            Assert.True(cachedValue.HasValue);
            Assert.Equal("cachedValue", cachedValue.Value);
            
            A.CallTo(() => dataRetriever.Invoke()).MustNotHaveHappened();
            fakeDistributedProvider.CallToGetWithDataRetriever<string>().MustNotHaveHappened();
            A.CallTo(() => fakeDistributedProvider.GetExpiration("key")).MustNotHaveHappened();
        }

        [Fact]
        public async void GetAsyncWithDataRetriever_LocalCacheHasValue_DistributedCacheAndDataRetrieverShouldNotBeCalled()
        {
            var (hybridProvider, localProvider, fakeDistributedProvider, _) = CreateCachingProviderWithFakes();
            var dataRetriever = CreateFakeAsyncDataRetriever(result: "value");
            localProvider.Set("key", "cachedValue", Expiration);
            
            
            var res = await hybridProvider.GetAsync("key", dataRetriever, Expiration);
            
            
            Assert.True(res.HasValue);
            Assert.Equal("cachedValue", res.Value);
            
            var cachedValue = localProvider.Get<string>("key");
            Assert.True(cachedValue.HasValue);
            Assert.Equal("cachedValue", cachedValue.Value);
            
            A.CallTo(() => dataRetriever.Invoke()).MustNotHaveHappened();
            fakeDistributedProvider.CallToGetAsyncWithDataRetriever<string>().MustNotHaveHappened();
            A.CallTo(() => fakeDistributedProvider.GetExpirationAsync("key")).MustNotHaveHappened();
        }

        [Fact]
        public void GetWithDataRetriever_DistributedCacheHasValue_DataRetrieverShouldNotBeCalled_ShouldGetExpirationFromDistributedCache()
        {
            var (hybridProvider, localProvider, fakeDistributedProvider, _) = CreateCachingProviderWithFakes(
                setupFakeDistributedProvider: distributedProvider => 
                    distributedProvider.CallToGetWithDataRetriever<string>().Returns(new CacheValue<string>("cachedValue", hasValue: true)));
            var dataRetriever = CreateFakeDataRetriever(result: "value");
            
            
            var res = hybridProvider.Get("key", dataRetriever, Expiration);
            
            
            Assert.True(res.HasValue);
            Assert.Equal("cachedValue", res.Value);
            
            var cachedValue = localProvider.Get<string>("key");
            Assert.True(cachedValue.HasValue);
            Assert.Equal("cachedValue", cachedValue.Value);
            
            A.CallTo(() => dataRetriever.Invoke()).MustNotHaveHappened();
            fakeDistributedProvider.CallToGetWithDataRetriever<string>().MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeDistributedProvider.GetExpiration("key")).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async void GetAsyncWithDataRetriever_DistributedCacheHasValue_DataRetrieverShouldNotBeCalled_ShouldGetExpirationFromDistributedCache()
        {
            var (hybridProvider, localProvider, fakeDistributedProvider, _) = CreateCachingProviderWithFakes(
                setupFakeDistributedProvider: distributedProvider => 
                    distributedProvider.CallToGetAsyncWithDataRetriever<string>().Returns(new CacheValue<string>("cachedValue", hasValue: true)));
            var dataRetriever = CreateFakeAsyncDataRetriever(result: "value");
            
            
            var res = await hybridProvider.GetAsync("key", dataRetriever, Expiration);
            
            
            Assert.True(res.HasValue);
            Assert.Equal("cachedValue", res.Value);
            
            var cachedValue = localProvider.Get<string>("key");
            Assert.True(cachedValue.HasValue);
            Assert.Equal("cachedValue", cachedValue.Value);
            
            A.CallTo(() => dataRetriever.Invoke()).MustNotHaveHappened();
            fakeDistributedProvider.CallToGetAsyncWithDataRetriever<string>().MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeDistributedProvider.GetExpirationAsync("key")).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public void GetWithDataRetriever_DistributedCacheHasNoValue_ShouldReturnValue()
        {
            var (hybridProvider, localProvider, fakeDistributedProvider, _) = CreateCachingProviderWithFakes(
                setupFakeDistributedProvider: distributedProvider => 
                    distributedProvider.CallToGetWithDataRetriever<string>().CallsDataRetriever());
            var dataRetriever = CreateFakeDataRetriever(result: "value");
            
            
            var res = hybridProvider.Get("key", dataRetriever, Expiration);
            
            
            Assert.True(res.HasValue);
            Assert.Equal("value", res.Value);
            
            var cachedValue = localProvider.Get<string>("key");
            Assert.True(cachedValue.HasValue);
            Assert.Equal("value", cachedValue.Value);
            
            A.CallTo(() => dataRetriever.Invoke()).MustHaveHappenedOnceExactly();
            fakeDistributedProvider.CallToGetWithDataRetriever<string>().MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeDistributedProvider.GetExpiration("key")).MustNotHaveHappened();
        }

        [Fact]
        public async void GetAsyncWithDataRetriever_DistributedCacheHasNoValue_ShouldReturnValue()
        {
            var (hybridProvider, localProvider, fakeDistributedProvider, _) = CreateCachingProviderWithFakes(
                setupFakeDistributedProvider: distributedProvider => 
                    distributedProvider.CallToGetAsyncWithDataRetriever<string>().CallsDataRetriever());
            var dataRetriever = CreateFakeAsyncDataRetriever(result: "value");
            
            
            var res = await hybridProvider.GetAsync("key", dataRetriever, Expiration);
            
            
            Assert.True(res.HasValue);
            Assert.Equal("value", res.Value);
            
            var cachedValue = localProvider.Get<string>("key");
            Assert.True(cachedValue.HasValue);
            Assert.Equal("value", cachedValue.Value);
            
            A.CallTo(() => dataRetriever.Invoke()).MustHaveHappenedOnceExactly();
            fakeDistributedProvider.CallToGetAsyncWithDataRetriever<string>().MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeDistributedProvider.GetExpirationAsync("key")).MustNotHaveHappened();
        }

        [Fact]
        public void GetWithDataRetriever_DistributedCacheThrowsException_ShouldReturnValue()
        {
            var (hybridProvider, localProvider, fakeDistributedProvider, _) = CreateCachingProviderWithFakes(
                setupFakeDistributedProvider: distributedProvider => 
                    distributedProvider.CallToGetWithDataRetriever<string>().Throws(new InvalidOperationException()));
            var dataRetriever = CreateFakeDataRetriever(result: "value");
            
            
            var res = hybridProvider.Get("key", dataRetriever, Expiration);
            
            
            Assert.True(res.HasValue);
            Assert.Equal("value", res.Value);
            
            var cachedValue = localProvider.Get<string>("key");
            Assert.True(cachedValue.HasValue);
            Assert.Equal("value", cachedValue.Value);
            
            A.CallTo(() => dataRetriever.Invoke()).MustHaveHappenedOnceExactly();
            fakeDistributedProvider.CallToGetWithDataRetriever<string>().MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeDistributedProvider.GetExpiration("key")).MustNotHaveHappened();
        }

        [Fact]
        public async Task GetAsyncWithDataRetriever_DistributedCacheThrowsException_ShouldReturnValue()
        {
            var (hybridProvider, localProvider, fakeDistributedProvider, _) = CreateCachingProviderWithFakes(
                setupFakeDistributedProvider: distributedProvider => 
                    distributedProvider.CallToGetAsyncWithDataRetriever<string>().Throws(new InvalidOperationException()));
            var dataRetriever = CreateFakeAsyncDataRetriever(result: "value");
            
            
            var res = await hybridProvider.GetAsync("key", dataRetriever, Expiration);
            
            
            Assert.True(res.HasValue);
            Assert.Equal("value", res.Value);
            
            var cachedValue = localProvider.Get<string>("key");
            Assert.True(cachedValue.HasValue);
            Assert.Equal("value", cachedValue.Value);
            
            A.CallTo(() => dataRetriever.Invoke()).MustHaveHappenedOnceExactly();
            fakeDistributedProvider.CallToGetAsyncWithDataRetriever<string>().MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeDistributedProvider.GetExpirationAsync("key")).MustNotHaveHappened();
        }

        [Fact]
        public void GetWithDataRetriever_DataRetrieverThrowsException_DataRetrieverShouldBeCalledOnce()
        {
            var (hybridProvider, _, _, _) = CreateCachingProviderWithFakes(
                setupFakeDistributedProvider: distributedProvider => 
                    distributedProvider.CallToGetWithDataRetriever<string>().Throws(new InvalidOperationException()));
            var dataRetriever = CreateFakeDataRetrieverWithException(new InvalidOperationException("DataRetrieverError"));
            
            var exception = Assert.Throws<InvalidOperationException>(() => hybridProvider.Get("key", dataRetriever, Expiration));
            
            Assert.Equal("DataRetrieverError", exception.Message);
            A.CallTo(() => dataRetriever.Invoke()).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async void GetAsyncWithDataRetriever_DataRetrieverThrowsException_DataRetrieverShouldBeCalledOnce()
        {
            var hybridProvider = CreateCachingProvider();
            var dataRetriever = CreateFakeAsyncDataRetrieverWithException(new InvalidOperationException("DataRetrieverError"));
            
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                async () => await hybridProvider.GetAsync("key", dataRetriever, Expiration));
            
            Assert.Equal("DataRetrieverError", exception.Message);
            A.CallTo(() => dataRetriever.Invoke()).MustHaveHappenedOnceExactly();
        }

        [Theory]
        [InlineData(1)]
        [InlineData(3)]
        public void Distributed_Set_Throw_Exception_Should_Not_Break(int attemptsCount)
        {
            var (hybridProvider, _, _, _) = CreateCachingProviderWithFakes(
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
        [InlineData(3)]
        public async Task Distributed_Set_Async_Throw_Exception_Should_Not_Break(int attemptsCount)
        {
            var (hybridProvider, _, _, _) = CreateCachingProviderWithFakes(
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
        [InlineData(AvailableRedis, 3)]
        [InlineData(UnavailableRedis, 1)]
        [InlineData(UnavailableRedis, 3)]
        [InlineData(UnavailableRedisWithAbortConnect, 1)]
        [InlineData(UnavailableRedisWithAbortConnect, 3)]
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