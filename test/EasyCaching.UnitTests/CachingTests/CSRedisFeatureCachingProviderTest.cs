namespace EasyCaching.UnitTests
{
    using EasyCaching.Core;
    using EasyCaching.CSRedis;
    using Microsoft.Extensions.DependencyInjection;
    using System;

    public class CSRedisFeatureCachingProviderTest  : BaseRedisFeatureCachingProviderTest
    {     
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
                            "127.0.0.1:6388,defaultDatabase=10,poolsize=10"
                        }
                    };
                });
            });

            IServiceProvider serviceProvider = services.BuildServiceProvider();
            _provider = serviceProvider.GetService<IRedisCachingProvider>();
            _nameSpace = "CSRedisFeature";
        }    
    }
}
