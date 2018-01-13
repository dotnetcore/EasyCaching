namespace EasyCaching.InMemory
{
    using EasyCaching.Core;
    using EasyCaching.Core.Internal;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;

    public static class InMemoryCacheServiceCollectionExtensions
    {
        /// <summary>
        /// Adds the default in memory cache.
        /// </summary>
        /// <returns>The default in memory cache.</returns>
        /// <param name="services">Services.</param>
        public static IServiceCollection AddDefaultInMemoryCache(this IServiceCollection services)
        {
            ArgumentCheck.NotNull(services, nameof(services));

            services.TryAddSingleton<IEasyCachingProvider, InMemoryCachingProvider>();

            return services;
        }
    }
}
