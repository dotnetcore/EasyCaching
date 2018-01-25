namespace EasyCaching.UnitTests
{
    using EasyCaching.Core;
    using FakeItEasy;
    using System;
    using System.Threading.Tasks;
    using Xunit;

    public abstract class BaseCachingProviderTest
    {
        protected IEasyCachingProvider _provider;
        protected TimeSpan _defaultTs;

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
    }
}
