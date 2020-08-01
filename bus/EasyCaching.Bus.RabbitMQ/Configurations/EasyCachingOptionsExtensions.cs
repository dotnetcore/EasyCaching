namespace Microsoft.Extensions.DependencyInjection
{
    using System;
    using EasyCaching.Bus.RabbitMQ;
    using EasyCaching.Core;
    using EasyCaching.Core.Configurations;
    using Microsoft.Extensions.Configuration;

    /// <summary>
    /// EasyCaching options extensions.
    /// </summary>
    public static class EasyCachingOptionsExtensions
    {
        /// <summary>
        /// Withs the RabbitMQ bus (specify the config via hard code).
        /// </summary>
        /// <param name="options">Options.</param>
        /// <param name="configure">Configure bus settings.</param>
        public static EasyCachingOptions WithRabbitMQBus(
            this EasyCachingOptions options
            , Action<RabbitMQBusOptions> configure
            )
        {
            ArgumentCheck.NotNull(configure, nameof(configure));

            options.RegisterExtension(new RabbitMQBusOptionsExtension(configure));
            return options;
        }

        /// <summary>
        /// Withs the RabbitMQ bus (read config from configuration file).
        /// </summary>
        /// <param name="options">Options.</param>
        /// <param name="configuration">The configuration.</param>
        /// <param name="sectionName">The section name in the configuration file.</param>
        public static EasyCachingOptions WithRabbitMQBus(
            this EasyCachingOptions options
            , IConfiguration configuration
            , string sectionName = EasyCachingConstValue.RabbitMQBusSection
            )
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
                //x.RouteKey = busOptions.RouteKey;
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
