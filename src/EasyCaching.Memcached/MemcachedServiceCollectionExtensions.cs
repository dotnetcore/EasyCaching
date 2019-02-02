namespace EasyCaching.Memcached
{
    using System;
    using System.Linq;
    using EasyCaching.Core;
    using EasyCaching.Core.Serialization;
    using Enyim.Caching.Configuration;
    using Enyim.Caching.Memcached;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;

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
            services.AddSingleton<IMemcachedClientConfiguration, EasyCachingMemcachedClientConfiguration>(x =>
            {
                var options = x.GetRequiredService<IOptionsMonitor<MemcachedOptions>>();
                var loggerFactory = x.GetRequiredService<ILoggerFactory>();
                var transcoder = x.GetRequiredService<ITranscoder>();
                var transformer = x.GetRequiredService<IMemcachedKeyTransformer>();
                return new EasyCachingMemcachedClientConfiguration(loggerFactory, options, transcoder, transformer);
            });

            services.AddSingleton<EasyCachingMemcachedClient>(x=> 
            {
                var loggerFactory = x.GetRequiredService<ILoggerFactory>();
                var config = x.GetRequiredService<IMemcachedClientConfiguration>();
                return new EasyCachingMemcachedClient(EasyCachingConstValue.DefaultMemcachedName, loggerFactory, config);
            });

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
            ArgumentCheck.NotNull(services, nameof(services));

            var cacheConfig = configuration.GetSection(EasyCachingConstValue.MemcachedSection);
            services.Configure<MemcachedOptions>(cacheConfig);

            services.TryAddSingleton<ITranscoder, EasyCachingTranscoder>();
            services.TryAddSingleton<IMemcachedKeyTransformer, DefaultKeyTransformer>();
            services.TryAddSingleton<IEasyCachingSerializer, DefaultBinaryFormatterSerializer>();
            services.AddSingleton<IMemcachedClientConfiguration, EasyCachingMemcachedClientConfiguration>(x =>
            {
                var options = x.GetRequiredService<IOptionsMonitor<MemcachedOptions>>();
                var loggerFactory = x.GetRequiredService<ILoggerFactory>();
                var transcoder = x.GetRequiredService<ITranscoder>();
                var transformer = x.GetRequiredService<IMemcachedKeyTransformer>();
                return new EasyCachingMemcachedClientConfiguration(loggerFactory, options, transcoder, transformer);
            });

            services.AddSingleton<EasyCachingMemcachedClient>(x =>
            {
                var loggerFactory = x.GetRequiredService<ILoggerFactory>();
                var config = x.GetRequiredService<IMemcachedClientConfiguration>();
                return new EasyCachingMemcachedClient(EasyCachingConstValue.DefaultMemcachedName, loggerFactory, config);
            });

            services.AddSingleton<IEasyCachingProvider, DefaultMemcachedCachingProvider>();
            return services;
        }

        /// <summary>
        /// Adds the default memcached with factory.
        /// </summary>
        /// <returns>The default memcached with factory.</returns>
        /// <param name="services">Services.</param>
        /// <param name="providerName">Provider name.</param>
        /// <param name="providerAction">Provider action.</param>
        public static IServiceCollection AddDefaultMemcachedWithFactory(
           this IServiceCollection services,
           string providerName,
           Action<MemcachedOptions> providerAction)
        {
            ArgumentCheck.NotNull(services, nameof(services));
            ArgumentCheck.NotNullOrWhiteSpace(providerName, nameof(providerName));
            ArgumentCheck.NotNull(providerAction, nameof(providerAction));

            services.AddOptions();
            services.Configure(providerName, providerAction);

            services.AddSingleton<IEasyCachingProviderFactory, DefaultEasyCachingProviderFactory>();
            services.TryAddSingleton<ITranscoder, EasyCachingTranscoder>();
            services.TryAddSingleton<IMemcachedKeyTransformer, DefaultKeyTransformer>();
            services.TryAddSingleton<IEasyCachingSerializer, DefaultBinaryFormatterSerializer>();
            services.AddSingleton<EasyCachingMemcachedClientConfiguration>(x =>
            {
                var optionsMon = x.GetRequiredService<IOptionsMonitor<MemcachedOptions>>();
                var options = optionsMon.Get(providerName);
                var loggerFactory = x.GetRequiredService<ILoggerFactory>();
                var transcoder = x.GetRequiredService<ITranscoder>();
                var transformer = x.GetRequiredService<IMemcachedKeyTransformer>();
                return new EasyCachingMemcachedClientConfiguration(providerName, loggerFactory, options, transcoder, transformer);
            });

            services.AddSingleton<EasyCachingMemcachedClient>(x =>
            {
                var loggerFactory = x.GetRequiredService<ILoggerFactory>();
                var configs = x.GetServices<EasyCachingMemcachedClientConfiguration>();
                var config = configs.FirstOrDefault(y => y.Name.Equals(providerName));
                return new EasyCachingMemcachedClient(providerName, loggerFactory, config);
            });

            services.AddSingleton<IEasyCachingProvider, DefaultMemcachedCachingProvider>(x =>
            {
                var clients = x.GetServices<EasyCachingMemcachedClient>();
                var optionsMon = x.GetRequiredService<IOptionsMonitor<MemcachedOptions>>();
                var options = optionsMon.Get(providerName);
                var factory = x.GetService<ILoggerFactory>();
                return new DefaultMemcachedCachingProvider(providerName, clients, options, factory);
            });

            return services;
        }

        /// <summary>
        /// Adds the default memcached with factory.
        /// </summary>
        /// <returns>The default memcached with factory.</returns>
        /// <param name="services">Services.</param>
        /// <param name="providerName">Provider name.</param>
        /// <param name="sectionName">Section name.</param>
        /// <param name="configuration">Configuration.</param>
        public static IServiceCollection AddDefaultMemcachedWithFactory(
          this IServiceCollection services,
           string providerName,
           string sectionName,
           IConfiguration configuration)
        {
            ArgumentCheck.NotNull(services, nameof(services));
            ArgumentCheck.NotNullOrWhiteSpace(providerName, nameof(providerName));
            ArgumentCheck.NotNullOrWhiteSpace(sectionName, nameof(sectionName));

            var cacheConfig = configuration.GetSection(sectionName);
            services.Configure<MemcachedOptions>(providerName,cacheConfig);

            services.AddSingleton<IEasyCachingProviderFactory, DefaultEasyCachingProviderFactory>();
            services.TryAddSingleton<ITranscoder, EasyCachingTranscoder>();
            services.TryAddSingleton<IMemcachedKeyTransformer, DefaultKeyTransformer>();
            services.TryAddSingleton<IEasyCachingSerializer, DefaultBinaryFormatterSerializer>();
            services.AddSingleton<EasyCachingMemcachedClientConfiguration>(x =>
            {
                var optionsMon = x.GetRequiredService<IOptionsMonitor<MemcachedOptions>>();
                var options = optionsMon.Get(providerName);
                var loggerFactory = x.GetRequiredService<ILoggerFactory>();
                var transcoder = x.GetRequiredService<ITranscoder>();
                var transformer = x.GetRequiredService<IMemcachedKeyTransformer>();
                return new EasyCachingMemcachedClientConfiguration(providerName, loggerFactory, options, transcoder, transformer);
            });

            services.AddSingleton<EasyCachingMemcachedClient>(x =>
            {
                var loggerFactory = x.GetRequiredService<ILoggerFactory>();
                var configs = x.GetServices<EasyCachingMemcachedClientConfiguration>();
                var config = configs.FirstOrDefault(y => y.Name.Equals(providerName));
                return new EasyCachingMemcachedClient(providerName, loggerFactory, config);
            });

            services.AddSingleton<IEasyCachingProvider, DefaultMemcachedCachingProvider>(x =>
            {
                var clients = x.GetServices<EasyCachingMemcachedClient>();
                var optionsMon = x.GetRequiredService<IOptionsMonitor<MemcachedOptions>>();
                var options = optionsMon.Get(providerName);
                var factory = x.GetService<ILoggerFactory>();
                return new DefaultMemcachedCachingProvider(providerName, clients, options, factory);
            });

            return services;
        }
    }
}
