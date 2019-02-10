namespace EasyCaching.Bus.Redis
{
    using Microsoft.Extensions.Options;
    using StackExchange.Redis;
    using System;

    /// <summary>
    /// Redis database provider.
    /// </summary>
    internal class RedisSubscriberProvider : IRedisSubscriberProvider
    {
        /// <summary>
        /// The options.
        /// </summary>
        private readonly RedisBusOptions _options;

        /// <summary>
        /// The connection multiplexer.
        /// </summary>
        private readonly Lazy<ConnectionMultiplexer> _connectionMultiplexer;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:EasyCaching.Bus.Redis.RedisSubscriberProvider"/> class.
        /// </summary>
        /// <param name="options">Options.</param>
        public RedisSubscriberProvider(IOptions<RedisBusOptions> options)
        {
            _options = options.Value;
            _connectionMultiplexer = new Lazy<ConnectionMultiplexer>(CreateConnectionMultiplexer);
        }

        /// <summary>
        /// Gets the database connection.
        /// </summary>
        public ISubscriber GetSubscriber()
        {
           return _connectionMultiplexer.Value.GetSubscriber();
        }

        /// <summary>
        /// Creates the connection multiplexer.
        /// </summary>
        /// <returns>The connection multiplexer.</returns>
        private ConnectionMultiplexer CreateConnectionMultiplexer()
        {
            if (string.IsNullOrWhiteSpace(_options.Configuration))
            {
                var configurationOptions = new ConfigurationOptions
                {
                    ConnectTimeout = _options.ConnectionTimeout,
                    Password = _options.Password,
                    Ssl = _options.IsSsl,
                    SslHost = _options.SslHost,
                    AllowAdmin = _options.AllowAdmin,
                    DefaultDatabase = _options.Database,
                    AbortOnConnectFail = _options.AbortOnConnectFail,
                };

                foreach (var endpoint in _options.Endpoints)
                {
                    configurationOptions.EndPoints.Add(endpoint.Host, endpoint.Port);
                }

                return ConnectionMultiplexer.Connect(configurationOptions.ToString());
            }
            else
            {
                return ConnectionMultiplexer.Connect(_options.Configuration);
            }
        }
    }
}
