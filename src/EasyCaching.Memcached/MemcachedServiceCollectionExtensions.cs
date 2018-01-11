namespace EasyCaching.Memcached
{
    using EasyCaching.Core;
    using EasyCaching.Core.Internal;
    using Enyim.Caching;
    using Enyim.Caching.Configuration;
    using Microsoft.Extensions.DependencyInjection;
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
            services.AddTransient<IMemcachedClientConfiguration, MemcachedClientConfiguration>();
            services.AddSingleton<MemcachedClient, MemcachedClient>();
            services.AddSingleton<IMemcachedClient>(factory => factory.GetService<MemcachedClient>());

            services.Add(ServiceDescriptor.Singleton<IEasyCachingProvider, DefaultMemcachedCachingProvider>());

            return services;
        }
    }
}
