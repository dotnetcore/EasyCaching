namespace EasyCaching.Serialization.MessagePack
{
    using EasyCaching.Core;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// Message pack options extension.
    /// </summary>
    internal sealed class MessagePackOptionsExtension : IEasyCachingOptionsExtension
    {
        /// <summary>
        /// Adds the services.
        /// </summary>
        /// <param name="services">Services.</param>
        public void AddServices(IServiceCollection services)
        {
            services.AddSingleton<IEasyCachingSerializer, DefaultMessagePackSerializer>();
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
