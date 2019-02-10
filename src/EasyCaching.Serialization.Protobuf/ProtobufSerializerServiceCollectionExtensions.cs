namespace EasyCaching.Serialization.Protobuf
{
    using System;
    using EasyCaching.Core.Serialization;
    using Microsoft.Extensions.DependencyInjection;

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

            services.AddSingleton<IEasyCachingSerializer, DefaultProtobufSerializer>();

            return services;
        }

        ///// <summary>
        ///// Adds the default protobuf serializer.
        ///// </summary>
        ///// <returns>The default protobuf serializer.</returns>
        ///// <param name="services">Services.</param>
        ///// <param name="assemblys">Assemblys.</param>
        //public static IServiceCollection AddDefaultProtobufSerializer(this IServiceCollection services, IList<Assembly> assemblys)
        //{
        //    if (services == null)
        //    {
        //        throw new ArgumentNullException(nameof(services));
        //    }

        //    services.TryAddSingleton<IEasyCachingSerializer, DefaultProtobufSerializer>();

        //    //Handle Protobuf Attribute
        //    foreach (var assembly in assemblys)
        //    {
        //        foreach (Type type in assembly.GetTypes())
        //        {
        //            SerializerBuilder.Build(type);
        //        }
        //    }

        //    return services;
        //}
    }
}
