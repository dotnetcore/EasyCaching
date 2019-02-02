namespace EasyCaching.Interceptor.AspectCore
{
    using global::AspectCore.Configuration;
    using global::AspectCore.Extensions.DependencyInjection;
    using global::AspectCore.Injector;
    using EasyCaching.Core;
    using EasyCaching.Core.Internal;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;
    using System;
    using System.Linq;
    using System.Reflection;
    using EasyCaching.Core.Interceptor;

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
        public static IServiceProvider ConfigureAspectCoreInterceptor(this IServiceCollection services)
        {
            services.TryAddSingleton<IEasyCachingKeyGenerator,DefaultEasyCachingKeyGenerator>();

            var container = services.ToServiceContainer();

            return container.Configure(config =>
            {
                bool all(MethodInfo x) => x.CustomAttributes.Any(data => typeof(EasyCachingInterceptorAttribute).GetTypeInfo().IsAssignableFrom(data.AttributeType));               

                config.Interceptors.AddTyped<EasyCachingInterceptor>(all);
            }).Build();                        
        }
               
        /// <summary>
        /// Configures the AspectCore interceptor.
        /// </summary>
        /// <returns>The aspect core interceptor.</returns>
        /// <param name="services">Services.</param>
        /// <param name="action">Action.</param>
        public static IServiceProvider ConfigureAspectCoreInterceptor(this IServiceCollection services, Action<IServiceContainer> action)
        {
            services.TryAddSingleton<IEasyCachingKeyGenerator, DefaultEasyCachingKeyGenerator>();

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
        public static IServiceProvider ConfigureAspectCoreInterceptor(this IServiceCollection services, Action<IServiceContainer> action, bool isRemoveDefault)
        {
            if (isRemoveDefault)
            {
                services.TryAddSingleton<IEasyCachingKeyGenerator, DefaultEasyCachingKeyGenerator>();

                var container = services.ToServiceContainer();

                action(container);

                return container.Build();
            }
            else
            {
                return services.ConfigureAspectCoreInterceptor(action);
            }
        }
    }
}
