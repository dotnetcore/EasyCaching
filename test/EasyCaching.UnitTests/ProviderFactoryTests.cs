namespace EasyCaching.UnitTests
{
    using EasyCaching.Core;
    using EasyCaching.Core.Configurations;
    using EasyCaching.CSRedis;
    using EasyCaching.Redis;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using Xunit;

    public class ProviderFactoryTests
    {
        private readonly IEasyCachingProvider _e1;
        private readonly IEasyCachingProvider _e2;
        private readonly IRedisCachingProvider _r1;
        private readonly IRedisCachingProvider _r2;

        public ProviderFactoryTests()
        {
            IServiceCollection services = new ServiceCollection();
            services.AddEasyCaching(option =>
            {
                option.UseRedis(config =>
                {
                    config.DBConfig = new RedisDBOptions
                    {
                        AllowAdmin = true
                    };
                    config.DBConfig.Endpoints.Add(new ServerEndPoint("127.0.0.1", 6380));
                    config.DBConfig.Database = 12;
                }, "redis1");

                option.UseCSRedis(config =>
                {
                    config.DBConfig = new CSRedisDBOptions
                    {
                        ConnectionStrings = new System.Collections.Generic.List<string>
                        {
                            "127.0.0.1:6388,defaultDatabase=12,poolsize=10"
                        }
                    };
                }, "redis2");
            });


            IServiceProvider serviceProvider = services.BuildServiceProvider();

            var factory = serviceProvider.GetService<IEasyCachingProviderFactory>();

            _e1 = factory.GetCachingProvider("redis1");
            _e2 = factory.GetCachingProvider("redis2");
            _r1 = factory.GetRedisProvider("redis1");
            _r2 = factory.GetRedisProvider("redis2");
        }

        [Fact]
        public void ProviderName_Should_Be_Same()
        {        
            Assert.Equal("redis1", _r1.RedisName);
            Assert.Equal("redis1", _e1.Name);

            Assert.Equal("redis2", _r2.RedisName);
            Assert.Equal("redis2", _e2.Name);
        }

        [Fact]
        public void Redis_Providers_Should_Be_Independents()
        {
            var cacheKey = $"factory-{Guid.NewGuid().ToString()}";

            var flag = _r1.StringSet(cacheKey, "123");
            Assert.True(flag);

            var val1 = _r1.KeyExists(cacheKey);
            Assert.True( val1);

            var val2 = _r2.KeyExists(cacheKey);
            Assert.False(val2);

            _r1.KeyDel(cacheKey);
        }
    }
}
