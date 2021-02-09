namespace EasyCaching.Bus.RabbitMQ
{
    using System;
    using EasyCaching.Core.Bus;
    using EasyCaching.Core.Decoration;
    using EasyCaching.Core.Configurations;
    using EasyCaching.Core.Serialization;
    using global::RabbitMQ.Client;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;
    using Microsoft.Extensions.ObjectPool;
    using Microsoft.Extensions.Options;

    /// <summary>
    /// RabbitMQ Bus options extension.
    /// </summary>
    public class RabbitMQBusOptionsExtension : IEasyCachingOptionsExtension
    {
        private readonly string _name;

        /// <summary>
        /// The configure.
        /// </summary>
        private readonly Action<RabbitMQBusOptions> _configure;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:EasyCaching.Bus.RabbitMQ.RabbitMQBusOptionsExtension"/> class.
        /// </summary>
        /// <param name="name">Unique name of the bus.</param>
        /// <param name="configure">Configure.</param>
        public RabbitMQBusOptionsExtension(string name, Action<RabbitMQBusOptions> configure)
        {
            _name = name;
            _configure = configure;
        }

        /// <summary>
        /// Adds the services.
        /// </summary>
        /// <param name="services">Services.</param>
        public void AddServices(IServiceCollection services)
        {
            services.AddOptions();
            services.Configure(_configure);

            services.TryAddSingleton<IEasyCachingSerializer, DefaultBinaryFormatterSerializer>();            
            services.AddSingleton<IPooledObjectPolicy<IModel>, ModelPooledObjectPolicy>();

            services.AddSingleton<IEasyCachingBus>(serviceProvider =>
            {
                var objectPolicy = serviceProvider.GetRequiredService<IPooledObjectPolicy<IModel>>();
                var optionsMon = serviceProvider.GetRequiredService<IOptionsMonitor<RabbitMQBusOptions>>();
                var options = optionsMon.Get(_name);
                var wrappedOptions = serviceProvider.GetRequiredService<IOptions<RabbitMQBusOptions>>();
                var serializer = serviceProvider.GetRequiredService<IEasyCachingSerializer>();
                    
                return options.CreateDecoratedBus(
                    _name,
                    serviceProvider,
                    () => new DefaultRabbitMQBus(_name, objectPolicy, wrappedOptions, serializer));
            });
        }
    }
}
