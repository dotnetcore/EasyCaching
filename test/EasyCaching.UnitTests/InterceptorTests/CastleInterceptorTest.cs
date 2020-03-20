namespace EasyCaching.UnitTests
{
    using Autofac;
    using Autofac.Extensions.DependencyInjection;
    using EasyCaching.Core;
    using EasyCaching.Core.Interceptor;
    using EasyCaching.Interceptor.Castle;
    using EasyCaching.UnitTests.Infrastructure;
    using Microsoft.Extensions.DependencyInjection;
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Xunit;

    public abstract class BaseCastleInterceptorTest
    {
        protected IEasyCachingProvider _cachingProvider;

        protected IEasyCachingProvider _secondCachingProvider;

        protected ICastleExampleService _service;

        protected IEasyCachingKeyGenerator _keyGenerator;

        [Fact]
        protected virtual void Interceptor_Attribute_Method_Should_Handle_Caching()
        {
            var tick1 = _service.GetCurrentUTC();

            Thread.Sleep(1);

            var tick2 = _service.GetCurrentUTC();

            Assert.Equal(tick1, tick2);
        }

        [Fact]
        protected virtual void Interceptor_Attribute_Method_Should_Handle_Caching_Twice()
        {
            var tick1 = _service.GetCurrentUTC();

            Thread.Sleep(1100);

            var tick2 = _service.GetCurrentUTC();

            Assert.NotEqual(tick1, tick2);
        }


        [Fact]
        protected virtual void Not_Interceptor_Attribute_Method_Should_Not_Handle_Caching()
        {
            var tick1 = _service.GetCurrentUTCTick();

            Thread.Sleep(1);

            var tick2 = _service.GetCurrentUTCTick();

            Assert.NotEqual(tick1, tick2);
        }


        [Fact]
        protected virtual void Put_Should_Succeed()
        {
            var str = _service.PutTest(1);

            System.Reflection.MethodInfo method = typeof(CastleExampleService).GetMethod("PutTest");

            var key = _keyGenerator.GetCacheKey(method, new object[] { 1, "123" }, "CastleExample");

            var value = _cachingProvider.Get<string>(key);

            Assert.True(value.HasValue);
            Assert.Equal("PutTest-1", value.Value);
        }

        [Fact]
        protected virtual void Put_Switch_Provider_Should_Succeed()
        {
            var str = _service.PutSwitchProviderTest(1);

            System.Reflection.MethodInfo method = typeof(CastleExampleService).GetMethod("PutTest");

            var key = _keyGenerator.GetCacheKey(method, new object[] { 1, "123" }, "CastleExample");

            var value = _cachingProvider.Get<string>(key);

            Assert.False(value.HasValue);

            value = _secondCachingProvider.Get<string>(key);

            Assert.True(value.HasValue);
        }

        [Fact]
        protected virtual void Evict_Should_Succeed()
        {
            System.Reflection.MethodInfo method = typeof(CastleExampleService).GetMethod("EvictTest");

            var key = _keyGenerator.GetCacheKey(method, null, "CastleExample");

            _cachingProvider.Set(key, "AAA", TimeSpan.FromSeconds(30));

            var value = _cachingProvider.Get<string>(key);

            Assert.Equal("AAA", value.Value);


            _service.EvictTest();

            var after = _cachingProvider.Get<string>(key);

            Assert.False(after.HasValue);
        }

        [Fact]
        protected virtual void EvictAll_Should_Succeed()
        {
            System.Reflection.MethodInfo method = typeof(CastleExampleService).GetMethod("EvictAllTest");

            var prefix = _keyGenerator.GetCacheKeyPrefix(method, "CastleExample");

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

        [Fact]
        protected virtual async Task Interceptor_Able_Attribute_Task_Method_Should_Succeed()
        {
            var tick1 = await _service.AbleTestAsync();

            Thread.Sleep(1);

            var tick2 = await _service.AbleTestAsync();

            Assert.Equal(tick1, tick2);
        }

        [Fact]
        protected virtual async Task Interceptor_Put_With_Task_Method_Should_Succeed()
        {
            var str = await _service.PutTestAsync(1);

            System.Reflection.MethodInfo method = typeof(CastleExampleService).GetMethod("PutTestAsync");

            var key = _keyGenerator.GetCacheKey(method, new object[] { 1, "123" }, "CastleExample");

            var value = _cachingProvider.Get<string>(key);

            Assert.True(value.HasValue);
            Assert.Equal(str, value.Value);
        }

        [Fact]
        protected virtual async Task Interceptor_Evict_With_Task_Method_Should_Succeed()
        {
            System.Reflection.MethodInfo method = typeof(CastleExampleService).GetMethod("EvictTest");

            var key = _keyGenerator.GetCacheKey(method, null, "CastleExample");

            var cachedValue = Guid.NewGuid().ToString();

            _cachingProvider.Set(key, cachedValue, TimeSpan.FromSeconds(30));

            var value = _cachingProvider.Get<string>(key);

            Assert.True(value.HasValue);
            Assert.Equal(cachedValue, value.Value);

            await _service.EvictTestAsync();

            var after = _cachingProvider.Get<string>(key);

            Assert.False(after.HasValue);
        }

        [Fact]
        protected virtual async Task Issues106_Interceptor_Able_Null_Value_Test()
        {
            var tick1 = await _service.AbleTestWithNullValueAsync();

            var tick2 = await _service.AbleTestWithNullValueAsync();

            Assert.Null(tick1);
            Assert.Equal(tick1, tick2);
        }

        [Fact]
        protected virtual void Interceptor_Should_Recognize_Subclass_Of_EasyCachingAble_Attribute()
        {
            var tick1 = _service.CustomAbleAttributeTest();

            Thread.Sleep(1);

            var tick2 = _service.CustomAbleAttributeTest();

            Assert.Equal(tick1, tick2);

            Thread.Sleep(1100);

            var tick3 = _service.CustomAbleAttributeTest();

            Assert.NotEqual(tick3, tick1);
        }

        [Fact]
        protected virtual void Interceptor_Should_Recognize_Subclass_Of_EasyCachingPut_Attribute()
        {
            var str = _service.CustomPutAttributeTest(1);

            var method = typeof(AspectCoreExampleService).GetMethod("CustomPutAttributeTest");

            var key = _keyGenerator.GetCacheKey(method, new object[] { 1 }, "Custom");

            var value = _cachingProvider.Get<string>(key);

            Assert.True(value.HasValue);
            Assert.Equal("CustomPutTest-1", value.Value);
        }

        [Fact]
        protected virtual void Interceptor_Should_Recognize_Subclass_Of_EasyCachingEvict_Attribute()
        {
            var method = typeof(AspectCoreExampleService).GetMethod("CustomEvictAttributeTest");

            var key = _keyGenerator.GetCacheKey(method, null, "Custom");

            var cachedValue = Guid.NewGuid().ToString();

            _cachingProvider.Set(key, cachedValue, TimeSpan.FromSeconds(30));

            var value = _cachingProvider.Get<string>(key);

            Assert.True(value.HasValue);
            Assert.Equal(cachedValue, value.Value);

            _service.CustomEvictAttributeTest();

            var after = _cachingProvider.Get<string>(key);

            Assert.False(after.HasValue);
        }
    }

    public class CastleInterceptorTest : BaseCastleInterceptorTest
    {
        public CastleInterceptorTest()
        {
            const string firstCacheProviderName = "first";
            const string secondCacheProviderName = "second";
            IServiceCollection services = new ServiceCollection();
            services.AddTransient<ICastleExampleService, CastleExampleService>();
            services.AddEasyCaching(x =>
            {
                x.UseInMemory(options => options.MaxRdSecond = 0, firstCacheProviderName);
                x.UseInMemory(options => options.MaxRdSecond = 0, secondCacheProviderName);
            });
            services.AddLogging();
            services.ConfigureCastleInterceptor(options => options.CacheProviderName = firstCacheProviderName);

            var builder = new ContainerBuilder();
            builder.Populate(services);
            builder.ConfigureCastleInterceptorForTest();

            IServiceProvider serviceProvider = new AutofacServiceProvider(builder.Build());

            var factory = serviceProvider.GetService<IEasyCachingProviderFactory>();
            _cachingProvider = factory.GetCachingProvider(firstCacheProviderName);
            _secondCachingProvider = factory.GetCachingProvider(secondCacheProviderName);
            _service = serviceProvider.GetService<ICastleExampleService>();
            _keyGenerator = serviceProvider.GetService<IEasyCachingKeyGenerator>();
        }
    }
}
