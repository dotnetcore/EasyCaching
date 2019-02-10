namespace EasyCaching.Core.Configurations
{
    public class BaseRabbitMQOptions
    {                        
        /// <summary>
        /// The host to connect to.
        /// </summary>
        public string HostName { get; set; } = "localhost";

        /// <summary>
        /// Password to use when authenticating to the server.
        /// </summary>
        public string Password { get; set; } = "guest";

        /// <summary>
        /// Username to use when authenticating to the server.
        /// </summary>
        public string UserName { get; set; } = "guest";

        /// <summary>
        /// Virtual host to access during this connection.
        /// </summary>
        public string VirtualHost { get; set; } = "/";

        /// <summary>
        /// Topic exchange name when declare a topic exchange.
        /// </summary>
        public string TopicExchangeName { get; set; } = "rmq.exchange.topic.easycaching";

        /// <summary>
        /// Timeout setting for connection attempts (in milliseconds).
        /// </summary>
        public int RequestedConnectionTimeout { get; set; } = 30 * 1000;

        /// <summary>
        /// Timeout setting for socket read operations (in milliseconds).
        /// </summary>
        public int SocketReadTimeout { get; set; } = 30 * 1000;

        /// <summary>
        /// Timeout setting for socket write operations (in milliseconds).
        /// </summary>
        public int SocketWriteTimeout { get; set; } = 30 * 1000;

        /// <summary>
        /// The port to connect on.
        /// </summary>
        public int Port { get; set; } = -1;

        /// <summary>
        /// Gets or sets queue message automatic deletion time (in milliseconds). Default 864000000 ms (10 days).
        /// </summary>
        public int QueueMessageExpires { get; set; } = 864000000;
    }
}
