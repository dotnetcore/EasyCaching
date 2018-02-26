namespace EasyCaching.UnitTests
{
    using EasyCaching.Core;
    using EasyCaching.InMemory;
    using FakeItEasy;
    using Microsoft.Extensions.Caching.Memory;
    using Microsoft.Extensions.DependencyInjection;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Xunit;

    public class MemoryCachingProviderTest : BaseCachingProviderTest
    {
        public MemoryCachingProviderTest()
        {
            var cache = new MemoryCache(new MemoryCacheOptions());
            _provider = new DefaultInMemoryCachingProvider(cache);
            _defaultTs = TimeSpan.FromSeconds(30);
        }

        [Fact]
        public void Set_Value_And_Get_Cached_Value_Should_Succeed()
        {
            var cacheKey = Guid.NewGuid().ToString();
            string cacheVlaue = "value";

            _provider.Set(cacheKey, cacheVlaue, _defaultTs);
            var res = _provider.Get<string>(cacheKey, null, _defaultTs);

            Assert.Equal(cacheVlaue, res.Value);
        }

        [Fact]
        public async Task Set_Value_And_Get_Cached_Value_Async_Should_Succeed()
        {
            var cacheKey = Guid.NewGuid().ToString();
            string cacheVlaue = "value";

            await _provider.SetAsync(cacheKey, cacheVlaue, _defaultTs);
            var res = await _provider.GetAsync<string>(cacheKey, null, _defaultTs);

            Assert.Equal(cacheVlaue, res.Value);
        }

        [Fact]
        public void Get_Cached_Value_Should_Not_Call_Retriever()
        {
            var cacheKey = Guid.NewGuid().ToString();
            var func = Create_Fake_Retriever_Return_String();
            var cacheVlaue = "Memory";

            _provider.Set(cacheKey, cacheVlaue, _defaultTs);
            var res = _provider.Get(cacheKey, func, _defaultTs);

            A.CallTo(() => func.Invoke()).MustNotHaveHappened();
        }

        [Fact]
        public async Task Get_Cached_Value_Async_Should_Not_Call_Retriever()
        {
            var cacheKey = Guid.NewGuid().ToString();
            var func = Create_Fake_Retriever_Return_String_Async();
            var cacheVlaue = "Memory";

            await _provider.SetAsync(cacheKey, cacheVlaue, _defaultTs);
            var res = await _provider.GetAsync(cacheKey, func, _defaultTs);

            A.CallTo(() => func.Invoke()).MustNotHaveHappened();
        }

        [Fact]
        public void Get_Not_Cached_Value_Should_Call_Retriever_And_Return_Value()
        {
            var cacheKey = Guid.NewGuid().ToString();
            var func = Create_Fake_Retriever_Return_String();

            var res = _provider.Get(cacheKey, func, _defaultTs);

            Assert.Equal("123", res.Value);
        }

        [Fact]
        public async Task Get_Not_Cached_Value_Async_Should_Call_Retriever_And_Return_Value()
        {
            var cacheKey = Guid.NewGuid().ToString();
            var func = Create_Fake_Retriever_Return_String_Async();

            var res = await _provider.GetAsync(cacheKey, func, _defaultTs);

            Assert.Equal("123", res.Value);
        }

        [Fact]
        public void Get_Not_Cached_Value_Without_Retriever_Should_Return_Default_Value()
        {
            var cacheKey = Guid.NewGuid().ToString();

            var res = _provider.Get<string>(cacheKey);

            Assert.Equal(default(string), res.Value);
        }

        [Fact]
        public async Task Get_Not_Cached_Value_Without_Retriever_Async_Should_Return_Default_Value()
        {
            var cacheKey = Guid.NewGuid().ToString();

            var res = await _provider.GetAsync<string>(cacheKey);

            Assert.Equal(default(string), res.Value);
        }

        [Fact]
        public void Get_Cached_Value_Without_Retriever_Should_Return_Default_Value()
        {
            var cacheKey = Guid.NewGuid().ToString();

            _provider.Set(cacheKey, "123", _defaultTs);

            var res = _provider.Get<string>(cacheKey);

            Assert.Equal("123", res.Value);
        }

        [Fact]
        public async Task Get_Cached_Value_Without_Retriever_Async_Should_Return_Default_Value()
        {
            var cacheKey = Guid.NewGuid().ToString();

            await _provider.SetAsync(cacheKey, "123", _defaultTs);

            var res = await _provider.GetAsync<string>(cacheKey);

            Assert.Equal("123", res.Value);
        }

        [Fact]
        public void Remove_Cached_Value_Should_Succeed()
        {
            var cacheKey = Guid.NewGuid().ToString();
            var cacheValue = "value";
            _provider.Set(cacheKey, cacheValue, _defaultTs);
            var valBeforeRemove = _provider.Get<string>(cacheKey, null, _defaultTs);
            Assert.NotNull(valBeforeRemove);

            _provider.Remove(cacheKey);
            var valAfterRemove = _provider.Get(cacheKey, () => "123", _defaultTs);
            Assert.Equal("123", valAfterRemove.Value);
        }

        [Fact]
        public async Task Remove_Cached_Value_Async_Should_Succeed()
        {
            var cacheKey = Guid.NewGuid().ToString();
            var cacheValue = "value";
            await _provider.SetAsync(cacheKey, cacheValue, _defaultTs);
            var valBeforeRemove = await _provider.GetAsync<string>(cacheKey, null, _defaultTs);
            Assert.NotNull(valBeforeRemove);

            await _provider.RemoveAsync(cacheKey);
            var valAfterRemove = await _provider.GetAsync<string>(cacheKey, async () => await Task.FromResult("123"), _defaultTs);
            Assert.Equal("123", valAfterRemove.Value);
        }

        [Fact]
        public void Refresh_Should_Succeed()
        {
            var cacheKey = Guid.NewGuid().ToString();
            var cacheValue = "value";
            _provider.Set(cacheKey, cacheValue, _defaultTs);

            var tmp = _provider.Get<string>(cacheKey);
            Assert.Equal("value", tmp.Value);

            _provider.Refresh(cacheKey, "NewValue", _defaultTs);

            var act = _provider.Get<string>(cacheKey);

            Assert.Equal("NewValue", act.Value);
        }

        [Fact]
        public async Task Refresh_Async_Should_Succeed()
        {
            var cacheKey = Guid.NewGuid().ToString();
            var cacheValue = "value";
            await _provider.SetAsync(cacheKey, cacheValue, _defaultTs);

            var tmp = await _provider.GetAsync<string>(cacheKey);
            Assert.Equal("value", tmp.Value);

            await _provider.RefreshAsync(cacheKey, "NewValue", _defaultTs);

            var act = await _provider.GetAsync<string>(cacheKey);

            Assert.Equal("NewValue", act.Value);
        }

        [Fact]
        public void RemoveByPrefix_Should_Succeed()
        {
            SetCacheItem("demo:1", "1");
            SetCacheItem("demo:2", "2");
            SetCacheItem("demo:3", "3");
            SetCacheItem("demo:4", "4");
            SetCacheItem("xxx:1", "1");

            _provider.RemoveByPrefix("demo");

            var demo1 = _provider.Get<string>("demo:1");
            var demo2 = _provider.Get<string>("demo:2");
            var demo3 = _provider.Get<string>("demo:3");
            var demo4 = _provider.Get<string>("demo:4");
            var xxx1 = _provider.Get<string>("xxx:1");

            Assert.False(demo1.HasValue);
            Assert.False(demo2.HasValue);
            Assert.False(demo3.HasValue);
            Assert.False(demo4.HasValue);
            Assert.True(xxx1.HasValue);
        }

        [Fact]
        public async Task RemoveByPrefixAsync_Should_Succeed()
        {
            SetCacheItem("demo:1", "1");
            SetCacheItem("demo:2", "2");
            SetCacheItem("demo:3", "3");
            SetCacheItem("demo:4", "4");
            SetCacheItem("xxx:1", "1");

            await _provider.RemoveByPrefixAsync("demo");

            var demo1 = _provider.Get<string>("demo:1");
            var demo2 = _provider.Get<string>("demo:2");
            var demo3 = _provider.Get<string>("demo:3");
            var demo4 = _provider.Get<string>("demo:4");
            var xxx1 = _provider.Get<string>("xxx:1");

            Assert.False(demo1.HasValue);
            Assert.False(demo2.HasValue);
            Assert.False(demo3.HasValue);
            Assert.False(demo4.HasValue);
            Assert.True(xxx1.HasValue);
        }

        [Fact]
        public void SetAll_Should_Succeed()
        {
            var dict = GetMultiDict();

            _provider.SetAll(dict, _defaultTs);

            var res1 = _provider.Get<string>("key:1");
            var res2 = _provider.Get<string>("key:2");

            Assert.Equal("value1", res1.Value);
            Assert.Equal("value2", res2.Value);
        }

        [Fact]
        public async Task SetAllAsync_Should_Succeed()
        {
            var dict = GetMultiDict();

            await _provider.SetAllAsync(dict, _defaultTs);

            var res1 = _provider.Get<string>("key:1");
            var res2 = _provider.Get<string>("key:2");

            Assert.Equal("value1", res1.Value);
            Assert.Equal("value2", res2.Value);
        }

        [Fact]
        public void GetAll_Should_Succeed()
        {
            var dict = GetMultiDict();

            _provider.SetAll(dict, _defaultTs);

            var res = _provider.GetAll<string>(new List<string> { "key:1", "key:2" });

            Assert.Equal(2, res.Count);

            Assert.True(res.Select(x => x.Key).Contains("key:1"));
            Assert.True(res.Select(x => x.Key).Contains("key:2"));
            Assert.Equal(res.Where(x => x.Key == "key:1").Select(x => x.Value).FirstOrDefault().Value, "value1");
            Assert.Equal(res.Where(x => x.Key == "key:2").Select(x => x.Value).FirstOrDefault().Value, "value2");
        }

        [Fact]
        public async Task GetAllAsync_Should_Succeed()
        {
            var dict = GetMultiDict();

            _provider.SetAll(dict, _defaultTs);

            var res = await _provider.GetAllAsync<string>(new List<string> { "key:1", "key:2" });

            Assert.Equal(2, res.Count);

            Assert.True(res.Select(x => x.Key).Contains("key:1"));
            Assert.True(res.Select(x => x.Key).Contains("key:2"));
            Assert.Equal(res.Where(x => x.Key == "key:1").Select(x => x.Value).FirstOrDefault().Value, "value1");
            Assert.Equal(res.Where(x => x.Key == "key:2").Select(x => x.Value).FirstOrDefault().Value, "value2");
        }

        [Fact]
        public void GetByPrefix_Should_Succeed()
        {
            var dict = GetMultiDict();

            _provider.SetAll(dict, _defaultTs);

            string prefix = "key:";

            var res = _provider.GetByPrefix<string>(prefix);

            Assert.Equal(2, res.Count);
            Assert.True(res.Select(x => x.Key).Contains("key:1"));
            Assert.True(res.Select(x => x.Key).Contains("key:2"));
            Assert.Equal(res.Where(x => x.Key == "key:1").Select(x => x.Value).FirstOrDefault().Value, "value1");
            Assert.Equal(res.Where(x => x.Key == "key:2").Select(x => x.Value).FirstOrDefault().Value, "value2");
        }

        [Fact]
        public async Task GetByPrefixAsync_Should_Succeed()
        {
            var dict = GetMultiDict();

            _provider.SetAll(dict, _defaultTs);

            string prefix = "key:";

            var res = await _provider.GetByPrefixAsync<string>(prefix);

            Assert.Equal(2, res.Count);
            Assert.True(res.Select(x => x.Key).Contains("key:1"));
            Assert.True(res.Select(x => x.Key).Contains("key:2"));
            Assert.Equal(res.Where(x => x.Key == "key:1").Select(x => x.Value).FirstOrDefault().Value, "value1");
            Assert.Equal(res.Where(x => x.Key == "key:2").Select(x => x.Value).FirstOrDefault().Value, "value2");
        }

        [Fact]
        public void RemoveAll_Should_Succeed()
        {
            var dict = GetMultiDict();

            _provider.SetAll(dict, _defaultTs);

            _provider.RemoveAll(new List<string> { "key:1", "key:2" });

            var res1 = _provider.Get<string>("key:1");
            var res2 = _provider.Get<string>("key:2");

            Assert.False(res1.HasValue);
            Assert.False(res2.HasValue);
        }

        [Fact]
        public async Task RemoveAllAsync_Should_Succeed()
        {
            var dict = GetMultiDict();

            _provider.SetAll(dict, _defaultTs);

            await _provider.RemoveAllAsync(new List<string> { "key:1", "key:2" });

            var res1 = _provider.Get<string>("key:1");
            var res2 = _provider.Get<string>("key:2");

            Assert.False(res1.HasValue);
            Assert.False(res2.HasValue);
        }

        private void SetCacheItem(string cacheKey, string cacheValue)
        {
            _provider.Set(cacheKey, cacheValue, _defaultTs);

            var val = _provider.Get<string>(cacheKey);
            Assert.Equal(cacheValue, val.Value);
        }

        private Dictionary<string, string> GetMultiDict()
        {
            return new Dictionary<string, string>()
            {
                {"key:1","value1"},
                {"key:2","value2"}
            };
        }
    }
}
