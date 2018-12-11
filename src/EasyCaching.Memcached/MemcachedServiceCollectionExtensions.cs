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
    using Microsoft.Extensions.Options;
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
        /// <param name="providerAction">Options.</param>
        public static IServiceCollection AddDefaultMemcached(
            this IServiceCollection services,
            Action<MemcachedOptions> providerAction)
        {
            ArgumentCheck.NotNull(services, nameof(services));
            ArgumentCheck.NotNull(providerAction, nameof(providerAction));

            services.AddOptions();
            services.Configure(providerAction);

            services.TryAddSingleton<ITranscoder, EasyCachingTranscoder>();
            services.TryAddSingleton<IMemcachedKeyTransformer, DefaultKeyTransformer>();
            services.TryAddSingleton<IEasyCachingSerializer, DefaultBinaryFormatterSerializer>();
            services.TryAddSingleton<IMemcachedClientConfiguration, EasyCachingMemcachedClientConfiguration>();

            services.TryAddSingleton<EasyCachingMemcachedClient>();
            services.TryAddSingleton<IMemcachedClient>(factory => factory.GetService<EasyCachingMemcachedClient>());

            services.AddSingleton<IEasyCachingProvider, DefaultMemcachedCachingProvider>();

            return services;
        }

        /// <summary>
        /// Adds the default memcached.
        /// </summary>
        /// <example>
        /// <![CDATA[
        /// "easycaching": {
        ///     "memcached":{
        ///         "CachingProviderType": 3,
        ///         "MaxRdSecond": 120,
        ///         "Order": 2,
        ///         "dbconfig": {            
        ///             "Servers": [
        ///                 {
        ///                 "Address": "memcached",
        ///                 "Port": 11211
        ///                 }
        ///             ],
        ///             "socketPool": {
        ///                 "minPoolSize": "5",
        ///                 "maxPoolSize": "25",
        ///                 "connectionTimeout": "00:00:15",
        ///                 "receiveTimeout": "00:00:15",
        ///                 "deadTimeout": "00:00:15",
        ///                 "queueTimeout": "00:00:00.150"
        ///             } 
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
            var cacheConfig = configuration.GetSection(EasyCachingConstValue.MemcachedSection);
            services.Configure<MemcachedOptions>(cacheConfig);

            //var memcachedConfig = configuration.GetSection(EasyCachingConstValue.ConfigChildSection);
            //services.Configure<EasyCachingMemcachedClientOptions>(memcachedConfig);

            services.TryAddTransient<IMemcachedClientConfiguration, EasyCachingMemcachedClientConfiguration>();
            services.TryAddSingleton<EasyCachingMemcachedClient>();
            services.TryAddSingleton<IMemcachedClient>(factory => factory.GetService<EasyCachingMemcachedClient>());

            services.TryAddSingleton<ITranscoder, EasyCachingTranscoder>();
            services.TryAddSingleton<IEasyCachingSerializer, DefaultBinaryFormatterSerializer>();
            services.TryAddSingleton<IMemcachedKeyTransformer, DefaultKeyTransformer>();

            services.TryAddSingleton<IEasyCachingProvider, DefaultMemcachedCachingProvider>();


            return services;
        } 
    }
}
