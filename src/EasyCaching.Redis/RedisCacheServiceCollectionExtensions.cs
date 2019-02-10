namespace EasyCaching.Redis
{
    using System;
    using EasyCaching.Core;
    using EasyCaching.Core.Serialization;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;

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
            services.AddSingleton<IEasyCachingProvider, DefaultRedisCachingProvider>();

            return services;
        }

        /// <summary>
        /// Adds the default redis cache.
        /// </summary>
        /// <example>
        /// <![CDATA[
        /// "easycaching": {
        ///     "redis":{
        ///         "CachingProviderType": 2,
        ///         "MaxRdSecond": 120,
        ///         "Order": 2,
        ///         "dbconfig": {            
        ///             "Password": null,
        ///             "IsSsl": false,
        ///             "SslHost": null,
        ///             "ConnectionTimeout": 5000,
        ///             "AllowAdmin": true,
        ///             "Endpoints": [
        ///                 {
        ///                     "Host": "localhost",
        ///                     "Port": 6739
        ///                 }
        ///             ],
        ///             "Database": 0,
        ///             "Configuration":"localhost:6379,allowAdmin=false"
        ///         }
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
            ArgumentCheck.NotNull(services, nameof(services));

            var cacheConfig = configuration.GetSection(EasyCachingConstValue.RedisSection);
            services.Configure<RedisOptions>(cacheConfig);

            services.TryAddSingleton<IEasyCachingSerializer, DefaultBinaryFormatterSerializer>();
            services.TryAddSingleton<IRedisDatabaseProvider, RedisDatabaseProvider>();
            services.AddSingleton<IEasyCachingProvider, DefaultRedisCachingProvider>();

            return services;
        }

        /// <summary>
        /// Adds the default redis cache with factory.
        /// </summary>
        /// <returns>The default redis cache with factory.</returns>
        /// <param name="services">Services.</param>
        /// <param name="providerName">Provider name.</param>
        /// <param name="providerAction">Provider action.</param>
        public static IServiceCollection AddDefaultRedisCacheWithFactory(
            this IServiceCollection services,
            string providerName,
            Action<RedisOptions> providerAction)
        {
            ArgumentCheck.NotNull(services, nameof(services));
            ArgumentCheck.NotNullOrWhiteSpace(providerName, nameof(providerName));

            services.AddOptions();
            services.Configure(providerName, providerAction);

            services.AddSingleton<IEasyCachingProviderFactory, DefaultEasyCachingProviderFactory>();
            services.TryAddSingleton<IEasyCachingSerializer, DefaultBinaryFormatterSerializer>();
            services.AddSingleton<IRedisDatabaseProvider, RedisDatabaseProvider>(x =>
            {
                var optionsMon = x.GetRequiredService<IOptionsMonitor<RedisOptions>>();
                var options = optionsMon.Get(providerName);
                return new RedisDatabaseProvider(providerName, options);
            });

            services.AddSingleton<IEasyCachingProvider, DefaultRedisCachingProvider>(x =>
            {
                var dbProviders = x.GetServices<IRedisDatabaseProvider>();
                var serializer = x.GetRequiredService<IEasyCachingSerializer>();
                var options = x.GetRequiredService<IOptionsMonitor<RedisOptions>>();
                var factory = x.GetService<ILoggerFactory>();
                return new DefaultRedisCachingProvider(providerName, dbProviders, serializer, options, factory);
            });

            return services;
        }

        /// <summary>
        /// Adds the default redis cache with factory.
        /// </summary>
        /// <returns>The default redis cache with factory.</returns>
        /// <param name="services">Services.</param>
        /// <param name="providerName">Provider name.</param>
        /// <param name="sectionName">Section name.</param>
        /// <param name="configuration">Configuration.</param>
        public static IServiceCollection AddDefaultRedisCacheWithFactory(
           this IServiceCollection services,
            string providerName,
            string sectionName,
            IConfiguration configuration)
        {
            ArgumentCheck.NotNull(services, nameof(services));
            ArgumentCheck.NotNullOrWhiteSpace(providerName, nameof(providerName));
            ArgumentCheck.NotNullOrWhiteSpace(sectionName, nameof(sectionName));

            var cacheConfig = configuration.GetSection(sectionName);
            services.Configure<RedisOptions>(providerName,cacheConfig);

            services.AddSingleton<IEasyCachingProviderFactory, DefaultEasyCachingProviderFactory>();
            services.TryAddSingleton<IEasyCachingSerializer, DefaultBinaryFormatterSerializer>();
            services.AddSingleton<IRedisDatabaseProvider, RedisDatabaseProvider>(x =>
            {
                var optionsMon = x.GetRequiredService<IOptionsMonitor<RedisOptions>>();
                var options = optionsMon.Get(providerName);
                return new RedisDatabaseProvider(providerName, options);
            });

            services.AddSingleton<IEasyCachingProvider, DefaultRedisCachingProvider>(x =>
            {
                var dbProviders = x.GetServices<IRedisDatabaseProvider>();
                var serializer = x.GetRequiredService<IEasyCachingSerializer>();
                var options = x.GetRequiredService<IOptionsMonitor<RedisOptions>>();
                var factory = x.GetService<ILoggerFactory>();
                return new DefaultRedisCachingProvider(providerName, dbProviders, serializer, options, factory);
            });

            return services;
        }

    }
}
