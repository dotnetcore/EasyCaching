namespace EasyCaching.InMemory
{
    using EasyCaching.Core;
    using Microsoft.Extensions.DependencyInjection;
    using System;

    public static class InMemoryCacheServiceCollectionExtensions
    {
        /// <summary>
        /// Adds the default in memory cache.
        /// </summary>
        /// <returns>The default in memory cache.</returns>
        /// <param name="services">Services.</param>
        public static IServiceCollection AddDefaultInMemoryCache(this IServiceCollection services)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            services.Add(ServiceDescriptor.Singleton<IEasyCachingProvider, InMemoryCachingProvider>());

            return services;
        }
    }
}
