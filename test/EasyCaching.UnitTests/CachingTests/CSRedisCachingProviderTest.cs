using System.Threading.Tasks;

namespace EasyCaching.UnitTests
{
    using System;
    using EasyCaching.Core;
    using EasyCaching.Core.Configurations;
    using EasyCaching.CSRedis;
    using EasyCaching.Serialization.Json;
    using EasyCaching.Serialization.MessagePack;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Options;
    using Xunit;

    public class CSRedisCachingProviderTest : BaseCachingProviderTest
    {
        public CSRedisCachingProviderTest()
        {

            _defaultTs = TimeSpan.FromSeconds(30);
            _nameSpace = "CSRedisBase";
        }

        protected override IEasyCachingProvider CreateCachingProvider(Action<BaseProviderOptions> additionalSetup)
        {
            var services = new ServiceCollection();
            services.AddEasyCaching(x =>
                x.UseCSRedis(options =>
                {
                    options.DBConfig = new CSRedisDBOptions
                    {
                        ConnectionStrings = new System.Collections.Generic.List<string>
                        {
                            "127.0.0.1:6388,defaultDatabase=13,poolsize=10"
                        }
                    };
                    additionalSetup(options);
                }).UseCSRedisLock().WithJson(EasyCachingConstValue.DefaultCSRedisName));

            IServiceProvider serviceProvider = services.BuildServiceProvider();
            return serviceProvider.GetService<IEasyCachingProvider>();
        }

        [Fact]
        public void GetDatabase_Should_Succeed()
        {
            var db = _provider.Database;

            Assert.NotNull(db);
            Assert.IsType<EasyCachingCSRedisClient>(db);
        }

        [Fact]
        public void GetDatabase_And_Use_Raw_Method_Should_Succeed()
        {
            var db = (EasyCachingCSRedisClient)_provider.Database;
            var flag = db.Ping();
            Assert.True(flag);
        }
    }

    public class CSRedisCachingProviderWithNamedSerTest
    {
        private readonly IEasyCachingProviderFactory _providerFactory;

        public CSRedisCachingProviderWithNamedSerTest()
        {
            IServiceCollection services = new ServiceCollection();
            services.AddEasyCaching(option =>
            {
                option.UseCSRedis(config =>
                {
                    config.DBConfig = new CSRedisDBOptions
                    {
                        ConnectionStrings = new System.Collections.Generic.List<string>
                        {
                            "127.0.0.1:6388,defaultDatabase=2,poolsize=10"
                        }
                    };

                    config.SerializerName = "cs11";
                }, "cs1");

                option.UseCSRedis(config =>
                {
                    config.DBConfig = new CSRedisDBOptions
                    {
                        ConnectionStrings = new System.Collections.Generic.List<string>
                        {
                            "127.0.0.1:6388,defaultDatabase=3,poolsize=10"
                        }
                    };

                }, "cs2");

                option.WithJson("json").WithMessagePack("cs11").WithJson("cs2");
            });

            IServiceProvider serviceProvider = services.BuildServiceProvider();
            _providerFactory = serviceProvider.GetService<IEasyCachingProviderFactory>();
        }

        [Fact]
        public void NamedSerializerTest()
        {
            var cs1 = _providerFactory.GetCachingProvider("cs1");
            var cs2 = _providerFactory.GetCachingProvider("cs2");

            var info1 = cs1.GetProviderInfo();
            var info2 = cs2.GetProviderInfo();

            Assert.Equal("cs11", info1.Serializer.Name);
            Assert.Equal("cs2", info2.Serializer.Name);
        }
    }

    public class CSRedisCachingProviderWithKeyPrefixTest
    {
        private readonly IEasyCachingProviderFactory _providerFactory;

        public CSRedisCachingProviderWithKeyPrefixTest()
        {
            IServiceCollection services = new ServiceCollection();
            services.AddEasyCaching(x =>
            {
                x.UseCSRedis(config =>
                {
                    config.DBConfig = new CSRedisDBOptions
                    {
                        ConnectionStrings = new System.Collections.Generic.List<string>
                        {
                            "127.0.0.1:6388,defaultDatabase=3,poolsize=10"
                        }
                    };

                    config.SerializerName = "json";
                }, "NotKeyPrefix");

                x.UseCSRedis(config =>
                {
                    config.DBConfig = new CSRedisDBOptions
                    {
                        ConnectionStrings = new System.Collections.Generic.List<string>
                        {
                            "127.0.0.1:6388,defaultDatabase=3,poolsize=10,prefix=foo:"
                        }
                    };
                    config.SerializerName = "json";

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
