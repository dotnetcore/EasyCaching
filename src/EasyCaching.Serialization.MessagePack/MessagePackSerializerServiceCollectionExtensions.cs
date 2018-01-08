namespace EasyCaching.Serialization.MessagePack
{
    using EasyCaching.Core;
    using Microsoft.Extensions.DependencyInjection;
    using System;

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

            services.Add(ServiceDescriptor.Singleton<IEasyCachingSerializer, DefaultMessagePackSerializer>());

            return services;
        }
    }
}
