namespace EasyCaching.InMemory
{    
    using EasyCaching.Core;
    using EasyCaching.Core.Internal;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;
    using System;

    /// <summary>
    /// In memory cache service collection extensions.
    /// </summary>
    public static class InMemoryCacheServiceCollectionExtensions
    {
        /// <summary>
        /// Adds the default in-memory cache.
        /// </summary>
        /// <returns>The default in-memory cache.</returns>
        /// <param name="services">Services.</param>
        public static IServiceCollection AddDefaultInMemoryCache(this IServiceCollection services)
        {            
            var option = new InMemoryOptions();

            return services.AddDefaultInMemoryCache(x=>
            {
                x.CachingProviderType = option.CachingProviderType;
                x.MaxRdSecond = option.MaxRdSecond;
                x.Order = option.Order;
            });
        }

        /// <summary>
        /// Adds the default in-memory cache.
        /// </summary>
        /// <returns>The default in-memory cache.</returns>
        /// <param name="services">Services.</param>
        /// <param name="providerAction">Option.</param>
        public static IServiceCollection AddDefaultInMemoryCache(
            this IServiceCollection services, 
            Action<InMemoryOptions> providerAction)
        {
            ArgumentCheck.NotNull(services, nameof(services));
            ArgumentCheck.NotNull(providerAction, nameof(providerAction));

            services.AddOptions();
            services.Configure(providerAction);
                       
            services.AddMemoryCache();
            services.AddSingleton<IEasyCachingProvider, DefaultInMemoryCachingProvider>();
            //services.AddSingleton<IEasyCachingProvider, DefaultInMemoryCachingProvider>(x=>
            //{
            //    var mCache = x.GetRequiredService<Microsoft.Extensions.Caching.Memory.IMemoryCache>();
            //    var mOptions = x.GetRequiredService<Microsoft.Extensions.Options.IOptionsMonitor<InMemoryOptions>>();
            //    var mLog = x.GetRequiredService<Microsoft.Extensions.Logging.ILoggerFactory>();
            //    return new DefaultInMemoryCachingProvider(name, mCache, mOptions,mLog);                           
            //});

            return services;
        }

        /// <summary>
        /// Adds the default in-memory cache.
        /// </summary>
        /// <example>
        /// <![CDATA[
        /// "easycaching": {
        ///     "inmemory": {
        ///         "CachingProviderType": 3,
        ///         "MaxRdSecond": 120,
        ///         "Order": 2
        ///     }
        /// }
        /// ]]>
        /// </example>
        /// <returns>The default in memory cache.</returns>
        /// <param name="services">Services.</param>
        /// <param name="configuration">Configuration.</param>
        public static IServiceCollection AddDefaultInMemoryCache(
           this IServiceCollection services,
            IConfiguration configuration)
        {
            var dbConfig = configuration.GetSection(EasyCachingConstValue.InMemorySection);
            services.Configure<InMemoryOptions>(dbConfig);

            services.AddMemoryCache();
            services.AddSingleton<IEasyCachingProvider, DefaultInMemoryCachingProvider>();
            //services.AddSingleton<IEasyCachingProvider, DefaultInMemoryCachingProvider>(x =>
            //{
            //    var mCache = x.GetRequiredService<Microsoft.Extensions.Caching.Memory.IMemoryCache>();
            //    var mOptions = x.GetRequiredService<Microsoft.Extensions.Options.IOptionsMonitor<InMemoryOptions>>();
            //    var mLog = x.GetRequiredService<Microsoft.Extensions.Logging.ILoggerFactory>();
            //    return new DefaultInMemoryCachingProvider(name, mCache, mOptions, mLog);
            //});

            return services;
        }




        /// <summary>
        /// Adds the default in-memory cache.
        /// </summary>
        /// <returns>The default in-memory cache.</returns>
        /// <param name="services">Services.</param>
        public static IServiceCollection AddDefaultInMemoryCacheWithFactory(this IServiceCollection services,string providerName = EasyCachingConstValue.DefaultInMemoryName)
        {
            var option = new InMemoryOptions();

            return services.AddDefaultInMemoryCacheWithFactory(providerName, x =>
            {
                x.CachingProviderType = option.CachingProviderType;
                x.MaxRdSecond = option.MaxRdSecond;
                x.Order = option.Order;
            });
        }

        /// <summary>
        /// Adds the default in-memory cache.
        /// </summary>
        /// <returns>The default in-memory cache.</returns>
        /// <param name="services">Services.</param>
        /// <param name="providerAction">Option.</param>
        public static IServiceCollection AddDefaultInMemoryCacheWithFactory(
            this IServiceCollection services,
            string providerName,
            Action<InMemoryOptions> providerAction)
        {
            ArgumentCheck.NotNull(services, nameof(services));
            ArgumentCheck.NotNull(providerAction, nameof(providerAction));

            services.AddOptions();
            services.Configure(providerAction);

            services.AddMemoryCache();

            services.AddSingleton<IEasyCachingProviderFactory, DefaultEasyCachingProviderFactory>();
            //services.AddSingleton<IEasyCachingProvider, DefaultInMemoryCachingProvider>();
            services.AddSingleton<IEasyCachingProvider, DefaultInMemoryCachingProvider>(x=>
            {
                var mCache = x.GetRequiredService<Microsoft.Extensions.Caching.Memory.IMemoryCache>();
                var options = x.GetRequiredService<Microsoft.Extensions.Options.IOptionsMonitor<InMemoryOptions>>();
                //ILoggerFactory can be null
                var factory = x.GetService<Microsoft.Extensions.Logging.ILoggerFactory>();
                return new DefaultInMemoryCachingProvider(providerName, mCache, options,factory);                           
            });

            return services;
        }
    }
}
