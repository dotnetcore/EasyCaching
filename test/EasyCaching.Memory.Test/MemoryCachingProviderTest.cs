namespace EasyCaching.Memory.Test
{
    using System;
    using Xunit;
    using FakeItEasy;
    using Microsoft.Extensions.Caching.Memory;

    public class MemoryCachingProviderTest
    {
        private readonly MemoryCachingProvider _provider;
        private readonly string _key;
        private readonly TimeSpan _defaultTs;

        public MemoryCachingProviderTest()
        {
            var cache = new MemoryCache(new MemoryCacheOptions());

            _provider = new MemoryCachingProvider(cache);
            _key = Guid.NewGuid().ToString();
            _defaultTs = TimeSpan.FromSeconds(30);
        }

        [Fact]
        public void Get_Cached_Value_Should_Succeed()
        {
            _provider.Set(_key, "value", _defaultTs);

            var res = _provider.Get(_key, null, _defaultTs);

            Assert.Equal("value", res.ToString());
        }

        [Fact]
        public void Get_Not_Cached_Value_Should_Return_Null()
        {
            var res = _provider.Get(_key, null, _defaultTs);

            Assert.Null(res);
        }

        [Fact]
        public void Get_Cached_Value_Should_Not_Call_Retriever()
        {
            _provider.Set(_key, "Memory", _defaultTs);

            var func = A.Fake<Func<string>>();

            var res = _provider.Get(_key, func, _defaultTs);

            A.CallTo(() => func.Invoke()).MustNotHaveHappened();
        }

        [Fact]
        public void Get_Not_Cached_Value_Should_Call_Retriever()
        {
            var func = A.Fake<Func<string>>();

            var res = _provider.Get(_key, func, _defaultTs);

            A.CallTo(() => func.Invoke()).MustHaveHappened(Repeated.Exactly.Once);
        }


        [Fact]
        public void Get_Not_Cached_Value_Should_Call_Retriever_And_Return_Value()
        {
            var func = A.Fake<Func<string>>();

            A.CallTo(() => func.Invoke()).Returns("123");

            var res = _provider.Get(_key, func, _defaultTs);

            Assert.Equal("123", res.ToString());
        }

        [Fact]
        public void Remove_Cached_Value_Should_Succeed()
        {
            _provider.Set(_key,"Remove",_defaultTs);

            var first = _provider.Get(_key, null, _defaultTs);

            Assert.NotNull(first);

            _provider.Remove(_key);

            var second = _provider.Get(_key,null,_defaultTs);

            Assert.Null(second);
        }
    }
}
