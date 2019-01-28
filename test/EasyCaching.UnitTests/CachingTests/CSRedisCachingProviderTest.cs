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
                            "127.0.0.1:6388,defaultDatabase=13,poolsize=10"
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
}
