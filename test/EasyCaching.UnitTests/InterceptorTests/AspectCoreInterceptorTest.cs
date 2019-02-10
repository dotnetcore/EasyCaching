namespace EasyCaching.UnitTests
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using EasyCaching.Core;
    using EasyCaching.Core.Interceptor;
    using EasyCaching.InMemory;
    using EasyCaching.Interceptor.AspectCore;
    using EasyCaching.UnitTests.Infrastructure;
    using Microsoft.Extensions.DependencyInjection;
    using Xunit;

    public abstract class BaseAspectCoreInterceptorTest
    {
        protected IEasyCachingProvider _cachingProvider;

        protected IAspectCoreExampleService _service;

        protected IEasyCachingKeyGenerator _keyGenerator;

        [Fact(Skip="some reason")]        
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

            System.Reflection.MethodInfo method = typeof(AspectCoreExampleService).GetMethod("PutTest");

            var key = _keyGenerator.GetCacheKey(method, new object[] { 1, "123" }, "AspectCoreExample");

            var value = _cachingProvider.Get<string>(key);

            Assert.True(value.HasValue);
            Assert.Equal("PutTest-1", value.Value);
        }

        [Fact]
        protected virtual void Evict_Should_Succeed()
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
        protected virtual void EvictAll_Should_Succeed()
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

            System.Reflection.MethodInfo method = typeof(AspectCoreExampleService).GetMethod("PutTestAsync");

            var key = _keyGenerator.GetCacheKey(method, new object[] { 1, "123" }, "AspectCoreExample");

            var value = _cachingProvider.Get<string>(key);

            Assert.True(value.HasValue);
            Assert.Equal(str, value.Value);
        }

        [Fact]
        protected virtual async Task Interceptor_Evict_With_Task_Method_Should_Succeed()
        {
            System.Reflection.MethodInfo method = typeof(AspectCoreExampleService).GetMethod("EvictTest");

            var key = _keyGenerator.GetCacheKey(method, null, "AspectCoreExample");

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
        protected virtual async Task Issues74_Interceptor_Able_IEnumerable_Test()
        {
            var list1 = await _service.AbleIEnumerableTest();

            Thread.Sleep(1);

            var list2 = await _service.AbleIEnumerableTest();

            Assert.Equal(list1.First().Prop, list2.First().Prop);
        }
    }

    public class AspectCoreInterceptorTest : BaseAspectCoreInterceptorTest
    {
        public AspectCoreInterceptorTest()
        {
            IServiceCollection services = new ServiceCollection();
            services.AddTransient<IAspectCoreExampleService, AspectCoreExampleService>();
            services.AddDefaultInMemoryCache(x =>
            {
                x.MaxRdSecond = 0;
            });
            IServiceProvider serviceProvider = services.ConfigureAspectCoreInterceptor();

            _cachingProvider = serviceProvider.GetService<IEasyCachingProvider>();
            _service = serviceProvider.GetService<IAspectCoreExampleService>();
            _keyGenerator = serviceProvider.GetService<IEasyCachingKeyGenerator>();
        }
    }

    //public class AspectCoreInterceptorWithActionTest : BaseAspectCoreInterceptorTest
    //{
    //    private ITestInterface _interface;

    //    public AspectCoreInterceptorWithActionTest()
    //    {
    //        IServiceCollection services = new ServiceCollection();
    //        services.AddTransient<IAspectCoreExampleService, AspectCoreExampleService>();
    //        services.AddDefaultInMemoryCache();

    //        Action<IServiceContainer> action = x =>
    //        {
    //            x.AddType<ITestInterface, TestInterface>();
    //        };

    //        IServiceProvider serviceProvider = services.ConfigureAspectCoreInterceptor(action);

    //        _cachingProvider = serviceProvider.GetService<IEasyCachingProvider>();
    //        _service = serviceProvider.GetService<IAspectCoreExampleService>();
    //        _keyGenerator = serviceProvider.GetService<IEasyCachingKeyGenerator>();

    //        _interface = serviceProvider.GetService<ITestInterface>();
    //    }

    //    [Fact]
    //    public void Add_Other_Types_Should_Succeed()
    //    {
    //        Assert.IsType<TestInterface>(_interface);
    //    }
    //}

    //public class AspectCoreInterceptorWithActionAndIsRemoveDefaultTest : BaseAspectCoreInterceptorTest
    //{
    //    private ITestInterface _interface;

    //    public AspectCoreInterceptorWithActionAndIsRemoveDefaultTest()
    //    {
    //        IServiceCollection services = new ServiceCollection();
    //        services.AddTransient<IAspectCoreExampleService, AspectCoreExampleService>();
    //        services.AddDefaultInMemoryCache();

    //        Action<IServiceContainer> action = x =>
    //        {
    //            x.AddType<ITestInterface, TestInterface>();
    //            x.Configure(config =>
    //            {
    //                config.Interceptors.AddTyped<EasyCachingInterceptor>(method => typeof(Core.Internal.IEasyCaching).IsAssignableFrom(method.DeclaringType));
    //            });
    //        };

    //        IServiceProvider serviceProvider = services.ConfigureAspectCoreInterceptor(action, true);

    //        _cachingProvider = serviceProvider.GetService<IEasyCachingProvider>();
    //        _service = serviceProvider.GetService<IAspectCoreExampleService>();
    //        _keyGenerator = serviceProvider.GetService<IEasyCachingKeyGenerator>();

    //        _interface = serviceProvider.GetService<ITestInterface>();
    //    }

    //    [Fact]
    //    public void Add_Other_Types_Should_Succeed()
    //    {
    //        Assert.IsType<TestInterface>(_interface);
    //    }
    //}
}
