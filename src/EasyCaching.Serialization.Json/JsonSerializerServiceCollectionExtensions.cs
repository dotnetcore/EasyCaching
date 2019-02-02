namespace EasyCaching.Serialization.Json
{
    using EasyCaching.Core;
    using EasyCaching.Core.Serialization;
    using Microsoft.Extensions.DependencyInjection;
    using System;

    /// <summary>
    /// Json serializer service collection extensions.
    /// </summary>
    public static class JsonSerializerServiceCollectionExtensions
    {        
        /// <summary>
        /// Adds the default json serializer.
        /// </summary>
        /// <returns>The default json serializer.</returns>
        /// <param name="services">Services.</param>
        public static IServiceCollection AddDefaultJsonSerializer(this IServiceCollection services)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }
            services.AddOptions();
            services.Configure<EasyCachingJsonSerializerOptions>(x=>{});
            services.AddSingleton<IEasyCachingSerializer, DefaultJsonSerializer>();

            return services;
        }

        /// <summary>
        /// Adds the default json serializer.
        /// </summary>
        /// <returns>The default json serializer.</returns>
        /// <param name="services">Services.</param>
        /// <param name="action">Action.</param>
        public static IServiceCollection AddDefaultJsonSerializer(this IServiceCollection services, Action<EasyCachingJsonSerializerOptions> action)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            services.AddOptions();
            services.Configure(action);

            services.AddSingleton<IEasyCachingSerializer, DefaultJsonSerializer>();

            return services;
        }
    }
}
