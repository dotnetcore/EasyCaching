namespace EasyCaching.Bus.RabbitMQ
{
    using System;
    using EasyCaching.Core.Configurations;
    using Microsoft.Extensions.Configuration;

    /// <summary>
    /// EasyCaching options extensions.
    /// </summary>
    public static class EasyCachingOptionsExtensions
    {
        /// <summary>
        /// Withs the RabbitMQ Bus.
        /// </summary>
        /// <returns>The rabbit MQB us.</returns>
        /// <param name="options">Options.</param>
        /// <param name="configure">Configure.</param>
        public static EasyCachingOptions WithRabbitMQBus(this EasyCachingOptions options, Action<RabbitMQBusOptions> configure)
        {
            if (configure == null)
            {
                throw new ArgumentNullException(nameof(configure));
            }

            options.RegisterExtension(new RabbitMQBusOptionsExtension(configure));
            return options;
        }

        /// <summary>
        /// Withs the RabbitMQ Bus.
        /// </summary>
        /// <returns>The rabbit MQB us.</returns>
        /// <param name="options">Options.</param>
        /// <param name="configuration">Configuration.</param>
        /// <param name="sectionName">Section name.</param>
        public static EasyCachingOptions WithRabbitMQBus(this EasyCachingOptions options, IConfiguration configuration, string sectionName = "rabbitmqbus")
        {
            var dbConfig = configuration.GetSection(sectionName);
            var busOptions = new RabbitMQBusOptions();
            dbConfig.Bind(busOptions);

            void configure(RabbitMQBusOptions x)
            {
                x.HostName = busOptions.HostName;
                x.Password = busOptions.Password;
                x.Port = busOptions.Port;
                x.QueueMessageExpires = busOptions.QueueMessageExpires;
                x.RequestedConnectionTimeout = busOptions.RequestedConnectionTimeout;
                x.RouteKey = busOptions.RouteKey;
                x.SocketReadTimeout = busOptions.SocketReadTimeout;
                x.SocketWriteTimeout = busOptions.SocketWriteTimeout;
                x.TopicExchangeName = busOptions.TopicExchangeName;
                x.UserName = busOptions.UserName;
                x.VirtualHost = busOptions.VirtualHost;
                x.QueueName = busOptions.QueueName;
            }

            options.RegisterExtension(new RabbitMQBusOptionsExtension(configure));
            return options;
        }
    }
}
