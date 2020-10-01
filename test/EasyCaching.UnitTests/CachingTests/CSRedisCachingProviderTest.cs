namespace EasyCaching.UnitTests
{
    using System;
    using EasyCaching.Core;
    using EasyCaching.Core.Configurations;
    using EasyCaching.CSRedis;
    using EasyCaching.Serialization.Json;
    using EasyCaching.Serialization.MessagePack;
    using Microsoft.Extensions.DependencyInjection;
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
                })
            );

            IServiceProvider serviceProvider = services.BuildServiceProvider();
            return serviceProvider.GetService<IEasyCachingProvider>();
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

                option.UseCSRedis(config =>
                {
                    config.DBConfig = new CSRedisDBOptions
                    {
                        ConnectionStrings = new System.Collections.Generic.List<string>
                        {
                            "127.0.0.1:6388,defaultDatabase=3,poolsize=10"
                        }
                    };

                }, "cs3");

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
            var cs3 = _providerFactory.GetCachingProvider("cs3");

            var info1 = cs1.GetProviderInfo();
            var info2 = cs2.GetProviderInfo();
            var info3 = cs3.GetProviderInfo();

            Assert.Equal("cs11", info1.Serializer.Name);
            Assert.Equal("cs2", info2.Serializer.Name);
            Assert.Equal(EasyCachingConstValue.DefaultSerializerName, info3.Serializer.Name);
        }
    }
}
