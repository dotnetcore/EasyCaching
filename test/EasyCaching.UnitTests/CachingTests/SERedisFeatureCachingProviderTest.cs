namespace EasyCaching.UnitTests
{
    using EasyCaching.Core;
    using EasyCaching.Core.Configurations;
    using EasyCaching.Redis;
    using Microsoft.Extensions.DependencyInjection;
    using System;
    using System.Collections.Generic;
    using System.Linq;
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
            _baseProvider = serviceProvider.GetService<IEasyCachingProvider>();
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

        protected override async Task GeoAddAsync_And_GeoPosAsync_Should_Succeed()
        {
            var cacheKey = $"{_nameSpace}-geohashasync-{Guid.NewGuid().ToString()}";

            var res = await _provider.GeoAddAsync(cacheKey, new List<(double longitude, double latitude, string member)> { (13.361389, 38.115556, "Palermo"), (15.087269, 37.502669, "Catania") });

            Assert.Equal(2, res);

            var pos = await _provider.GeoPosAsync(cacheKey, new List<string> { "Palermo", "Catania", "NonExisting" });

            Assert.Equal(3, pos.Count);
            Assert.Contains(13.361389338970184m, pos.Where(x => x.HasValue).Select(x => x.Value.longitude));
            Assert.Contains(15.087267458438873m, pos.Where(x => x.HasValue).Select(x => x.Value.longitude));
            Assert.Contains(null, pos);

            await _provider.KeyDelAsync(cacheKey);
        }

        protected override void GeoAdd_And_GeoPos_Should_Succeed()
        {
            var cacheKey = $"{_nameSpace}-geohash-{Guid.NewGuid().ToString()}";

            var res = _provider.GeoAdd(cacheKey, new List<(double longitude, double latitude, string member)> { (13.361389, 38.115556, "Palermo"), (15.087269, 37.502669, "Catania") });

            Assert.Equal(2, res);

            var pos = _provider.GeoPos(cacheKey, new List<string> { "Palermo", "Catania", "NonExisting" });

            Assert.Equal(3, pos.Count);
            Assert.Contains(13.361389338970184m, pos.Where(x => x.HasValue).Select(x => x.Value.longitude));
            Assert.Contains(15.087267458438873m, pos.Where(x => x.HasValue).Select(x => x.Value.longitude));
            Assert.Contains(null, pos);

            _provider.KeyDel(cacheKey);
        }
    }
}
