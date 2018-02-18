namespace EasyCaching.Bus.RabbitMQ
{
    using EasyCaching.Core;
    using EasyCaching.Core.Internal;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;
    using System;

    /// <summary>
    /// RabbitMQ Bus service collection extensions.
    /// </summary>
    public static class RabbitMQBusServiceCollectionExtensions
    {
        /// <summary>
        /// Adds the default rabbitMQ Bus.
        /// </summary>
        /// <returns>The default rabbit MQB us.</returns>
        /// <param name="services">Services.</param>
        /// <param name="optionsAction">Options action.</param>
        public static IServiceCollection AddDefaultRabbitMQBus(this IServiceCollection services, Action<RabbitMQBusOptions> optionsAction)
        {
            ArgumentCheck.NotNull(services, nameof(services));
            ArgumentCheck.NotNull(optionsAction, nameof(optionsAction));

            var options = new RabbitMQBusOptions();
            optionsAction?.Invoke(options);
            services.AddSingleton(options);

            services.TryAddSingleton<IEasyCachingSerializer, DefaultBinaryFormatterSerializer>();
            services.TryAddSingleton<IConnectionChannelPool, ConnectionChannelPool>();
            services.TryAddSingleton<IEasyCachingBus, DefaultRabbitMQBus>();
            services.TryAddSingleton<IEasyCachingSerializer, DefaultBinaryFormatterSerializer>();

            return services;
        }
    }
}
