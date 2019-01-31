namespace EasyCaching.UnitTests
{
    using EasyCaching.Core;
    using System;
    using System.Threading.Tasks;
    using Xunit;

    public abstract class BaseUsingEasyCachingTest
    {
        protected IEasyCachingProvider _provider;
        protected TimeSpan _defaultTs;
        protected string _nameSpace = string.Empty;

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
