using EasyCaching.Core;

namespace EasyCaching.ResponseCaching
{
    using Microsoft.AspNetCore.ResponseCaching;
    using Microsoft.AspNetCore.ResponseCaching.Internal;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;
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
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            services.Configure(action);

            services.TryAddSingleton<IResponseCachingPolicyProvider, ResponseCachingPolicyProvider>();
            services.TryAddSingleton<IResponseCachingKeyProvider, ResponseCachingKeyProvider>();
            services.AddSingleton<IResponseCache, EasyCachingResponseCache>(x =>
            {
                var factory = x.GetRequiredService<IEasyCachingProviderFactory>();
                return new EasyCachingResponseCache(name, factory);
            });

            return services;
        }
    }
}