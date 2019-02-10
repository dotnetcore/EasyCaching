namespace EasyCaching.Bus.RabbitMQ
{
    using System;
    using EasyCaching.Core.Bus;
    using EasyCaching.Core.Configurations;
    using EasyCaching.Core.Serialization;
    using global::RabbitMQ.Client;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;
    using Microsoft.Extensions.ObjectPool;

    /// <summary>
    /// RabbitMQ Bus options extension.
    /// </summary>
    public class RabbitMQBusOptionsExtension : IEasyCachingOptionsExtension
    {
        /// <summary>
        /// The configure.
        /// </summary>
        private readonly Action<RabbitMQBusOptions> configure;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:EasyCaching.Bus.RabbitMQ.RabbitMQBusOptionsExtension"/> class.
        /// </summary>
        /// <param name="configure">Configure.</param>
        public RabbitMQBusOptionsExtension(Action<RabbitMQBusOptions> configure)
        {
            this.configure = configure;
        }

        /// <summary>
        /// Adds the services.
        /// </summary>
        /// <param name="services">Services.</param>
        public void AddServices(IServiceCollection services)
        {
            services.AddOptions();
            services.Configure(configure);

            services.TryAddSingleton<IEasyCachingSerializer, DefaultBinaryFormatterSerializer>();
            services.AddSingleton<IPooledObjectPolicy<IConnection>, ConnectionPooledObjectPolicy>();
            services.AddSingleton<IEasyCachingBus, DefaultRabbitMQBus>();
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
