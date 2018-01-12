namespace EasyCaching.UnitTests
{
    using EasyCaching.InMemory;
    using FakeItEasy;
    using Microsoft.Extensions.Caching.Memory;
    using System;
    using Xunit;

    public class MemoryCachingProviderTest : BaseCachingProviderTest
    {
        public MemoryCachingProviderTest()
        {
            var cache = new MemoryCache(new MemoryCacheOptions());
            _provider = new InMemoryCachingProvider(cache);
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

            Assert.Equal("123", res.Value);
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
    }
}
