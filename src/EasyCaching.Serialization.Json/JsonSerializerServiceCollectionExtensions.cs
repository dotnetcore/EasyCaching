namespace EasyCaching.Serialization.Json
{
    using EasyCaching.Core;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;
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

            services.TryAddSingleton<IEasyCachingSerializer, DefaultJsonSerializer>();

            return services;
        }
    }
}
