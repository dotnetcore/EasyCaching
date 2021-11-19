namespace EasyCaching.UnitTests
{
    using Core;
    using Core.Bus;
    using Core.Configurations;
    using FakeItEasy;
    using InMemory;
    using Microsoft.Extensions.DependencyInjection;
    using System;

    public static class ServiceBuilders
    {
        public static IServiceProvider CreateServiceProvider(Action<IServiceCollection> configure)
        {
            var services = new ServiceCollection();
            configure(services);
            return services.BuildServiceProvider();
        }

        public static IServiceProvider CreateServiceProviderWithEasyCaching(Action<EasyCachingOptions> configure) =>
            CreateServiceProvider(services => services.AddEasyCaching(configure));

        public static TService CreateService<TService>(Action<IServiceCollection> configure) =>
            CreateServiceProvider(configure).GetService<TService>();
        
        public static IEasyCachingProvider CreateEasyCachingProvider(Action<EasyCachingOptions> configure) => 
            CreateService<IEasyCachingProvider>(services => services.AddEasyCaching(configure));
        
        public static IEasyCachingProvider CreateFakeProvider(Action<FakeProviderOptions> configure) => CreateService<IEasyCachingProvider>(
            services => services.AddEasyCaching(x => x.UseFakeProvider(configure)));

        public static T CreateFake<T>(Action<Fake<T>> configure) where T : class
        {
            var fake = new Fake<T>();
            configure(fake);
            return fake.FakedObject;
        }
        
        public static IEasyCachingBus CreateFakeBus(Action<FakeBusOptions> configure) => CreateService<IEasyCachingBus>(
            services => services.AddEasyCaching(x => x.WithFakeBus(configure)));
    }
}