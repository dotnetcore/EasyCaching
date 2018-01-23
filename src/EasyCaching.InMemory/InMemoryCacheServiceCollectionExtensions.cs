namespace EasyCaching.InMemory
{    
    using EasyCaching.Core;
    using EasyCaching.Core.Internal;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;

    /// <summary>
    /// In memory cache service collection extensions.
    /// </summary>
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

            services.AddMemoryCache();
            services.TryAddSingleton<IEasyCachingProvider, InMemoryCachingProvider>();

            return services;
        }

        /// <summary>
        /// Adds the default in memory cache for hybrid.
        /// </summary>
        /// <returns>The default in memory cache for hybrid.</returns>
        /// <param name="services">Services.</param>
        public static IServiceCollection AddDefaultInMemoryCacheForHybrid(this IServiceCollection services)
        {
            ArgumentCheck.NotNull(services, nameof(services));

            services.AddMemoryCache();
            services.TryAddSingleton<InMemoryCachingProvider>();

            //services.AddSingleton(factory =>
            //{
            //    Func<string, IEasyCachingProvider> accesor = key =>
            //    {
            //        if(key.Equals(HybridCachingKeyType.LocalKey))
            //        {
            //            return factory.GetService<InMemoryCachingProvider>();
            //        }     
            //        else
            //        {
            //            throw new KeyNotFoundException();
            //        }
            //    };
            //    return accesor;
            //});

            return services;
        }
    }
}
