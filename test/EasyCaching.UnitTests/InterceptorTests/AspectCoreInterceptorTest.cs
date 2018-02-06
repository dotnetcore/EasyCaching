namespace EasyCaching.UnitTests
{
    using EasyCaching.Core;
    using EasyCaching.InMemory;
    using EasyCaching.Interceptor.AspectCore;
    using EasyCaching.UnitTests.Infrastructure;
    using Microsoft.Extensions.DependencyInjection;
    using System;
    using System.Threading;
    using Xunit;

    public class AspectCoreInterceptorTest
    {
        private readonly IEasyCachingProvider _cachingProvider;

        private readonly IAspectCoreExampleService _service;

        private readonly IEasyCachingKeyGenerator _keyGenerator;

        public AspectCoreInterceptorTest()
        {
            IServiceCollection services = new ServiceCollection();
            services.AddTransient<IAspectCoreExampleService, AspectCoreExampleService>();
            services.AddDefaultInMemoryCache();
            IServiceProvider serviceProvider = services.ConfigureAspectCoreInterceptor();

            _cachingProvider = serviceProvider.GetService<IEasyCachingProvider>();
            _service = serviceProvider.GetService<IAspectCoreExampleService>();
            _keyGenerator = serviceProvider.GetService<IEasyCachingKeyGenerator>();
        }

        [Fact]
        public void Interceptor_Attribute_Method_Should_Handle_Caching()
        {
            var tick1 = _service.GetCurrentUTC();

            Thread.Sleep(1);

            var tick2 = _service.GetCurrentUTC();

            Assert.Equal(tick1, tick2);
        }

        [Fact]
        public void Interceptor_Attribute_Method_Should_Handle_Caching_Twice()
        {
            var tick1 = _service.GetCurrentUTC();

            Thread.Sleep(1100);

            var tick2 = _service.GetCurrentUTC();

            Assert.NotEqual(tick1, tick2);
        }


        [Fact]
        public void Not_Interceptor_Attribute_Method_Should_Not_Handle_Caching()
        {
            var tick1 = _service.GetCurrentUTCTick();

            Thread.Sleep(1);

            var tick2 = _service.GetCurrentUTCTick();

            Assert.NotEqual(tick1, tick2);
        }

        [Fact]
        public void Put_Should_Succeed()
        {
            var str = _service.PutTest(1);

            System.Reflection.MethodInfo method = typeof(AspectCoreExampleService).GetMethod("PutTest");

            var key = _keyGenerator.GetCacheKey(method, new object[] { 1, "123" }, "AspectCoreExample");

            var value = _cachingProvider.Get<string>(key);

            Assert.True(value.HasValue);
            Assert.Equal("PutTest-1", value.Value);
        }

        [Fact]
        public void Evict_Should_Succeed()
        {
            System.Reflection.MethodInfo method = typeof(AspectCoreExampleService).GetMethod("EvictTest");

            var key = _keyGenerator.GetCacheKey(method, null, "AspectCoreExample");

            _cachingProvider.Set(key, "AAA", TimeSpan.FromSeconds(30));

            var value = _cachingProvider.Get<string>(key);

            Assert.Equal("AAA", value.Value);


            _service.EvictTest();

            var after = _cachingProvider.Get<string>(key);

            Assert.False(after.HasValue);
        }

        [Fact]
        public void EvictAll_Should_Succeed()
        {
            System.Reflection.MethodInfo method = typeof(AspectCoreExampleService).GetMethod("EvictAllTest");

            var prefix = _keyGenerator.GetCacheKeyPrefix(method, "AspectCoreExample");

            _cachingProvider.Set(string.Concat(prefix, "1"), "AAA", TimeSpan.FromSeconds(30));
            _cachingProvider.Set(string.Concat(prefix, "2"), "AAA", TimeSpan.FromSeconds(30));

            var value1 = _cachingProvider.Get<string>(string.Concat(prefix, "1"));
            var value2 = _cachingProvider.Get<string>(string.Concat(prefix, "2"));

            Assert.Equal("AAA", value1.Value);
            Assert.Equal("AAA", value2.Value);

            _service.EvictAllTest();

            var after1 = _cachingProvider.Get<string>(string.Concat(prefix, "1"));
            var after2 = _cachingProvider.Get<string>(string.Concat(prefix, "2"));

            Assert.False(after1.HasValue);
            Assert.False(after2.HasValue);
        }

    }
}