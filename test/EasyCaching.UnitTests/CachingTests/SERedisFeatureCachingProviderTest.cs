namespace EasyCaching.UnitTests
{
    using EasyCaching.Core;
    using EasyCaching.Core.Configurations;
    using EasyCaching.Core.Internal;
    using EasyCaching.Redis;
    using Microsoft.Extensions.DependencyInjection;
    using System;
    using System.Threading.Tasks;
    using Xunit;    

    public class SERedisFeatureCachingProviderTest : BaseRedisFeatureCachingProviderTest
    {     
        public SERedisFeatureCachingProviderTest()
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
                    config.DBConfig.Database = 10;
                });
            });

            IServiceProvider serviceProvider = services.BuildServiceProvider();
            _provider = serviceProvider.GetService<IRedisCachingProvider>();
            _nameSpace = "SERedisFeature";
        }

        [Fact]
        protected override async Task LPushXAsync_Not_Exist_CacheKey_Should_Return_Zero()
        {
            await Task.FromResult(1);                
        }

        [Fact]
        protected override async Task LPushXAsync_Should_Succeed()
        {
            await Task.FromResult(1);
        }

        [Fact]
        protected override async Task RPushXAsync_Should_Succeed()
        {
            await Task.FromResult(1);
        }

        [Fact]
        protected override void LPushX_Not_Exist_CacheKey_Should_Return_Zero()
        {
            
        }

        [Fact]
        protected override void LPushX_Should_Succeed()
        {
            
        }

        [Fact]
        protected override void RPushX_Should_Succeed()
        {
            
        }
    }
}
