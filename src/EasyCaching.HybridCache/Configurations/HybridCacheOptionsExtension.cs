namespace Microsoft.Extensions.DependencyInjection
{
    using System;
    using EasyCaching.Core;
    using EasyCaching.Core.Configurations;
    using EasyCaching.HybridCache;
    using Microsoft.Extensions.DependencyInjection.Extensions;

    /// <summary>
    /// HybridCache options extension.
    /// </summary>
    internal sealed class HybridCacheOptionsExtension : IEasyCachingOptionsExtension
    {
        /// <summary>
        /// The configure.
        /// </summary>
        private readonly Action<HybridCachingOptions> _configure;

        public HybridCacheOptionsExtension( Action<HybridCachingOptions> configure)
        {
            this._configure = configure;
        }

        /// <summary>
        /// Adds the services.
        /// </summary>
        /// <param name="services">Services.</param>
        public void AddServices(IServiceCollection services)
        {
            services.AddOptions();
            services.Configure(_configure);
            services.TryAddSingleton<IHybridCachingProvider, HybridCachingProvider>();
        }
    }
}
