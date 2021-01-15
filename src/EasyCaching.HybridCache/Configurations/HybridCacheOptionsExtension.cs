namespace Microsoft.Extensions.DependencyInjection
{
    using System;
    using EasyCaching.Core;
    using EasyCaching.Core.Bus;
    using EasyCaching.Core.Configurations;
    using EasyCaching.Core.Decoration;
    using EasyCaching.HybridCache;
    using Microsoft.Extensions.DependencyInjection.Extensions;

    /// <summary>
    /// HybridCache options extension.
    /// </summary>
    internal sealed class HybridCacheOptionsExtension : IEasyCachingOptionsExtension
    {
        /// <summary>
        /// The name.
        /// </summary>
        private readonly string _name;

        /// <summary>
        /// The configure.
        /// </summary>
        private readonly Action<HybridCachingOptions> _configure;

        public HybridCacheOptionsExtension(string name, Action<HybridCachingOptions> configure)
        {
            this._name = name;
            this._configure = configure;
        }

        /// <summary>
        /// Adds the services.
        /// </summary>
        /// <param name="services">Services.</param>
        public void AddServices(IServiceCollection services)
        {
            services.AddOptions();
            services.Configure(_name, _configure);

            services.TryAddSingleton<IHybridProviderFactory, DefaultHybridProviderFactory>();

            services.AddSingleton<IHybridCachingProvider>(serviceProvider =>
            {                
                var optionsMon = serviceProvider.GetRequiredService<Microsoft.Extensions.Options.IOptionsMonitor<HybridCachingOptions>>();
                var options = optionsMon.Get(_name);

                var providerFactory = serviceProvider.GetService<IEasyCachingProviderFactory>();
                var bus = serviceProvider.GetService<IEasyCachingBus>();                
                var loggerFactory = serviceProvider.GetService<Microsoft.Extensions.Logging.ILoggerFactory>();

                return options.CreateDecoratedProvider(
                    _name,
                    serviceProvider,
                    () => new HybridCachingProvider(_name, options, providerFactory, bus, loggerFactory));
            });
        }
    }
}
