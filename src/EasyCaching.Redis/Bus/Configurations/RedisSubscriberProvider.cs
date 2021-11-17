namespace EasyCaching.Redis
{
    using StackExchange.Redis;
    using System;

    /// <summary>
    /// Redis database provider.
    /// </summary>
    internal class RedisSubscriberProvider : IRedisSubscriberProvider
    {
        private readonly string _name;

        /// <summary>
        /// The options.
        /// </summary>
        private readonly RedisBusOptions _options;

        private readonly ConnectionMultiplexerProvider _connectionMultiplexerProvider;

        /// <summary>
        /// The connection multiplexer.
        /// </summary>
        private readonly Lazy<ConnectionMultiplexer> _connectionMultiplexer;

        public RedisSubscriberProvider(
            string name, 
            RedisBusOptions options,
            ConnectionMultiplexerProvider connectionMultiplexerProvider)
        {
            _name = name;
            _options = options;
            _connectionMultiplexerProvider = connectionMultiplexerProvider;
            _connectionMultiplexer = new Lazy<ConnectionMultiplexer>(CreateConnectionMultiplexer);
        }

        public string SubscriberName => _name;

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
                    User = _options.Username,
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

                return _connectionMultiplexerProvider.GetConnectionMultiplexer(configurationOptions.ToString());
            }
            else
            {
                return _connectionMultiplexerProvider.GetConnectionMultiplexer(_options.Configuration);
            }
        }
    }
}
