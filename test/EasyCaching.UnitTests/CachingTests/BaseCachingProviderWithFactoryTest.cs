namespace EasyCaching.UnitTests
{
    using System;
    using System.Threading.Tasks;
    using EasyCaching.Core;
    using Xunit;

    public abstract class BaseCachingProviderWithFactoryTest
    {
        protected IEasyCachingProvider _provider;
        protected TimeSpan _defaultTs;
        protected IEasyCachingProvider _secondProvider;
        public const string SECOND_PROVIDER_NAME = "second";
        protected string _nameSpace = string.Empty;

        [Fact]
        protected virtual void Multi_Instance_Set_And_Get_Should_Succeed()
        {
            var cacheKey1 = $"{_nameSpace}named-provider-1";
            var cacheKey2 = $"{_nameSpace}named-provider-2";

            var value1 = Guid.NewGuid().ToString();
            var value2 = Guid.NewGuid().ToString("N");

            _provider.Set(cacheKey1, value1, _defaultTs);
            _secondProvider.Set(cacheKey2, value2, _defaultTs);

            var p1 = _provider.Get<string>(cacheKey1);
            var p2 = _provider.Get<string>(cacheKey2);

            var s1 = _secondProvider.Get<string>(cacheKey1);
            var s2 = _secondProvider.Get<string>(cacheKey2);

            Assert.Equal(value1, p1.Value);
            Assert.False(p2.HasValue);

            Assert.False(s1.HasValue);
            Assert.Equal(value2, s2.Value);
        }

        [Fact]
        protected virtual void Set_Value_And_Get_Cached_Value_Should_Succeed()
        {
            var cacheKey = $"{_nameSpace}{Guid.NewGuid().ToString()}";
            var cacheValue = "value";

            _provider.Set(cacheKey, cacheValue, _defaultTs);

            var val = _provider.Get<string>(cacheKey);
            Assert.True(val.HasValue);
            Assert.Equal(cacheValue, val.Value);
        }

        [Fact]
        protected virtual async Task Set_Value_And_Get_Cached_Value_Async_Should_Succeed()
        {
            var cacheKey = $"{_nameSpace}{Guid.NewGuid().ToString()}";
            var cacheValue = "value";

            await _provider.SetAsync(cacheKey, cacheValue, _defaultTs);

            var val = await _provider.GetAsync<string>(cacheKey);
            Assert.True(val.HasValue);
            Assert.Equal(cacheValue, val.Value);
        }
    }
}
