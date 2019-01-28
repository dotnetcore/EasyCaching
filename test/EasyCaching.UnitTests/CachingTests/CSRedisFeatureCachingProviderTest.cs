namespace EasyCaching.UnitTests
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using EasyCaching.Core;
    using EasyCaching.CSRedis;
    using Microsoft.Extensions.DependencyInjection;
    using Xunit;

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
                            "127.0.0.1:6388,defaultDatabase=10,poolsize=10"
                        }
                    };
                });
            });

            IServiceProvider serviceProvider = services.BuildServiceProvider();
            _provider = serviceProvider.GetService<IRedisCachingProvider>();
            //_defaultTs = TimeSpan.FromSeconds(30);
        }

        [Fact]
        public void HMSet_And_HMGet_Should_Succeed()
        {
            var cacheKey = $"{_nameSpace}-{Guid.NewGuid().ToString()}";

            var res = _provider.HMSet(cacheKey, new Dictionary<string, string>
            {
                {"a1","v1"},{"a2","v2"}
            });

            Assert.True(res);

            var dict = _provider.HMGet(cacheKey,new List<string> { "a1", "a2"});

            Assert.Contains("v1", dict.Values);
            Assert.Contains("v2", dict.Values);

            _provider.HDel(cacheKey);
        }

        [Fact]
        public async Task HMSetAsync_And_HMGetAsync_Should_Succeed()
        {
            var cacheKey = $"{_nameSpace}-{Guid.NewGuid().ToString()}";

            var res = await _provider.HMSetAsync(cacheKey, new Dictionary<string, string>
            {
                {"a1","v1"},{"a2","v2"}
            });

            Assert.True(res);

            var dict = await _provider.HMGetAsync(cacheKey, new List<string> { "a1", "a2" });

            Assert.Contains("v1", dict.Values);
            Assert.Contains("v2", dict.Values);

            await _provider.HDelAsync(cacheKey);
        }

        [Fact]
        public void HKeys_Should_Succeed()
        {
            var cacheKey = $"{_nameSpace}-g-{Guid.NewGuid().ToString()}";

            var res = _provider.HMSet(cacheKey, new Dictionary<string, string>
            {
                {"k1","v1"},{"k2","v2"}
            });

            var keys = _provider.HKeys(cacheKey);

            Assert.Contains("k1", keys);
            Assert.Contains("k2", keys);

            _provider.HDel(cacheKey);
        }

        [Fact]
        public async Task HKeysAsync_Should_Succeed()
        {
            var cacheKey = $"{_nameSpace}-g-{Guid.NewGuid().ToString()}";

            var res = await _provider.HMSetAsync(cacheKey, new Dictionary<string, string>
            {
                {"k1","v1"},{"k2","v2"}
            });

            var keys = await _provider.HKeysAsync(cacheKey);

            Assert.Contains("k1", keys);
            Assert.Contains("k2", keys);

            await _provider.HDelAsync(cacheKey);
        }

        [Fact]
        public void HVals_Should_Succeed()
        {
            var cacheKey = $"{_nameSpace}-g-{Guid.NewGuid().ToString()}";

            var res = _provider.HMSet(cacheKey, new Dictionary<string, string>
            {
                {"k1","v1"},{"k2","v2"}
            });

            var vals = _provider.HVals(cacheKey);

            Assert.Contains("v1", vals);
            Assert.Contains("v2", vals);

            _provider.HDel(cacheKey);
        }

        [Fact]
        public async Task HValsAsync_Should_Succeed()
        {
            var cacheKey = $"{_nameSpace}-g-{Guid.NewGuid().ToString()}";

            var res = await _provider.HMSetAsync(cacheKey, new Dictionary<string, string>
            {
                {"k1","v1"},{"k2","v2"}
            });

            var vals = await _provider.HValsAsync(cacheKey);

            Assert.Contains("v1", vals);
            Assert.Contains("v2", vals);

            await _provider.HDelAsync(cacheKey);
        }

        [Fact]
        public void HGetAll_Should_Succeed()
        {
            var cacheKey = $"{_nameSpace}-g-{Guid.NewGuid().ToString()}";

            var res = _provider.HMSet(cacheKey, new Dictionary<string, string>
            {
                {"a1","v1"},{"a2","v2"}
            });

            var all = _provider.HGetAll(cacheKey);

            Assert.Equal(2, all.Count);
                       
            _provider.HDel(cacheKey);
        }

        [Fact]
        public async Task HGetAllAsync_Should_Succeed()
        {
            var cacheKey = $"{_nameSpace}-g-{Guid.NewGuid().ToString()}";

            var res = await _provider.HMSetAsync(cacheKey, new Dictionary<string, string>
            {
                {"a1","v1"},{"a2","v2"}
            });

            var all = await _provider.HGetAllAsync(cacheKey);

            Assert.Equal(2, all.Count);

            await _provider.HDelAsync(cacheKey);
        }


        [Fact]
        public void HDel_With_Fields_Should_Succeed()
        {
            var cacheKey = $"{_nameSpace}-d-{Guid.NewGuid().ToString()}";

            var res = _provider.HMSet(cacheKey, new Dictionary<string, string>
            {
                {"a1","v1"},{"a2","v2"},{"a3","v3"}
            });

            var del = _provider.HDel(cacheKey, new List<string> { "a1","a3"});

            Assert.Equal(2, del);

            var len = _provider.HLen(cacheKey);

            Assert.Equal(1, len);

            _provider.HDel(cacheKey);
        }

        [Fact]
        public async Task HDelAsync_With_Fields_Should_Succeed()
        {
            var cacheKey = $"{_nameSpace}-d-{Guid.NewGuid().ToString()}";

            var res = await _provider.HMSetAsync(cacheKey, new Dictionary<string, string>
            {
                {"a1","v1"},{"a2","v2"},{"a3","v3"}
            });

            var del = await _provider.HDelAsync(cacheKey, new List<string> { "a1", "a3" });

            Assert.Equal(2, del);

            var len = await _provider.HLenAsync(cacheKey);

            Assert.Equal(1, len);

            await _provider.HDelAsync(cacheKey);
        }

        [Fact]
        public void HDel_WithOut_Fields_Should_Succeed()
        {
            var cacheKey = $"{_nameSpace}-d-{Guid.NewGuid().ToString()}";

            var res = _provider.HMSet(cacheKey, new Dictionary<string, string>
            {
                {"a1","v1"},{"a2","v2"},{"a3","v3"}
            });

            var del = _provider.HDel(cacheKey);

            Assert.Equal(1, del);

            var len = _provider.HLen(cacheKey);

            Assert.Equal(0, len);
        }

        [Fact]
        public async Task HDelAsync_WithOut_Fields_Should_Succeed()
        {
            var cacheKey = $"{_nameSpace}-d-{Guid.NewGuid().ToString()}";

            var res = await _provider.HMSetAsync(cacheKey, new Dictionary<string, string>
            {
                {"a1","v1"},{"a2","v2"},{"a3","v3"}
            });

            var del = await _provider.HDelAsync(cacheKey);

            Assert.Equal(1, del);

            var len = await _provider.HLenAsync(cacheKey);

            Assert.Equal(0, len);
        }

        [Fact]
        public void HIncrBy_Should_Succeed()
        {
            var cacheKey = $"{_nameSpace}-i-{Guid.NewGuid().ToString()}";

            var res = _provider.HIncrBy(cacheKey, "a1");

            Assert.Equal(1, res);

            _provider.HDel(cacheKey);
        }

        [Fact]
        public async Task HIncrByAsync_Should_Succeed()
        {
            var cacheKey = $"{_nameSpace}-i-{Guid.NewGuid().ToString()}";

            var res = await _provider.HIncrByAsync(cacheKey, "a1");

            Assert.Equal(1, res);

            await _provider.HDelAsync(cacheKey);
        }

        [Fact]
        public void HIncrBy_With_Val_Should_Succeed()
        {
            var cacheKey = $"{_nameSpace}-i-{Guid.NewGuid().ToString()}";

            var res = _provider.HIncrBy(cacheKey, "a1", 3);

            Assert.Equal(3, res);

            _provider.HDel(cacheKey);
        }

        [Fact]
        public async Task HIncrByAsync_With_Val_Should_Succeed()
        {
            var cacheKey = $"{_nameSpace}-i-{Guid.NewGuid().ToString()}";

            var res = await _provider.HIncrByAsync(cacheKey, "a1", 3);

            Assert.Equal(3, res);

            await _provider.HDelAsync(cacheKey);
        }

        [Fact]
        public void HExists_With_Other_CacheKey_Should_Return_False()
        {
            var cacheKey = $"{_nameSpace}-e-{Guid.NewGuid().ToString()}";
                      
            var flag = _provider.HExists(cacheKey, "field");

            Assert.False(flag);
        }

        [Fact]
        public async Task HExistsAsync_With_Other_CacheKey_Should_Return_False()
        {
            var cacheKey = $"{_nameSpace}-e-{Guid.NewGuid().ToString()}";

            var flag = await _provider.HExistsAsync(cacheKey, "field");

            Assert.False(flag);
        }


        [Fact]
        public void HExists_With_Other_Field_Should_Return_False()
        {
            var cacheKey = $"{_nameSpace}-e-{Guid.NewGuid().ToString()}";

            var res = _provider.HSet(cacheKey, "a1" , "v1");

            Assert.True(res);

            var flag = _provider.HExists(cacheKey, "field");

            Assert.False(flag);

            _provider.HDel(cacheKey);
        }

        [Fact]
        public async Task HExistsAsync_With_Other_Field_Should_Return_False()
        {
            var cacheKey = $"{_nameSpace}-e-{Guid.NewGuid().ToString()}";

            var res = await _provider.HSetAsync(cacheKey, "a1", "v1");

            Assert.True(res);

            var flag = await _provider.HExistsAsync(cacheKey, "field");

            Assert.False(flag);

            await _provider.HDelAsync(cacheKey);
        }

        [Fact]
        public void HExists_With_Field_Should_Return_True()
        {
            var cacheKey = $"{_nameSpace}-e-{Guid.NewGuid().ToString()}";

            var res = _provider.HSet(cacheKey, "a1", "v1");
            Assert.True(res);

            var flag = _provider.HExists(cacheKey, "a1");
            Assert.True(flag);

            _provider.HDel(cacheKey);
        }

        [Fact]
        public async Task HExistsAsync_With_Field_Should_Return_True()
        {
            var cacheKey = $"{_nameSpace}-e-{Guid.NewGuid().ToString()}";

            var res = await _provider.HSetAsync(cacheKey, "a1", "v1");
            Assert.True(res);

            var flag = await _provider.HExistsAsync(cacheKey, "a1");
            Assert.True(flag);

            await _provider.HDelAsync(cacheKey);
        }

        [Fact]
        public void HSet_With_Field_Should_Succeed()
        {
            var cacheKey = $"{_nameSpace}-{Guid.NewGuid().ToString()}";

            var res = _provider.HMSet(cacheKey, new Dictionary<string, string>
            {
                {"a1","v1"},{"a2","v2"}
            });

            Assert.True(res);

            var flag = _provider.HSet(cacheKey, "a3", "v3");

            Assert.True(flag);

            var len = _provider.HLen(cacheKey);

            Assert.Equal(3, len);

            _provider.HDel(cacheKey);
        }

        [Fact]
        public async Task HSetAsync_With_Field_Should_Succeed()
        {
            var cacheKey = $"{_nameSpace}-{Guid.NewGuid().ToString()}";

            var res = await _provider.HMSetAsync(cacheKey, new Dictionary<string, string>
            {
                {"a1","v1"},{"a2","v2"}
            });

            Assert.True(res);

            var flag = await _provider.HSetAsync(cacheKey, "a3", "v3");

            Assert.True(flag);

            var len = await _provider.HLenAsync(cacheKey);

            Assert.Equal(3, len);

            await _provider.HDelAsync(cacheKey);
        }

        [Fact]
        public void HGet_With_Field_Should_Succeed()
        {
            var cacheKey = $"{_nameSpace}-{Guid.NewGuid().ToString()}";

            var res = _provider.HMSet(cacheKey, new Dictionary<string, string>
            {
                {"a1","v1"},{"a2","v2"}
            });

            Assert.True(res);

            var val = _provider.HGet(cacheKey, "a1");

            Assert.NotNull(val);
            Assert.Equal("v1", val);

            _provider.HDel(cacheKey);
        }

        [Fact]
        public async Task HGetAsync_With_Field_Should_Succeed()
        {
            var cacheKey = $"{_nameSpace}-{Guid.NewGuid().ToString()}";

            var res = await _provider.HMSetAsync(cacheKey, new Dictionary<string, string>
            {
                {"a1","v1"},{"a2","v2"}
            });

            Assert.True(res);

            var val = await _provider.HGetAsync(cacheKey, "a1");

            Assert.NotNull(val);
            Assert.Equal("v1", val);

            await _provider.HDelAsync(cacheKey);
        }

        [Fact]
        public void HGet_With_Other_Field_Should_Return_Null()
        {
            var cacheKey = $"{_nameSpace}-{Guid.NewGuid().ToString()}";

            var res = _provider.HMSet(cacheKey, new Dictionary<string, string>
            {
                {"a1","v1"},{"a2","v2"}
            });

            Assert.True(res);

            var val = _provider.HGet(cacheKey, "a3");

            Assert.Null(val);

            _provider.HDel(cacheKey);
        }

        [Fact]
        public async Task HGetAsync_With_Other_Field_Should_Return_Null()
        {
            var cacheKey = $"{_nameSpace}-{Guid.NewGuid().ToString()}";

            var res = await _provider.HMSetAsync(cacheKey, new Dictionary<string, string>
            {
                {"a1","v1"},{"a2","v2"}
            });

            Assert.True(res);

            var val = await _provider.HGetAsync(cacheKey, "a3");

            Assert.Null(val);

            await _provider.HDelAsync(cacheKey);
        }

        [Fact]
        public void HLen_Should_Succeed()
        {
            var cacheKey = $"{_nameSpace}-{Guid.NewGuid().ToString()}";

            var res = _provider.HMSet(cacheKey, new Dictionary<string, string>
            {
                {"a1","v1"},{"a2","v2"}
            });

            Assert.True(res);

            var len = _provider.HLen(cacheKey);

            Assert.Equal(2, len);

            _provider.HDel(cacheKey);
        }

        [Fact]
        public async Task HLenAsync_Should_Succeed()
        {
            var cacheKey = $"{_nameSpace}-{Guid.NewGuid().ToString()}";

            var res = await _provider.HMSetAsync(cacheKey, new Dictionary<string, string>
            {
                {"a1","v1"},{"a2","v2"}
            });

            Assert.True(res);

            var len = await _provider.HLenAsync(cacheKey);

            Assert.Equal(2, len);

            await _provider.HDelAsync(cacheKey);
        }

        [Fact]
        public void HMSet_With_Expiration_Should_Succeed()
        {
            var cacheKey = $"{_nameSpace}-{Guid.NewGuid().ToString()}";

            var res = _provider.HMSet(cacheKey, new Dictionary<string, string>
            {
                {"a1","v1"},{"a2","v2"}
            }, TimeSpan.FromSeconds(1));

            Assert.True(res);

            System.Threading.Thread.Sleep(1000);

            var flag = _provider.Exists(cacheKey);
            Assert.False(flag);

            _provider.HDel(cacheKey);
        }

        [Fact]
        public async Task HMSetAsync_With_Expiration_Should_Succeed()
        {
            var cacheKey = $"{_nameSpace}-{Guid.NewGuid().ToString()}";

            var res = await _provider.HMSetAsync(cacheKey, new Dictionary<string, string>
            {
                {"a1","v1"},{"a2","v2"}
            }, TimeSpan.FromSeconds(1));

            Assert.True(res);

            await Task.Delay(1000);

            var flag = await _provider.ExistsAsync(cacheKey);
            Assert.False(flag);

            await _provider.HDelAsync(cacheKey);
        }
    }
}
