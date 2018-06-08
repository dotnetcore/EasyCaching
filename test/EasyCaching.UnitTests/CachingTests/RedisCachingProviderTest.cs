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

    }
}
