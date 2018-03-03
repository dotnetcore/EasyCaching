namespace EasyCaching.UnitTests
{
    using EasyCaching.Core;
    using EasyCaching.InMemory;
    using Microsoft.Extensions.DependencyInjection;
    using System;
    using Xunit;

    public class MemoryCachingProviderTest : BaseCachingProviderTest
    {
        public MemoryCachingProviderTest()
        {
            IServiceCollection services = new ServiceCollection();
            services.AddDefaultInMemoryCache();
            IServiceProvider serviceProvider = services.BuildServiceProvider();
            _provider = serviceProvider.GetService<IEasyCachingProvider>();
            _defaultTs = TimeSpan.FromSeconds(30);
        }

        [Fact]
        public void CacheKey_Length_GT_250_Should_Call_SHA1()
        {
            var cacheKey = "";
            var part = "1000000000";

            for (int i = 0; i < 26; i++)
                cacheKey += part;            

            var cacheValue = "value";

            _provider.Set(cacheKey, cacheValue, _defaultTs);

            var val = _provider.Get<string>(cacheKey);
            Assert.True(val.HasValue);
        }
    }
}
