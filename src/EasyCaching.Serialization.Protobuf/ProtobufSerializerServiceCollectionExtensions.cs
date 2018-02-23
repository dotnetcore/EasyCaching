namespace EasyCaching.Serialization.Protobuf
{
    using EasyCaching.Core;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    /// <summary>
    /// Protobuf serializer service collection extensions.
    /// </summary>
    public static class ProtobufSerializerServiceCollectionExtensions
    {
        /// <summary>
        /// Adds the default protobuf serializer.
        /// </summary>
        /// <returns>The default protobuf serializer.</returns>
        /// <param name="services">Services.</param>
        public static IServiceCollection AddDefaultProtobufSerializer(this IServiceCollection services)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            services.TryAddSingleton<IEasyCachingSerializer, DefaultProtobufSerializer>();

            return services;
        }

        /// <summary>
        /// Adds the default protobuf serializer.
        /// </summary>
        /// <returns>The default protobuf serializer.</returns>
        /// <param name="services">Services.</param>
        /// <param name="assemblys">Assemblys.</param>
        public static IServiceCollection AddDefaultProtobufSerializer(this IServiceCollection services, IList<Assembly> assemblys)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            services.TryAddSingleton<IEasyCachingSerializer, DefaultProtobufSerializer>();

            //Handle Protobuf Attribute
            foreach (var assembly in assemblys)
            {
                foreach (Type type in assembly.GetTypes())
                {
                    SerializerBuilder.Build(type);
                }
            }

            return services;
        }
    }
}
