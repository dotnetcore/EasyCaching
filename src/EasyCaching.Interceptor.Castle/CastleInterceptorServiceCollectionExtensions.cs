namespace EasyCaching.Interceptor.Castle
{
    using Autofac;
    using Autofac.Extensions.DependencyInjection;
    using Autofac.Extras.DynamicProxy;
    using EasyCaching.Core.Configurations;
    using EasyCaching.Core.Interceptor;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;
    using System;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// Castle interceptor service collection extensions.
    /// </summary>
    public static class CastleInterceptorServiceCollectionExtensions
    {
        /// <summary>
        /// Configures the castle interceptor.
        /// </summary>
        /// <returns>The castle interceptor.</returns>
        /// <param name="services">Services.</param>
        /// <param name="options">Easycaching Interceptor config</param>
        public static IServiceProvider ConfigureCastleInterceptor(this IServiceCollection services, Action<EasyCachingInterceptorOptions> options)
        {
            {
                services.TryAddSingleton<IEasyCachingKeyGenerator, DefaultEasyCachingKeyGenerator>();
                services.Configure(options);

                var builder = new ContainerBuilder();
                builder.Populate(services);

                var assembly = Assembly.GetCallingAssembly();
                builder.RegisterType<EasyCachingInterceptor>();

                builder.RegisterAssemblyTypes(assembly)
                    .Where(t => !t.IsAbstract && t.GetInterfaces().SelectMany(x => x.GetMethods()).Any(
                       y => y.CustomAttributes.Any(data =>
                                        typeof(EasyCachingInterceptorAttribute).GetTypeInfo().IsAssignableFrom(data.AttributeType)
                                  )))
                    .AsImplementedInterfaces()
                    .InstancePerLifetimeScope()
                    .EnableInterfaceInterceptors()
                    .InterceptedBy(typeof(EasyCachingInterceptor));
                
                return new AutofacServiceProvider(builder.Build());
            }
        }

        /// <summary>
        /// Configures the castle interceptor.
        /// </summary>
        /// <returns>The castle interceptor.</returns>
        /// <param name="services">Services.</param>
        /// <param name="action">Action.</param>
        /// <param name="options">Easycaching Interceptor config</param>
        public static IServiceProvider ConfigureCastleInterceptor(this IServiceCollection services, Action<ContainerBuilder> action, Action<EasyCachingInterceptorOptions> options)
        {
            services.TryAddSingleton<IEasyCachingKeyGenerator, DefaultEasyCachingKeyGenerator>();
            services.Configure(options);

            var builder = new ContainerBuilder();
            builder.Populate(services);

            var assembly = Assembly.GetCallingAssembly();
            builder.RegisterType<EasyCachingInterceptor>();

            builder.RegisterAssemblyTypes(assembly)
                .Where(t => !t.IsAbstract && t.GetInterfaces().SelectMany(x => x.GetMethods()).Any(
                   y => y.CustomAttributes.Any(data =>
                                    typeof(EasyCachingInterceptorAttribute).GetTypeInfo().IsAssignableFrom(data.AttributeType)
                              )))
                .AsImplementedInterfaces()
                .InstancePerLifetimeScope()
                .EnableInterfaceInterceptors()
                .InterceptedBy(typeof(EasyCachingInterceptor));

            action(builder);

            return new AutofacServiceProvider(builder.Build());
        }

        /// <summary>
        /// Configures the castle interceptor.
        /// </summary>
        /// <returns>The castle interceptor.</returns>
        /// <param name="services">Services.</param>
        /// <param name="action">Action.</param>
        /// <param name="isRemoveDefault">If set to <c>true</c> is remove default.</param>
        /// <param name="options">Easycaching Interceptor config</param>
        public static IServiceProvider ConfigureCastleInterceptor(this IServiceCollection services, Action<ContainerBuilder> action, bool isRemoveDefault, Action<EasyCachingInterceptorOptions> options)
        {
            if (isRemoveDefault)
            {
                services.TryAddSingleton<IEasyCachingKeyGenerator, DefaultEasyCachingKeyGenerator>();
                services.Configure(options);

                var builder = new ContainerBuilder();
                builder.Populate(services);
                action(builder);
                return new AutofacServiceProvider(builder.Build());
            }
            else
            {
                return services.ConfigureCastleInterceptor(action, options);
            }
        }
    }
}
