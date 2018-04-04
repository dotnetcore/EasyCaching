namespace EasyCaching.ResponseCaching
{
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
        public static IServiceCollection AddEasyCachingResponseCaching(this IServiceCollection services)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            services.TryAddSingleton<IResponseCachingPolicyProvider, ResponseCachingPolicyProvider>();
            services.TryAddSingleton<IResponseCachingKeyProvider, ResponseCachingKeyProvider>();
            services.AddSingleton<IResponseCache, EasyCachingResponseCache>();

            return services;
        }

        /// <summary>
        /// Adds the EasyCaching response caching.
        /// </summary>
        /// <returns>The easy caching response caching.</returns>
        /// <param name="services">Services.</param>
        /// <param name="action">Action.</param>
        public static IServiceCollection AddEasyCachingResponseCaching(this IServiceCollection services, Action<EasyCachingResponseOptions> action)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            var option = new EasyCachingResponseOptions { Services = services };

            action(option);

            services.TryAddSingleton<IResponseCachingPolicyProvider, ResponseCachingPolicyProvider>();
            services.TryAddSingleton<IResponseCachingKeyProvider, ResponseCachingKeyProvider>();
            services.AddSingleton<IResponseCache, EasyCachingResponseCache>();

            return services;
        }
    }
}