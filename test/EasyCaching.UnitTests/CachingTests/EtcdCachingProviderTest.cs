using System;
using System.Threading.Tasks;
using EasyCaching.Core;
using EasyCaching.Core.Configurations;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Xunit;
using Xunit.Abstractions;

namespace EasyCaching.UnitTests.CachingTests
{
    public class EtcdCachingProviderTest : BaseCachingProviderTest
    {
        private readonly ITestOutputHelper _testOutputHelper;

        public EtcdCachingProviderTest(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
            _defaultTs = TimeSpan.FromSeconds(30);
        }

        protected override IEasyCachingProvider CreateCachingProvider(Action<BaseProviderOptions> additionalSetup)
        {
            var services = new ServiceCollection();
            services.AddEasyCaching(x =>
                x.UseEtcd(options =>
                {
                    options.Address = "http://127.0.0.1:2379";
                    options.Timeout = 30000;
                    options.SerializerName = "json";
                    additionalSetup(options);
                }, "e1").WithJson(jsonSerializerSettingsConfigure: x =>
                {
                    x.TypeNameHandling = Newtonsoft.Json.TypeNameHandling.None;
                    x.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                }, "json"));

            IServiceProvider serviceProvider = services.BuildServiceProvider();
            return serviceProvider.GetService<IEasyCachingProvider>();
        }

        protected override Task GetAsync_Parallel_Should_Succeed()
        {
            return Task.FromResult(1);
        }

        protected override void Get_Parallel_Should_Succeed()
        {
        }

        protected override Task GetAllByPrefix_Async_Should_Throw_ArgumentNullException_When_Prefix_IsNullOrWhiteSpace(
            string preifx)
        {
            return Task.CompletedTask;
        }

        protected override void GetAllByPrefix_Should_Throw_ArgumentNullException_When_CacheKey_IsNullOrWhiteSpace(
            string prefix)
        {
        }

        protected override Task Get_Count_Async_With_Prefix_Should_Succeed()
        {
            return Task.CompletedTask;
        }

        protected override Task Get_Count_Async_Without_Prefix_Should_Succeed()
        {
            return Task.CompletedTask;
        }

        protected override void Get_Count_With_Prefix_Should_Succeed()
        {
        }

        protected override void Get_Count_Without_Prefix_Should_Succeed()
        {
        }

        protected override void GetByPrefix_Should_Succeed()
        {
        }

        protected override void GetByPrefix_With_Not_Existed_Prefix_Should_Return_Empty_Dict()
        {
        }

        protected override Task GetByPrefixAsync_Should_Succeed()
        {
            return Task.CompletedTask;
        }

        protected override Task GetByPrefixAsync_With_Not_Existed_Prefix_Should_Return_Empty_Dict()
        {
            return Task.CompletedTask;
        }

        protected override Task GetExpiration_Async_Should_Succeed()
        {
            return Task.CompletedTask;
        }

        protected override void GetExpiration_Should_Succeed()
        {
        }

        protected override void RemoveByPattern_Should_Succeed()
        {
        }

        protected override Task RemoveByPatternAsync_Should_Succeed()
        {
            return Task.CompletedTask;
        }

        protected override Task RemoveByPrefix_Async_Should_Throw_ArgumentNullException_When_Prefix_IsNullOrWhiteSpace(
            string preifx)

        {
            return Task.CompletedTask;
        }

        protected override void RemoveByPrefix_Should_Succeed()
        {
        }

        protected override void RemoveByPrefix_Should_Throw_ArgumentNullException_When_CacheKey_IsNullOrWhiteSpace(
            string prefix)
        {
        }

        protected override Task RemoveByPrefixAsync_Should_Succeed()
        {
            return Task.CompletedTask;
        }
    }
}
