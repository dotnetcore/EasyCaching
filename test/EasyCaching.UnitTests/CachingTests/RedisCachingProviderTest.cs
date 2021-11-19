namespace EasyCaching.UnitTests
{
    using EasyCaching.Core;
    using EasyCaching.Core.Configurations;
    using EasyCaching.Core.Decoration;
    using EasyCaching.Decoration.Polly;
    using EasyCaching.Redis;
    using Microsoft.Extensions.DependencyInjection;
    using System;
    using System.Linq;
    using Xunit;
    using static ServiceBuilders;

    public class RedisCachingProviderTest : DistributedCachingProviderTest
    {
        private readonly string ProviderName = "Test";

        public RedisCachingProviderTest()
        {
            _defaultTs = TimeSpan.FromSeconds(30);
            _nameSpace = "RedisBasic";
        }

        private IServiceProvider CreateServiceProviderWithRedis(Action<RedisOptions> setup) => CreateServiceProviderWithEasyCaching(
           easyCachingOptions => easyCachingOptions.UseRedis(setup, ProviderName));
        
        protected override void SetupCachingProvider(EasyCachingOptions options, Action<BaseProviderOptions> additionalSetup)
        {
            options.UseRedis(providerOptions =>
            {
                providerOptions.ConnectionString = "127.0.0.1:6380,allowAdmin=true";
                additionalSetup(providerOptions);
            });
        }

        private IEasyCachingProvider CreateCachingProviderWithUnavailableRedisAndFallback()
        {
            var serviceProvider = CreateServiceProviderWithRedis(options =>
            {
                options.ConnectionString = "127.0.0.1:9999,allowAdmin=false,defaultDatabase=9,connectTimeout=1";

                var initCircuitBreakerParameters =
                    new CircuitBreakerParameters(
                        exceptionsAllowedBeforeBreaking: 1,
                        durationOfBreak: TimeSpan.FromMinutes(1));
                var executeCircuitBreakerParameters =
                    new AdvancedCircuitBreakerParameters(
                        failureThreshold: 0.1,
                        samplingDuration: TimeSpan.FromSeconds(15),
                        minimumThroughput: 10,
                        durationOfBreak: TimeSpan.FromSeconds(60));
            
                options
                    .DecorateWithCircuitBreaker(
                        initCircuitBreakerParameters,
                        executeCircuitBreakerParameters,
                        exceptionFilter: RedisOptionsExtensions.RedisExceptionFilter)
                    .DecorateWithFallback(
                        (name, _) => new NullCachingProvider(name, options),
                        exceptionFilter: RedisOptionsExtensions.RedisExceptionFilter);
            });
            return serviceProvider.GetService<IEasyCachingProvider>();
        }

        [Fact]
        public void Prefix_Equal_Asterisk_Should_Throw_ArgumentException()
        {
            Assert.Throws<ArgumentException>(() => _provider.RemoveByPrefix("*"));
        }

        [Fact]
        public void Flush_Should_Fail_When_AllowAdmin_Is_False()
        {
            IServiceCollection services = new ServiceCollection();
            services.AddEasyCaching(x =>
                x.UseRedis(
                    options => options.ConnectionString = "127.0.0.1:6380,allowAdmin=false",
                    ProviderName)
            );
            IServiceProvider serviceProvider = services.BuildServiceProvider();
            var factory = serviceProvider.GetRequiredService<IEasyCachingProviderFactory>();
            var provider = factory.GetCachingProvider(ProviderName);

            Assert.Throws<StackExchange.Redis.RedisCommandException>(() => provider.Flush());
        }


        [Fact]
        public void Issues16_DateTimeTest()
        {
            var model = new Model
            {
                Dt = Convert.ToDateTime("2018-02-12 12:11:00")
            };

            _provider.Set("activity", model, _defaultTs);

            var res = _provider.Get<Model>("activity");

            Assert.Equal("2018-02-12 12:11:00", res.Value.Dt?.ToString("yyyy-MM-dd HH:mm:ss"));
        }

        [Serializable]
        public class Model
        {
            public DateTime? Dt { get; set; }
        }

        [Fact]
        public void Use_Configuration_String_Should_Succeed()
        {
            var serviceProvider = CreateServiceProviderWithRedis(options =>
            {
                options.ConnectionString = "127.0.0.1:6380,allowAdmin=false,defaultDatabase=8";
            });
            var dbProvider = serviceProvider.GetService<IRedisDatabaseProvider>();
            Assert.NotNull(dbProvider);

            Assert.Equal(8, dbProvider.GetDatabase().Database);
        }

        [Fact]
        public void Use_Unavailable_Redis_With_Fallback_Get_Should_Return_Empty_Value()
        {
            var cachingProvider = CreateCachingProviderWithUnavailableRedisAndFallback();
            var cacheKey = GetUniqueCacheKey();
            
            var result = cachingProvider.Get<string>(cacheKey);
            
            Assert.False(result.HasValue);
            Assert.Null(result.Value);
        }

        [Fact]
        public void Use_Unavailable_Redis_With_Fallback_Set_Should_Do_Nothing()
        {
            var cachingProvider = CreateCachingProviderWithUnavailableRedisAndFallback();
            var cacheKey = GetUniqueCacheKey();

            cachingProvider.Set(cacheKey, "value", _defaultTs);

            var result = cachingProvider.Get<string>(cacheKey);
            Assert.False(result.HasValue);
        }

        [Fact]
        public void Use_Unavailable_Redis_With_Fallback_Get_With_Data_Retriever_Should_Succeed()
        {
            var cachingProvider = CreateCachingProviderWithUnavailableRedisAndFallback();
            var cacheKey = GetUniqueCacheKey();

            var result = cachingProvider.Get(cacheKey, () => "value", _defaultTs);
            
            Assert.Equal("value", result.Value);
        }

        [Fact]
        public void TwoCachingProviderWithSameConnectionStrings_ConnectionMultiplexerReused()
        {
            var serviceProvider = CreateServiceProviderWithEasyCaching(
                easyCachingOptions =>
                {
                    const string connectionString = "127.0.0.1:6380";
                    
                    easyCachingOptions.UseRedis(
                        options => options.ConnectionString = connectionString, 
                        "Cache1");
                    easyCachingOptions.UseRedis(
                        options => options.ConnectionString = connectionString, 
                        "Cache2");
                });

            var services = serviceProvider.GetServices<IRedisDatabaseProvider>().ToArray();

            Assert.Equal(2, services.Length);
            Assert.Equal(services[0].GetDatabase().Multiplexer, services[1].GetDatabase().Multiplexer); 
        }

        [Fact]
        public void TwoCachingProviderWithDifferentConnectionStrings_DifferentConnectionMultiplexers()
        {
            var serviceProvider = CreateServiceProviderWithEasyCaching(
                easyCachingOptions =>
                {
                    easyCachingOptions.UseRedis(
                        options => options.ConnectionString = "127.0.0.1:6380", 
                        "Cache1");
                    easyCachingOptions.UseRedis(
                        options => options.ConnectionString = "127.0.0.1:6380,allowAdmin=true", 
                        "Cache2");
                });

            var services = serviceProvider.GetServices<IRedisDatabaseProvider>().ToArray();

            Assert.Equal(2, services.Length);
            Assert.NotEqual(services[0].GetDatabase().Multiplexer, services[1].GetDatabase().Multiplexer); 
        }

        [Fact]
        public void CachingProviderAndBusWithSameConnectionStrings_ConnectionMultiplexerReused()
        {
            var serviceProvider = CreateServiceProviderWithEasyCaching(
                easyCachingOptions =>
                {
                    const string connectionString = "127.0.0.1:6380";
                    
                    easyCachingOptions.UseRedis(
                        options => options.ConnectionString = connectionString, 
                        "Cache");
                    easyCachingOptions.WithRedisBus(
                        options => options.ConnectionString = connectionString, 
                        "Bus");
                });

            var databaseProvider = serviceProvider.GetRequiredService<IRedisDatabaseProvider>();
            var subscriberProvider = serviceProvider.GetRequiredService<IRedisSubscriberProvider>();

            Assert.Equal(databaseProvider.GetDatabase().Multiplexer, subscriberProvider.GetSubscriber().Multiplexer); 
        }
    }


    public class RedisCachingProviderWithFactoryTest : BaseCachingProviderWithFactoryTest
    {
        public RedisCachingProviderWithFactoryTest()
        {
            IServiceCollection services = new ServiceCollection();
            services.AddEasyCaching(x =>
            {
                x.UseRedis(options =>
                {
                    options.ConnectionString = "127.0.0.1:6380,allowAdmin=true,defaultDatabase=3";
                });


                x.UseRedis(options =>
                {
                    options.ConnectionString = "127.0.0.1:6380,allowAdmin=true,defaultDatabase=4";
                }, SECOND_PROVIDER_NAME);
            });
            IServiceProvider serviceProvider = services.BuildServiceProvider();
            var factory = serviceProvider.GetService<IEasyCachingProviderFactory>();
            _provider = factory.GetCachingProvider(EasyCachingConstValue.DefaultRedisName);
            _secondProvider = factory.GetCachingProvider(SECOND_PROVIDER_NAME);
            _defaultTs = TimeSpan.FromSeconds(30);
            _nameSpace = "RedisFactory";
        }
    }

    public class RedisCachingProviderWithNamedSerTest
    {
        private readonly IEasyCachingProviderFactory _providerFactory;

        public RedisCachingProviderWithNamedSerTest()
        {
            IServiceCollection services = new ServiceCollection();
            services.AddEasyCaching(x =>
            {

                x.UseRedis(options =>
                {
                    options.ConnectionString = "127.0.0.1:6380,allowAdmin=true,defaultDatabase=13";
                    options.SerializerName = "cs11";
                }, "se1");

                x.UseRedis(options =>
                {
                    options.ConnectionString = "127.0.0.1:6380,allowAdmin=true,defaultDatabase=14";
                }, "se2");

                x.UseRedis(options =>
                {
                    options.ConnectionString = "127.0.0.1:6380,allowAdmin=true,defaultDatabase=11";
                }, "se3");

                x.WithJson("json").WithMessagePack("cs11").WithJson("se2");
            });

            IServiceProvider serviceProvider = services.BuildServiceProvider();
            _providerFactory = serviceProvider.GetService<IEasyCachingProviderFactory>();
        }

        [Fact]
        public void NamedSerializerTest()
        {
            var se1 = _providerFactory.GetCachingProvider("se1");
            var se2 = _providerFactory.GetCachingProvider("se2");
            var se3 = _providerFactory.GetCachingProvider("se3");

            var info1 = se1.GetProviderInfo();
            var info2 = se2.GetProviderInfo();
            var info3 = se3.GetProviderInfo();

            Assert.Equal("cs11", info1.Serializer.Name);
            Assert.Equal("se2", info2.Serializer.Name);
            Assert.Equal(EasyCachingConstValue.DefaultSerializerName, info3.Serializer.Name);
        }
    }
}