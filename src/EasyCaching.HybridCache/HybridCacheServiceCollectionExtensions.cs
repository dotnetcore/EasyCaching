namespace EasyCaching.HybridCache
{
    using EasyCaching.Core;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;

    /// <summary>
    /// Hybrid cache service collection extensions.
    /// </summary>
    public static class HybridCacheServiceCollectionExtensions
    {
        /// <summary>
        /// Adds the default hybrid cache.
        /// </summary>
        /// <returns>The default hybrid cache.</returns>
        /// <param name="services">Services.</param>
        public static IServiceCollection AddDefaultHybridCache(this IServiceCollection services)
        {
            ArgumentCheck.NotNull(services, nameof(services));

            services.TryAddSingleton<IHybridCachingProvider, HybridCachingProvider>();

            return services;
        }
    }
}
