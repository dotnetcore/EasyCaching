namespace EasyCaching.Memcached
{
    using EasyCaching.Core;
    using EasyCaching.Core.Internal;
    using Enyim.Caching;
    using Enyim.Caching.Configuration;
    using Enyim.Caching.Memcached;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;
    using System;

    /// <summary>
    /// Memcached service collection extensions.
    /// </summary>
    public static class MemcachedServiceCollectionExtensions
    {
        /// <summary>
        /// Adds the default memcached.
        /// </summary>
        /// <returns>The default redis cache.</returns>
        /// <param name="services">Services.</param>
        /// <param name="options">Options.</param>
        public static IServiceCollection AddDefaultMemcached(
            this IServiceCollection services, 
            Action<EasyCachingMemcachedClientOptions> options)
        {
            var providerOptioin = new MemcachedOptions();

            return services.AddDefaultMemcached(options, x =>
            {
                x.CachingProviderType = providerOptioin.CachingProviderType;
                x.MaxRdSecond = providerOptioin.MaxRdSecond;
                x.Order = providerOptioin.Order;

            });
        }

        /// <summary>
        /// Adds the default memcached.
        /// </summary>
        /// <returns>The default memcached.</returns>
        /// <param name="services">Services.</param>
        /// <param name="options">Options.</param>
        /// <param name="providerAction">Provider action.</param>
        public static IServiceCollection AddDefaultMemcached(
            this IServiceCollection services,
            Action<EasyCachingMemcachedClientOptions> options,
            Action<MemcachedOptions> providerAction)
        {
            ArgumentCheck.NotNull(services, nameof(services));
            ArgumentCheck.NotNull(options, nameof(options));
            ArgumentCheck.NotNull(providerAction, nameof(providerAction));

            services.AddOptions();
            services.Configure(options);

            var providerOptioin = new MemcachedOptions();
            providerAction(providerOptioin);
            services.AddSingleton(providerOptioin);

            services.TryAddTransient<IMemcachedClientConfiguration, EasyCachingMemcachedClientConfiguration>();
            services.TryAddSingleton<MemcachedClient, MemcachedClient>();
            services.TryAddSingleton<IMemcachedClient>(factory => factory.GetService<MemcachedClient>());

            services.TryAddSingleton<ITranscoder, EasyCachingTranscoder>();
            services.TryAddSingleton<IEasyCachingSerializer, DefaultBinaryFormatterSerializer>();
            services.TryAddSingleton<IMemcachedKeyTransformer, DefaultKeyTransformer>();

            services.TryAddSingleton<IEasyCachingProvider, DefaultMemcachedCachingProvider>();

            return services;
        }

        /// <summary>
        /// Adds the default memcached.
        /// </summary>
        /// <example>
        /// <![CDATA[
        /// "easycaching": {
        ///     "CachingProviderType": 3,
        ///     "MaxRdSecond": 120,
        ///     "Order": 2,
        ///     "dbconfig": {            
        ///         "Servers": [
        ///          {
        ///             "Address": "memcached",
        ///             "Port": 11211
        ///           }
        ///         ],
        ///         "socketPool": {
        ///           "minPoolSize": "5",
        ///           "maxPoolSize": "25",
        ///           "connectionTimeout": "00:00:15",
        ///           "receiveTimeout": "00:00:15",
        ///           "deadTimeout": "00:00:15",
        ///           "queueTimeout": "00:00:00.150"
        ///         } 
        ///     }
        /// }      
        /// ]]>
        /// </example>
        /// <returns>The default memcached.</returns>
        /// <param name="services">Services.</param>
        /// <param name="configuration">Configuration.</param>
        public static IServiceCollection AddDefaultMemcached(
           this IServiceCollection services,
            IConfiguration configuration)
        {
            var cacheConfig = configuration.GetSection(EasyCachingConstValue.ConfigSection);
            services.Configure<MemcachedOptions>(cacheConfig);

            var redisConfig = configuration.GetSection(EasyCachingConstValue.ConfigChildSection);
            services.Configure<EasyCachingMemcachedClientOptions>(redisConfig);

            services.TryAddTransient<IMemcachedClientConfiguration, EasyCachingMemcachedClientConfiguration>();
            services.TryAddSingleton<MemcachedClient, MemcachedClient>();
            services.TryAddSingleton<IMemcachedClient>(factory => factory.GetService<MemcachedClient>());

            services.TryAddSingleton<ITranscoder, EasyCachingTranscoder>();
            services.TryAddSingleton<IEasyCachingSerializer, DefaultBinaryFormatterSerializer>();
            services.TryAddSingleton<IMemcachedKeyTransformer, DefaultKeyTransformer>();

            services.TryAddSingleton<IEasyCachingProvider, DefaultMemcachedCachingProvider>();


            return services;
        } 
    }
}
