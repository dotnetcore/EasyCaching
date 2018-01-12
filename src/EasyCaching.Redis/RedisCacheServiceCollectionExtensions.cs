namespace EasyCaching.Redis
{
    using EasyCaching.Core;
    using EasyCaching.Core.Internal;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;
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
        /// <param name="optionsAction">Options Action.</param>
        public static IServiceCollection AddDefaultRedisCache(this IServiceCollection services, Action<RedisCacheOptions> optionsAction)
        {
            ArgumentCheck.NotNull(services, nameof(services));
            ArgumentCheck.NotNull(optionsAction, nameof(optionsAction));

            services.AddOptions();
            services.Configure(optionsAction);
            services.TryAddSingleton<IRedisDatabaseProvider, RedisDatabaseProvider>();
            services.TryAddSingleton<IEasyCachingProvider, DefaultRedisCachingProvider>();

            return services;
        }
    }
}
