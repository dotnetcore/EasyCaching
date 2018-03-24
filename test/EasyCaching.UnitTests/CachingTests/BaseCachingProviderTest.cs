namespace EasyCaching.UnitTests
{
    using EasyCaching.Core;
    using FakeItEasy;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Xunit;

    public abstract class BaseCachingProviderTest
    {
        protected IEasyCachingProvider _provider;
        protected TimeSpan _defaultTs;

        #region Parameter Check Test
        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData(null)]
        public void Set_Value_Should_Throw_ArgumentNullException_When_CacheKey_IsNullOrWhiteSpace(string cacheKey)
        {
            var cacheVlaue = "value";
            Assert.Throws<ArgumentNullException>(() => _provider.Set(cacheKey, cacheVlaue, _defaultTs));
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData(null)]
        public async Task Set_Value_Async_Should_Throw_ArgumentNullException_When_CacheKey_IsNullOrWhiteSpace(string cacheKey)
        {
            var cacheVlaue = "value";
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await _provider.SetAsync(cacheKey, cacheVlaue, _defaultTs));
        }

        [Fact]
        public void Set_Value_Should_Throw_ArgumentNullException_When_CacheValue_IsNull()
        {
            var cacheKey = Guid.NewGuid().ToString();
            string cacheVlaue = null;
            Assert.Throws<ArgumentNullException>(() => _provider.Set(cacheKey, cacheVlaue, _defaultTs));
        }

        [Fact]
        public async Task Set_Value_Async_Should_Throw_ArgumentNullException_When_CacheValue_IsNull()
        {
            var cacheKey = Guid.NewGuid().ToString();
            string cacheVlaue = null;
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await _provider.SetAsync(cacheKey, cacheVlaue, _defaultTs));
        }

        [Fact]
        public void Set_Value_Should_Throw_ArgumentOutOfRangeException_When_Expiration_Is_Zero()
        {
            var cacheKey = Guid.NewGuid().ToString();
            string cacheVlaue = "123";
            var expiration = TimeSpan.Zero;
            Assert.Throws<ArgumentOutOfRangeException>(() => _provider.Set(cacheKey, cacheVlaue, expiration));
        }

        [Fact]
        public async Task Set_Value_Async_Should_Throw_ArgumentOutOfRangeException_When_Expiration_Is_Zero()
        {
            var cacheKey = Guid.NewGuid().ToString();
            string cacheVlaue = "123";
            var expiration = TimeSpan.Zero;
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () => await _provider.SetAsync(cacheKey, cacheVlaue, expiration));
        }

        [Fact]
        public void Set_Value_Should_Throw_ArgumentOutOfRangeException_When_Expiration_Is_Negative()
        {
            var cacheKey = Guid.NewGuid().ToString();
            string cacheVlaue = "123";
            var expiration = new TimeSpan(0, 0, -1);
            Assert.Throws<ArgumentOutOfRangeException>(() => _provider.Set(cacheKey, cacheVlaue, expiration));
        }

        [Fact]
        public async Task Set_Value_Async_Should_Throw_ArgumentOutOfRangeException_When_Expiration_Is_Negative()
        {
            var cacheKey = Guid.NewGuid().ToString();
            string cacheVlaue = "123";
            var expiration = new TimeSpan(0, 0, -1);
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () => await _provider.SetAsync(cacheKey, cacheVlaue, expiration));
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData(null)]
        public void Get_Cached_Value_Should_Throw_ArgumentNullException_When_CacheKey_IsNullOrWhiteSpace(string cachekey)
        {
            Assert.Throws<ArgumentNullException>(() => _provider.Get<string>(cachekey, null, _defaultTs));
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData(null)]
        public async Task Get_Cached_Value_Async_Should_Throw_ArgumentNullException_When_CacheKey_IsNullOrWhiteSpace(string cachekey)
        {
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await _provider.GetAsync<string>(cachekey, null, _defaultTs));
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData(null)]
        public void Get_Cached_Value_Without_Retriever_Should_Throw_ArgumentNullException_When_CacheKey_IsNullOrWhiteSpace(string cachekey)
        {
            Assert.Throws<ArgumentNullException>(() => _provider.Get<string>(cachekey));
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData(null)]
        public async Task Get_Cached_Value_Without_Retriever_Async_Should_Throw_ArgumentNullException_When_CacheKey_IsNullOrWhiteSpace(string cachekey)
        {
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await _provider.GetAsync<string>(cachekey));
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData(null)]
        public void Remove_Cached_Value_Should_Throw_ArgumentNullException_When_CacheKey_IsNullOrWhiteSpace(string cacheKey)
        {
            Assert.Throws<ArgumentNullException>(() => _provider.Remove(cacheKey));
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData(null)]
        public async Task Remove_Cached_Value_Async_Should_Throw_ArgumentNullException_When_CacheKey_IsNullOrWhiteSpace(string cacheKey)
        {
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await _provider.RemoveAsync(cacheKey));
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData(null)]
        public void Exists_Cached_Value_Should_Throw_ArgumentNullException_When_CacheKey_IsNullOrWhiteSpace(string cacheKey)
        {
            Assert.Throws<ArgumentNullException>(() => _provider.Exists(cacheKey));
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData(null)]
        public async Task Exists_Cached_Value_Async_Should_Throw_ArgumentNullException_When_CacheKey_IsNullOrWhiteSpace(string cacheKey)
        {
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await _provider.ExistsAsync(cacheKey));
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData(null)]
        public void Refresh_Should_Throw_ArgumentNullException_When_CacheKey_IsNullOrWhiteSpace(string cacheKey)
        {
            var cacheVlaue = "value";
            Assert.Throws<ArgumentNullException>(() => _provider.Set(cacheKey, cacheVlaue, _defaultTs));
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData(null)]
        public async Task Refresh_Async_Should_Throw_ArgumentNullException_When_CacheKey_IsNullOrWhiteSpace(string cacheKey)
        {
            var cacheVlaue = "value";
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await _provider.SetAsync(cacheKey, cacheVlaue, _defaultTs));
        }

        [Fact]
        public void Refresh_Should_Throw_ArgumentNullException_When_CacheValue_IsNull()
        {
            var cacheKey = Guid.NewGuid().ToString();
            string cacheVlaue = null;
            Assert.Throws<ArgumentNullException>(() => _provider.Set(cacheKey, cacheVlaue, _defaultTs));
        }

        [Fact]
        public async Task Refresh_Async_Should_Throw_ArgumentNullException_When_CacheValue_IsNull()
        {
            var cacheKey = Guid.NewGuid().ToString();
            string cacheVlaue = null;
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await _provider.SetAsync(cacheKey, cacheVlaue, _defaultTs));
        }

        [Fact]
        public void Refresh_Should_Throw_ArgumentOutOfRangeException_When_Expiration_Is_Zero()
        {
            var cacheKey = Guid.NewGuid().ToString();
            string cacheVlaue = "123";
            var expiration = TimeSpan.Zero;
            Assert.Throws<ArgumentOutOfRangeException>(() => _provider.Set(cacheKey, cacheVlaue, expiration));
        }

        [Fact]
        public async Task Refresh_Async_Should_Throw_ArgumentOutOfRangeException_When_Expiration_Is_Zero()
        {
            var cacheKey = Guid.NewGuid().ToString();
            string cacheVlaue = "123";
            var expiration = TimeSpan.Zero;
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () => await _provider.SetAsync(cacheKey, cacheVlaue, expiration));
        }

        [Fact]
        public void Refresh_Should_Throw_ArgumentOutOfRangeException_When_Expiration_Is_Negative()
        {
            var cacheKey = Guid.NewGuid().ToString();
            string cacheVlaue = "123";
            var expiration = new TimeSpan(0, 0, -1);
            Assert.Throws<ArgumentOutOfRangeException>(() => _provider.Set(cacheKey, cacheVlaue, expiration));
        }

        [Fact]
        public async Task Refresh_Async_Should_Throw_ArgumentOutOfRangeException_When_Expiration_Is_Negative()
        {
            var cacheKey = Guid.NewGuid().ToString();
            string cacheVlaue = "123";
            var expiration = new TimeSpan(0, 0, -1);
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () => await _provider.SetAsync(cacheKey, cacheVlaue, expiration));
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData(null)]
        public void RemoveByPrefix_Should_Throw_ArgumentNullException_When_CacheKey_IsNullOrWhiteSpace(string prefix)
        {
            Assert.Throws<ArgumentNullException>(() => _provider.RemoveByPrefix(prefix));
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData(null)]
        public async Task RemoveByPrefix_Async_Should_Throw_ArgumentNullException_When_Prefix_IsNullOrWhiteSpace(string preifx)
        {
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await _provider.RemoveByPrefixAsync(preifx));
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData(null)]
        public void GetAllByPrefix_Should_Throw_ArgumentNullException_When_CacheKey_IsNullOrWhiteSpace(string prefix)
        {
            Assert.Throws<ArgumentNullException>(() => _provider.RemoveByPrefix(prefix));
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData(null)]
        public async Task GetAllByPrefix_Async_Should_Throw_ArgumentNullException_When_Prefix_IsNullOrWhiteSpace(string preifx)
        {
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await _provider.RemoveByPrefixAsync(preifx));
        }



        #endregion

        #region Set/SetAsync
        [Fact]
        public void Set_Value_And_Get_Cached_Value_Should_Succeed()
        {
            var cacheKey = Guid.NewGuid().ToString();
            var cacheValue = "value";

            _provider.Set(cacheKey, cacheValue, _defaultTs);

            var val = _provider.Get<string>(cacheKey);
            Assert.True(val.HasValue);
            Assert.Equal(cacheValue, val.Value);
        }

        [Fact]
        public async Task Set_Value_And_Get_Cached_Value_Async_Should_Succeed()
        {
            var cacheKey = Guid.NewGuid().ToString();
            var cacheValue = "value";

            await _provider.SetAsync(cacheKey, cacheValue, _defaultTs);

            var val = await _provider.GetAsync<string>(cacheKey);
            Assert.True(val.HasValue);
            Assert.Equal(cacheValue, val.Value);
        }

        [Fact]
        protected virtual void Set_Object_Value_And_Get_Cached_Value_Should_Succeed()
        {
            var cacheKey = Guid.NewGuid().ToString();
            var cacheValue = new Product { Id = 999, Name = "product999" };

            _provider.Set(cacheKey, cacheValue, _defaultTs);

            var val = _provider.Get<Product>(cacheKey);
            Assert.True(val.HasValue);
            Assert.Equal(cacheValue.Id, val.Value.Id);
            Assert.Equal(cacheValue.Name, val.Value.Name);
        }

        [Fact]
        protected virtual async Task Set_Object_Value_And_Get_Cached_Value_Async_Should_Succeed()
        {
            var cacheKey = Guid.NewGuid().ToString();
            var cacheValue = new Product { Id = 999, Name = "product999" };

            await _provider.SetAsync(cacheKey, cacheValue, _defaultTs);

            var val = await _provider.GetAsync<Product>(cacheKey);
            Assert.True(val.HasValue);
            Assert.Equal(cacheValue.Id, val.Value.Id);
            Assert.Equal(cacheValue.Name, val.Value.Name);
        }

        #endregion

        #region Get/GetAsync
        [Fact]
        public void Get_Not_Cached_Value_Should_Call_Retriever()
        {
            var cacheKey = Guid.NewGuid().ToString();

            var func = Create_Fake_Retriever_Return_String();

            var res = _provider.Get(cacheKey, func, _defaultTs);

            A.CallTo(() => func.Invoke()).MustHaveHappened();
        }

        [Fact]
        public async Task Get_Not_Cached_Value_Async_Should_Call_Retriever()
        {
            var cacheKey = Guid.NewGuid().ToString();

            var func = Create_Fake_Retriever_Return_String_Async();

            var res = await _provider.GetAsync(cacheKey, func, _defaultTs);

            A.CallTo(() => func.Invoke()).MustHaveHappened();
        }

        [Fact]
        public void Get_Not_Cached_Value_Should_Call_Retriever_And_Return_Null()
        {
            var cacheKey = Guid.NewGuid().ToString();
            var func = Create_Fake_Retriever_Return_NULL();

            var res = _provider.Get(cacheKey, func, _defaultTs);

            Assert.Equal(res.Value, default(string));
        }

        [Fact]
        public async Task Get_Not_Cached_Value_Async_Should_Call_Retriever_And_Return_Null()
        {
            var cacheKey = Guid.NewGuid().ToString();
            var func = Create_Fake_Retriever_Return_NULL_Async();

            var res = await _provider.GetAsync(cacheKey, func, _defaultTs);

            Assert.Equal(res.Value, default(string));
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
        public void Get_Cached_Value_Should_Succeed()
        {
            var cacheKey = Guid.NewGuid().ToString();
            var cacheValue = "value";

            _provider.Set(cacheKey, cacheValue, _defaultTs);
            var val = _provider.Get<string>(cacheKey, null, _defaultTs);

            Assert.Equal(cacheValue, val.Value);
        }

        [Fact]
        public async Task Get_Cached_Value_Async_Should_Succeed()
        {
            var cacheKey = Guid.NewGuid().ToString();
            var cacheValue = "value";

            await _provider.SetAsync(cacheKey, cacheValue, _defaultTs);
            var val = await _provider.GetAsync<string>(cacheKey, null, _defaultTs);

            Assert.Equal(cacheValue, val.Value);
        }

        [Fact]
        public void Get_Cached_Value_Without_Retriever_Should_Succeed()
        {
            var cacheKey = Guid.NewGuid().ToString();
            var cacheValue = "value";

            _provider.Set(cacheKey, cacheValue, _defaultTs);
            var val = _provider.Get<string>(cacheKey);

            Assert.Equal(cacheValue, val.Value);
        }

        [Fact]
        public async Task Get_Cached_Value_Without_Retriever_Async_Should_Succeed()
        {
            var cacheKey = Guid.NewGuid().ToString();
            var cacheValue = "value";

            await _provider.SetAsync(cacheKey, cacheValue, _defaultTs);
            var val = await _provider.GetAsync<string>(cacheKey);

            Assert.Equal(cacheValue, val.Value);
        }

        [Fact]
        public void Get_Not_Cached_Value_Without_Retriever_Should_Succeed()
        {
            var cacheKey = Guid.NewGuid().ToString();

            var val = _provider.Get<string>(cacheKey);

            Assert.Equal(default(string), val.Value);
        }

        [Fact]
        public async Task Get_Not_Cached_Value_Without_Retriever_Async_Should_Succeed()
        {
            var cacheKey = Guid.NewGuid().ToString();

            var val = await _provider.GetAsync<string>(cacheKey);

            Assert.Equal(default(string), val.Value);
        }

        #endregion

        #region Refresh/RefreshAsync
        [Fact]
        public void Refresh_Should_Succeed()
        {
            var cacheKey = Guid.NewGuid().ToString();
            var cacheValue = "value";

            _provider.Set(cacheKey, cacheValue, _defaultTs);

            var tmp = _provider.Get<string>(cacheKey);
            Assert.Equal("value", tmp.Value);

            var newValue = "NewValue";

            _provider.Refresh(cacheKey, newValue, _defaultTs);

            var act = _provider.Get<string>(cacheKey);

            Assert.Equal("NewValue", act.Value);
        }

        [Fact]
        public async Task Refresh_Async_Should_Succeed()
        {
            var cacheKey = Guid.NewGuid().ToString();
            var cacheValue = "value";
            var cacheBytes = new byte[] { 0x01 };

            await _provider.SetAsync(cacheKey, cacheValue, _defaultTs);

            var tmp = await _provider.GetAsync<string>(cacheKey);
            Assert.Equal("value", tmp.Value);

            var newValue = "NewValue";


            await _provider.RefreshAsync(cacheKey, newValue, _defaultTs);

            var act = await _provider.GetAsync<string>(cacheKey);

            Assert.Equal("NewValue", act.Value);
        }
        #endregion

        #region Remove/RemoveAsync
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
            var valAfterRemove = await _provider.GetAsync(cacheKey, async () => await Task.FromResult("123"), _defaultTs);
            Assert.Equal("123", valAfterRemove.Value);
        }
        #endregion

        #region Exists/ExistsAsync
        [Fact]
        public void Exists_Cached_Value_Should_Return_True()
        {
            var cacheKey = Guid.NewGuid().ToString();
            var cacheValue = "value";
            _provider.Set(cacheKey, cacheValue, _defaultTs);

            var flag = _provider.Exists(cacheKey);

            Assert.True(flag);
        }

        [Fact]
        public async Task Exists_Cached_Value_Async_Should_Return_True()
        {
            var cacheKey = Guid.NewGuid().ToString();
            var cacheValue = "value";
            await _provider.SetAsync(cacheKey, cacheValue, _defaultTs);

            var flag = await _provider.ExistsAsync(cacheKey);

            Assert.True(flag);
        }

        [Fact]
        public void Exists_Cached_Value_Should_Return_False()
        {
            var cacheKey = Guid.NewGuid().ToString();

            var flag = _provider.Exists(cacheKey);

            Assert.False(flag);
        }

        [Fact]
        public async Task Exists_Cached_Value_Async_Should_Return_False()
        {
            var cacheKey = Guid.NewGuid().ToString();

            var flag = await _provider.ExistsAsync(cacheKey);

            Assert.False(flag);
        }
        #endregion

        #region RemoveByPrefix/RemoveByPrefixAsync
        [Fact]
        protected virtual void RemoveByPrefix_Should_Succeed()
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
        protected virtual async Task RemoveByPrefixAsync_Should_Succeed()
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
        #endregion

        #region SetAll/SetAllAsync
        [Fact]
        protected virtual void SetAll_Should_Succeed()
        {
            var dict = GetMultiDict("setall:");

            _provider.SetAll(dict, _defaultTs);

            var res1 = _provider.Get<string>("setall:key:1");
            var res2 = _provider.Get<string>("setall:key:2");

            Assert.Equal("value1", res1.Value);
            Assert.Equal("value2", res2.Value);
        }

        [Fact]
        protected virtual async Task SetAllAsync_Should_Succeed()
        {
            var dict = GetMultiDict("setallasync:");

            await _provider.SetAllAsync(dict, _defaultTs);

            var res1 = _provider.Get<string>("setallasync:key:1");
            var res2 = _provider.Get<string>("setallasync:key:2");

            Assert.Equal("value1", res1.Value);
            Assert.Equal("value2", res2.Value);
        }
        #endregion

        #region GetAll/GetAllAsync
        [Fact]
        protected virtual void GetAll_Should_Succeed()
        {
            _provider.RemoveAll(new List<string> { "getall:key:1", "getall:key:2" });
            var dict = GetMultiDict("getall:");

            _provider.SetAll(dict, _defaultTs);

            var res = _provider.GetAll<string>(new List<string> { "getall:key:1", "getall:key:2" });

            Assert.Equal(2, res.Count);

            Assert.True(res.Select(x => x.Key).Contains("getall:key:1"));
            Assert.True(res.Select(x => x.Key).Contains("getall:key:2"));
            Assert.Equal(res.Where(x => x.Key == "getall:key:1").Select(x => x.Value).FirstOrDefault().Value, "value1");
            Assert.Equal(res.Where(x => x.Key == "getall:key:2").Select(x => x.Value).FirstOrDefault().Value, "value2");
        }

        [Fact]
        protected virtual async Task GetAllAsync_Should_Succeed()
        {
            _provider.RemoveAll(new List<string> { "getallasync:key:1", "getallasync:key:2" });
            var dict = GetMultiDict("getallasync:");

            _provider.SetAll(dict, _defaultTs);

            var res = await _provider.GetAllAsync<string>(new List<string> { "getallasync:key:1", "getallasync:key:2" });

            Assert.Equal(2, res.Count);

            Assert.True(res.Select(x => x.Key).Contains("getallasync:key:1"));
            Assert.True(res.Select(x => x.Key).Contains("getallasync:key:2"));
            Assert.Equal(res.Where(x => x.Key == "getallasync:key:1").Select(x => x.Value).FirstOrDefault().Value, "value1");
            Assert.Equal(res.Where(x => x.Key == "getallasync:key:2").Select(x => x.Value).FirstOrDefault().Value, "value2");
        }
        #endregion

        #region GetByPrefix/GetByPrefixAsync
        [Fact]
        protected virtual void GetByPrefix_Should_Succeed()
        {
            _provider.RemoveAll(new List<string> { "getbyprefix:key:1", "getbyprefix:key:2" });
            var dict = GetMultiDict("getbyprefix:");

            _provider.SetAll(dict, _defaultTs);

            string prefix = "getbyprefix:key:";

            var res = _provider.GetByPrefix<string>(prefix);

            Assert.Equal(2, res.Count);
            Assert.True(res.Select(x => x.Key).Contains("getbyprefix:key:1"));
            Assert.True(res.Select(x => x.Key).Contains("getbyprefix:key:2"));
            Assert.Equal(res.Where(x => x.Key == "getbyprefix:key:1").Select(x => x.Value).FirstOrDefault().Value, "value1");
            Assert.Equal(res.Where(x => x.Key == "getbyprefix:key:2").Select(x => x.Value).FirstOrDefault().Value, "value2");
        }

        [Fact]
        protected virtual async Task GetByPrefixAsync_Should_Succeed()
        {
            _provider.RemoveAll(new List<string> { "getbyprefixasync:key:1", "getbyprefixasync:key:2" });
            var dict = GetMultiDict("getbyprefixasync:");

            _provider.SetAll(dict, _defaultTs);

            string prefix = "getbyprefixasync:key:";

            var res = await _provider.GetByPrefixAsync<string>(prefix);

            Assert.Equal(2, res.Count);
            Assert.True(res.Select(x => x.Key).Contains("getbyprefixasync:key:1"));
            Assert.True(res.Select(x => x.Key).Contains("getbyprefixasync:key:2"));
            Assert.Equal(res.Where(x => x.Key == "getbyprefixasync:key:1").Select(x => x.Value).FirstOrDefault().Value, "value1");
            Assert.Equal(res.Where(x => x.Key == "getbyprefixasync:key:2").Select(x => x.Value).FirstOrDefault().Value, "value2");
        }

        [Fact]
        protected virtual void GetByPrefix_With_Not_Existed_Prefix_Should_Return_Empty_Dict()
        {
            string prefix = Guid.NewGuid().ToString();

            var res = _provider.GetByPrefix<string>(prefix);

            Assert.Equal(0, res.Count);
        }

        [Fact]
        protected virtual async Task GetByPrefixAsync_With_Not_Existed_Prefix_Should_Return_Empty_Dict()
        {
            string prefix = Guid.NewGuid().ToString();

            var res = await _provider.GetByPrefixAsync<string>(prefix);

            Assert.Equal(0, res.Count);
        }

        #endregion    

        #region RemoveAll/RemoveAllAsync
        [Fact]
        protected virtual void RemoveAll_Should_Succeed()
        {
            var dict = GetMultiDict("removeall:");

            _provider.SetAll(dict, _defaultTs);

            _provider.RemoveAll(new List<string> { "removeall:key:1", "removeall:key:2" });

            var res1 = _provider.Get<string>("removeall:key:1");
            var res2 = _provider.Get<string>("removeall:key:2");

            Assert.False(res1.HasValue);
            Assert.False(res2.HasValue);
        }

        [Fact]
        protected virtual async Task RemoveAllAsync_Should_Succeed()
        {
            var dict = GetMultiDict("removeallasync:");

            _provider.SetAll(dict, _defaultTs);

            await _provider.RemoveAllAsync(new List<string> { "removeallasync:key:1", "removeallasync:key:2" });

            var res1 = _provider.Get<string>("removeallasync:key:1");
            var res2 = _provider.Get<string>("removeallasync:key:2");

            Assert.False(res1.HasValue);
            Assert.False(res2.HasValue);
        }
        #endregion

        #region Flush
        [Fact]
        protected virtual void Flush_Should_Succeed()
        {
            for (var i = 0; i < 5; i++)
                _provider.Set($"key{i}", $"value{i}", _defaultTs);

            for (var i = 0; i < 5; i++)
                Assert.Equal($"value{i}", _provider.Get<string>($"key{i}").Value);

            _provider.Flush();

            for (var i = 0; i < 5; i++)
                Assert.False(_provider.Get<string>($"key{i}").HasValue);
        }
        #endregion

        #region GetCount
        [Fact]
        protected virtual void Get_Count_Without_Prefix_Should_Succeed()
        {
            _provider.Flush();

            for (var i = 0; i < 5; i++)
                _provider.Set($"key{i}", $"value{i}", _defaultTs);

            Assert.Equal(5, _provider.GetCount());

            _provider.Remove("key4");

            Assert.Equal(4, _provider.GetCount());
        }

        [Fact]
        protected virtual void Get_Count_With_Prefix_Should_Succeed()
        {
            _provider.Flush();

            for (var i = 0; i < 5; i++)
                _provider.Set($"getcount:withprefix:{i}", $"value{i}", _defaultTs);

            Assert.Equal(5, _provider.GetCount("getcount:withprefix:"));

            _provider.Remove("getcount:withprefix:1");

            Assert.Equal(4, _provider.GetCount("getcount:withprefix:"));
        }
        #endregion


        #region common method
        protected Dictionary<string, string> GetMultiDict(string prefix = "")
        {
            return new Dictionary<string, string>()
            {
                {string.Concat(prefix,"key:1"), "value1"},
                {string.Concat(prefix,"key:2"), "value2"}
            };
        }

        protected void SetCacheItem(string cacheKey, string cacheValue)
        {
            _provider.Set(cacheKey, cacheValue, _defaultTs);
            var val = _provider.Get<string>(cacheKey);
            Assert.Equal(cacheValue, val.Value);
        }

        protected Func<string> Create_Fake_Retriever_Return_String()
        {
            var func = A.Fake<Func<string>>();

            A.CallTo(() => func.Invoke()).Returns("123");

            return func;
        }

        protected Func<string> Create_Fake_Retriever_Return_NULL()
        {
            var func = A.Fake<Func<string>>();

            A.CallTo(() => func.Invoke()).Returns(null);

            return func;
        }

        protected Func<Task<string>> Create_Fake_Retriever_Return_String_Async()
        {
            var func = A.Fake<Func<Task<string>>>();

            A.CallTo(() => func.Invoke()).Returns(Task.FromResult("123"));

            return func;
        }

        protected Func<Task<string>> Create_Fake_Retriever_Return_NULL_Async()
        {
            var func = A.Fake<Func<Task<string>>>();
            string res = null;
            A.CallTo(() => func.Invoke()).Returns(Task.FromResult(res));

            return func;
        }
        #endregion
    }
}