namespace EasyCaching.Extensions
{
    using AspectCore.Configuration;
    using AspectCore.Extensions.DependencyInjection;
    using AspectCore.Injector;
    using EasyCaching.Core.Internal;
    using Microsoft.Extensions.DependencyInjection;
    using System;

    /// <summary>
    /// Service extension.
    /// </summary>
    public static class ServiceExtension
    {

        /// <summary>
        /// Configures the easy caching.
        /// </summary>
        /// <returns>The easy caching.</returns>
        /// <param name="services">Services.</param>
        public static IServiceProvider ConfigureEasyCaching(this IServiceCollection services)
        {
            var container = services.ToServiceContainer();

            container.Configure(config => 
            {
                config.Interceptors.AddTyped<CachingInterceptor>(method=>typeof(IEasyCaching).IsAssignableFrom(method.DeclaringType));
            });

            return container.Build();
        }

        /// <summary>
        /// Configures the easy caching.
        /// </summary>
        /// <returns>The easy caching.</returns>
        /// <param name="services">Services.</param>
        /// <param name="action">Action.</param>
        public static IServiceProvider ConfigureEasyCaching(this IServiceCollection services , Action<IAspectConfiguration> action)
        {
            var container = services.ToServiceContainer();

            container.Configure(config =>
            {
                config.Interceptors.AddTyped<CachingInterceptor>(method => typeof(IEasyCaching).IsAssignableFrom(method.DeclaringType));
                action(config);
            });

            return container.Build();
        }
    }
}
