namespace EasyCaching.Interceptor.Castle
{
    using Autofac;
    using Autofac.Extensions.DependencyInjection;
    using Autofac.Extras.DynamicProxy;
    using EasyCaching.Core;
    using EasyCaching.Core.Internal;
    using Microsoft.Extensions.DependencyInjection;
    using System;
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
        public static IServiceProvider ConfigureCastleInterceptor(this IServiceCollection services)
        {
            services.AddSingleton<IEasyCachingKeyGenerator, DefaultEasyCachingKeyGenerator>();

            var builder = new ContainerBuilder();
            builder.Populate(services);

            var assembly = Assembly.GetCallingAssembly();
            builder.RegisterType<EasyCachingInterceptor>();

            builder.RegisterAssemblyTypes(assembly)
                         .Where(type => typeof(IEasyCaching).IsAssignableFrom(type) && !type.GetTypeInfo().IsAbstract)
                         .AsImplementedInterfaces()
                         .InstancePerLifetimeScope()
                         .EnableInterfaceInterceptors()
                         .InterceptedBy(typeof(EasyCachingInterceptor));
                                          
            return new AutofacServiceProvider(builder.Build());
        }             
    }
}
