namespace EasyCaching.UnitTests
{
    using EasyCaching.InMemory;
    using FakeItEasy;
    using Microsoft.Extensions.Caching.Memory;
    using System;
    using Xunit;

    public class MemoryCachingProviderTest
    {
        private readonly InMemoryCachingProvider _provider;
        private readonly TimeSpan _defaultTs;

        public MemoryCachingProviderTest()
        {
            var cache = new MemoryCache(new MemoryCacheOptions());
            _provider = new InMemoryCachingProvider(cache);
            _defaultTs = TimeSpan.FromSeconds(30);
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData(null)]
        public void Set_Value_Should_Throw_ArgumentNullException_When_CacheKey_IsNullOrWhiteSpace(string cacheKey)
        {
            var cacheVlaue = "value";
            Assert.Throws<ArgumentNullException>(()=>_provider.Set(cacheKey,cacheVlaue,_defaultTs));
        }

        [Fact]
        public void Set_Value_Should_Throw_ArgumentNullException_When_CacheValue_IsNull()
        {
            var cacheKey = Guid.NewGuid().ToString();
            object cacheVlaue = null;
            Assert.Throws<ArgumentNullException>(() => _provider.Set(cacheKey, cacheVlaue, _defaultTs));
        }

        [Fact]
        public void Set_Value_And_Get_Cached_Value_Should_Succeed()
        {
            var cacheKey = Guid.NewGuid().ToString();
            object cacheVlaue = "value";

            _provider.Set(cacheKey, cacheVlaue, _defaultTs);
            var res = _provider.Get(cacheKey, null, _defaultTs);

            Assert.Equal(cacheVlaue, res);
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
        public void Get_Not_Cached_Value_Should_Call_Retriever_And_Return_Value()
        {
            var cacheKey = Guid.NewGuid().ToString();
            var func = Create_Fake_Retriever_Return_String();

            var res = _provider.Get(cacheKey, func, _defaultTs);

            Assert.Equal("123", res);
        }

        [Fact]
        public void Remove_Cached_Value_Should_Succeed()
        {
            var cacheKey = Guid.NewGuid().ToString();
            _provider.Set(cacheKey,"Remove",_defaultTs);

            var first = _provider.Get(cacheKey, null, _defaultTs);

            Assert.NotNull(first);

            _provider.Remove(cacheKey);

            var second = _provider.Get(cacheKey,null,_defaultTs);

            Assert.Null(second);
        }

        private Func<string> Create_Fake_Retriever_Return_String()
        {
            var func = A.Fake<Func<string>>();

            A.CallTo(() => func.Invoke()).Returns("123");

            return func;
        }
    }
}
