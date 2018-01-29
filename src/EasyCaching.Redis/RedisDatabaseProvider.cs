namespace EasyCaching.Redis
{
    using Microsoft.Extensions.Options;
    using StackExchange.Redis;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;

    /// <summary>
    /// Redis database provider.
    /// </summary>
    public class RedisDatabaseProvider : IRedisDatabaseProvider
    {
        /// <summary>
        /// The options.
        /// </summary>
        private readonly RedisCacheOptions _options;

        /// <summary>
        /// The connection multiplexer.
        /// </summary>
        private readonly Lazy<ConnectionMultiplexer> _connectionMultiplexer;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:EasyCaching.Redis.RedisDatabaseProvider"/> class.
        /// </summary>
        /// <param name="options">Options.</param>
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

        /// <summary>
        /// Gets the server list.
        /// </summary>
        /// <returns>The server list.</returns>
        public IEnumerable<IServer> GetServerList()
        {
            var endpoints = GetMastersServersEndpoints();

            foreach (var endpoint in endpoints)
            {
                yield return _connectionMultiplexer.Value.GetServer(endpoint);
            }
        }

        /// <summary>
        /// Creates the connection multiplexer.
        /// </summary>
        /// <returns>The connection multiplexer.</returns>
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

        /// <summary>
        /// Gets the masters servers endpoints.
        /// </summary>
        private List<EndPoint> GetMastersServersEndpoints()
        {
            var masters = new List<EndPoint>();
            foreach (var ep in _connectionMultiplexer.Value.GetEndPoints())
            {
                var server = _connectionMultiplexer.Value.GetServer(ep);
                if (server.IsConnected)
                {
                    //Cluster
                    if (server.ServerType == ServerType.Cluster)
                    {
                        masters.AddRange(server.ClusterConfiguration.Nodes.Where(n => !n.IsSlave).Select(n => n.EndPoint));
                        break;
                    }
                    // Single , Master-Slave
                    if (server.ServerType == ServerType.Standalone && !server.IsSlave)
                    {
                        masters.Add(ep);
                        break;
                    }
                }
            }
            return masters;
        }
    }
}