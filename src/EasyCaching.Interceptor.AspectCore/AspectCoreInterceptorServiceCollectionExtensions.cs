namespace EasyCaching.Interceptor.AspectCore
{
    using global::AspectCore.Configuration;
    using global::AspectCore.Extensions.DependencyInjection;
    using global::AspectCore.Injector;
    using EasyCaching.Core.Internal;
    using Microsoft.Extensions.DependencyInjection;
    using System;

    /// <summary>
    /// Aspectcore interceptor service collection extensions.
    /// </summary>
    public static class AspectCoreInterceptorServiceCollectionExtensions
    {
        /// <summary>
        /// Configures the easy caching.
        /// </summary>
        /// <returns>The easy caching.</returns>
        /// <param name="services">Services.</param>
        public static IServiceProvider ConfigureAspectCoreInterceptor(this IServiceCollection services)
        {
            var container = services.ToServiceContainer();

            container.Configure(config =>
            {
                config.Interceptors.AddTyped<DefaultEasyCachingInterceptor>(method => typeof(IEasyCaching).IsAssignableFrom(method.DeclaringType));
            });

            return container.Build();
        }

        /// <summary>
        /// Configures the easy caching.
        /// </summary>
        /// <returns>The easy caching.</returns>
        /// <param name="services">Services.</param>
        /// <param name="action">Action.</param>
        public static IServiceProvider ConfigureAspectCoreInterceptor(this IServiceCollection services, Action<IAspectConfiguration> action)
        {
            var container = services.ToServiceContainer();

            container.Configure(config =>
            {
                config.Interceptors.AddTyped<DefaultEasyCachingInterceptor>(method => typeof(IEasyCaching).IsAssignableFrom(method.DeclaringType));
                action(config);
            });

            return container.Build();
        }
    }
}
