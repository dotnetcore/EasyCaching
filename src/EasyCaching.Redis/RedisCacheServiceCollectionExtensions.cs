namespace EasyCaching.Redis
{
    using EasyCaching.Core;
    using Microsoft.Extensions.DependencyInjection;
    using System;

    /// <summary>
    /// Redis cache service collection extensions.
    /// </summary>
    public static class RedisCacheServiceCollectionExtensions
    {
        /// <summary>
        /// Adds the default redis cache.
        /// </summary>
        /// <returns>The default redis cache.</returns>
        /// <param name="services">Services.</param>
        /// <param name="options">Options.</param>
        public static IServiceCollection AddDefaultRedisCache(this IServiceCollection services, Action<RedisCacheOptions> options)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            services.AddOptions();
            services.Configure(options);
            services.Add(ServiceDescriptor.Singleton<IEasyCachingProvider, DefaultRedisCachingProvider>());

            return services;
        }
    }
}
