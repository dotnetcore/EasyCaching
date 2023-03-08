using System.Threading.Tasks;

namespace EasyCaching.UnitTests
{
    using EasyCaching.Core;
    using EasyCaching.Core.Configurations;
    using EasyCaching.Redis;
    using EasyCaching.Serialization.Json;
    using EasyCaching.Serialization.MessagePack;
    using Microsoft.Extensions.DependencyInjection;
    using StackExchange.Redis;
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
                }, ProviderName).UseRedisLock().WithJson(ProviderName));
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
                    ProviderName).WithJson(ProviderName)
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
                }, ProviderName).WithJson(ProviderName));
            IServiceProvider serviceProvider = services.BuildServiceProvider();
            var dbProvider = serviceProvider.GetService<IRedisDatabaseProvider>();
            Assert.NotNull(dbProvider);

            Assert.Equal(8, dbProvider.GetDatabase().Database);
        }

        [Fact]
        public void Use_Configuration_Options_Should_Succeed()
        {
            IServiceCollection services = new ServiceCollection();
            var redisConfig = ConfigurationOptions.Parse("127.0.0.1:6380");
            redisConfig.DefaultDatabase = 8;
            services.AddEasyCaching(x =>
                 x.UseRedis(options =>
                 {
                     options.DBConfig.ConfigurationOptions = redisConfig;
                 }, ProviderName).UseRedisLock().WithJson(ProviderName));
            IServiceProvider serviceProvider = services.BuildServiceProvider();
            var dbProvider = serviceProvider.GetService<IRedisDatabaseProvider>();
            Assert.NotNull(dbProvider);

            Assert.Equal(8, dbProvider.GetDatabase().Database);
        }

        [Fact]
        public void GetDatabase_Should_Succeed()
        {
            var db = _provider.Database;

            Assert.NotNull(db);
            Assert.IsAssignableFrom<StackExchange.Redis.IDatabase>(db);
        }

        [Fact]
        public void GetDatabase_And_Use_Raw_Method_Should_Succeed()
        {
            var db = (StackExchange.Redis.IDatabase)_provider.Database;
            var ts = db.Ping();
            Assert.True(ts.Ticks > 0);
        }
    }


    public class RedisCachingProviderWithFactoryTest : BaseCachingProviderWithFactoryTest
    {
        public RedisCachingProviderWithFactoryTest()
        {
            IServiceCollection services = new ServiceCollection();
            services.AddEasyCaching(x =>
            {
                x.WithJson("ser");

                x.UseRedis(options =>
                {
                    options.DBConfig = new RedisDBOptions
                    {
                        AllowAdmin = true
                    };
                    options.DBConfig.Endpoints.Add(new ServerEndPoint("127.0.0.1", 6380));
                    options.DBConfig.Database = 3;
                    options.SerializerName = "ser";
                });


                x.UseRedis(options =>
                {
                    options.DBConfig = new RedisDBOptions
                    {
                        AllowAdmin = true
                    };
                    options.DBConfig.Endpoints.Add(new ServerEndPoint("127.0.0.1", 6380));
                    options.DBConfig.Database = 4;
                    options.SerializerName = "ser";
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

            var info1 = se1.GetProviderInfo();
            var info2 = se2.GetProviderInfo();

            Assert.Equal("cs11", info1.Serializer.Name);
            Assert.Equal("se2", info2.Serializer.Name);
        }
    }

    public class RedisCachingProviderWithKeyPrefixTest
    {
        private readonly IEasyCachingProviderFactory _providerFactory;

        public RedisCachingProviderWithKeyPrefixTest()
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
                    options.DBConfig.Database = 8;
                    options.SerializerName = "json";
                }, "NotKeyPrefix");

                x.UseRedis(options =>
                {
                    options.DBConfig = new RedisDBOptions
                    {
                        AllowAdmin = true,
                        KeyPrefix = "foo:"
                    };
                    options.DBConfig.Endpoints.Add(new ServerEndPoint("127.0.0.1", 6380));
                    options.DBConfig.Database = 8;
                    options.SerializerName = "json";
                }, "WithKeyPrefix");

                x.WithJson("json");
            });

            IServiceProvider serviceProvider = services.BuildServiceProvider();
            _providerFactory = serviceProvider.GetService<IEasyCachingProviderFactory>();
        }

        [Fact]
        public void KeyPrefixTest()
        {
            var NotKeyPrefix = _providerFactory.GetCachingProvider("NotKeyPrefix");
            var WithKeyPrefix = _providerFactory.GetCachingProvider("WithKeyPrefix");

            WithKeyPrefix.Set("KeyPrefix", "ok", TimeSpan.FromSeconds(10));

            var val1 = NotKeyPrefix.Get<string>("foo:" + "KeyPrefix");
            var val2 = WithKeyPrefix.Get<string>("foo:" + "KeyPrefix");
            Assert.NotEqual(val1.Value, val2.Value);

            var val3 = WithKeyPrefix.Get<string>("KeyPrefix");
            Assert.Equal(val1.Value, val3.Value);
        }

        [Fact]
        public void RemoveByPrefixTest()
        {
            var WithKeyPrefix = _providerFactory.GetCachingProvider("WithKeyPrefix");

            WithKeyPrefix.Set("KeyPrefix1", "ok", TimeSpan.FromSeconds(10));
            WithKeyPrefix.Set("KeyPrefix2", "ok", TimeSpan.FromSeconds(10));

            var val1 = WithKeyPrefix.Get<string>("KeyPrefix1");
            var val2 = WithKeyPrefix.Get<string>("KeyPrefix2");
            
            Assert.True(val1.HasValue);
            Assert.True(val2.HasValue);
            Assert.Equal(val1.Value, val2.Value);
            
            WithKeyPrefix.RemoveByPrefix("Key");
            
            var val3 = WithKeyPrefix.Get<string>("KeyPrefix1");
            var val4 = WithKeyPrefix.Get<string>("KeyPrefix2");
            Assert.False(val3.HasValue);
            Assert.False(val4.HasValue);
        }
        
        [Theory]
        [InlineData("WithKeyPrefix")]
        [InlineData("NotKeyPrefix")]
        public void RemoveByKeyPatternTest(string provider)
        {
            var WithKeyPrefix = _providerFactory.GetCachingProvider(provider);

            WithKeyPrefix.Set("garden:pots:flowers", "ok", TimeSpan.FromSeconds(10));
            WithKeyPrefix.Set("garden:pots:flowers:test", "ok", TimeSpan.FromSeconds(10));
            WithKeyPrefix.Set("garden:flowerspots:test", "ok", TimeSpan.FromSeconds(10));
            WithKeyPrefix.Set("boo:foo", "ok", TimeSpan.FromSeconds(10));
            WithKeyPrefix.Set("boo:test:foo", "ok", TimeSpan.FromSeconds(10));
            WithKeyPrefix.Set("sky:birds:bar", "ok", TimeSpan.FromSeconds(10));
            WithKeyPrefix.Set("sky:birds:test:bar", "ok", TimeSpan.FromSeconds(10));
            WithKeyPrefix.Set("akey", "ok", TimeSpan.FromSeconds(10));

            var val1 = WithKeyPrefix.Get<string>("garden:pots:flowers");
            var val2 = WithKeyPrefix.Get<string>("garden:pots:flowers:test");
            var val3 = WithKeyPrefix.Get<string>("garden:flowerspots:test");
            var val4 = WithKeyPrefix.Get<string>("boo:foo");
            var val5 = WithKeyPrefix.Get<string>("boo:test:foo");
            var val6 = WithKeyPrefix.Get<string>("sky:birds:bar");
            var val7 = WithKeyPrefix.Get<string>("sky:birds:test:bar");
            var val8 = WithKeyPrefix.Get<string>("akey");
            
            Assert.True(val1.HasValue);
            Assert.True(val2.HasValue);
            Assert.True(val3.HasValue);
            Assert.True(val4.HasValue);
            Assert.True(val5.HasValue);
            Assert.True(val6.HasValue);
            Assert.True(val7.HasValue);
            Assert.True(val8.HasValue);

            // contains
            WithKeyPrefix.RemoveByPattern("*:pots:*");
            
            // postfix
            WithKeyPrefix.RemoveByPattern("*foo");
            
            // prefix
            WithKeyPrefix.RemoveByPattern("sky*"); 
            
            // exact   
            WithKeyPrefix.RemoveByPattern("akey"); 

            var val9 = WithKeyPrefix.Get<string>("garden:pots:flowers");
            var val10 = WithKeyPrefix.Get<string>("garden:pots:flowers:test");
            var val11 = WithKeyPrefix.Get<string>("garden:flowerspots:test");
            var val12 = WithKeyPrefix.Get<string>("boo:foo");
            var val13 = WithKeyPrefix.Get<string>("boo:test:foo");
            var val14 = WithKeyPrefix.Get<string>("sky:birds:bar");
            var val15 = WithKeyPrefix.Get<string>("sky:birds:test:bar");
            var val16 = WithKeyPrefix.Get<string>("akey");
            
            Assert.False(val9.HasValue);
            Assert.False(val10.HasValue);
            Assert.True(val11.HasValue);
            Assert.False(val12.HasValue);
            Assert.False(val13.HasValue);
            Assert.False(val14.HasValue);
            Assert.False(val15.HasValue);
            Assert.False(val16.HasValue);
        }
        
        [Theory]
        [InlineData("WithKeyPrefix")]
        [InlineData("NotKeyPrefix")]
        public async Task RemoveByKeyPatternAsyncTest(string provider)
        {
            var WithKeyPrefix = _providerFactory.GetCachingProvider(provider);

            await WithKeyPrefix.SetAsync("garden:pots:flowers", "ok", TimeSpan.FromSeconds(10));
            await WithKeyPrefix.SetAsync("garden:pots:flowers:test", "ok", TimeSpan.FromSeconds(10));
            await WithKeyPrefix.SetAsync("garden:flowerspots:test", "ok", TimeSpan.FromSeconds(10));
            await WithKeyPrefix.SetAsync("boo:foo", "ok", TimeSpan.FromSeconds(10));
            await WithKeyPrefix.SetAsync("boo:test:foo", "ok", TimeSpan.FromSeconds(10));
            await WithKeyPrefix.SetAsync("sky:birds:bar", "ok", TimeSpan.FromSeconds(10));
            await WithKeyPrefix.SetAsync("sky:birds:test:bar", "ok", TimeSpan.FromSeconds(10));
            await WithKeyPrefix.SetAsync("akey", "ok", TimeSpan.FromSeconds(10));

            var val1 = WithKeyPrefix.Get<string>("garden:pots:flowers");
            var val2 = WithKeyPrefix.Get<string>("garden:pots:flowers:test");
            var val3 = WithKeyPrefix.Get<string>("garden:flowerspots:test");
            var val4 = WithKeyPrefix.Get<string>("boo:foo");
            var val5 = WithKeyPrefix.Get<string>("boo:test:foo");
            var val6 = WithKeyPrefix.Get<string>("sky:birds:bar");
            var val7 = WithKeyPrefix.Get<string>("sky:birds:test:bar");
            var val8 = WithKeyPrefix.Get<string>("akey");
            
            Assert.True(val1.HasValue);
            Assert.True(val2.HasValue);
            Assert.True(val3.HasValue);
            Assert.True(val4.HasValue);
            Assert.True(val5.HasValue);
            Assert.True(val6.HasValue);
            Assert.True(val7.HasValue);
            Assert.True(val8.HasValue);

            // contains
            await WithKeyPrefix.RemoveByPatternAsync("*:pots:*");
            
            // postfix
            await WithKeyPrefix.RemoveByPatternAsync("*foo");
            
            // prefix
            await WithKeyPrefix.RemoveByPatternAsync("sky*"); 
            
            // exact   
            await WithKeyPrefix.RemoveByPatternAsync("akey"); 

            var val9 = WithKeyPrefix.Get<string>("garden:pots:flowers");
            var val10 = WithKeyPrefix.Get<string>("garden:pots:flowers:test");
            var val11 = WithKeyPrefix.Get<string>("garden:flowerspots:test");
            var val12 = WithKeyPrefix.Get<string>("boo:foo");
            var val13 = WithKeyPrefix.Get<string>("boo:test:foo");
            var val14 = WithKeyPrefix.Get<string>("sky:birds:bar");
            var val15 = WithKeyPrefix.Get<string>("sky:birds:test:bar");
            var val16 = WithKeyPrefix.Get<string>("akey");
            
            Assert.False(val9.HasValue);
            Assert.False(val10.HasValue);
            Assert.True(val11.HasValue);
            Assert.False(val12.HasValue);
            Assert.False(val13.HasValue);
            Assert.False(val14.HasValue);
            Assert.False(val15.HasValue);
            Assert.False(val16.HasValue);
        }
    }
}
