namespace EasyCaching.UnitTests
{
    using EasyCaching.Core.Internal;
    using EasyCaching.UnitTests.Infrastructure;
    using EasyCaching.Interceptor.Castle;
    using EasyCaching.InMemory;
    using System;
    using System.Threading;
    using Xunit;
    using Autofac;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Caching.Memory;
    using EasyCaching.Core;

    public class CastleInterceptorTest
    {
        private readonly IEasyCachingProvider _cachingProvider;

        private readonly ICastleExampleService _service;

        public CastleInterceptorTest()
        {
            IServiceCollection services = new ServiceCollection();
            services.AddTransient<ICastleExampleService, CastleExampleService>();
            services.AddMemoryCache();
            services.AddDefaultInMemoryCache();
            IServiceProvider serviceProvider = services.ConfigureCastleInterceptor();

            _cachingProvider = serviceProvider.GetService<IEasyCachingProvider>();
            _service = serviceProvider.GetService<ICastleExampleService>();
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
    }
}