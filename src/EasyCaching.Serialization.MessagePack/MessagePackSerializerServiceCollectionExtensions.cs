namespace EasyCaching.Serialization.MessagePack
{
    using System;
    using EasyCaching.Core.Serialization;
    using Microsoft.Extensions.DependencyInjection;

    public static class MessagePackSerializerServiceCollectionExtensions
    {
        /// <summary>
        /// Adds the default message pack serializer.
        /// </summary>
        /// <returns>The default message pack serializer.</returns>
        /// <param name="services">Services.</param>
        public static IServiceCollection AddDefaultMessagePackSerializer(this IServiceCollection services)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            services.AddSingleton<IEasyCachingSerializer, DefaultMessagePackSerializer>();

            return services;
        }
    }
}
