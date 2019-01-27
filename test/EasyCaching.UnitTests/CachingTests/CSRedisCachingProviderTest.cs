namespace EasyCaching.UnitTests
{
    using System;
    using EasyCaching.Core;
    using EasyCaching.CSRedis;
    using Microsoft.Extensions.DependencyInjection;
    using Xunit;

    public class CSRedisCachingProviderTest : BaseCachingProviderTest
    {
        public CSRedisCachingProviderTest()
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
                            "127.0.0.1:6379,defaultDatabase=13,poolsize=10"
                        }
                    };
                });
            });

            IServiceProvider serviceProvider = services.BuildServiceProvider();
            _provider = serviceProvider.GetService<IEasyCachingProvider>();
            _defaultTs = TimeSpan.FromSeconds(30);
            _nameSpace = "CSRedisBase";
        }
    }

    public class CSRedisFeatureCachingProviderTest 
    {
        private readonly IRedisCachingProvider _provider;
        private readonly string _nameSpace = "CSRedisFeature";

        public CSRedisFeatureCachingProviderTest()
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
                            "127.0.0.1:6379,defaultDatabase=10,poolsize=10"
                        }
                    };
                });
            });

            IServiceProvider serviceProvider = services.BuildServiceProvider();
            _provider = serviceProvider.GetService<IRedisCachingProvider>();
            //_defaultTs = TimeSpan.FromSeconds(30);
        }

        [Fact]
        public void HMSet_Test_Should_Succeed()
        {
            var cacheKey = $"{_nameSpace}-hmset-{Guid.NewGuid().ToString()}";

            var res = _provider.HMSet(cacheKey, new System.Collections.Generic.Dictionary<string, string>
            {
                {"a1","v1"},{"a2","v2"}
            });

            Assert.True(res);

            var len = _provider.HLen(cacheKey);

            Assert.Equal(2, len);

        }
    }
}
