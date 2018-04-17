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
        /// <param name="dbAction">Options Action.</param>
        public static IServiceCollection AddDefaultRedisCache(
            this IServiceCollection services, 
            Action<RedisDBOptions> dbAction)
        {            
            var options = new RedisOptions();

            return services.AddDefaultRedisCache(dbAction, x =>
             {
                 x.CachingProviderType = options.CachingProviderType;
                 x.MaxRdSecond = options.MaxRdSecond;
                 x.Order = options.Order;
             });
        }

        /// <summary>
        /// Adds the default redis cache.
        /// </summary>
        /// <returns>The default redis cache.</returns>
        /// <param name="services">Services.</param>
        /// <param name="dbAction">Db action.</param>
        /// <param name="providerAction">Provider action.</param>
        public static IServiceCollection AddDefaultRedisCache(
            this IServiceCollection services, 
            Action<RedisDBOptions> dbAction, 
            Action<RedisOptions> providerAction)
        {
            ArgumentCheck.NotNull(services, nameof(services));
            ArgumentCheck.NotNull(dbAction, nameof(dbAction));
            ArgumentCheck.NotNull(providerAction, nameof(providerAction));

            services.AddOptions();
            services.Configure(dbAction);

            var providerOption = new RedisOptions();
            providerAction(providerOption);
            services.AddSingleton(providerOption);

            services.TryAddSingleton<IEasyCachingSerializer, DefaultBinaryFormatterSerializer>();
            services.TryAddSingleton<IRedisDatabaseProvider, RedisDatabaseProvider>();
            services.TryAddSingleton<IEasyCachingProvider, DefaultRedisCachingProvider>();

            return services;
        }               
    }
}
