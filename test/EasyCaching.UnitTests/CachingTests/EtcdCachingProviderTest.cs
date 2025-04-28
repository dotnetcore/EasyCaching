using EasyCaching.Core;
using EasyCaching.Core.Configurations;
using EasyCaching.Etcd;
using FakeItEasy;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Xml.Linq;
using Xunit;

namespace EasyCaching.UnitTests.CachingTests
{
    public class EtcdCachingProviderTest : BaseCachingProviderTest
    {
        private readonly string ProviderName = "EtcdTest";


        public EtcdCachingProviderTest()
        {
            _defaultTs = TimeSpan.FromSeconds(30);
        }

        protected override IEasyCachingProvider CreateCachingProvider(Action<BaseProviderOptions> additionalSetup)
        {
            IServiceCollection services = getServiceCollection();
            IServiceProvider serviceProvider = services.BuildServiceProvider();
            return serviceProvider.GetService<IEasyCachingProvider>();
        }

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
            Assert.True(val.HasValue);
            Assert.Equal("123", val.Value);
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

        [Fact]
        protected override void GetByPrefix_Should_Succeed()
        {

        }

    }
}
