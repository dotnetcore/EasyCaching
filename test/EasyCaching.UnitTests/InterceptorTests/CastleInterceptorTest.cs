namespace EasyCaching.UnitTests
{
    using EasyCaching.Core.Internal;
    using EasyCaching.UnitTests.Infrastructure;
    using EasyCaching.Interceptor.Castle;
    using EasyCaching.InMemory;
    using System;
    using System.Threading;
    using Xunit;
    using Microsoft.Extensions.DependencyInjection;
    using EasyCaching.Core;
    using Autofac;
    using System.Reflection;
    using Autofac.Extras.DynamicProxy;
    using System.Threading.Tasks;

    public abstract class BaseCastleInterceptorTest
    {
        protected IEasyCachingProvider _cachingProvider;

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
            System.Reflection.MethodInfo method = typeof(AspectCoreExampleService).GetMethod("EvictAllTest");

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

            System.Console.WriteLine($"test:{key}");

            var value = _cachingProvider.Get<Task<string>>(key);

            Assert.True(value.HasValue);
            Assert.Equal(str, value.Value.Result);
        }

        [Fact]
        protected virtual async Task Interceptor_Evict_With_Task_Method_Should_Succeed()
        {
            System.Reflection.MethodInfo method = typeof(AspectCoreExampleService).GetMethod("EvictTest");

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

    }

    public class CastleInterceptorTest : BaseCastleInterceptorTest
    {
        public CastleInterceptorTest()
        {
            IServiceCollection services = new ServiceCollection();
            services.AddTransient<ICastleExampleService, CastleExampleService>();
            services.AddDefaultInMemoryCache();
            IServiceProvider serviceProvider = services.ConfigureCastleInterceptor();

            _cachingProvider = serviceProvider.GetService<IEasyCachingProvider>();
            _service = serviceProvider.GetService<ICastleExampleService>();
            _keyGenerator = serviceProvider.GetService<IEasyCachingKeyGenerator>();
        }
    }

    public class CastleInterceptorWithActionTest : BaseCastleInterceptorTest
    {
        private ITestInterface _interface;

        public CastleInterceptorWithActionTest()
        {
            IServiceCollection services = new ServiceCollection();
            services.AddTransient<IAspectCoreExampleService, AspectCoreExampleService>();
            services.AddDefaultInMemoryCache();

            Action<ContainerBuilder> action = x =>
            {
                x.RegisterType<TestInterface>().As<ITestInterface>();
            };

            IServiceProvider serviceProvider = services.ConfigureCastleInterceptor(action);

            _cachingProvider = serviceProvider.GetService<IEasyCachingProvider>();
            _service = serviceProvider.GetService<ICastleExampleService>();
            _keyGenerator = serviceProvider.GetService<IEasyCachingKeyGenerator>();

            _interface = serviceProvider.GetService<ITestInterface>();
        }

        [Fact]
        public void Add_Other_Types_Should_Succeed()
        {
            Assert.IsType<TestInterface>(_interface);
        }
    }

    public class CastleInterceptorWithActionAndIsRemoveDefaultTest : BaseCastleInterceptorTest
    {
        private ITestInterface _interface;

        public CastleInterceptorWithActionAndIsRemoveDefaultTest()
        {
            IServiceCollection services = new ServiceCollection();
            services.AddTransient<IAspectCoreExampleService, AspectCoreExampleService>();
            services.AddDefaultInMemoryCache();

            Action<ContainerBuilder> action = x =>
            {
                x.RegisterType<TestInterface>().As<ITestInterface>();

                var assembly = Assembly.GetExecutingAssembly();
                x.RegisterType<EasyCachingInterceptor>();

                x.RegisterAssemblyTypes(assembly)
                    .Where(type => typeof(IEasyCaching).IsAssignableFrom(type) && !type.GetTypeInfo().IsAbstract)
                    .AsImplementedInterfaces()
                    .InstancePerLifetimeScope()
                    .EnableInterfaceInterceptors()
                    .InterceptedBy(typeof(EasyCachingInterceptor));
            };

            IServiceProvider serviceProvider = services.ConfigureCastleInterceptor(action, true);

            _cachingProvider = serviceProvider.GetService<IEasyCachingProvider>();
            _service = serviceProvider.GetService<ICastleExampleService>();
            _keyGenerator = serviceProvider.GetService<IEasyCachingKeyGenerator>();

            _interface = serviceProvider.GetService<ITestInterface>();
        }

        [Fact]
        public void Add_Other_Types_Should_Succeed()
        {
            Assert.IsType<TestInterface>(_interface);
        }
    }

}
