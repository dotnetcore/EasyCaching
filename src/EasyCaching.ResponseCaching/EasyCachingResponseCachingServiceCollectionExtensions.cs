namespace Microsoft.Extensions.DependencyInjection
{
    using EasyCaching.Core;
    using EasyCaching.ResponseCaching;
    using Microsoft.Extensions.DependencyInjection.Extensions;
    using Microsoft.Extensions.ObjectPool;
    using System;

    /// <summary>
    /// EasyCaching response caching service collection extensions.
    /// </summary>
    public static class EasyCachingResponseCachingServiceCollectionExtensions
    {
        /// <summary>
        /// Adds the EasyCaching response caching.
        /// </summary>
        /// <returns>The easy caching response caching.</returns>
        /// <param name="services">Services.</param>
        /// <param name="name">Provider name.</param>
        public static IServiceCollection AddEasyCachingResponseCaching(this IServiceCollection services, string name) =>
            services.AddEasyCachingResponseCaching(
                x => { }, name
            );

        /// <summary>
        /// Adds the EasyCaching response caching.
        /// </summary>
        /// <returns>The easy caching response caching.</returns>
        /// <param name="services">Services.</param>
        /// <param name="action">Action.</param>
        /// <param name="name">Provider name.</param>
        public static IServiceCollection AddEasyCachingResponseCaching(this IServiceCollection services,
            Action<ResponseCachingOptions> action, string name)
        {
            ArgumentCheck.NotNull(services, nameof(services));

            services.Configure(action);

            services.TryAddSingleton<IResponseCachingPolicyProvider, ResponseCachingPolicyProvider>();
            services.TryAddSingleton<IResponseCachingKeyProvider, ResponseCachingKeyProvider>();
            services.TryAddSingleton<ObjectPoolProvider, DefaultObjectPoolProvider>();
            services.AddSingleton<IResponseCache, EasyCachingResponseCache>(x =>
            {
                var factory = x.GetRequiredService<IEasyCachingProviderFactory>();
                return new EasyCachingResponseCache(name, factory);
            });

            return services;
        }
    }
}