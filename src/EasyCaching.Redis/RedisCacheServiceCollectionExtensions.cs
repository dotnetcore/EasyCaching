namespace EasyCaching.Redis
{
    using EasyCaching.Core;
    using EasyCaching.Core.Internal;
    using Microsoft.Extensions.Configuration;
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
        /// <param name="providerAction">Options Action.</param>       
        public static IServiceCollection AddDefaultRedisCache(
            this IServiceCollection services,
            Action<RedisOptions> providerAction)
        {
            ArgumentCheck.NotNull(services, nameof(services));

            services.AddOptions();
            services.Configure(providerAction);

            services.TryAddSingleton<IEasyCachingSerializer, DefaultBinaryFormatterSerializer>();
            services.TryAddSingleton<IRedisDatabaseProvider, RedisDatabaseProvider>();
            services.TryAddSingleton<IEasyCachingProvider, DefaultRedisCachingProvider>();

            return services;
        } 

        /// <summary>
        /// Adds the default redis cache.
        /// </summary>
        /// <example>
        /// <![CDATA[
        /// "easycaching": {
        ///     "CachingProviderType": 3,
        ///     "MaxRdSecond": 120,
        ///     "Order": 2,
        ///     "dbconfig": {            
        ///         "Password": null,
        ///         "IsSsl": false,
        ///         "SslHost": null,
        ///         "ConnectionTimeout": 5000,
        ///         "AllowAdmin": true,
        ///         "Endpoints": [
        ///             {
        ///                 "Host": "localhost",
        ///                 "Port": 6739
        ///             }
        ///         ],
        ///         "Database": 0
        ///     }
        /// }      
        /// ]]>
        /// </example>
        /// <returns>The default redis cache.</returns>
        /// <param name="services">Services.</param>
        /// <param name="configuration">Configuration.</param>
        public static IServiceCollection AddDefaultRedisCache(
           this IServiceCollection services,
            IConfiguration configuration)
        {
            var cacheConfig = configuration.GetSection(EasyCachingConstValue.ConfigSection);
            services.Configure<RedisOptions>(cacheConfig);

            //var redisConfig = configuration.GetSection(EasyCachingConstValue.ConfigChildSection);
            //services.Configure<RedisDBOptions>(redisConfig);

            services.TryAddSingleton<IEasyCachingSerializer, DefaultBinaryFormatterSerializer>();
            services.TryAddSingleton<IRedisDatabaseProvider, RedisDatabaseProvider>();
            services.TryAddSingleton<IEasyCachingProvider, DefaultRedisCachingProvider>();

            return services;
        }

    }
}
