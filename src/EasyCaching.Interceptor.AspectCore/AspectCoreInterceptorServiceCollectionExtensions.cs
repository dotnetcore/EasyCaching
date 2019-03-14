namespace EasyCaching.Interceptor.AspectCore
{
    using EasyCaching.Core.Configurations;
    using EasyCaching.Core.Interceptor;
    using global::AspectCore.Configuration;
    using global::AspectCore.Extensions.DependencyInjection;
    using global::AspectCore.Injector;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;
    using System;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// Aspectcore interceptor service collection extensions.
    /// </summary>
    public static class AspectCoreInterceptorServiceCollectionExtensions
    {
        /// <summary>
        /// Configures the AspectCore interceptor.
        /// </summary>
        /// <returns>The aspect core interceptor.</returns>
        /// <param name="services">Services.</param>
        /// <param name="options">Easycaching Interceptor config</param>
        public static IServiceProvider ConfigureAspectCoreInterceptor(this IServiceCollection services, Action<EasyCachingInterceptorOptions> options) => services.ConfigureAspectCoreInterceptor(x => { }, options);

        /// <summary>
        /// Configures the AspectCore interceptor.
        /// </summary>
        /// <returns>The aspect core interceptor.</returns>
        /// <param name="services">Services.</param>
        /// <param name="action">Action.</param>
        /// <param name="options">Easycaching Interceptor config</param>
        public static IServiceProvider ConfigureAspectCoreInterceptor(this IServiceCollection services, Action<IServiceContainer> action, Action<EasyCachingInterceptorOptions> options)
        {
            services.TryAddSingleton<IEasyCachingKeyGenerator, DefaultEasyCachingKeyGenerator>();
            services.Configure(options);

            var container = services.ToServiceContainer();

            action(container);

            return container.Configure(config =>
            {
                bool all(MethodInfo x) => x.CustomAttributes.Any(data => typeof(EasyCachingInterceptorAttribute).GetTypeInfo().IsAssignableFrom(data.AttributeType));

                config.Interceptors.AddTyped<EasyCachingInterceptor>(all);
            }).Build();
        }

        /// <summary>
        /// Configures the aspect core interceptor.
        /// </summary>
        /// <returns>The aspect core interceptor.</returns>
        /// <param name="services">Services.</param>
        /// <param name="action">Action.</param>
        /// <param name="isRemoveDefault">If set to <c>true</c> is remove default.</param>
        /// <param name="options">Easycaching Interceptor config</param>
        public static IServiceProvider ConfigureAspectCoreInterceptor(this IServiceCollection services, Action<IServiceContainer> action, bool isRemoveDefault, Action<EasyCachingInterceptorOptions> options)
        {
            if (isRemoveDefault)
            {
                services.TryAddSingleton<IEasyCachingKeyGenerator, DefaultEasyCachingKeyGenerator>();
                services.Configure(options);

                var container = services.ToServiceContainer();

                action(container);

                return container.Build();
            }
            else
            {
                return services.ConfigureAspectCoreInterceptor(action, options);
            }
        }
    }
}
