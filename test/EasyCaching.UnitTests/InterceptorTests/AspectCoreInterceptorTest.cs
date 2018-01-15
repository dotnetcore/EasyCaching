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

        public AspectCoreInterceptorTest()
        {
            IServiceCollection services = new  ServiceCollection();
            services.AddTransient<IAspectCoreExampleService,AspectCoreExampleService>();            
            services.AddMemoryCache();
            services.AddDefaultInMemoryCache();
            IServiceProvider serviceProvider = services.ConfigureAspectCoreInterceptor();
            
            _cachingProvider = serviceProvider.GetService<IEasyCachingProvider>();
            _service = serviceProvider.GetService<IAspectCoreExampleService>();            
        }

        [Fact]
        public void Interceptor_Cached_Value_Should_Equal()
        {                     
            var tick1 = _service.GetCurrentUTC();            

            Thread.Sleep(500);

            var tick2 = _service.GetCurrentUTC();

            Assert.Equal(tick1, tick2);
        }

        [Fact]
        public void Interceptor_Cached_Value_Should_Not_Equal()
        {                     
            var tick1 = _service.GetCurrentUTC();            

            Thread.Sleep(1100);

            var tick2 = _service.GetCurrentUTC();

            Assert.NotEqual(tick1, tick2);
        }
    }
}
