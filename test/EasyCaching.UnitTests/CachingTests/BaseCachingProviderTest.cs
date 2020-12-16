namespace EasyCaching.UnitTests
{
    using EasyCaching.Core;
    using EasyCaching.Core.Configurations;
    using FakeItEasy;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Xunit;

    public abstract class BaseCachingProviderTest
    {
        protected IEasyCachingProvider _provider;
        protected IEasyCachingProvider _providerWithNullsCached;

        protected TimeSpan _defaultTs;
        protected string _nameSpace = string.Empty;

        protected BaseCachingProviderTest()
        {
            _provider = CreateCachingProvider(options => { });
            _providerWithNullsCached = CreateCachingProvider(options => options.CacheNulls = true);
        }

        protected abstract IEasyCachingProvider CreateCachingProvider(Action<BaseProviderOptions> additionalSetup);

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
            var cacheKey = $"{_nameSpace}{Guid.NewGuid().ToString()}";
            string cacheVlaue = null;
            Assert.Throws<ArgumentNullException>(() => _provider.Set(cacheKey, cacheVlaue, _defaultTs));
        }

        [Fact]
        public async Task Refresh_Async_Should_Throw_ArgumentNullException_When_CacheValue_IsNull()
        {
            var cacheKey = $"{_nameSpace}{Guid.NewGuid().ToString()}";
            string cacheVlaue = null;
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await _provider.SetAsync(cacheKey, cacheVlaue, _defaultTs));
        }

        [Fact]
        public void Refresh_Should_Throw_ArgumentOutOfRangeException_When_Expiration_Is_Zero()
        {
            var cacheKey = $"{_nameSpace}{Guid.NewGuid().ToString()}";
            string cacheVlaue = "123";
            var expiration = TimeSpan.Zero;
            Assert.Throws<ArgumentOutOfRangeException>(() => _provider.Set(cacheKey, cacheVlaue, expiration));
        }

        [Fact]
        public async Task Refresh_Async_Should_Throw_ArgumentOutOfRangeException_When_Expiration_Is_Zero()
        {
            var cacheKey = $"{_nameSpace}{Guid.NewGuid().ToString()}";
            string cacheVlaue = "123";
            var expiration = TimeSpan.Zero;
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () => await _provider.SetAsync(cacheKey, cacheVlaue, expiration));
        }

        [Fact]
        public void Refresh_Should_Throw_ArgumentOutOfRangeException_When_Expiration_Is_Negative()
        {
            var cacheKey = $"{_nameSpace}{Guid.NewGuid().ToString()}";
            string cacheVlaue = "123";
            var expiration = new TimeSpan(0, 0, -1);
            Assert.Throws<ArgumentOutOfRangeException>(() => _provider.Set(cacheKey, cacheVlaue, expiration));
        }

        [Fact]
        public async Task Refresh_Async_Should_Throw_ArgumentOutOfRangeException_When_Expiration_Is_Negative()
        {
            var cacheKey = $"{_nameSpace}{Guid.NewGuid().ToString()}";
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
            var cacheKey = $"{_nameSpace}-sg-{Guid.NewGuid().ToString()}";
            var cacheValue = "value";

            _provider.Set(cacheKey, cacheValue, _defaultTs);

            var val = _provider.Get<string>(cacheKey);
            Assert.True(val.HasValue);
            Assert.Equal(cacheValue, val.Value);

            _provider.Remove(cacheKey);
        }
        
        [Fact]
        public void Set_Value_And_Get_Cached_Value_Should_Succeed_When_CacheValue_IsNull_And_Nulls_Are_Cached()
        {
            var cacheKey = $"{_nameSpace}-sg-{Guid.NewGuid().ToString()}";
            string cacheValue = null;

            _providerWithNullsCached.Set(cacheKey, cacheValue, _defaultTs);

            var val = _providerWithNullsCached.Get<string>(cacheKey);
            Assert.True(val.HasValue);
            Assert.Null(val.Value);

            _providerWithNullsCached.Remove(cacheKey);
        }

        [Fact]
        public async Task Set_Value_And_Get_Cached_Value_Async_Should_Succeed()
        {
            var cacheKey = $"{_nameSpace}-sgasync{Guid.NewGuid().ToString()}";
            var cacheValue = "value";

            await _provider.SetAsync(cacheKey, cacheValue, _defaultTs);

            var val = await _provider.GetAsync<string>(cacheKey);
            Assert.True(val.HasValue);
            Assert.Equal(cacheValue, val.Value);

            await _provider.RemoveAsync(cacheKey);
        }
        
        [Fact]
        public async Task Set_Value_And_Get_Cached_Value_Async_Should_Succeed_When_CacheValue_IsNull_And_Nulls_Are_Cached()
        {
            var cacheKey = $"{_nameSpace}-sg-{Guid.NewGuid().ToString()}";
            string cacheValue = null;

            await _providerWithNullsCached.SetAsync(cacheKey, cacheValue, _defaultTs);

            var val = await _providerWithNullsCached.GetAsync<string>(cacheKey);
            Assert.True(val.HasValue);
            Assert.Null(val.Value);

            await _providerWithNullsCached.RemoveAsync(cacheKey);
        }

        [Fact]
        protected virtual void Set_Object_Value_And_Get_Cached_Value_Should_Succeed()
        {
            var cacheKey = $"{_nameSpace}-sog-{Guid.NewGuid().ToString()}";
            var cacheValue = new Product { Id = 999, Name = "product999" };

            _provider.Set(cacheKey, cacheValue, _defaultTs);

            var val = _provider.Get<Product>(cacheKey);
            Assert.True(val.HasValue);
            Assert.Equal(cacheValue.Id, val.Value.Id);
            Assert.Equal(cacheValue.Name, val.Value.Name);

            _provider.Remove(cacheKey);
        }

        [Fact]
        protected virtual void Set_Object_Value_And_Get_Cached_Value_Should_Succeed_When_CacheValue_IsNull_And_Nulls_Are_Cached()
        {
            var cacheKey = $"{_nameSpace}-sog-{Guid.NewGuid().ToString()}";
            Product cacheValue = null;

            _providerWithNullsCached.Set(cacheKey, cacheValue, _defaultTs);

            var val = _providerWithNullsCached.Get<Product>(cacheKey);
            Assert.True(val.HasValue);
            Assert.Null(val.Value);

            _providerWithNullsCached.Remove(cacheKey);
        }

        [Fact]
        protected virtual async Task Set_Object_Value_And_Get_Cached_Value_Async_Should_Succeed()
        {
            var cacheKey = $"{_nameSpace}-sogasync-{Guid.NewGuid().ToString()}";
            var cacheValue = new Product { Id = 999, Name = "product999" };

            await _provider.SetAsync(cacheKey, cacheValue, _defaultTs);

            var val = await _provider.GetAsync<Product>(cacheKey);
            Assert.True(val.HasValue);
            Assert.Equal(cacheValue.Id, val.Value.Id);
            Assert.Equal(cacheValue.Name, val.Value.Name);

            await _provider.RemoveAsync(cacheKey);
        }

        [Fact]
        protected virtual async Task Set_Object_Value_And_Get_Cached_Value_Async_Should_Succeed_When_CacheValue_IsNull_And_Nulls_Are_Cached()
        {
            var cacheKey = $"{_nameSpace}-sogasync-{Guid.NewGuid().ToString()}";
            Product cacheValue = null;

            await _providerWithNullsCached.SetAsync(cacheKey, cacheValue, _defaultTs);

            var val = await _providerWithNullsCached.GetAsync<Product>(cacheKey);
            Assert.True(val.HasValue);
            Assert.Null(val.Value);

            await _providerWithNullsCached.RemoveAsync(cacheKey);
        }

        [Fact]
        protected virtual void Set_And_Get_Value_Type_Should_Succeed()
        {
            var cacheKey = $"{_nameSpace}-svg-{Guid.NewGuid().ToString()}";
            var cacheValue = 100;

            _provider.Set(cacheKey, cacheValue, _defaultTs);

            var val = _provider.Get<int>(cacheKey);
            Assert.True(val.HasValue);
            Assert.Equal(cacheValue, val.Value);

            _provider.Remove(cacheKey);
        }

        [Fact]
        protected virtual void Set_And_Get_Value_Type_Should_Succeed_When_CacheValue_IsNull_And_Nulls_Are_Cached()
        {
            var cacheKey = $"{_nameSpace}-svg-{Guid.NewGuid().ToString()}";
            int? cacheValue = null;

            _providerWithNullsCached.Set(cacheKey, cacheValue, _defaultTs);

            var val = _providerWithNullsCached.Get<int?>(cacheKey);
            Assert.True(val.HasValue);
            Assert.Null(val.Value);

            _providerWithNullsCached.Remove(cacheKey);
        }
         
        [Fact]
        protected virtual async Task Set_And_Get_Value_Type_Async_Should_Succeed()
        {
            var cacheKey = $"{_nameSpace}-svgasync-{Guid.NewGuid().ToString()}";
            var cacheValue = 100;

            await  _provider.SetAsync(cacheKey, cacheValue, _defaultTs);

            var val = await _provider.GetAsync<int>(cacheKey);
            Assert.True(val.HasValue);
            Assert.Equal(cacheValue, val.Value);

            await _provider.RemoveAsync(cacheKey);
        }
         
        [Fact]
        protected virtual async Task Set_And_Get_Value_Type_Async_Should_Succeed_When_CacheValue_IsNull_And_Nulls_Are_Cached()
        {
            var cacheKey = $"{_nameSpace}-svgasync-{Guid.NewGuid().ToString()}";
            int? cacheValue = null;

            await _providerWithNullsCached.SetAsync(cacheKey, cacheValue, _defaultTs);

            var val = await _providerWithNullsCached.GetAsync<int?>(cacheKey);
            Assert.True(val.HasValue);
            Assert.Null(val.Value);

            await _providerWithNullsCached.RemoveAsync(cacheKey);
        }
        #endregion

        #region Get/GetAsync
        [Fact]
        public void Get_Not_Cached_Value_Should_Call_Retriever()
        {
            var cacheKey = $"{_nameSpace}{Guid.NewGuid().ToString()}";

            var func = Create_Fake_Retriever_Return_String();

            var res = _provider.Get(cacheKey, func, _defaultTs);

            A.CallTo(() => func.Invoke()).MustHaveHappened();
        }

        [Fact]
        public async Task Get_Not_Cached_Value_Async_Should_Call_Retriever()
        {
            var cacheKey = $"{_nameSpace}{Guid.NewGuid().ToString()}";

            var func = Create_Fake_Retriever_Return_String_Async();

            var res = await _provider.GetAsync(cacheKey, func, _defaultTs);

            A.CallTo(() => func.Invoke()).MustHaveHappened();
        }

        [Fact]
        public void Get_Not_Cached_Value_Should_Call_Retriever_And_Return_Null_Without_Caching()
        {
            var cacheKey = $"{_nameSpace}{Guid.NewGuid().ToString()}";
            var func = Create_Fake_Retriever_Return_NULL();

            var res = _provider.Get(cacheKey, func, _defaultTs);

            Assert.Equal(default(string),res.Value);
            var cachedValue = _provider.Get<string>(cacheKey);
            Assert.False(cachedValue.HasValue);
            Assert.Null(cachedValue.Value);
        }

        [Fact]
        public async Task Get_Not_Cached_Value_Async_Should_Call_Retriever_And_Return_Null()
        {
            var cacheKey = $"{_nameSpace}{Guid.NewGuid().ToString()}";
            var func = Create_Fake_Retriever_Return_NULL_Async();

            var res = await _provider.GetAsync(cacheKey, func, _defaultTs);

            Assert.Equal(default(string), res.Value);
        }

        [Fact]
        public void Get_Not_Cached_Value_Should_Call_Retriever_And_Return_Null_With_Caching_When_Nulls_Are_Cached()
        {
            var cacheKey = $"{_nameSpace}{Guid.NewGuid().ToString()}";
            var func = Create_Fake_Retriever_Return_NULL();

            var res = _providerWithNullsCached.Get(cacheKey, func, _defaultTs);

            Assert.Equal(default(string),res.Value);
            var cachedValue = _providerWithNullsCached.Get<string>(cacheKey);
            Assert.True(cachedValue.HasValue);
            Assert.Null(cachedValue.Value);
        }

        [Fact]
        public void Get_Cached_Value_Should_Not_Call_Retriever()
        {
            var cacheKey = $"{_nameSpace}{Guid.NewGuid().ToString()}";
            var func = Create_Fake_Retriever_Return_String();
            var cacheVlaue = "Memory";

            _provider.Set(cacheKey, cacheVlaue, _defaultTs);
            var res = _provider.Get(cacheKey, func, _defaultTs);

            A.CallTo(() => func.Invoke()).MustNotHaveHappened();
        }

        [Fact]
        public async Task Get_Cached_Value_Async_Should_Not_Call_Retriever()
        {
            var cacheKey = $"{_nameSpace}{Guid.NewGuid().ToString()}";
            var func = Create_Fake_Retriever_Return_String_Async();
            var cacheVlaue = "Memory";

            await _provider.SetAsync(cacheKey, cacheVlaue, _defaultTs);
            var res = await _provider.GetAsync(cacheKey, func, _defaultTs);

            A.CallTo(() => func.Invoke()).MustNotHaveHappened();
        }

        [Fact]
        public void Get_Not_Cached_Value_Should_Call_Retriever_And_Return_Value()
        {
            var cacheKey = $"{_nameSpace}{Guid.NewGuid().ToString()}";
            var func = Create_Fake_Retriever_Return_String();

            var res = _provider.Get(cacheKey, func, _defaultTs);

            Assert.Equal("123", res.Value);
        }

        [Fact]
        public async Task Get_Not_Cached_Value_Async_Should_Call_Retriever_And_Return_Value()
        {
            var cacheKey = $"{_nameSpace}{Guid.NewGuid().ToString()}";
            var func = Create_Fake_Retriever_Return_String_Async();

            var res = await _provider.GetAsync(cacheKey, func, _defaultTs);

            Assert.Equal("123", res.Value);
        }

        [Fact]
        public void Get_Not_Cached_Value_Without_Retriever_Should_Return_Default_Value()
        {
            var cacheKey = $"{_nameSpace}{Guid.NewGuid().ToString()}";

            var res = _provider.Get<string>(cacheKey);

            Assert.Equal(default(string), res.Value);
        }

        [Fact]
        public async Task Get_Not_Cached_Value_Without_Retriever_Async_Should_Return_Default_Value()
        {
            var cacheKey = $"{_nameSpace}{Guid.NewGuid().ToString()}";

            var res = await _provider.GetAsync<string>(cacheKey);

            Assert.Equal(default(string), res.Value);
        }

        [Fact]
        public void Get_Cached_Value_Without_Retriever_Should_Return_Default_Value()
        {
            var cacheKey = $"{_nameSpace}{Guid.NewGuid().ToString()}";

            _provider.Set(cacheKey, "123", _defaultTs);

            var res = _provider.Get<string>(cacheKey);

            Assert.Equal("123", res.Value);
        }

        [Fact]
        public async Task Get_Cached_Value_Without_Retriever_Async_Should_Return_Default_Value()
        {
            var cacheKey = $"{_nameSpace}{Guid.NewGuid().ToString()}";

            await _provider.SetAsync(cacheKey, "123", _defaultTs);

            var res = await _provider.GetAsync<string>(cacheKey);

            Assert.Equal("123", res.Value);
        }

        [Fact]
        public void Get_Cached_Value_Should_Succeed()
        {
            var cacheKey = $"{_nameSpace}{Guid.NewGuid().ToString()}";
            var cacheValue = "value";

            _provider.Set(cacheKey, cacheValue, _defaultTs);
            var val = _provider.Get<string>(cacheKey, null, _defaultTs);

            Assert.Equal(cacheValue, val.Value);
        }

        [Fact]
        public async Task Get_Cached_Value_Async_Should_Succeed()
        {
            var cacheKey = $"{_nameSpace}{Guid.NewGuid().ToString()}";
            var cacheValue = "value";

            await _provider.SetAsync(cacheKey, cacheValue, _defaultTs);
            var val = await _provider.GetAsync<string>(cacheKey, null, _defaultTs);

            Assert.Equal(cacheValue, val.Value);
        }

        [Fact]
        public void Get_Cached_Value_Without_Retriever_Should_Succeed()
        {
            var cacheKey = $"{_nameSpace}{Guid.NewGuid().ToString()}";
            var cacheValue = "value";

            _provider.Set(cacheKey, cacheValue, _defaultTs);
            var val = _provider.Get<string>(cacheKey);

            Assert.Equal(cacheValue, val.Value);
        }

        [Fact]
        public async Task Get_Cached_Value_Without_Retriever_Async_Should_Succeed()
        {
            var cacheKey = $"{_nameSpace}{Guid.NewGuid().ToString()}";
            var cacheValue = "value";

            await _provider.SetAsync(cacheKey, cacheValue, _defaultTs);
            var val = await _provider.GetAsync<string>(cacheKey);

            Assert.Equal(cacheValue, val.Value);
        }

        [Fact]
        public void Get_Not_Cached_Value_Without_Retriever_Should_Succeed()
        {
            var cacheKey = $"{_nameSpace}{Guid.NewGuid().ToString()}";

            var val = _provider.Get<string>(cacheKey);

            Assert.Equal(default(string), val.Value);
        }

        [Fact]
        public async Task Get_Not_Cached_Value_Without_Retriever_Async_Should_Succeed()
        {
            var cacheKey = $"{_nameSpace}{Guid.NewGuid().ToString()}";

            var val = await _provider.GetAsync<string>(cacheKey);

            Assert.Equal(default(string), val.Value);
        }

        [Fact]
        protected virtual void Get_Parallel_Should_Succeed()
        {
            var cacheKey = $"{_nameSpace}parallel{Guid.NewGuid().ToString()}";

            int count = 0;

            Parallel.For(0, 20, x =>
            {
                _provider.Get<int>(cacheKey, () =>
                {
                    System.Threading.Interlocked.Increment(ref count);
                    return 1;
                }, _defaultTs);
            });

            Assert.InRange(count, 1, 5);
        }

        [Fact]
        protected virtual async Task GetAsync_Parallel_Should_Succeed()
        {
            var cacheKey = $"{_nameSpace}parallelasync{Guid.NewGuid().ToString()}";
            int count = 0;

            var tasks = Enumerable.Range(0, 20)
                        .Select(i => _provider.GetAsync(cacheKey, () =>
                        {
                            System.Threading.Interlocked.Increment(ref count);
                            return Task.FromResult(1);
                        }, _defaultTs));

            await Task.WhenAll(tasks);

            Assert.InRange(count, 1, 5);
        }
        #endregion              

        #region Remove/RemoveAsync
        [Fact]
        public void Remove_Cached_Value_Should_Succeed()
        {
            var cacheKey = $"{_nameSpace}{Guid.NewGuid().ToString()}";
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
            var cacheKey = $"{_nameSpace}{Guid.NewGuid().ToString()}";
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
            var cacheKey = $"{_nameSpace}{Guid.NewGuid().ToString()}";
            var cacheValue = "value";
            _provider.Set(cacheKey, cacheValue, _defaultTs);

            var flag = _provider.Exists(cacheKey);

            Assert.True(flag);
        }

        [Fact]
        public async Task Exists_Cached_Value_Async_Should_Return_True()
        {
            var cacheKey = $"{_nameSpace}{Guid.NewGuid().ToString()}";
            var cacheValue = "value";
            await _provider.SetAsync(cacheKey, cacheValue, _defaultTs);

            var flag = await _provider.ExistsAsync(cacheKey);

            Assert.True(flag);
        }

        [Fact]
        public void Exists_Cached_Value_Should_Return_False()
        {
            var cacheKey = $"{_nameSpace}{Guid.NewGuid().ToString()}";

            var flag = _provider.Exists(cacheKey);

            Assert.False(flag);
        }

        [Fact]
        public async Task Exists_Cached_Value_Async_Should_Return_False()
        {
            var cacheKey = $"{_nameSpace}{Guid.NewGuid().ToString()}";

            var flag = await _provider.ExistsAsync(cacheKey);

            Assert.False(flag);
        }              
        #endregion

        #region RemoveByPrefix/RemoveByPrefixAsync
        [Fact]
        protected virtual void RemoveByPrefix_Should_Succeed()
        {
            SetCacheItem($"{_nameSpace}demo:1", "1");
            SetCacheItem($"{_nameSpace}demo:2", "2");
            SetCacheItem($"{_nameSpace}demo:3", "3");
            SetCacheItem($"{_nameSpace}demo:4", "4");
            SetCacheItem($"{_nameSpace}xxx:1", "1");

            _provider.RemoveByPrefix($"{_nameSpace}demo");

            var demo1 = _provider.Get<string>($"{_nameSpace}demo:1");
            var demo2 = _provider.Get<string>($"{_nameSpace}demo:2");
            var demo3 = _provider.Get<string>($"{_nameSpace}demo:3");
            var demo4 = _provider.Get<string>($"{_nameSpace}demo:4");
            var xxx1 = _provider.Get<string>($"{_nameSpace}xxx:1");

            Assert.False(demo1.HasValue);
            Assert.False(demo2.HasValue);
            Assert.False(demo3.HasValue);
            Assert.False(demo4.HasValue);
            Assert.True(xxx1.HasValue);
        }

        [Fact]
        protected virtual async Task RemoveByPrefixAsync_Should_Succeed()
        {
            SetCacheItem($"{_nameSpace}demo:1#async", "1");
            SetCacheItem($"{_nameSpace}demo:2#async", "2");
            SetCacheItem($"{_nameSpace}demo:3#async", "3");
            SetCacheItem($"{_nameSpace}demo:4#async", "4");
            SetCacheItem($"{_nameSpace}xxx:1#async", "1");

            await _provider.RemoveByPrefixAsync($"{_nameSpace}demo");

            var demo1 = _provider.Get<string>($"{_nameSpace}demo:1#async");
            var demo2 = _provider.Get<string>($"{_nameSpace}demo:2#async");
            var demo3 = _provider.Get<string>($"{_nameSpace}demo:3#async");
            var demo4 = _provider.Get<string>($"{_nameSpace}demo:4#async");
            var xxx1 = _provider.Get<string>($"{_nameSpace}xxx:1#async");

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
            var dict = GetMultiDict($"{_nameSpace}setall:");

            _provider.SetAll(dict, _defaultTs);

            var res1 = _provider.Get<string>($"{_nameSpace}setall:key:1");
            var res2 = _provider.Get<string>($"{_nameSpace}setall:key:2");

            Assert.Equal("value1", res1.Value);
            Assert.Equal("value2", res2.Value);
        }
        
        [Fact]
        protected virtual void SetAll_Should_Succeed_When_One_Of_Values_Is_Null_And_Nulls_Are_Cached()
        {
            var dict = new Dictionary<string, string>()
            {
                [$"{_nameSpace}setallwithnull:key:1"] = "value1",
                [$"{_nameSpace}setallwithnull:key:2"] = null,
            };;

            _providerWithNullsCached.SetAll(dict, _defaultTs);

            var res1 = _providerWithNullsCached.Get<string>($"{_nameSpace}setallwithnull:key:1");
            var res2 = _providerWithNullsCached.Get<string>($"{_nameSpace}setallwithnull:key:2");

            Assert.Equal("value1", res1.Value);
            Assert.True(res2.HasValue);
            Assert.Null(res2.Value);
        }

        [Fact]
        protected virtual async Task SetAllAsync_Should_Succeed()
        {
            var dict = GetMultiDict($"{_nameSpace}setallasync:");

            await _provider.SetAllAsync(dict, _defaultTs);

            var res1 = _provider.Get<string>($"{_nameSpace}setallasync:key:1");
            var res2 = _provider.Get<string>($"{_nameSpace}setallasync:key:2");

            Assert.Equal("value1", res1.Value);
            Assert.Equal("value2", res2.Value);
        }
        
        [Fact]
        protected virtual async Task SetAllAsync_Should_Succeed_When_One_Of_Values_Is_Null_And_Nulls_Are_Cached()
        {
            var dict = new Dictionary<string, string>()
            {
                [$"{_nameSpace}setallasyncwithnull:key:1"] = "value1",
                [$"{_nameSpace}setallasyncwithnull:key:2"] = null,
            };;

            await _providerWithNullsCached.SetAllAsync(dict, _defaultTs);

            var res1 = _providerWithNullsCached.Get<string>($"{_nameSpace}setallasyncwithnull:key:1");
            var res2 = _providerWithNullsCached.Get<string>($"{_nameSpace}setallasyncwithnull:key:2");

            Assert.Equal("value1", res1.Value);
            Assert.True(res2.HasValue);
            Assert.Null(res2.Value);
        }
        #endregion

        #region GetAll/GetAllAsync
        [Fact]
        protected virtual void GetAll_Should_Succeed()
        {
            _provider.RemoveAll(new List<string> { $"{_nameSpace}getall:key:1", $"{_nameSpace}getall:key:2" });
            var dict = GetMultiDict($"{_nameSpace}getall:");

            _provider.SetAll(dict, _defaultTs);

            var res = _provider.GetAll<string>(new List<string> { $"{_nameSpace}getall:key:1", $"{_nameSpace}getall:key:2" });

            Assert.Equal(2, res.Count);

            Assert.Contains($"{_nameSpace}getall:key:1",res.Select(x => x.Key));
            Assert.Contains($"{_nameSpace}getall:key:2", res.Select(x => x.Key));
            Assert.Equal("value1", res.Where(x => x.Key == $"{_nameSpace}getall:key:1").Select(x => x.Value).FirstOrDefault().Value);
            Assert.Equal("value2", res.Where(x => x.Key == $"{_nameSpace}getall:key:2").Select(x => x.Value).FirstOrDefault().Value);
        }
        
        [Fact]
        protected virtual void GetAll_Should_Succeed_When_One_Of_Values_Is_Null_And_Nulls_Are_Cached()
        {
            _providerWithNullsCached.RemoveAll(new List<string> { $"{_nameSpace}getall:key:1", $"{_nameSpace}getall:key:2" });
            var dict = new Dictionary<string, string>()
            {
                [$"{_nameSpace}getallwithnull:key:1"] = "value1",
                [$"{_nameSpace}getallwithnull:key:2"] = null,
            };;

            _providerWithNullsCached.SetAll(dict, _defaultTs);

            var res = _providerWithNullsCached.GetAll<string>(new List<string> { $"{_nameSpace}getallwithnull:key:1", $"{_nameSpace}getallwithnull:key:2" });

            Assert.Equal(2, res.Count);

            Assert.Contains($"{_nameSpace}getallwithnull:key:1",res.Select(x => x.Key));
            Assert.Contains($"{_nameSpace}getallwithnull:key:2", res.Select(x => x.Key));
            Assert.Equal("value1", res.Where(x => x.Key == $"{_nameSpace}getallwithnull:key:1").Select(x => x.Value).FirstOrDefault().Value);
            Assert.Null(res.Where(x => x.Key == $"{_nameSpace}getallwithnull:key:2").Select(x => x.Value).FirstOrDefault().Value);
        }

        [Fact]
        protected virtual async Task GetAllAsync_Should_Succeed()
        {
            _provider.RemoveAll(new List<string> { $"{_nameSpace}getallasync:key:1", $"{_nameSpace}getallasync:key:2" });
            var dict = GetMultiDict($"{_nameSpace}getallasync:");

            _provider.SetAll(dict, _defaultTs);

            var res = await _provider.GetAllAsync<string>(new List<string> { $"{_nameSpace}getallasync:key:1", $"{_nameSpace}getallasync:key:2" });

            Assert.Equal(2, res.Count);

            Assert.Contains($"{_nameSpace}getallasync:key:1", res.Select(x => x.Key));
            Assert.Contains($"{_nameSpace}getallasync:key:2", res.Select(x => x.Key));
            Assert.Equal("value1",res.Where(x => x.Key == $"{_nameSpace}getallasync:key:1").Select(x => x.Value).FirstOrDefault().Value) ;
            Assert.Equal("value2",res.Where(x => x.Key == $"{_nameSpace}getallasync:key:2").Select(x => x.Value).FirstOrDefault().Value);
        }
        
        [Fact]
        protected virtual async Task GetAllAsync_Should_Succeed_When_One_Of_Values_Is_Null_And_Nulls_Are_Cached()
        {
            _providerWithNullsCached.RemoveAll(new List<string> { $"{_nameSpace}getall:key:1", $"{_nameSpace}getall:key:2" });
            var dict = new Dictionary<string, string>()
            {
                [$"{_nameSpace}getallasyncwithnull:key:1"] = "value1",
                [$"{_nameSpace}getallasyncwithnull:key:2"] = null,
            };;

            _providerWithNullsCached.SetAll(dict, _defaultTs);

            var res = await _providerWithNullsCached.GetAllAsync<string>(new List<string> { $"{_nameSpace}getallasyncwithnull:key:1", $"{_nameSpace}getallasyncwithnull:key:2" });

            Assert.Equal(2, res.Count);

            Assert.Contains($"{_nameSpace}getallasyncwithnull:key:1", res.Select(x => x.Key));
            Assert.Contains($"{_nameSpace}getallasyncwithnull:key:2", res.Select(x => x.Key));
            Assert.Equal("value1", res.Where(x => x.Key == $"{_nameSpace}getallasyncwithnull:key:1").Select(x => x.Value).FirstOrDefault().Value);
            Assert.Null(res.Where(x => x.Key == $"{_nameSpace}getallasyncwithnull:key:2").Select(x => x.Value).FirstOrDefault().Value);
        }

        [Fact]
        protected virtual void GetAll_With_Value_Type_Should_Succeed()
        {
            _provider.RemoveAll(new List<string> { $"{_nameSpace}getall:valuetype:key:1", $"{_nameSpace}getall:valuetype:key:2" });
            var dict = new Dictionary<string, int> {{ $"{_nameSpace}getall:valuetype:key:1",10},{ $"{_nameSpace}getall:valuetype:key:2",100} };

            _provider.SetAll(dict, _defaultTs);

            var res = _provider.GetAll<int>(new List<string> { $"{_nameSpace}getall:valuetype:key:1", $"{_nameSpace}getall:valuetype:key:2" });

            Assert.Equal(2, res.Count);

            Assert.Contains($"{_nameSpace}getall:valuetype:key:1", res.Select(x => x.Key));
            Assert.Contains($"{_nameSpace}getall:valuetype:key:2", res.Select(x => x.Key));
            Assert.Equal(10, res.Where(x => x.Key == $"{_nameSpace}getall:valuetype:key:1").Select(x => x.Value).FirstOrDefault().Value);
            Assert.Equal(100,res.Where(x => x.Key == $"{_nameSpace}getall:valuetype:key:2").Select(x => x.Value).FirstOrDefault().Value);
        }

        [Fact]
        protected virtual async Task GetAll_Async_With_Value_Type_Should_Succeed()
        {
            _provider.RemoveAll(new List<string> { $"{_nameSpace}getallasync:valuetype:key:1", $"{_nameSpace}getallasync:valuetype:key:2" });
            var dict = new Dictionary<string, int> { { $"{_nameSpace}getallasync:valuetype:key:1", 10 }, { $"{_nameSpace}getallasync:valuetype:key:2", 100 } };

            _provider.SetAll(dict, _defaultTs);

            var res = await _provider.GetAllAsync<int>(new List<string> { $"{_nameSpace}getallasync:valuetype:key:1", $"{_nameSpace}getallasync:valuetype:key:2" });

            Assert.Equal(2, res.Count);

            Assert.Contains($"{_nameSpace}getallasync:valuetype:key:1", res.Select(x => x.Key));
            Assert.Contains($"{_nameSpace}getallasync:valuetype:key:2", res.Select(x => x.Key));
            Assert.Equal(10, res.Where(x => x.Key == $"{_nameSpace}getallasync:valuetype:key:1").Select(x => x.Value).FirstOrDefault().Value);
            Assert.Equal(100, res.Where(x => x.Key == $"{_nameSpace}getallasync:valuetype:key:2").Select(x => x.Value).FirstOrDefault().Value);
        }
        #endregion

        #region GetByPrefix/GetByPrefixAsync
        [Fact]
        protected virtual void GetByPrefix_Should_Succeed()
        {
            _provider.RemoveAll(new List<string> { $"{_nameSpace}getbyprefix:key:1", $"{_nameSpace}getbyprefix:key:2" });
            var dict = GetMultiDict($"{_nameSpace}getbyprefix:");

            _provider.SetAll(dict, _defaultTs);

            string prefix = $"{_nameSpace}getbyprefix:key:";

            var res = _provider.GetByPrefix<string>(prefix);

            Assert.Equal(2, res.Count);
            Assert.Contains($"{_nameSpace}getbyprefix:key:1", res.Select(x => x.Key));
            Assert.Contains($"{_nameSpace}getbyprefix:key:2", res.Select(x => x.Key));
            Assert.Equal("value1",res.Where(x => x.Key == $"{_nameSpace}getbyprefix:key:1").Select(x => x.Value).FirstOrDefault().Value);
            Assert.Equal("value2",res.Where(x => x.Key == $"{_nameSpace}getbyprefix:key:2").Select(x => x.Value).FirstOrDefault().Value);
        }

        [Fact]
        protected virtual async Task GetByPrefixAsync_Should_Succeed()
        {
            _provider.RemoveAll(new List<string> { $"{_nameSpace}getbyprefixasync:key:1", $"{_nameSpace}getbyprefixasync:key:2" });
            var dict = GetMultiDict($"{_nameSpace}getbyprefixasync:");

            _provider.SetAll(dict, _defaultTs);

            string prefix = $"{_nameSpace}getbyprefixasync:key:";

            var res = await _provider.GetByPrefixAsync<string>(prefix);

            Assert.Equal(2, res.Count);
            Assert.Contains($"{_nameSpace}getbyprefixasync:key:1", res.Select(x => x.Key));
            Assert.Contains($"{_nameSpace}getbyprefixasync:key:2", res.Select(x => x.Key));
            Assert.Equal("value1",res.Where(x => x.Key == $"{_nameSpace}getbyprefixasync:key:1").Select(x => x.Value).FirstOrDefault().Value);
            Assert.Equal("value2",res.Where(x => x.Key == $"{_nameSpace}getbyprefixasync:key:2").Select(x => x.Value).FirstOrDefault().Value);
        }

        [Fact]
        protected virtual void GetByPrefix_With_Not_Existed_Prefix_Should_Return_Empty_Dict()
        {
            var prefix = $"{_nameSpace}{Guid.NewGuid().ToString()}";

            var res = _provider.GetByPrefix<string>(prefix);

            Assert.Equal(0, res.Count);
        }

        [Fact]
        protected virtual async Task GetByPrefixAsync_With_Not_Existed_Prefix_Should_Return_Empty_Dict()
        {
            var prefix = $"{_nameSpace}{Guid.NewGuid().ToString()}";

            var res = await _provider.GetByPrefixAsync<string>(prefix);

            Assert.Equal(0, res.Count);
        }

        #endregion    

        #region RemoveAll/RemoveAllAsync
        [Fact]
        protected virtual void RemoveAll_Should_Succeed()
        {
            var dict = GetMultiDict($"{_nameSpace}removeall:");

            _provider.SetAll(dict, _defaultTs);

            _provider.RemoveAll(new List<string> { $"{_nameSpace}removeall:key:1", $"{_nameSpace}removeall:key:2" });

            var res1 = _provider.Get<string>($"{_nameSpace}removeall:key:1");
            var res2 = _provider.Get<string>($"{_nameSpace}removeall:key:2");

            Assert.False(res1.HasValue);
            Assert.False(res2.HasValue);
        }

        [Fact]
        protected virtual async Task RemoveAllAsync_Should_Succeed()
        {
            var dict = GetMultiDict($"{_nameSpace}removeallasync:");

            _provider.SetAll(dict, _defaultTs);

            await _provider.RemoveAllAsync(new List<string> { $"{_nameSpace}removeallasync:key:1", $"{_nameSpace}removeallasync:key:2" });

            var res1 = _provider.Get<string>($"{_nameSpace}removeallasync:key:1");
            var res2 = _provider.Get<string>($"{_nameSpace}removeallasync:key:2");

            Assert.False(res1.HasValue);
            Assert.False(res2.HasValue);
        }
        #endregion

        #region Flush/FlushAsync
        [Fact]
        protected virtual void Flush_Should_Succeed()
        {
            for (var i = 0; i < 5; i++)
                _provider.Set($"{_nameSpace}flush:{i}", $"value{i}", _defaultTs);

            for (var i = 0; i < 5; i++)
                Assert.Equal($"value{i}", _provider.Get<string>($"{_nameSpace}flush:{i}").Value);

            _provider.Flush();

            for (var i = 0; i < 5; i++)
                Assert.False(_provider.Get<string>($"{_nameSpace}flush:{i}").HasValue);
        }

        [Fact]
        protected virtual async Task FlushAsync_Should_Succeed()
        {
            for (var i = 0; i < 5; i++)
                _provider.Set($"{_nameSpace}flushasync:{i}", $"value{i}", _defaultTs);

            for (var i = 0; i < 5; i++)
                Assert.Equal($"value{i}", _provider.Get<string>($"{_nameSpace}flushasync:{i}").Value);

            await _provider.FlushAsync();

            for (var i = 0; i < 5; i++)
                Assert.False(_provider.Get<string>($"{_nameSpace}flushasync:{i}").HasValue);
        }
        #endregion

        #region GetCount
        [Fact]
        protected virtual void Get_Count_Without_Prefix_Should_Succeed()
        {
            _provider.Flush();
            var rd = Guid.NewGuid().ToString();

            for (var i = 0; i < 5; i++)
                _provider.Set($"{rd}:getcount:{i}", $"value{i}", _defaultTs);

            Assert.Equal(5, _provider.GetCount());

            _provider.Remove($"{rd}:getcount:4");

            Assert.Equal(4, _provider.GetCount());
        }

        [Fact]
        protected virtual void Get_Count_With_Prefix_Should_Succeed()
        {
            _provider.Flush();
            var rd = Guid.NewGuid().ToString();

            for (var i = 0; i < 5; i++)
                _provider.Set($"{rd}:getcount:withprefix:{i}", $"value{i}", _defaultTs);

            Assert.Equal(5, _provider.GetCount($"{rd}:getcount:withprefix:"));

            _provider.Remove($"{rd}:getcount:withprefix:1");

            Assert.Equal(4, _provider.GetCount($"{rd}:getcount:withprefix:"));
        }

        [Fact]
        protected virtual async Task Get_Count_Async_Without_Prefix_Should_Succeed()
        {
            _provider.Flush();
            var rd = Guid.NewGuid().ToString();

            for (var i = 0; i < 5; i++)
                _provider.Set($"{rd}:getcountaync:{i}", $"value{i}", _defaultTs);

            Assert.Equal(5, await _provider.GetCountAsync());

            _provider.Remove($"{rd}:getcountaync:4");

            Assert.Equal(4, await _provider.GetCountAsync());
        }

        [Fact]
        protected virtual async Task Get_Count_Async_With_Prefix_Should_Succeed()
        {
            _provider.Flush();
            var rd = Guid.NewGuid().ToString();

            for (var i = 0; i < 5; i++)
                _provider.Set($"{rd}:getcountaync:withprefix:{i}", $"value{i}", _defaultTs);

            Assert.Equal(5, await _provider.GetCountAsync($"{rd}:getcountaync:withprefix:"));

            _provider.Remove($"{rd}:getcountaync:withprefix:1");

            Assert.Equal(4, await _provider.GetCountAsync($"{rd}:getcountaync:withprefix:"));
        }
        #endregion

        #region TrySet
        [Fact]
        protected virtual void TrySet_Value_And_Get_Cached_Value_Should_Succeed()
        {
            var cacheKey = $"{_nameSpace}{Guid.NewGuid().ToString()}";
            var cacheValue1 = "value1";
            var cacheValue2 = "value2";

            var first = _provider.TrySet(cacheKey, cacheValue1, _defaultTs);
            var second = _provider.TrySet(cacheKey, cacheValue2, _defaultTs);

            Assert.True(first);
            Assert.False(second);

            var val = _provider.Get<string>(cacheKey);
            Assert.True(val.HasValue);
            Assert.Equal(cacheValue1, val.Value);
        }
        
        [Fact]
        public void TrySet_Value_And_Get_Cached_Value_Should_Succeed_When_CacheValue_IsNull_And_Nulls_Are_Cached()
        {
            var cacheKey = $"{_nameSpace}-sg-{Guid.NewGuid().ToString()}";
            string cacheValue = null;

            _providerWithNullsCached.TrySet(cacheKey, cacheValue, _defaultTs);

            var val = _providerWithNullsCached.Get<string>(cacheKey);
            Assert.True(val.HasValue);
            Assert.Null(val.Value);

            _providerWithNullsCached.Remove(cacheKey);
        }

        [Fact]
        protected virtual async Task TrySet_Value_And_Get_Cached_Value_Async_Should_Succeed()
        {
            var cacheKey = $"{_nameSpace}{Guid.NewGuid().ToString()}";
            var cacheValue1 = "value1";
            var cacheValue2 = "value2";

            var first = await _provider.TrySetAsync(cacheKey, cacheValue1, _defaultTs);
            var second = await _provider.TrySetAsync(cacheKey, cacheValue2, _defaultTs);

            Assert.True(first);
            Assert.False(second);

            var val = _provider.Get<string>(cacheKey);
            Assert.True(val.HasValue);
            Assert.Equal(cacheValue1, val.Value);
        }
        
        [Fact]
        public async Task TrySet_Value_And_Get_Cached_Value_Async_Should_Succeed_When_CacheValue_IsNull_And_Nulls_Are_Cached()
        {
            var cacheKey = $"{_nameSpace}-sg-{Guid.NewGuid().ToString()}";
            string cacheValue = null;

            await _providerWithNullsCached.TrySetAsync(cacheKey, cacheValue, _defaultTs);

            var val = await _providerWithNullsCached.GetAsync<string>(cacheKey);
            Assert.True(val.HasValue);
            Assert.Null(val.Value);

            await _providerWithNullsCached.RemoveAsync(cacheKey);
        }
        #endregion

        #region GetExpiration
        [Fact]
        protected virtual void GetExpiration_Should_Succeed()
        {
            var cacheKey = $"{_nameSpace}{Guid.NewGuid().ToString()}";
            var cacheValue1 = "value1";

            _provider.Set(cacheKey, cacheValue1, _defaultTs);

            var ts = _provider.GetExpiration(cacheKey);

            Assert.InRange(ts, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(30 + 120));
        }

        [Fact]
        protected virtual async Task GetExpiration_Async_Should_Succeed()
        {
            var cacheKey = $"{_nameSpace}{Guid.NewGuid().ToString()}";
            var cacheValue1 = "value1";

            await _provider.SetAsync(cacheKey, cacheValue1, _defaultTs);

            var ts = await _provider.GetExpirationAsync(cacheKey);

            Assert.InRange(ts, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(30 + 120));
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

        [Fact]
        protected virtual void OnHit_Should_Return_One_And_OnMiss_Should_Return_Zero()
        {
            var cacheKey = $"{_nameSpace}{Guid.NewGuid().ToString()}";
            _provider.Set(cacheKey, "onhit", _defaultTs);
            _provider.Get<string>(cacheKey);

            var hitRes = _provider.CacheStats.GetStatistic(StatsType.Hit);
            var missedRes = _provider.CacheStats.GetStatistic(StatsType.Missed);

            Assert.Equal(1, hitRes);
            Assert.Equal(0, missedRes);
        }

        [Fact]
        protected virtual void OnHit_Should_Return_Zero_And_OnMiss_Should_Return_One()
        {
            var cacheKey = $"{_nameSpace}{Guid.NewGuid().ToString()}";
            _provider.Get<string>(cacheKey);

            var hitRes = _provider.CacheStats.GetStatistic(StatsType.Hit);
            var missedRes = _provider.CacheStats.GetStatistic(StatsType.Missed);

            Assert.Equal(0, hitRes);
            Assert.Equal(1, missedRes);
        }
    }
}