namespace EasyCaching.Bus.RabbitMQ
{
    using Microsoft.Extensions.ObjectPool;
    using Microsoft.Extensions.Options;
    using global::RabbitMQ.Client;

    /// <summary>
    /// Connection pooled object policy.
    /// </summary>
    public class ConnectionPooledObjectPolicy : IPooledObjectPolicy<IConnection>
    {
        /// <summary>
        /// The options.
        /// </summary>
        private readonly RabbitMQBusOptions _options;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:EasyCaching.Bus.RabbitMQ.ConnectionPooledObjectPolicy"/> class.
        /// </summary>
        /// <param name="optionsAccs">Options accs.</param>
        public ConnectionPooledObjectPolicy(IOptions<RabbitMQBusOptions> optionsAccs)
        {
            this._options = optionsAccs.Value;
        }

        /// <summary>
        /// Create this instance.
        /// </summary>
        /// <returns>The create.</returns>
        public IConnection Create()
        {
            var factory = new ConnectionFactory
            {
                HostName = _options.HostName,
                UserName = _options.UserName,
                Port = _options.Port,
                Password = _options.Password,
                VirtualHost = _options.VirtualHost,
                RequestedConnectionTimeout = _options.RequestedConnectionTimeout,
                SocketReadTimeout = _options.SocketReadTimeout,
                SocketWriteTimeout = _options.SocketWriteTimeout
            };

            var connection = factory.CreateConnection();
            return connection;
        }

        /// <summary>
        /// Return the specified connection.
        /// </summary>
        /// <returns>The return.</returns>
        /// <param name="connection">Connection.</param>
        public bool Return(IConnection connection)
        {
            return true;
        }
    }
}