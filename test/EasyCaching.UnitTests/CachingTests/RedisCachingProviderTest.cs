namespace EasyCaching.UnitTests
{
    using EasyCaching.Core;
    using EasyCaching.Core.Internal;
    using EasyCaching.Redis;
    using Microsoft.Extensions.DependencyInjection;
    using System;

    public class RedisCachingProviderTest : BaseCachingProviderTest
    {
        public RedisCachingProviderTest()
        {
            IServiceCollection services = new ServiceCollection();
            services.AddDefaultRedisCache(options =>
            {
                options.Endpoints.Add(new ServerEndPoint("127.0.0.1", 6379));
            });
            IServiceProvider serviceProvider = services.BuildServiceProvider();
            _provider = serviceProvider.GetService<IEasyCachingProvider>();
            _defaultTs = TimeSpan.FromSeconds(30);
        }
    }
}
