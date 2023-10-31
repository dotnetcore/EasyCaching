namespace EasyCaching.Bus.RabbitMQStream
{
    using System;
    using EasyCaching.Bus.RabbitMQ;
    using EasyCaching.Core.Bus;
    using EasyCaching.Core.Configurations;
    using global::RabbitMQ.Client;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.ObjectPool;

    /// <summary>
    /// RabbitMQ Bus options extension.
    /// </summary>
    public class RabbitMQStreamBusOptionsExtension : IEasyCachingOptionsExtension
    {
        /// <summary>
        /// The configure.
        /// </summary>
        private readonly Action<RabbitMQBusOptions> configure;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:EasyCaching.Bus.RabbitMQ.RabbitMQBusOptionsExtension"/> class.
        /// </summary>
        /// <param name="configure">Configure.</param>
        public RabbitMQStreamBusOptionsExtension(Action<RabbitMQBusOptions> configure)
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

            services.AddSingleton<IPooledObjectPolicy<IModel>, ModelPooledObjectPolicy>();
            services.AddSingleton<IEasyCachingBus, DefaultRabbitMQStreamBus>();
        }
    }
}
