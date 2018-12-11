namespace EasyCaching.UnitTests
{
    using EasyCaching.Memcached;
    using EasyCaching.Core;
    using Microsoft.Extensions.DependencyInjection;
    using System;
    using System.Threading.Tasks;
    using Xunit;

    public class MemcachedProviderTest : BaseCachingProviderTest
    {
        public MemcachedProviderTest()
        {
            IServiceCollection services = new ServiceCollection();
            services.AddDefaultMemcached(options =>
            {
                options.DBConfig.AddServer("127.0.0.1", 11211);
            });
            services.AddLogging();
            IServiceProvider serviceProvider = services.BuildServiceProvider();
            _provider = serviceProvider.GetService<IEasyCachingProvider>();
            _defaultTs = TimeSpan.FromSeconds(50);
        }

        [Fact]
        protected override void RemoveByPrefix_Should_Succeed()
        {
            string prefixKey = "demo";
            string prefixValue = "abc";

            _provider.Set(prefixKey, prefixValue, TimeSpan.FromSeconds(120));

            SetCacheItem("1", "1", prefixKey);
            SetCacheItem("2", "2", prefixKey);
            SetCacheItem("3", "3", prefixKey);
            SetCacheItem("4", "4", prefixKey);
            SetCacheItem("4", "4", "xxx");

            _provider.RemoveByPrefix(prefixKey);

            GetCacheItem("1", prefixKey);
            GetCacheItem("2", prefixKey);
            GetCacheItem("3", prefixKey);
            GetCacheItem("4", prefixKey);

            var pre = _provider.Get<string>("xxx");
            var cacheKey = string.Concat(pre, "4");
            var val = _provider.Get<string>(cacheKey);
            Assert.True(val.HasValue);

            var afterPrefixValue = _provider.Get<string>(prefixKey);
            Assert.NotEqual(prefixValue, afterPrefixValue.Value);
        }

        [Fact]
        protected override async Task RemoveByPrefixAsync_Should_Succeed()
        {
            string prefixKey = "demo";
            string prefixValue = "abc";

            _provider.Set("demo", prefixValue, TimeSpan.FromSeconds(120));

            SetCacheItem("1", "1", prefixKey);
            SetCacheItem("2", "2", prefixKey);
            SetCacheItem("3", "3", prefixKey);
            SetCacheItem("4", "4", prefixKey);
            SetCacheItem("4", "4", "xxx");

            await _provider.RemoveByPrefixAsync(prefixKey);

            GetCacheItem("1", prefixKey);
            GetCacheItem("2", prefixKey);
            GetCacheItem("3", prefixKey);
            GetCacheItem("4", prefixKey);

            var pre = _provider.Get<string>("xxx");
            var cacheKey = string.Concat(pre, "4");
            var val = _provider.Get<string>(cacheKey);
            Assert.True(val.HasValue);


            var afterPrefixValue = _provider.Get<string>(prefixKey);
            Assert.NotEqual(prefixValue, afterPrefixValue.Value);
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

        [Fact]
        protected override void GetByPrefix_Should_Succeed()
        {

        }

        [Fact]
        protected override async Task GetByPrefixAsync_Should_Succeed()
        {
            await Task.FromResult(1);
        }

        [Fact]
        protected override void GetByPrefix_With_Not_Existed_Prefix_Should_Return_Empty_Dict()
        {

        }

        [Fact]
        protected override async Task GetByPrefixAsync_With_Not_Existed_Prefix_Should_Return_Empty_Dict()
        {
            await Task.FromResult(1);
        }


        [Fact]
        protected override void Get_Count_Without_Prefix_Should_Succeed()
        {

        }

        [Fact]
        protected override void Get_Count_With_Prefix_Should_Succeed()
        {

        }

        private void SetCacheItem(string cacheKey, string cacheValue, string prefix)
        {
            var pre = _provider.Get<string>(prefix);

            cacheKey = string.Concat(pre, cacheKey);

            _provider.Set(cacheKey, cacheValue, _defaultTs);

            var val = _provider.Get<string>(cacheKey);
            Assert.Equal(cacheValue, val.Value);
        }

        private void GetCacheItem(string cacheKey, string prefix)
        {
            var pre = _provider.Get<string>(prefix);

            cacheKey = string.Concat(pre, cacheKey);

            var val = _provider.Get<string>(cacheKey);
            Assert.False(val.HasValue);
        }
    }


    public class MemcachedProviderWithFactoryTest : BaseCachingProviderTest
    {
        public MemcachedProviderWithFactoryTest()
        {
            IServiceCollection services = new ServiceCollection();
            services.AddDefaultMemcached("MyTest",options =>
            {
                options.DBConfig.AddServer("127.0.0.1", 11211);
            });
            services.AddLogging();
            IServiceProvider serviceProvider = services.BuildServiceProvider();
            _provider = serviceProvider.GetService<IEasyCachingProvider>();
            _defaultTs = TimeSpan.FromSeconds(50);
        }

        [Fact]
        protected override void RemoveByPrefix_Should_Succeed()
        {
            string prefixKey = "demowithfactory";
            string prefixValue = "abcwithfactory";

            _provider.Set(prefixKey, prefixValue, TimeSpan.FromSeconds(120));

            SetCacheItem("1", "1", prefixKey);
            SetCacheItem("2", "2", prefixKey);
            SetCacheItem("3", "3", prefixKey);
            SetCacheItem("4", "4", prefixKey);
            SetCacheItem("4", "4", "xxx");

            _provider.RemoveByPrefix(prefixKey);

            GetCacheItem("1", prefixKey);
            GetCacheItem("2", prefixKey);
            GetCacheItem("3", prefixKey);
            GetCacheItem("4", prefixKey);

            var pre = _provider.Get<string>("xxx");
            var cacheKey = string.Concat(pre, "4");
            var val = _provider.Get<string>(cacheKey);
            Assert.True(val.HasValue);

            var afterPrefixValue = _provider.Get<string>(prefixKey);
            Assert.NotEqual(prefixValue, afterPrefixValue.Value);
        }

        [Fact]
        protected override async Task RemoveByPrefixAsync_Should_Succeed()
        {
            string prefixKey = "demowithfactoryasync";
            string prefixValue = "abcwithfactoryasync";

            _provider.Set("demowithfactoryasync", prefixValue, TimeSpan.FromSeconds(120));

            SetCacheItem("1", "1", prefixKey);
            SetCacheItem("2", "2", prefixKey);
            SetCacheItem("3", "3", prefixKey);
            SetCacheItem("4", "4", prefixKey);
            SetCacheItem("4", "4", "xxx");

            await _provider.RemoveByPrefixAsync(prefixKey);

            GetCacheItem("1", prefixKey);
            GetCacheItem("2", prefixKey);
            GetCacheItem("3", prefixKey);
            GetCacheItem("4", prefixKey);

            var pre = _provider.Get<string>("xxx");
            var cacheKey = string.Concat(pre, "4");
            var val = _provider.Get<string>(cacheKey);
            Assert.True(val.HasValue);


            var afterPrefixValue = _provider.Get<string>(prefixKey);
            Assert.NotEqual(prefixValue, afterPrefixValue.Value);
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

        [Fact]
        protected override void GetByPrefix_Should_Succeed()
        {

        }

        [Fact]
        protected override async Task GetByPrefixAsync_Should_Succeed()
        {
            await Task.FromResult(1);
        }

        [Fact]
        protected override void GetByPrefix_With_Not_Existed_Prefix_Should_Return_Empty_Dict()
        {

        }

        [Fact]
        protected override async Task GetByPrefixAsync_With_Not_Existed_Prefix_Should_Return_Empty_Dict()
        {
            await Task.FromResult(1);
        }


        [Fact]
        protected override void Get_Count_Without_Prefix_Should_Succeed()
        {

        }

        [Fact]
        protected override void Get_Count_With_Prefix_Should_Succeed()
        {

        }

        private void SetCacheItem(string cacheKey, string cacheValue, string prefix)
        {
            var pre = _provider.Get<string>(prefix);

            cacheKey = string.Concat(pre, cacheKey);

            _provider.Set(cacheKey, cacheValue, _defaultTs);

            var val = _provider.Get<string>(cacheKey);
            Assert.Equal(cacheValue, val.Value);
        }

        private void GetCacheItem(string cacheKey, string prefix)
        {
            var pre = _provider.Get<string>(prefix);

            cacheKey = string.Concat(pre, cacheKey);

            var val = _provider.Get<string>(cacheKey);
            Assert.False(val.HasValue);
        }
    }

}
