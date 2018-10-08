namespace EasyCaching.UnitTests
{
    using EasyCaching.Core;
    using EasyCaching.InMemory;
    using Microsoft.Extensions.DependencyInjection;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
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
        public void Deault_MaxRdSecond_Should_Be_120()
        {
            Assert.Equal(120, _provider.MaxRdSecond);
        }
    }

    public class MemoryCachingProviderWithFactoryTest : BaseCachingProviderTest
    {
        private readonly IEasyCachingProvider _secondProvider;

        private const string SECOND_PROVIDER_NAME = "second";

        public MemoryCachingProviderWithFactoryTest()
        {
            IServiceCollection services = new ServiceCollection();
            services.AddDefaultInMemoryCacheWithFactory();
            services.AddDefaultInMemoryCacheWithFactory(SECOND_PROVIDER_NAME);
            IServiceProvider serviceProvider = services.BuildServiceProvider();
            var factory = serviceProvider.GetService<IEasyCachingProviderFactory>();
            _provider = factory.GetCachingProvider(EasyCachingConstValue.DefaultInMemoryName);
            _secondProvider = factory.GetCachingProvider(SECOND_PROVIDER_NAME);
            _defaultTs = TimeSpan.FromSeconds(30);
        }


        [Fact]
        public void Multi_Instance_Set_And_Get_Should_Succeed()
        {
            var cacheKey1 = "named-provider-1";
            var cacheKey2 = "named-provider-2";

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
        protected override void GetByPrefix_Should_Succeed()
        {
            _provider.RemoveAll(new List<string> { "getbyprefix:key:1", "getbyprefix:key:2" });
            var dict = GetMultiDict("getbyprefix:");

            _provider.SetAll(dict, _defaultTs);

            string prefix = "getbyprefix:key:";

            var res = _provider.GetByPrefix<string>(prefix);

            Assert.Equal(2, res.Count);
            Assert.Contains($"{EasyCachingConstValue.DefaultInMemoryName}-getbyprefix:key:1", res.Select(x => x.Key));
            Assert.Contains($"{EasyCachingConstValue.DefaultInMemoryName}-getbyprefix:key:2", res.Select(x => x.Key));
            Assert.Equal("value1", res.Where(x => x.Key == $"{EasyCachingConstValue.DefaultInMemoryName}-getbyprefix:key:1").Select(x => x.Value).FirstOrDefault().Value);
            Assert.Equal("value2", res.Where(x => x.Key == $"{EasyCachingConstValue.DefaultInMemoryName}-getbyprefix:key:2").Select(x => x.Value).FirstOrDefault().Value);
        }

        [Fact]
        protected override async Task GetByPrefixAsync_Should_Succeed()
        {
            _provider.RemoveAll(new List<string> { "getbyprefixasync:key:1", "getbyprefixasync:key:2" });
            var dict = GetMultiDict("getbyprefixasync:");

            _provider.SetAll(dict, _defaultTs);

            string prefix = "getbyprefixasync:key:";

            var res = await _provider.GetByPrefixAsync<string>(prefix);

            Assert.Equal(2, res.Count);
            Assert.Contains($"{EasyCachingConstValue.DefaultInMemoryName}-getbyprefixasync:key:1", res.Select(x => x.Key));
            Assert.Contains($"{EasyCachingConstValue.DefaultInMemoryName}-getbyprefixasync:key:2", res.Select(x => x.Key));
            Assert.Equal("value1", res.Where(x => x.Key == $"{EasyCachingConstValue.DefaultInMemoryName}-getbyprefixasync:key:1").Select(x => x.Value).FirstOrDefault().Value);
            Assert.Equal("value2", res.Where(x => x.Key == $"{EasyCachingConstValue.DefaultInMemoryName}-getbyprefixasync:key:2").Select(x => x.Value).FirstOrDefault().Value);
        }
    }
}
