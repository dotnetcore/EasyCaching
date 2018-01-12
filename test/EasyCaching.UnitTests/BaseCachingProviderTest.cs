namespace EasyCaching.UnitTests
{
    using EasyCaching.Core;
    using FakeItEasy;
    using System;
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

        [Fact]
        public void Set_Value_Should_Throw_ArgumentNullException_When_CacheValue_IsNull()
        {
            var cacheKey = Guid.NewGuid().ToString();
            string cacheVlaue = null;
            Assert.Throws<ArgumentNullException>(() => _provider.Set(cacheKey, cacheVlaue, _defaultTs));
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
        public void Set_Value_Should_Throw_ArgumentOutOfRangeException_When_Expiration_Is_Negative()
        {
            var cacheKey = Guid.NewGuid().ToString();
            string cacheVlaue = "123";
            var expiration = new TimeSpan(0, 0, -1);
            Assert.Throws<ArgumentOutOfRangeException>(() => _provider.Set(cacheKey, cacheVlaue, expiration));
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData(null)]
        public void Get_Cached_Value_Should_Throw_ArgumentNullException_When_CacheKey_IsNullOrWhiteSpace(string cachekey)
        {
            Assert.Throws<ArgumentNullException>(() => _provider.Get<string>(cachekey, null, _defaultTs));
        }

        [Fact]
        public void Get_Not_Cached_Value_Should_Call_Retriever()
        {
            var cacheKey = Guid.NewGuid().ToString();

            var func = Create_Fake_Retriever_Return_String();

            var res = _provider.Get(cacheKey, func, _defaultTs);

            A.CallTo(() => func.Invoke()).MustHaveHappened();
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
        public void Exists_Cached_Value_Should_Throw_ArgumentNullException_When_CacheKey_IsNullOrWhiteSpace(string cacheKey)
        {
            Assert.Throws<ArgumentNullException>(() => _provider.Exists(cacheKey));
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
        public void Exists_Cached_Value_Should_Return_False()
        {
            var cacheKey = Guid.NewGuid().ToString();

            var flag = _provider.Exists(cacheKey);

            Assert.False(flag);
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
    }
}
