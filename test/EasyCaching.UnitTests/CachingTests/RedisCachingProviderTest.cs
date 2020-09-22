namespace EasyCaching.UnitTests
{
    using EasyCaching.Core;
    using EasyCaching.Core.Configurations;
    using EasyCaching.Redis;
    using EasyCaching.Serialization.Json;
    using EasyCaching.Serialization.MessagePack;
    using Microsoft.Extensions.DependencyInjection;
    using System;
    using Xunit;

    public class RedisCachingProviderTest : BaseCachingProviderTest
    {
        private readonly string ProviderName = "Test";

        public RedisCachingProviderTest()
        {
            _defaultTs = TimeSpan.FromSeconds(30);
            _nameSpace = "RedisBasic";
        }

        protected override IEasyCachingProvider CreateCachingProvider(Action<BaseProviderOptions> additionalSetup)
        {
            IServiceCollection services = new ServiceCollection();
            services.AddEasyCaching(x =>
                x.UseRedis(options =>
                {
                    options.DBConfig = new RedisDBOptions
                    {
                        AllowAdmin = true
                    };
                    options.DBConfig.Endpoints.Add(new ServerEndPoint("127.0.0.1", 6380));
                    options.DBConfig.Database = 5;
                    additionalSetup(options);
                }, 
                ProviderName)
            );
            IServiceProvider serviceProvider = services.BuildServiceProvider();
            return serviceProvider.GetService<IEasyCachingProvider>();
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
            services.AddEasyCaching(x =>
                x.UseRedis(options => { options.DBConfig.Endpoints.Add(new ServerEndPoint("127.0.0.1", 6380)); },
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
            IServiceCollection services = new ServiceCollection();
            services.AddEasyCaching(x =>
                x.UseRedis(options =>
                {
                    options.DBConfig.Configuration = "127.0.0.1:6380,allowAdmin=false,defaultdatabase=8";
                }));
            IServiceProvider serviceProvider = services.BuildServiceProvider();
            var dbProvider = serviceProvider.GetService<IRedisDatabaseProvider>();
            Assert.NotNull(dbProvider);

            Assert.Equal(8, dbProvider.GetDatabase().Database);
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
                    options.DBConfig = new RedisDBOptions
                    {
                        AllowAdmin = true
                    };
                    options.DBConfig.Endpoints.Add(new ServerEndPoint("127.0.0.1", 6380));
                    options.DBConfig.Database = 3;
                });


                x.UseRedis(options =>
                {
                    options.DBConfig = new RedisDBOptions
                    {
                        AllowAdmin = true
                    };
                    options.DBConfig.Endpoints.Add(new ServerEndPoint("127.0.0.1", 6380));
                    options.DBConfig.Database = 4;
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
                    options.DBConfig = new RedisDBOptions
                    {
                        AllowAdmin = true
                    };
                    options.DBConfig.Endpoints.Add(new ServerEndPoint("127.0.0.1", 6380));
                    options.DBConfig.Database = 13;
                    options.SerializerName = "cs11";
                }, "se1");

                x.UseRedis(options =>
                {
                    options.DBConfig = new RedisDBOptions
                    {
                        AllowAdmin = true
                    };
                    options.DBConfig.Endpoints.Add(new ServerEndPoint("127.0.0.1", 6380));
                    options.DBConfig.Database = 14;
                }, "se2");

                x.UseRedis(options =>
                {
                    options.DBConfig = new RedisDBOptions
                    {
                        AllowAdmin = true
                    };
                    options.DBConfig.Endpoints.Add(new ServerEndPoint("127.0.0.1", 6380));
                    options.DBConfig.Database = 11;
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