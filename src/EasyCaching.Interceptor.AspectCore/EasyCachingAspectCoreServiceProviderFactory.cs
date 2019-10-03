namespace Microsoft.Extensions.Hosting
{
    using global::AspectCore.Extensions.DependencyInjection;
    using global::AspectCore.Injector;
    using Microsoft.Extensions.DependencyInjection;
    using System;

    public class EasyCachingAspectCoreServiceProviderFactory : IServiceProviderFactory<IServiceContainer>
    {
        private readonly Action<IServiceContainer> _configurationAction;

        public EasyCachingAspectCoreServiceProviderFactory(Action<IServiceContainer> configurationAction = null)
        {
            _configurationAction = configurationAction ?? (builder => { });
        }

        public IServiceContainer CreateBuilder(IServiceCollection services)
        {
            var container = services.ToServiceContainer();

            _configurationAction(container);

            return container;
        }

        public IServiceProvider CreateServiceProvider(IServiceContainer containerBuilder)
        {
            if (containerBuilder == null) throw new ArgumentNullException(nameof(containerBuilder));

            var container = containerBuilder.Build();

            return container;
        }
    }
}
