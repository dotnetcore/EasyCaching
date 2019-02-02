namespace EasyCaching.Serialization.Protobuf
{
    using EasyCaching.Core.Configurations;
    using EasyCaching.Core.Serialization;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// Protobuf options extension.
    /// </summary>
    internal sealed class ProtobufOptionsExtension : IEasyCachingOptionsExtension
    {
        /// <summary>
        /// Adds the services.
        /// </summary>
        /// <param name="services">Services.</param>
        public void AddServices(IServiceCollection services)
        {
            services.AddSingleton<IEasyCachingSerializer, DefaultProtobufSerializer>();
        }

        /// <summary>
        /// Withs the services.
        /// </summary>
        /// <param name="services">Services.</param>
        public void WithServices(IApplicationBuilder services)
        {
            // Method intentionally left empty.
        }
    }
}
