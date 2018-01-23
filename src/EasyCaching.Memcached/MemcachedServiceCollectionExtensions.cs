namespace EasyCaching.Memcached
{    
    using EasyCaching.Core;
    using EasyCaching.Core.Internal;
    using Enyim.Caching;
    using Enyim.Caching.Configuration;
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
        public static IServiceCollection AddDefaultMemcached(this IServiceCollection services, Action<MemcachedClientOptions> options)
        {
            ArgumentCheck.NotNull(services, nameof(services));

            ArgumentCheck.NotNull(options, nameof(options));

            services.AddOptions();
            services.Configure(options);
            services.TryAddTransient<IMemcachedClientConfiguration, MemcachedClientConfiguration>();
            services.TryAddSingleton<MemcachedClient, MemcachedClient>();
            services.TryAddSingleton<IMemcachedClient>(factory => factory.GetService<MemcachedClient>());

            services.TryAddSingleton<IEasyCachingProvider, DefaultMemcachedCachingProvider>();            

            return services;
        }

        /// <summary>
        /// Adds the default memcached for hybrid.
        /// </summary>
        /// <returns>The default memcached for hybrid.</returns>
        /// <param name="services">Services.</param>
        /// <param name="options">Options.</param>
        public static IServiceCollection AddDefaultMemcachedForHybrid(this IServiceCollection services, Action<MemcachedClientOptions> options)
        {
            ArgumentCheck.NotNull(services, nameof(services));
            ArgumentCheck.NotNull(options, nameof(options));

            services.AddOptions();
            services.Configure(options);

            services.TryAddTransient<IMemcachedClientConfiguration, MemcachedClientConfiguration>();
            services.TryAddSingleton<MemcachedClient, MemcachedClient>();
            services.TryAddSingleton<IMemcachedClient>(factory => factory.GetService<MemcachedClient>());

            services.TryAddSingleton<DefaultMemcachedCachingProvider>();

            //services.AddSingleton(factory =>
            //{
            //    Func<string, IEasyCachingProvider> accesor = key =>
            //    {
            //        if (key.Equals(HybridCachingKeyType.DistributedKey))
            //        {
            //            return factory.GetService<DefaultMemcachedCachingProvider>();
            //        }
            //        else
            //        {
            //            throw new KeyNotFoundException();
            //        }
            //    };
            //    return accesor;
            //});                     

            return services;
        }
    }
}
