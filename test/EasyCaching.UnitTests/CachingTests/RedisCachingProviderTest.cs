namespace EasyCaching.UnitTests
{
    using EasyCaching.Core;
    using EasyCaching.Core.Internal;
    using EasyCaching.Redis;
    using Microsoft.Extensions.DependencyInjection;
    using System;
    using Xunit;

    public class RedisCachingProviderTest : BaseCachingProviderTest
    {
        public RedisCachingProviderTest()
        {
            IServiceCollection services = new ServiceCollection();
            services.AddDefaultRedisCache(options =>
            {
                options.DBConfig = new RedisDBOptions
                {
                    AllowAdmin = true
                };
                options.DBConfig.Endpoints.Add(new ServerEndPoint("127.0.0.1", 6379));
                options.DBConfig.Database = 5;
            });
            IServiceProvider serviceProvider = services.BuildServiceProvider();
            _provider = serviceProvider.GetService<IEasyCachingProvider>();
            _defaultTs = TimeSpan.FromSeconds(30);
        }

        [Fact]
        public void Prefix_Equal_Asterisk_Should_Throw_ArgumentException()
        {
            Assert.Throws<ArgumentException>(() => _provider.RemoveByPrefix("*"));
        }

        [Fact]
        public void Fulsh_Should_Fail_When_AllowAdmin_Is_False()
        {
            IServiceCollection services = new ServiceCollection();
            services.AddDefaultRedisCache(options =>
            {
                options.DBConfig.Endpoints.Add(new ServerEndPoint("127.0.0.1", 6379));
            });
            IServiceProvider serviceProvider = services.BuildServiceProvider();
            var provider = serviceProvider.GetService<IEasyCachingProvider>();

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
            IServiceCollection services = new ServiceCollection();
            services.AddDefaultRedisCache(options =>
            {
                options.DBConfig.Configuration = "127.0.0.1:6379,allowAdmin=false,defaultdatabase=8";
            });
            IServiceProvider serviceProvider = services.BuildServiceProvider();
            var dbProvider = serviceProvider.GetService<IRedisDatabaseProvider>();
            Assert.NotNull(dbProvider);

            Assert.Equal(8, dbProvider.GetDatabase().Database);
        }

    }


    public class RedisCachingProviderWithFactoryTest : BaseCachingProviderTest
    {
        private readonly IEasyCachingProvider _secondProvider;

        private const string SECOND_PROVIDER_NAME = "second";

        public RedisCachingProviderWithFactoryTest()
        {
            IServiceCollection services = new ServiceCollection();
            services.AddDefaultRedisCacheWithFactory(EasyCachingConstValue.DefaultRedisName,options =>
            {
                options.DBConfig = new RedisDBOptions
                {
                    AllowAdmin = true
                };
                options.DBConfig.Endpoints.Add(new ServerEndPoint("127.0.0.1", 6379));
                options.DBConfig.Database = 3;
            });
            services.AddDefaultRedisCacheWithFactory(SECOND_PROVIDER_NAME, options =>
            {
                options.DBConfig = new RedisDBOptions
                {
                    AllowAdmin = true
                };
                options.DBConfig.Endpoints.Add(new ServerEndPoint("127.0.0.1", 6379));
                options.DBConfig.Database = 4;
            });
            IServiceProvider serviceProvider = services.BuildServiceProvider();
            var factory = serviceProvider.GetService<IEasyCachingProviderFactory>();
            _provider = factory.GetCachingProvider(EasyCachingConstValue.DefaultRedisName);
            _secondProvider = factory.GetCachingProvider(SECOND_PROVIDER_NAME);
            _defaultTs = TimeSpan.FromSeconds(30);
        }

        [Fact]
        public void Multi_Instance_Set_And_Get_Should_Succeed()
        {
            var cacheKey1 = "named-provider-1";
            var cacheKey2 = "named-provider-2";

            var value1 = Guid.NewGuid().ToString();
            var value2 = Guid.NewGuid().ToString("N");

            _provider.Set(cacheKey1, value1, _defaultTs);
            _secondProvider.Set(cacheKey2, value2, _defaultTs);

            var p1 = _provider.Get<string>(cacheKey1);
            var p2 = _provider.Get<string>(cacheKey2);

            var s1 = _secondProvider.Get<string>(cacheKey1);
            var s2 = _secondProvider.Get<string>(cacheKey2);

            Assert.Equal(value1, p1.Value);
            Assert.False(p2.HasValue);

            Assert.False(s1.HasValue);
            Assert.Equal(value2, s2.Value);
        }
    }
}
