namespace EasyCaching.UnitTests
{
    using EasyCaching.Memcached;
    using EasyCaching.Core;
    using Microsoft.Extensions.DependencyInjection;
    using System;
    using System.Threading.Tasks;
    using Xunit;
    using System.IO;
    using Microsoft.Extensions.Configuration;

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


    public class MemcachedProviderWithFactoryTest : BaseCachingProviderWithFactoryTest
    {
        public MemcachedProviderWithFactoryTest()
        {
            IServiceCollection services = new ServiceCollection();
            services.AddDefaultMemcachedWithFactory("MyTest",options =>
            {
                options.DBConfig.AddServer("127.0.0.1", 11212);
            });
            services.AddDefaultMemcachedWithFactory(SECOND_PROVIDER_NAME, options =>
            {
                options.DBConfig.AddServer("127.0.0.1", 11211);
            });
            services.AddLogging();
            IServiceProvider serviceProvider = services.BuildServiceProvider();

            var factory = serviceProvider.GetService<IEasyCachingProviderFactory>();
            _provider = factory.GetCachingProvider("MyTest");
            _secondProvider = factory.GetCachingProvider(SECOND_PROVIDER_NAME);
            _defaultTs = TimeSpan.FromSeconds(50);
        }

        [Fact]
        public void RemoveByPrefix_Should_Succeed()
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
        public async Task RemoveByPrefixAsync_Should_Succeed()
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


    public class MemcachedProviderUseEasyCachingTest : BaseUsingEasyCachingTest
    {
        public MemcachedProviderUseEasyCachingTest()
        {
            IServiceCollection services = new ServiceCollection();
            services.AddLogging();
            services.AddEasyCaching(option =>
            {
                option.UseMemcached(config =>
                {
                    config.DBConfig = new  EasyCachingMemcachedClientOptions
                    {
                        Servers = new System.Collections.Generic.List<Enyim.Caching.Configuration.Server> 
                        {
                            new Enyim.Caching.Configuration.Server(){ Address="127.0.0.1", Port=11212}
                        }
                    };
                }, EasyCachingConstValue.DefaultMemcachedName);              
            });

            IServiceProvider serviceProvider = services.BuildServiceProvider();
            var factory = serviceProvider.GetService<IEasyCachingProviderFactory>();
            _provider = factory.GetCachingProvider(EasyCachingConstValue.DefaultMemcachedName);

            _defaultTs = TimeSpan.FromSeconds(30);
        }
    }

    public class MemcachedProviderUseEasyCachingWithConfigTest : BaseUsingEasyCachingTest
    {
        public MemcachedProviderUseEasyCachingWithConfigTest()
        {
            IServiceCollection services = new ServiceCollection();

            var appsettings = @"
{
    'easycaching': {
        'memcached': {
            'MaxRdSecond': 600,
            'Order': 99,
            'dbconfig': {            
                'Servers': [
                    {
                        'Address': '127.0.0.1',
                        'Port': 11211
                    }
                ]
            } 
        }
    }
}";
            var path = TestHelpers.CreateTempFile(appsettings);
            var directory = Path.GetDirectoryName(path);
            var fileName = Path.GetFileName(path);

            var configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.SetBasePath(directory);
            configurationBuilder.AddJsonFile(fileName);
            var config = configurationBuilder.Build();
            services.AddLogging();
            services.AddEasyCaching(option =>
            {
                option.UseMemcached(config, "mName");
            });

            IServiceProvider serviceProvider = services.BuildServiceProvider();
            _provider = serviceProvider.GetService<IEasyCachingProvider>();                      
            _defaultTs = TimeSpan.FromSeconds(30);
        }

        [Fact]
        public void Provider_Information_Should_Be_Correct()
        {
            Assert.Equal(600, _provider.MaxRdSecond);
            Assert.Equal(99, _provider.Order);
            Assert.Equal("mName", _provider.Name);
        }
    }
}
