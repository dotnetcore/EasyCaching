namespace EasyCaching.Interceptor.AspectCore
{
    using EasyCaching.Core.Configurations;
    using EasyCaching.Core.Interceptor;
    using global::AspectCore.Configuration;
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
        public static void ConfigureAspectCoreInterceptor(this IServiceCollection services, Action<EasyCachingInterceptorOptions> options)
        {
            services.TryAddSingleton<IEasyCachingKeyGenerator, DefaultEasyCachingKeyGenerator>();
            services.Configure(options);
        }

        /// <summary>
        /// Configures the AspectCore interceptor.
        /// </summary>
        /// <returns>The aspect core interceptor.</returns>
        /// <param name="builder">Builder.</param>
        public static void ConfigureAspectCoreInterceptor(this IServiceContainer builder)
        {
            builder.Configure(config =>
            {
                bool all(MethodInfo x) => x.CustomAttributes.Any(data => typeof(EasyCachingInterceptorAttribute).GetTypeInfo().IsAssignableFrom(data.AttributeType));

                config.Interceptors.AddTyped<EasyCachingInterceptor>(all);
            });
        }
    }
}
