namespace EasyCaching.InMemory
{    
    using EasyCaching.Core;
    using EasyCaching.Core.Internal;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;
    using Microsoft.Extensions.Options;
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
        /// <param name="optionSetup">Option setup.</param>
        public static IServiceCollection AddDefaultInMemoryCache(
            this IServiceCollection services, 
            Action<InMemoryOptions> optionSetup)
        {
            ArgumentCheck.NotNull(services, nameof(services));
            ArgumentCheck.NotNull(optionSetup, nameof(optionSetup));

            var option = new InMemoryOptions();
            optionSetup(option);
            //services.AddSingleton(option);

            services.AddSingleton<IOptions<InMemoryOptions>>(new OptionsWrapper<InMemoryOptions>(option));


            services.AddMemoryCache();
            services.TryAddSingleton<IEasyCachingProvider, DefaultInMemoryCachingProvider>();

            return services;
        }

        /// <summary>
        /// Adds the default in-memory cache.
        /// </summary>
        /// <returns>The default in memory cache.</returns>
        /// <param name="services">Services.</param>
        /// <param name="configuration">Configuration.</param>
        public static IServiceCollection AddDefaultInMemoryCache(
           this IServiceCollection services,
            IConfiguration configuration)
        {
            var dbConfig = configuration.GetSection(EasyCachingConstValue.ConfigSection);
            services.Configure<InMemoryOptions>(dbConfig);

            services.AddMemoryCache();
            services.TryAddSingleton<IEasyCachingProvider, DefaultInMemoryCachingProvider>();

            return services;
        }
    }
}
