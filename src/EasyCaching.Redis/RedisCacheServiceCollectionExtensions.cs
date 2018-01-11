namespace EasyCaching.Redis
{
    using EasyCaching.Core;
    using EasyCaching.Core.Internal;
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
            ArgumentCheck.NotNull(services, nameof(services));

            ArgumentCheck.NotNull(options, nameof(options));

            services.AddOptions();
            services.Configure(options);
            services.Add(ServiceDescriptor.Singleton<IEasyCachingProvider, DefaultRedisCachingProvider>());

            return services;
        }
    }
}
