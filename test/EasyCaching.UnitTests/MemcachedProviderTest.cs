namespace EasyCaching.UnitTests
{
    using EasyCaching.Memcached;
    using Enyim.Caching;
    using Enyim.Caching.Configuration;
    using FakeItEasy;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Options;
    using System;
    using Xunit;

    public class MemcachedProviderTest
    {
        private IMemcachedClient _client;
        private readonly DefaultMemcachedCachingProvider _provider;
        private readonly string _key;
        private readonly TimeSpan _defaultTs;

        public MemcachedProviderTest()
        {
            //var fakeServices = A.Fake<IServiceCollection>();
            //A.CallTo(()=>fakeServices.AddDefaultMemcached(options => options.AddServer("127.0.0.1", 11211)));
            //A.CallTo(()=>fakeServices.AddLogging());
            IServiceCollection services = new ServiceCollection();
            services.AddEnyimMemcached(options => options.AddServer("127.0.0.1", 11211));
            services.AddLogging();
            IServiceProvider serviceProvider = services.BuildServiceProvider();
            _client = serviceProvider.GetService<IMemcachedClient>();

            _provider = new DefaultMemcachedCachingProvider(_client);
            _key = Guid.NewGuid().ToString();
            _defaultTs = TimeSpan.FromSeconds(5);
        }

        [Fact]
        public void Set_Value_Should_Succeed()
        {
            var cacheValue = "value";
            _provider.Set(_key, cacheValue, _defaultTs);

            var val = _provider.Get(_key, null, _defaultTs);
            Assert.NotNull(val);
            Assert.Equal(cacheValue, val);
        }

        [Fact]
        public void Remove_Cached_Value_Should_Succeed()
        {
            var cacheValue = "value";
            _provider.Set(_key, cacheValue, _defaultTs);
            var valBeforeRemove = _provider.Get(_key, null, _defaultTs);
            Assert.NotNull(valBeforeRemove);

            _provider.Remove(_key);
            var valAfterRemove = _provider.Get(_key, () => "123", _defaultTs);
            Assert.Equal("123", valAfterRemove);
        }
    }
}
