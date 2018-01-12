namespace EasyCaching.Redis
{
    using Microsoft.Extensions.Options;
    using StackExchange.Redis;
    using System;

    public class RedisDatabaseProvider : IRedisDatabaseProvider
    {
        private readonly RedisCacheOptions _options;
        private readonly Lazy<ConnectionMultiplexer> _connectionMultiplexer;

        public RedisDatabaseProvider(IOptions<RedisCacheOptions> options)
        {
            _options = options.Value;
            _connectionMultiplexer = new Lazy<ConnectionMultiplexer>(CreateConnectionMultiplexer);
        }

        /// <summary>
        /// Gets the database connection.
        /// </summary>
        public IDatabase GetDatabase()
        {
            return _connectionMultiplexer.Value.GetDatabase(_options.Database);
        }

        private ConnectionMultiplexer CreateConnectionMultiplexer()
        {
            var configurationOptions = new ConfigurationOptions
            {
                ConnectTimeout = _options.ConnectionTimeout,
                Password = _options.Password,
                Ssl = _options.IsSsl,
                SslHost = _options.SslHost,
            };

            foreach (var endpoint in _options.Endpoints)
            {
                configurationOptions.EndPoints.Add(endpoint.Host, endpoint.Port);
            }

            return ConnectionMultiplexer.Connect(configurationOptions.ToString());
        }
    }
}
