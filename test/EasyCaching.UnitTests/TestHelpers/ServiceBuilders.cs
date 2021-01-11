namespace EasyCaching.UnitTests
{
    using Core;
    using Core.Configurations;
    using FakeItEasy;
    using InMemory;
    using Microsoft.Extensions.DependencyInjection;
    using System;

    public static class ServiceBuilders
    {
        public static TService CreateService<TService>(Action<IServiceCollection> configure)
        {
            var services = new ServiceCollection();
            configure(services);
            var serviceProvider = services.BuildServiceProvider();
            return serviceProvider.GetService<TService>();
        }
        
        public static IEasyCachingProvider CreateEasyCachingProvider(Action<EasyCachingOptions> configure) => CreateService<IEasyCachingProvider>(
            services => services.AddEasyCaching(configure));
        
        public static IEasyCachingProvider CreateFakeProvider(Action<FakeOptions> configure) => CreateService<IEasyCachingProvider>(
            services => services.AddEasyCaching(x => x.UseFake(configure)));

        public static T CreateFake<T>(Action<Fake<T>> configure) where T : class
        {
            var fake = new Fake<T>();
            configure(fake);
            return fake.FakedObject;
        }
    }
}