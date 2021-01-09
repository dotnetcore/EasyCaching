namespace EasyCaching.UnitTests
{
    using Core;
    using Microsoft.Extensions.DependencyInjection;
    using System;

    public static class CachingProviderBuilders
    {
        public static IEasyCachingProvider CreateProvider(Action<IServiceCollection> configure)
        {
            IServiceCollection services = new ServiceCollection();
            configure(services);
            var serviceProvider = services.BuildServiceProvider();
            return serviceProvider.GetService<IEasyCachingProvider>();
        }
        
        public static IEasyCachingProvider CreateFakeProvider(Action<FakeOptions> configure)
        {
            var services = new ServiceCollection();
            services.AddEasyCaching(x => x
                .UseFake(configure)
            );
            var serviceProvider = services.BuildServiceProvider();
            return serviceProvider.GetService<IEasyCachingProvider>();
        }
    }
}