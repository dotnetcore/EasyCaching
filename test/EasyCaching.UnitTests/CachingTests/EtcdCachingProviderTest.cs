using EasyCaching.Core;
using EasyCaching.Etcd;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using Xunit;

namespace EasyCaching.UnitTests.CachingTests
{
    public class EtcdCachingProviderTest //: BaseCachingProviderTest
    {
        private readonly string ProviderName = "EtcdTest";
        private readonly IEasyCachingProvider _provider;

        public EtcdCachingProviderTest()
        {
            // _defaultTs = TimeSpan.FromSeconds(30);
            var services = getServiceCollection();
            IServiceProvider serviceProvider = services.BuildServiceProvider();
            _provider = serviceProvider.GetService<IEasyCachingProvider>();
        }

        //protected override IEasyCachingProvider CreateCachingProvider(Action<BaseProviderOptions> additionalSetup)
        //{
        //    IServiceCollection services = getServiceCollection();
        //    IServiceProvider serviceProvider = services.BuildServiceProvider();
        //    return serviceProvider.GetService<IEasyCachingProvider>();
        //}

        private IServiceCollection getServiceCollection()
        {
            IServiceCollection services = new ServiceCollection();
            services.AddEasyCaching(option =>
                option.UseEtcd(options =>
                {
                    options.Address = "http://127.0.0.1:2379";
                    options.Timeout = 30000;
                    options.SerializerName = "json";
                }, ProviderName).WithJson(jsonSerializerSettingsConfigure: x =>
                {
                    x.TypeNameHandling = Newtonsoft.Json.TypeNameHandling.None;
                    x.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                }, "json"));
            return services;
        }


        [Fact]
        public void Set_And_Get_Should_Succeed()
        {
            _provider.Set<string>("abc", "123", TimeSpan.FromSeconds(60));
            var val = _provider.Get<string>("abc");
            Assert.Equal("123", val.Value);
        }

        [Fact]
        public async Task SetAsync_And_GetAsync_Should_Succeed()
        {
            await _provider.SetAsync<string>("abcd", "1234", TimeSpan.FromSeconds(60));
            var val = await _provider.GetAsync<string>("abcd");
            Assert.True(val.HasValue);
            Assert.Equal("1234", val.Value);
        }

        [Fact]
        public void Remove_Should_Succeed()
        {
            _provider.Set<string>("abcf", "123", TimeSpan.FromSeconds(60));
            _provider.Remove("abcf");
        }

        [Fact]
        public async Task RemoveAsync_Should_Succeed()
        {
            await _provider.SetAsync<string>("abcf", "123", TimeSpan.FromSeconds(60));
            await _provider.RemoveAsync("abcf");
        }


        [Fact]
        public void Use_Configuration_Options_Should_Succeed()
        {
            IServiceCollection services = getServiceCollection();
            IServiceProvider serviceProvider = services.BuildServiceProvider();
            var dbProvider = serviceProvider.GetService<IEtcdCaching>();
            Assert.NotNull(dbProvider);

            Assert.Equal(ProviderName, dbProvider.ProviderName);
        }

    }
}
