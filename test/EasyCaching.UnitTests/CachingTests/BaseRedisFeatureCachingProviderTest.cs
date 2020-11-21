namespace EasyCaching.UnitTests
{
    using EasyCaching.Core;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Xunit;

    public abstract class BaseRedisFeatureCachingProviderTest
    {
        protected IRedisCachingProvider _provider;
        protected IEasyCachingProvider _baseProvider;
        protected string _nameSpace = string.Empty;

        #region Keys             
        [Fact]
        protected virtual void KeyDel_Should_Succeed()
        {
            var cacheKey = $"{_nameSpace}-{Guid.NewGuid().ToString()}";

            var res = _provider.StringSet(cacheKey, "123");

            Assert.True(res);

            var flag = _provider.KeyDel(cacheKey);

            Assert.True(flag);
        }

        [Fact]
        protected virtual async Task KeyDelAsync_Should_Succeed()
        {
            var cacheKey = $"{_nameSpace}-{Guid.NewGuid().ToString()}";

            var res = _provider.StringSet(cacheKey, "123");

            Assert.True(res);

            var flag = await _provider.KeyDelAsync(cacheKey);

            Assert.True(flag);
        }

        [Fact]
        protected virtual void StringSet_And_KeyExpire_And_TTL_Should_Succeed()
        {
            var cacheKey = $"{_nameSpace}-{Guid.NewGuid().ToString()}";

            var res = _provider.StringSet(cacheKey, "123");

            Assert.True(res);

            var flag = _provider.KeyExpire(cacheKey, 10);

            Assert.True(flag);

            var ttl = _provider.TTL(cacheKey);

            Assert.InRange(ttl, 1, 10);

            _provider.KeyDel(cacheKey);
        }

        [Fact]
        protected virtual async Task StringSet_And_KeyExpire_And_TTL_Async_Should_Succeed()
        {
            var cacheKey = $"{_nameSpace}-{Guid.NewGuid().ToString()}";

            var res = await _provider.StringSetAsync(cacheKey, "123");

            Assert.True(res);

            var flag = await _provider.KeyExpireAsync(cacheKey, 10);

            Assert.True(flag);

            var ttl = await _provider.TTLAsync(cacheKey);

            Assert.InRange(ttl, 1, 10);

            await _provider.KeyDelAsync(cacheKey);
        }
        #endregion

        #region String

        [Fact]
        protected virtual void StringSet_With_Nx_Should_Succeed()
        {
            var cacheKey = $"{_nameSpace}-{Guid.NewGuid().ToString()}";
            var cacheValue = Guid.NewGuid().ToString();

            var res = _provider.StringSet(cacheKey, cacheValue, when: "nx");
            Assert.True(res);

            var res2 = _provider.StringSet(cacheKey, cacheValue, when: "nx");
            Assert.False(res2);

            _provider.KeyDel(cacheKey);
        }

        [Fact]
        protected virtual async Task StringSetAsync_With_Nx_Should_Succeed()
        {
            var cacheKey = $"{_nameSpace}-{Guid.NewGuid().ToString()}";
            var cacheValue = Guid.NewGuid().ToString();

            var res = await _provider.StringSetAsync(cacheKey, cacheValue, when: "nx");
            Assert.True(res);

            var res2 = await _provider.StringSetAsync(cacheKey, cacheValue, when: "nx");
            Assert.False(res2);

            _provider.KeyDel(cacheKey);
        }

        [Fact]
        protected virtual void StringSet_With_Xx_Should_Succeed()
        {
            var cacheKey = $"{_nameSpace}-{Guid.NewGuid().ToString()}";
            var cacheValue = Guid.NewGuid().ToString();

            var res = _provider.StringSet(cacheKey, cacheValue, when: "xx");
            Assert.False(res);

            var res2 = _provider.StringSet(cacheKey, cacheValue);
            Assert.True(res2);
            var res3 = _provider.StringSet(cacheKey, cacheValue, when: "xx");
            Assert.True(res3);

            _provider.KeyDel(cacheKey);
        }

        [Fact]
        protected virtual async Task StringSetAsync_With_Xx_Should_Succeed()
        {
            var cacheKey = $"{_nameSpace}-{Guid.NewGuid().ToString()}";
            var cacheValue = Guid.NewGuid().ToString();

            var res = await _provider.StringSetAsync(cacheKey, cacheValue, when: "xx");
            Assert.False(res);

            var res2 = await _provider.StringSetAsync(cacheKey, cacheValue);
            Assert.True(res2);
            var res3 = await _provider.StringSetAsync(cacheKey, cacheValue, when: "xx");
            Assert.True(res3);

            _provider.KeyDel(cacheKey);
        }

        [Fact]
        protected virtual void StringSet_And_StringGet_Should_Succeed()
        {
            var cacheKey = $"{_nameSpace}-{Guid.NewGuid().ToString()}";
            var cacheValue = Guid.NewGuid().ToString();

            var res = _provider.StringSet(cacheKey, cacheValue);

            Assert.True(res);

            var val = _provider.StringGet(cacheKey);

            Assert.Equal(cacheValue, val);

            _provider.KeyDel(cacheKey);
        }

        [Fact]
        protected virtual async Task StringSet_And_StringGet_Async_Should_Succeed()
        {
            var cacheKey = $"{_nameSpace}-{Guid.NewGuid().ToString()}";
            var cacheValue = Guid.NewGuid().ToString();

            var res = await _provider.StringSetAsync(cacheKey, cacheValue);

            Assert.True(res);

            var val = await _provider.StringGetAsync(cacheKey);

            Assert.Equal(cacheValue, val);

            _provider.KeyDel(cacheKey);
        }

        [Fact]
        protected virtual void IncrBy_Should_Succeed()
        {
            var cacheKey = $"{_nameSpace}-{Guid.NewGuid().ToString()}";

            var res = _provider.IncrBy(cacheKey);

            Assert.Equal(1, res);

            var val = _provider.IncrBy(cacheKey, 3);

            Assert.Equal(4, val);

            _provider.KeyDel(cacheKey);
        }

        [Fact]
        protected virtual async Task IncrByAsync_Should_Succeed()
        {
            var cacheKey = $"{_nameSpace}-{Guid.NewGuid().ToString()}";

            var res = await _provider.IncrByAsync(cacheKey);

            Assert.Equal(1, res);

            var val = await _provider.IncrByAsync(cacheKey, 3);

            Assert.Equal(4, val);

            await _provider.KeyDelAsync(cacheKey);
        }

        [Fact]
        protected virtual void IncrByFloat_Should_Succeed()
        {
            var cacheKey = $"{_nameSpace}-{Guid.NewGuid().ToString()}";

            var res = _provider.IncrByFloat(cacheKey);

            Assert.Equal(1d, res);

            var val = _provider.IncrByFloat(cacheKey, 3d);

            Assert.Equal(4d, val);

            _provider.KeyDel(cacheKey);
        }

        [Fact]
        protected virtual async Task IncrByFloatAsync_Should_Succeed()
        {
            var cacheKey = $"{_nameSpace}-{Guid.NewGuid().ToString()}";

            var res = await _provider.IncrByFloatAsync(cacheKey);

            Assert.Equal(1d, res);

            var val = await _provider.IncrByFloatAsync(cacheKey, 3d);

            Assert.Equal(4d, val);

            await _provider.KeyDelAsync(cacheKey);
        }

        [Fact]
        protected virtual void StringLen_Should_Succeed()
        {
            var cacheKey = $"{_nameSpace}-{Guid.NewGuid().ToString()}";
            var cacheValue = "catcherwong";

            var flag = _provider.StringSet(cacheKey, cacheValue);

            Assert.True(flag);

            var len = _provider.StringLen(cacheKey);

            Assert.Equal(11, len);

            _provider.KeyDel(cacheKey);
        }

        [Fact]
        protected virtual async Task StringLenAsync_Should_Succeed()
        {
            var cacheKey = $"{_nameSpace}-{Guid.NewGuid().ToString()}";
            var cacheValue = "catcherwong";

            var flag = await _provider.StringSetAsync(cacheKey, cacheValue);

            Assert.True(flag);

            var len = await _provider.StringLenAsync(cacheKey);

            Assert.Equal(11, len);

            await _provider.KeyDelAsync(cacheKey);
        }

        [Fact]
        protected virtual void StringSetRange_Should_Succeed()
        {
            var cacheKey = $"{_nameSpace}-{Guid.NewGuid().ToString()}";
            var cacheValue = "Hello World";

            var flag = _provider.StringSet(cacheKey, cacheValue);

            Assert.True(flag);

            var len = _provider.StringSetRange(cacheKey, 6, "Redis");

            Assert.Equal(11, len);

            var val = _provider.StringGet(cacheKey);

            Assert.Equal("Hello Redis", val);

            _provider.KeyDel(cacheKey);
        }

        [Fact]
        protected virtual async Task StringSetRangeAsync_Should_Succeed()
        {
            var cacheKey = $"{_nameSpace}-{Guid.NewGuid().ToString()}";
            var cacheValue = "Hello World";

            var flag = await _provider.StringSetAsync(cacheKey, cacheValue);

            Assert.True(flag);

            var len = await _provider.StringSetRangeAsync(cacheKey, 6, "Redis");

            Assert.Equal(11, len);

            var val = await _provider.StringGetAsync(cacheKey);

            Assert.Equal("Hello Redis", val);

            await _provider.KeyDelAsync(cacheKey);
        }

        [Fact]
        protected virtual void StringGetRange_Should_Succeed()
        {
            var cacheKey = $"{_nameSpace}-{Guid.NewGuid().ToString()}";
            var cacheValue = "This is a string";

            var flag = _provider.StringSet(cacheKey, cacheValue);

            Assert.True(flag);

            var res1 = _provider.StringGetRange(cacheKey, 0, 3);

            Assert.Equal("This", res1);

            var res2 = _provider.StringGetRange(cacheKey, -3, -1);

            Assert.Equal("ing", res2);

            _provider.KeyDel(cacheKey);
        }

        [Fact]
        protected virtual async Task StringGetRangeAsync_Should_Succeed()
        {
            var cacheKey = $"{_nameSpace}-{Guid.NewGuid().ToString()}";
            var cacheValue = "This is a string";

            var flag = await _provider.StringSetAsync(cacheKey, cacheValue);

            Assert.True(flag);

            var res1 = await _provider.StringGetRangeAsync(cacheKey, 0, 3);

            Assert.Equal("This", res1);

            var res2 = await _provider.StringGetRangeAsync(cacheKey, -3, -1);

            Assert.Equal("ing", res2);

            await _provider.KeyDelAsync(cacheKey);
        }


        #endregion

        #region Hash
        [Fact]
        protected virtual void HMSet_And_HMGet_Should_Succeed()
        {
            var cacheKey = $"{_nameSpace}-{Guid.NewGuid().ToString()}";

            var res = _provider.HMSet(cacheKey, new Dictionary<string, string>
            {
                {"a1","v1"},{"a2","v2"}
            });

            Assert.True(res);

            var dict = _provider.HMGet(cacheKey, new List<string> { "a1", "a2" });

            Assert.Contains("v1", dict.Values);
            Assert.Contains("v2", dict.Values);

            _provider.HDel(cacheKey);
        }

        [Fact]
        protected virtual async Task HMSetAsync_And_HMGetAsync_Should_Succeed()
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
        protected virtual void HKeys_Should_Succeed()
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
        protected virtual async Task HKeysAsync_Should_Succeed()
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
        protected virtual void HVals_Should_Succeed()
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
        protected virtual async Task HValsAsync_Should_Succeed()
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
        protected virtual void HGetAll_Should_Succeed()
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
        protected virtual async Task HGetAllAsync_Should_Succeed()
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
        protected virtual void HDel_With_Fields_Should_Succeed()
        {
            var cacheKey = $"{_nameSpace}-d-{Guid.NewGuid().ToString()}";

            var res = _provider.HMSet(cacheKey, new Dictionary<string, string>
            {
                {"a1","v1"},{"a2","v2"},{"a3","v3"}
            });

            var del = _provider.HDel(cacheKey, new List<string> { "a1", "a3" });

            Assert.Equal(2, del);

            var len = _provider.HLen(cacheKey);

            Assert.Equal(1, len);

            _provider.HDel(cacheKey);
        }

        [Fact]
        protected virtual async Task HDelAsync_With_Fields_Should_Succeed()
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
        protected virtual void HDel_WithOut_Fields_Should_Succeed()
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
        protected virtual async Task HDelAsync_WithOut_Fields_Should_Succeed()
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
        protected virtual void HIncrBy_Should_Succeed()
        {
            var cacheKey = $"{_nameSpace}-i-{Guid.NewGuid().ToString()}";

            var res = _provider.HIncrBy(cacheKey, "a1");

            Assert.Equal(1, res);

            _provider.HDel(cacheKey);
        }

        [Fact]
        protected virtual async Task HIncrByAsync_Should_Succeed()
        {
            var cacheKey = $"{_nameSpace}-i-{Guid.NewGuid().ToString()}";

            var res = await _provider.HIncrByAsync(cacheKey, "a1");

            Assert.Equal(1, res);

            await _provider.HDelAsync(cacheKey);
        }

        [Fact]
        protected virtual void HIncrBy_With_Val_Should_Succeed()
        {
            var cacheKey = $"{_nameSpace}-i-{Guid.NewGuid().ToString()}";

            var res = _provider.HIncrBy(cacheKey, "a1", 3);

            Assert.Equal(3, res);

            _provider.HDel(cacheKey);
        }

        [Fact]
        protected virtual async Task HIncrByAsync_With_Val_Should_Succeed()
        {
            var cacheKey = $"{_nameSpace}-i-{Guid.NewGuid().ToString()}";

            var res = await _provider.HIncrByAsync(cacheKey, "a1", 3);

            Assert.Equal(3, res);

            await _provider.HDelAsync(cacheKey);
        }

        [Fact]
        protected virtual void HExists_With_Other_CacheKey_Should_Return_False()
        {
            var cacheKey = $"{_nameSpace}-e-{Guid.NewGuid().ToString()}";

            var flag = _provider.HExists(cacheKey, "field");

            Assert.False(flag);
        }

        [Fact]
        protected virtual async Task HExistsAsync_With_Other_CacheKey_Should_Return_False()
        {
            var cacheKey = $"{_nameSpace}-e-{Guid.NewGuid().ToString()}";

            var flag = await _provider.HExistsAsync(cacheKey, "field");

            Assert.False(flag);
        }


        [Fact]
        protected virtual void HExists_With_Other_Field_Should_Return_False()
        {
            var cacheKey = $"{_nameSpace}-e-{Guid.NewGuid().ToString()}";

            var res = _provider.HSet(cacheKey, "a1", "v1");

            Assert.True(res);

            var flag = _provider.HExists(cacheKey, "field");

            Assert.False(flag);

            _provider.HDel(cacheKey);
        }

        [Fact]
        protected virtual async Task HExistsAsync_With_Other_Field_Should_Return_False()
        {
            var cacheKey = $"{_nameSpace}-e-{Guid.NewGuid().ToString()}";

            var res = await _provider.HSetAsync(cacheKey, "a1", "v1");

            Assert.True(res);

            var flag = await _provider.HExistsAsync(cacheKey, "field");

            Assert.False(flag);

            await _provider.HDelAsync(cacheKey);
        }

        [Fact]
        protected virtual void HExists_With_Field_Should_Return_True()
        {
            var cacheKey = $"{_nameSpace}-e-{Guid.NewGuid().ToString()}";

            var res = _provider.HSet(cacheKey, "a1", "v1");
            Assert.True(res);

            var flag = _provider.HExists(cacheKey, "a1");
            Assert.True(flag);

            _provider.HDel(cacheKey);
        }

        [Fact]
        protected virtual async Task HExistsAsync_With_Field_Should_Return_True()
        {
            var cacheKey = $"{_nameSpace}-e-{Guid.NewGuid().ToString()}";

            var res = await _provider.HSetAsync(cacheKey, "a1", "v1");
            Assert.True(res);

            var flag = await _provider.HExistsAsync(cacheKey, "a1");
            Assert.True(flag);

            await _provider.HDelAsync(cacheKey);
        }

        [Fact]
        protected virtual void HSet_With_Field_Should_Succeed()
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
        protected virtual async Task HSetAsync_With_Field_Should_Succeed()
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
        protected virtual void HGet_With_Field_Should_Succeed()
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
        protected virtual async Task HGetAsync_With_Field_Should_Succeed()
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
        protected virtual void HGet_With_Other_Field_Should_Return_Null()
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
        protected virtual async Task HGetAsync_With_Other_Field_Should_Return_Null()
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
        protected virtual void HLen_Should_Succeed()
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
        protected virtual async Task HLenAsync_Should_Succeed()
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
        protected virtual void HMSet_With_Expiration_Should_Succeed()
        {
            var cacheKey = $"{_nameSpace}-{Guid.NewGuid().ToString()}";

            var res = _provider.HMSet(cacheKey, new Dictionary<string, string>
            {
                {"a1","v1"},{"a2","v2"}
            }, TimeSpan.FromSeconds(1));

            Assert.True(res);

            System.Threading.Thread.Sleep(1050);

            var val = _provider.HGet(cacheKey, "a1");
            Assert.Null(val);

            _provider.HDel(cacheKey);
        }

        [Fact]
        protected virtual async Task HMSetAsync_With_Expiration_Should_Succeed()
        {
            var cacheKey = $"{_nameSpace}-{Guid.NewGuid().ToString()}";

            var res = await _provider.HMSetAsync(cacheKey, new Dictionary<string, string>
            {
                {"a1","v1"},{"a2","v2"}
            }, TimeSpan.FromSeconds(1));

            Assert.True(res);

            await Task.Delay(1050);

            var val = await _provider.HGetAsync(cacheKey, "a1");
            Assert.Null(val);

            await _provider.HDelAsync(cacheKey);
        }
        #endregion

        #region List
        [Fact]
        protected virtual void LPush_And_LPop_Should_Succeed()
        {
            var cacheKey = $"{_nameSpace}-{Guid.NewGuid().ToString()}";

            var res = _provider.LPush(cacheKey, new List<string> { "p1", "p2" });
            Assert.Equal(2, res);

            var val = _provider.LPop<string>(cacheKey);
            Assert.Equal("p2", val);

            _baseProvider.Remove(cacheKey);
        }

        [Fact]
        protected virtual async Task LPushAsync_And_LPopAsync_Should_Succeed()
        {
            var cacheKey = $"{_nameSpace}-{Guid.NewGuid().ToString()}";

            var res = await _provider.LPushAsync(cacheKey, new List<string> { "p1", "p2" });
            Assert.Equal(2, res);

            var val = await _provider.LPopAsync<string>(cacheKey);
            Assert.Equal("p2", val);

            await _baseProvider.RemoveAsync(cacheKey);
        }

        [Fact]
        protected virtual void LPushX_Should_Succeed()
        {
            var cacheKey = $"{_nameSpace}-{Guid.NewGuid().ToString()}";

            var res = _provider.LPush(cacheKey, new List<string> { "p1", "p2" });
            Assert.Equal(2, res);

            var val = _provider.LPushX<string>(cacheKey, "p4");
            Assert.Equal(3, val);

            var pop = _provider.LPop<string>(cacheKey);
            Assert.Equal("p4", pop);

            _baseProvider.Remove(cacheKey);
        }


        [Fact]
        protected virtual async Task LPushXAsync_Should_Succeed()
        {
            var cacheKey = $"{_nameSpace}-{Guid.NewGuid().ToString()}";

            var res = await _provider.LPushAsync(cacheKey, new List<string> { "p1", "p2" });
            Assert.Equal(2, res);

            var val = await _provider.LPushXAsync<string>(cacheKey, "p4");
            Assert.Equal(3, val);

            var pop = await _provider.LPopAsync<string>(cacheKey);
            Assert.Equal("p4", pop);

            await _baseProvider.RemoveAsync(cacheKey);
        }

        [Fact]
        protected virtual void LPushX_Not_Exist_CacheKey_Should_Return_Zero()
        {
            var cacheKey = $"{_nameSpace}-nex-{Guid.NewGuid().ToString()}";

            var val = _provider.LPushX<string>(cacheKey, "p4");

            Assert.Equal(0, val);
        }

        [Fact]
        protected virtual async Task LPushXAsync_Not_Exist_CacheKey_Should_Return_Zero()
        {
            var cacheKey = $"{_nameSpace}-nex-{Guid.NewGuid().ToString()}";

            var val = await _provider.LPushXAsync<string>(cacheKey, "p4");

            Assert.Equal(0, val);
        }

        [Fact]
        protected virtual void LSet_And_LRange_Should_Succeed()
        {
            var cacheKey = $"{_nameSpace}-{Guid.NewGuid().ToString()}";

            var res = _provider.LPush(cacheKey, new List<string> { "p1" });
            Assert.Equal(1, res);

            var val = _provider.LSet<string>(cacheKey, 0, "p3");
            Assert.True(val);

            var list = _provider.LRange<string>(cacheKey, 0, -1);

            Assert.Single(list);
            Assert.Equal("p3", list[0]);

            _baseProvider.Remove(cacheKey);
        }

        [Fact]
        protected virtual async Task LSetAsync_And_LRangeAsync_Should_Succeed()
        {
            var cacheKey = $"{_nameSpace}-{Guid.NewGuid().ToString()}";

            var res = await _provider.LPushAsync(cacheKey, new List<string> { "p1" });
            Assert.Equal(1, res);

            var val = await _provider.LSetAsync<string>(cacheKey, 0, "p3");
            Assert.True(val);

            var list = await _provider.LRangeAsync<string>(cacheKey, 0, -1);

            Assert.Single(list);
            Assert.Equal("p3", list[0]);

            await _baseProvider.RemoveAsync(cacheKey);
        }

        [Fact]
        protected virtual void LIndex_Should_Succeed()
        {
            var cacheKey = $"{_nameSpace}-{Guid.NewGuid().ToString()}";

            var res = _provider.LPush(cacheKey, new List<string> { "p1", "p2" });

            Assert.Equal(2, res);

            var val0 = _provider.LIndex<string>(cacheKey, 0);
            var val1 = _provider.LIndex<string>(cacheKey, 1);

            Assert.Equal("p2", val0);
            Assert.Equal("p1", val1);

            _baseProvider.Remove(cacheKey);
        }

        [Fact]
        protected virtual async Task LIndexAsync_Should_Succeed()
        {
            var cacheKey = $"{_nameSpace}-{Guid.NewGuid().ToString()}";

            var res = await _provider.LPushAsync(cacheKey, new List<string> { "p1", "p2" });

            Assert.Equal(2, res);

            var val0 = await _provider.LIndexAsync<string>(cacheKey, 0);
            var val1 = await _provider.LIndexAsync<string>(cacheKey, 1);

            Assert.Equal("p2", val0);
            Assert.Equal("p1", val1);

            await _baseProvider.RemoveAsync(cacheKey);
        }

        [Fact]
        protected virtual void LRem_Should_Succeed()
        {
            var cacheKey = $"{_nameSpace}-{Guid.NewGuid().ToString()}";

            var res = _provider.LPush(cacheKey, new List<string> { "p1", "p2", "p1", "p2", "p1" });
            Assert.Equal(5, res);

            _provider.LRem<string>(cacheKey, 0, "p1");

            var len1 = _provider.LLen(cacheKey);
            Assert.Equal(2, len1);

            _provider.LRem<string>(cacheKey, 1, "p2");

            var len2 = _provider.LLen(cacheKey);
            Assert.Equal(1, len2);

            _baseProvider.Remove(cacheKey);
        }


        [Fact]
        protected virtual async Task LRemAsync_Should_Succeed()
        {
            var cacheKey = $"{_nameSpace}-{Guid.NewGuid().ToString()}";

            var res = await _provider.LPushAsync(cacheKey, new List<string> { "p1", "p2", "p1", "p2", "p1" });
            Assert.Equal(5, res);

            await _provider.LRemAsync<string>(cacheKey, 0, "p1");

            var len1 = await _provider.LLenAsync(cacheKey);
            Assert.Equal(2, len1);

            await _provider.LRemAsync<string>(cacheKey, 1, "p2");

            var len2 = await _provider.LLenAsync(cacheKey);
            Assert.Equal(1, len2);

            await _baseProvider.RemoveAsync(cacheKey);
        }

        [Fact]
        protected virtual void LTrim_Should_Succeed()
        {
            var cacheKey = $"{_nameSpace}-{Guid.NewGuid().ToString()}";

            var res = _provider.LPush(cacheKey, new List<string> { "p1", "p2", "p3" });
            Assert.Equal(3, res);

            var flag = _provider.LTrim(cacheKey, 2, -1);
            Assert.True(flag);

            var vals = _provider.LRange<string>(cacheKey, 0, -1);
            Assert.Single(vals);
            Assert.Equal("p1", vals[0]);

            _baseProvider.Remove(cacheKey);
        }

        [Fact]
        protected virtual async Task LTrimAsync_Should_Succeed()
        {
            var cacheKey = $"{_nameSpace}-{Guid.NewGuid().ToString()}";

            var res = await _provider.LPushAsync(cacheKey, new List<string> { "p1", "p2", "p3" });
            Assert.Equal(3, res);

            var flag = await _provider.LTrimAsync(cacheKey, 2, -1);
            Assert.True(flag);

            var vals = await _provider.LRangeAsync<string>(cacheKey, 0, -1);
            Assert.Single(vals);
            Assert.Equal("p1", vals[0]);

            await _baseProvider.RemoveAsync(cacheKey);
        }

        [Fact]
        protected virtual void LLen_Should_Succeed()
        {
            var cacheKey = $"{_nameSpace}-{Guid.NewGuid().ToString()}";

            var res = _provider.LPush(cacheKey, new List<string> { "p1", "p2" });
            Assert.Equal(2, res);

            var len = _provider.LLen(cacheKey);
            Assert.Equal(2, len);

            _baseProvider.Remove(cacheKey);
        }

        [Fact]
        protected virtual async Task LLenAsync_Should_Succeed()
        {
            var cacheKey = $"{_nameSpace}-{Guid.NewGuid().ToString()}";

            var res = await _provider.LPushAsync(cacheKey, new List<string> { "p1", "p2" });
            Assert.Equal(2, res);

            var len = await _provider.LLenAsync(cacheKey);
            Assert.Equal(2, len);

            await _baseProvider.RemoveAsync(cacheKey);
        }

        [Fact]
        protected virtual void RPush_And_RPop_Should_Succeed()
        {
            var cacheKey = $"{_nameSpace}-{Guid.NewGuid().ToString()}";

            var res = _provider.RPush(cacheKey, new List<string> { "p1", "p2" });
            Assert.Equal(2, res);

            var val = _provider.RPop<string>(cacheKey);
            Assert.Equal("p2", val);

            _baseProvider.Remove(cacheKey);
        }


        [Fact]
        protected virtual async Task RPushAsync_And_RPopAsync_Should_Succeed()
        {
            var cacheKey = $"{_nameSpace}-{Guid.NewGuid().ToString()}";

            var res = await _provider.RPushAsync(cacheKey, new List<string> { "p1", "p2" });
            Assert.Equal(2, res);

            var val = await _provider.RPopAsync<string>(cacheKey);
            Assert.Equal("p2", val);

            await _baseProvider.RemoveAsync(cacheKey);
        }

        [Fact]
        protected virtual void RPushX_Should_Succeed()
        {
            var cacheKey = $"{_nameSpace}-{Guid.NewGuid().ToString()}";

            var res = _provider.LPush(cacheKey, new List<string> { "p1", "p2" });
            Assert.Equal(2, res);

            var val = _provider.RPushX<string>(cacheKey, "p4");
            Assert.Equal(3, val);

            var pop = _provider.RPop<string>(cacheKey);
            Assert.Equal("p4", pop);

            _baseProvider.Remove(cacheKey);
        }

        [Fact]
        protected virtual async Task RPushXAsync_Should_Succeed()
        {
            var cacheKey = $"{_nameSpace}-{Guid.NewGuid().ToString()}";

            var res = await _provider.LPushAsync(cacheKey, new List<string> { "p1", "p2" });
            Assert.Equal(2, res);

            var val = await _provider.RPushXAsync<string>(cacheKey, "p4");
            Assert.Equal(3, val);

            var pop = await _provider.RPopAsync<string>(cacheKey);
            Assert.Equal("p4", pop);

            await _baseProvider.RemoveAsync(cacheKey);
        }

        [Fact]
        protected virtual void LInsertBefore_Should_Succeed()
        {
            var cacheKey = $"{_nameSpace}-{Guid.NewGuid().ToString()}";

            var res = _provider.LPush(cacheKey, new List<string> { "p1", "p2" });
            Assert.Equal(2, res);

            var val = _provider.LInsertBefore<string>(cacheKey, "p1", "p4");
            Assert.Equal(3, val);

            var list = _provider.LRange<string>(cacheKey, 0, -1);
            Assert.Equal("p4", list[1]);

            _baseProvider.Remove(cacheKey);
        }

        [Fact]
        protected virtual async Task LInsertBeforeAsync_Should_Succeed()
        {
            var cacheKey = $"{_nameSpace}-{Guid.NewGuid().ToString()}";

            var res = await _provider.LPushAsync(cacheKey, new List<string> { "p1", "p2" });
            Assert.Equal(2, res);

            var val = await _provider.LInsertBeforeAsync<string>(cacheKey, "p1", "p4");
            Assert.Equal(3, val);

            var list = await _provider.LRangeAsync<string>(cacheKey, 0, -1);
            Assert.Equal("p4", list[1]);

            await _baseProvider.RemoveAsync(cacheKey);
        }

        [Fact]
        protected virtual void LInsertAfter_Should_Succeed()
        {
            var cacheKey = $"{_nameSpace}-{Guid.NewGuid().ToString()}";

            var res = _provider.LPush(cacheKey, new List<string> { "p1", "p2" });
            Assert.Equal(2, res);

            var val = _provider.LInsertAfter<string>(cacheKey, "p1", "p4");
            Assert.Equal(3, val);

            var list = _provider.LRange<string>(cacheKey, 0, -1);
            Assert.Equal("p4", list[2]);

            _baseProvider.Remove(cacheKey);
        }

        [Fact]
        protected virtual async Task LInsertAfterAsync_Should_Succeed()
        {
            var cacheKey = $"{_nameSpace}-{Guid.NewGuid().ToString()}";

            var res = await _provider.LPushAsync(cacheKey, new List<string> { "p1", "p2" });
            Assert.Equal(2, res);

            var val = await _provider.LInsertAfterAsync<string>(cacheKey, "p1", "p4");
            Assert.Equal(3, val);

            var list = await _provider.LRangeAsync<string>(cacheKey, 0, -1);
            Assert.Equal("p4", list[2]);

            await _baseProvider.RemoveAsync(cacheKey);
        }
        #endregion

        #region Set
        [Fact]
        protected virtual void SAdd_And_SCard_Should_Succeed()
        {
            var cacheKey = $"{_nameSpace}-{Guid.NewGuid().ToString()}";

            var res = _provider.SAdd(cacheKey, new List<string> { "s1", "s2" });

            Assert.Equal(2, res);

            var len = _provider.SCard(cacheKey);

            Assert.Equal(2, len);

            _baseProvider.Remove(cacheKey);
        }

        [Fact]
        protected virtual void SAdd_With_Expiration_Should_Succeed()
        {
            var cacheKey = $"{_nameSpace}-{Guid.NewGuid().ToString()}";

            var res = _provider.SAdd(cacheKey, new List<string> { "s1", "s2" }, TimeSpan.FromSeconds(1));

            Assert.Equal(2, res);

            System.Threading.Thread.Sleep(1050);

            var flag = _baseProvider.Exists(cacheKey);

            Assert.False(flag);

            _baseProvider.Remove(cacheKey);
        }

        [Fact]
        protected virtual void SIsMember_With_Existed_Should_Return_True()
        {
            var cacheKey = $"{_nameSpace}-{Guid.NewGuid().ToString()}";

            var res = _provider.SAdd(cacheKey, new List<string> { "s1", "s2" });

            Assert.Equal(2, res);

            var i1 = _provider.SIsMember(cacheKey, "s1");
            var i2 = _provider.SIsMember(cacheKey, "s2");

            Assert.True(i1);
            Assert.True(i2);

            _baseProvider.Remove(cacheKey);
        }

        [Fact]
        protected virtual void SIsMember_With_Not_Existed_Should_Return_False()
        {
            var cacheKey = $"{_nameSpace}-{Guid.NewGuid().ToString()}";

            var res = _provider.SAdd(cacheKey, new List<string> { "s1", "s2" });

            Assert.Equal(2, res);

            var i1 = _provider.SIsMember(cacheKey, "s3");

            Assert.False(i1);

            _baseProvider.Remove(cacheKey);
        }

        [Fact]
        protected virtual void SMembers_Should_Succeed()
        {
            var cacheKey = $"{_nameSpace}-{Guid.NewGuid().ToString()}";

            var res = _provider.SAdd(cacheKey, new List<string> { "s1", "s2" });

            var vals = _provider.SMembers<string>(cacheKey);

            Assert.Equal(2, vals.Count);
            Assert.Contains("s1", vals);
            Assert.Contains("s2", vals);

            _baseProvider.Remove(cacheKey);
        }

        [Fact]
        protected virtual void SRem_With_Values_Should_Succeed()
        {
            var cacheKey = $"{_nameSpace}-{Guid.NewGuid().ToString()}";

            var res = _provider.SAdd(cacheKey, new List<string> { "s1", "s2" });

            var len = _provider.SRem<string>(cacheKey, new List<string> { "s1" });

            Assert.Equal(1, len);

            var flag = _provider.SIsMember<string>(cacheKey, "s1");
            Assert.False(flag);

            _baseProvider.Remove(cacheKey);
        }

        [Fact]
        protected virtual void SRem_Without_Values_Should_Succeed()
        {
            var cacheKey = $"{_nameSpace}-{Guid.NewGuid().ToString()}";

            var res = _provider.SAdd(cacheKey, new List<string> { "s1", "s2" });

            var len = _provider.SRem<string>(cacheKey);

            Assert.Equal(1, len);

            var flag = _baseProvider.Exists(cacheKey);
            Assert.False(flag);

            _baseProvider.Remove(cacheKey);
        }

        [Fact]
        protected virtual void SRandMember_With_Not_Exist_Should_Return_EmptyList()
        {
            var cacheKey = $"{_nameSpace}-srang-{Guid.NewGuid().ToString()}";

            var len = _provider.SRandMember<string>(cacheKey);

            Assert.Empty(len);

            _baseProvider.Remove(cacheKey);
        }

        [Fact]
        protected virtual void SRandMember_With_Exist_Should_Succeed()
        {
            var cacheKey = $"{_nameSpace}-srang-{Guid.NewGuid().ToString()}";

            var res = _provider.SAdd(cacheKey, new List<string> { "s1", "s2" });

            var vals = _provider.SRandMember<string>(cacheKey, 2);

            Assert.Equal(2, vals.Count);

            _baseProvider.Remove(cacheKey);
        }

        [Fact]
        protected virtual async Task SAddAsync_And_SCardAsync_Should_Succeed()
        {
            var cacheKey = $"{_nameSpace}-{Guid.NewGuid().ToString()}";

            var res = await _provider.SAddAsync(cacheKey, new List<string> { "s1", "s2" });

            Assert.Equal(2, res);

            var len = await _provider.SCardAsync(cacheKey);

            Assert.Equal(2, len);

            await _baseProvider.RemoveAsync(cacheKey);
        }

        [Fact]
        protected virtual async Task SAddAsync_With_Expiration_Should_Succeed()
        {
            var cacheKey = $"{_nameSpace}-{Guid.NewGuid().ToString()}";

            var res = await _provider.SAddAsync(cacheKey, new List<string> { "s1", "s2" }, TimeSpan.FromSeconds(1));

            Assert.Equal(2, res);

            await Task.Delay(1050);

            var len = await _provider.SCardAsync(cacheKey);

            Assert.Equal(0, len);

            _baseProvider.Remove(cacheKey);
        }

        [Fact]
        protected virtual async Task SIsMemberAsync_With_Existed_Should_Return_True()
        {
            var cacheKey = $"{_nameSpace}-{Guid.NewGuid().ToString()}";

            var res = await _provider.SAddAsync(cacheKey, new List<string> { "s1", "s2" });

            Assert.Equal(2, res);

            var i1 = await _provider.SIsMemberAsync(cacheKey, "s1");
            var i2 = await _provider.SIsMemberAsync(cacheKey, "s2");

            Assert.True(i1);
            Assert.True(i2);

            await _baseProvider.RemoveAsync(cacheKey);
        }

        [Fact]
        protected virtual async Task SIsMemberAsync_With_Not_Existed_Should_Return_False()
        {
            var cacheKey = $"{_nameSpace}-{Guid.NewGuid().ToString()}";

            var res = await _provider.SAddAsync(cacheKey, new List<string> { "s1", "s2" });

            Assert.Equal(2, res);

            var i1 = await _provider.SIsMemberAsync(cacheKey, "s3");

            Assert.False(i1);

            await _baseProvider.RemoveAsync(cacheKey);
        }

        [Fact]
        protected virtual async Task SMembersAsync_Should_Succeed()
        {
            var cacheKey = $"{_nameSpace}-{Guid.NewGuid().ToString()}";

            var res = await _provider.SAddAsync(cacheKey, new List<string> { "s1", "s2" });

            var vals = await _provider.SMembersAsync<string>(cacheKey);

            Assert.Equal(2, vals.Count);
            Assert.Contains("s1", vals);
            Assert.Contains("s2", vals);

            await _baseProvider.RemoveAsync(cacheKey);
        }


        [Fact]
        protected virtual async Task SRemAsync_With_Values_Should_Succeed()
        {
            var cacheKey = $"{_nameSpace}-{Guid.NewGuid().ToString()}";

            var res = await _provider.SAddAsync(cacheKey, new List<string> { "s1", "s2" });

            var len = await _provider.SRemAsync<string>(cacheKey, new List<string> { "s1" });

            Assert.Equal(1, len);

            var flag = await _provider.SIsMemberAsync<string>(cacheKey, "s1");
            Assert.False(flag);

            await _baseProvider.RemoveAsync(cacheKey);
        }

        [Fact]
        protected virtual async Task SRemAsync_Without_Values_Should_Succeed()
        {
            var cacheKey = $"{_nameSpace}-{Guid.NewGuid().ToString()}";

            var res = await _provider.SAddAsync(cacheKey, new List<string> { "s1", "s2" });

            var len = await _provider.SRemAsync<string>(cacheKey);

            Assert.Equal(1, len);

            var flag = await _baseProvider.ExistsAsync(cacheKey);
            Assert.False(flag);

            await _baseProvider.RemoveAsync(cacheKey);
        }

        [Fact]
        protected virtual async Task SRandMemberAsync_With_Not_Exist_Should_Return_EmptyList()
        {
            var cacheKey = $"{_nameSpace}-srang-{Guid.NewGuid().ToString()}";

            var len = await _provider.SRandMemberAsync<string>(cacheKey);

            Assert.Empty(len);

            await _baseProvider.RemoveAsync(cacheKey);
        }

        [Fact]
        protected virtual async Task SRandMemberAsync_With_Exist_Should_Succeed()
        {
            var cacheKey = $"{_nameSpace}-srang-{Guid.NewGuid().ToString()}";

            var res = await _provider.SAddAsync(cacheKey, new List<string> { "s1", "s2" });

            var vals = await _provider.SRandMemberAsync<string>(cacheKey, 2);

            Assert.Equal(2, vals.Count);

            await _baseProvider.RemoveAsync(cacheKey);
        }
        #endregion

        #region Hyperloglog             
        [Fact]
        protected virtual void PfAdd_Should_Succeed()
        {
            var cacheKey = $"{_nameSpace}-pfadd-{Guid.NewGuid().ToString()}";

            var res1 = _provider.PfAdd<string>(cacheKey, new List<string> { "foo", "bar", "zap" });
            var res2 = _provider.PfAdd<string>(cacheKey, new List<string> { "zap", "zap", "zap" });
            Assert.True(res1);
            Assert.False(res2);

            var count = _provider.PfCount(new List<string> { cacheKey });

            Assert.Equal(3, count);
            _provider.KeyDel(cacheKey);
        }

        [Fact]
        protected virtual async Task PfAddAsync_Should_Succeed()
        {
            var cacheKey = $"{_nameSpace}-pfaddasync-{Guid.NewGuid().ToString()}";

            var res1 = await _provider.PfAddAsync<string>(cacheKey, new List<string> { "foo", "bar", "zap" });
            var res2 = await _provider.PfAddAsync<string>(cacheKey, new List<string> { "zap", "zap", "zap" });
            Assert.True(res1);
            Assert.False(res2);

            var count = await _provider.PfCountAsync(new List<string> { cacheKey });

            Assert.Equal(3, count);

            await _provider.KeyDelAsync(cacheKey);
        }

        [Fact]
        protected virtual void PfMerge_Should_Succeed()
        {
            var cacheKey0 = $"{_nameSpace}-pfmerge-{Guid.NewGuid().ToString()}";
            var cacheKey1 = $"{_nameSpace}-pfmerge-{Guid.NewGuid().ToString()}";
            var cacheKey2 = $"{_nameSpace}-pfmerge-{Guid.NewGuid().ToString()}";

            var res1 = _provider.PfAdd<string>(cacheKey1, new List<string> { "foo", "bar", "zap", "a" });
            var res2 = _provider.PfAdd<string>(cacheKey2, new List<string> { "a", "b", "c", "foo" });
            Assert.True(res1);
            Assert.True(res2);


            var flag = _provider.PfMerge(cacheKey0, new List<string> { cacheKey1, cacheKey2 });
            Assert.True(flag);

            var count = _provider.PfCount(new List<string> { cacheKey0 });

            Assert.Equal(6, count);

            _provider.KeyDel(cacheKey0);
            _provider.KeyDel(cacheKey1);
            _provider.KeyDel(cacheKey2);
        }

        [Fact]
        protected virtual async Task PfMergeAsync_Should_Succeed()
        {
            var cacheKey0 = $"{_nameSpace}-pfmergeasync-{Guid.NewGuid().ToString()}";
            var cacheKey1 = $"{_nameSpace}-pfmergeasync-{Guid.NewGuid().ToString()}";
            var cacheKey2 = $"{_nameSpace}-pfmergeasync-{Guid.NewGuid().ToString()}";

            var res1 = await _provider.PfAddAsync<string>(cacheKey1, new List<string> { "foo", "bar", "zap", "a" });
            var res2 = await _provider.PfAddAsync<string>(cacheKey2, new List<string> { "a", "b", "c", "foo" });
            Assert.True(res1);
            Assert.True(res2);


            var flag = await _provider.PfMergeAsync(cacheKey0, new List<string> { cacheKey1, cacheKey2 });
            Assert.True(flag);

            var count = await _provider.PfCountAsync(new List<string> { cacheKey0 });

            Assert.Equal(6, count);

            await _provider.KeyDelAsync(cacheKey0);
            await _provider.KeyDelAsync(cacheKey1);
            await _provider.KeyDelAsync(cacheKey2);
        }
        #endregion

        #region Geo
        [Fact]
        protected virtual void GeoAdd_And_GeoDist_Should_Succeed()
        {
            var cacheKey = $"{_nameSpace}-geoadd-{Guid.NewGuid().ToString()}";

            var res = _provider.GeoAdd(cacheKey, new List<(double longitude, double latitude, string member)> { (13.361389, 38.115556, "Palermo"), (15.087269, 37.502669, "Catania") });

            Assert.Equal(2, res);

            var dist = _provider.GeoDist(cacheKey, "Palermo", "Catania");

            // precision？
            Assert.Equal(166274.1516, dist);
            _provider.KeyDel(cacheKey);
        }

        [Fact]
        protected virtual async Task GeoAddAsync_And_GeoDistAsync_Should_Succeed()
        {
            var cacheKey = $"{_nameSpace}-geoaddasync-{Guid.NewGuid().ToString()}";

            var res = await _provider.GeoAddAsync(cacheKey, new List<(double longitude, double latitude, string member)> { (13.361389, 38.115556, "Palermo"), (15.087269, 37.502669, "Catania") });

            Assert.Equal(2, res);

            var dist = await _provider.GeoDistAsync(cacheKey, "Palermo", "Catania");

            // precision？
            Assert.Equal(166274.1516, dist);

            await _provider.KeyDelAsync(cacheKey);
        }

        [Fact]
        protected virtual void GeoAdd_And_GeoHash_Should_Succeed()
        {
            var cacheKey = $"{_nameSpace}-geohash-{Guid.NewGuid().ToString()}";

            var res = _provider.GeoAdd(cacheKey, new List<(double longitude, double latitude, string member)> { (13.361389, 38.115556, "Palermo"), (15.087269, 37.502669, "Catania") });

            Assert.Equal(2, res);

            var hash = _provider.GeoHash(cacheKey, new List<string> { "Palermo", "Catania" });

            Assert.Equal(2, hash.Count);
            Assert.Contains("sqc8b49rny0", hash);
            Assert.Contains("sqdtr74hyu0", hash);

            _provider.KeyDel(cacheKey);
        }

        [Fact]
        protected virtual async Task GeoAddAsync_And_GeoHashAsync_Should_Succeed()
        {
            var cacheKey = $"{_nameSpace}-geohashasync-{Guid.NewGuid().ToString()}";

            var res = await _provider.GeoAddAsync(cacheKey, new List<(double longitude, double latitude, string member)> { (13.361389, 38.115556, "Palermo"), (15.087269, 37.502669, "Catania") });

            Assert.Equal(2, res);

            var hash = await _provider.GeoHashAsync(cacheKey, new List<string> { "Palermo", "Catania" });

            Assert.Equal(2, hash.Count);
            Assert.Contains("sqc8b49rny0", hash);
            Assert.Contains("sqdtr74hyu0", hash);

            await _provider.KeyDelAsync(cacheKey);
        }

        [Fact]
        protected virtual void GeoAdd_And_GeoPos_Should_Succeed()
        {
            var cacheKey = $"{_nameSpace}-geohash-{Guid.NewGuid().ToString()}";

            var res = _provider.GeoAdd(cacheKey, new List<(double longitude, double latitude, string member)> { (13.361389, 38.115556, "Palermo"), (15.087269, 37.502669, "Catania") });

            Assert.Equal(2, res);

            var pos = _provider.GeoPos(cacheKey, new List<string> { "Palermo", "Catania", "NonExisting" });

            Assert.Equal(3, pos.Count);
            Assert.Contains(13.36138933897018433m, pos.Where(x => x.HasValue).Select(x => x.Value.longitude));
            Assert.Contains(15.08726745843887329m, pos.Where(x => x.HasValue).Select(x => x.Value.longitude));
            Assert.Contains(null, pos);

            _provider.KeyDel(cacheKey);
        }

        [Fact]
        protected virtual async Task GeoAddAsync_And_GeoPosAsync_Should_Succeed()
        {
            var cacheKey = $"{_nameSpace}-geohashasync-{Guid.NewGuid().ToString()}";

            var res = await _provider.GeoAddAsync(cacheKey, new List<(double longitude, double latitude, string member)> { (13.361389, 38.115556, "Palermo"), (15.087269, 37.502669, "Catania") });

            Assert.Equal(2, res);

            var pos = await _provider.GeoPosAsync(cacheKey, new List<string> { "Palermo", "Catania", "NonExisting" });

            Assert.Equal(3, pos.Count);
            Assert.Contains(13.36138933897018433m, pos.Where(x => x.HasValue).Select(x => x.Value.longitude));
            Assert.Contains(15.08726745843887329m, pos.Where(x => x.HasValue).Select(x => x.Value.longitude));
            Assert.Contains(null, pos);

            await _provider.KeyDelAsync(cacheKey);
        }
        #endregion
    }
}
