namespace EasyCaching.UnitTests
{
    using EasyCaching.Memcached;
    using Enyim.Caching;
    using FakeItEasy;
    using Microsoft.Extensions.DependencyInjection;
    using System;
    using Xunit;

    public class MemcachedProviderTest : BaseCachingProviderTest
    {
        private IMemcachedClient _client;

        public MemcachedProviderTest()
        {            
            IServiceCollection services = new ServiceCollection();
            services.AddEnyimMemcached(options => options.AddServer("127.0.0.1", 11211));
            services.AddLogging();
            IServiceProvider serviceProvider = services.BuildServiceProvider();
            _client = serviceProvider.GetService<IMemcachedClient>();

            _provider = new DefaultMemcachedCachingProvider(_client);
            _defaultTs = TimeSpan.FromSeconds(50);
        }

        [Fact]
        public void Set_Value_And_Get_Cached_Value_Should_Succeed()
        {
            var cacheKey = Guid.NewGuid().ToString();
            var cacheValue = "value";
            _provider.Set(cacheKey, cacheValue, _defaultTs);

            var val = _provider.Get<string>(cacheKey, null, _defaultTs);
            Assert.NotNull(val);
            Assert.Equal(cacheValue, val.Value);
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
