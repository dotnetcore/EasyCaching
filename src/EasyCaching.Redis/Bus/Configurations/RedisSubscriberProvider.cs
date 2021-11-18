namespace EasyCaching.Redis
{
    using StackExchange.Redis;
    using System;

    /// <summary>
    /// Redis database provider.
    /// </summary>
    internal class RedisSubscriberProvider : IRedisSubscriberProvider
    {
        /// <summary>
        /// The connection multiplexer.
        /// </summary>
        private readonly Lazy<ConnectionMultiplexer> _connectionMultiplexer;

        public RedisSubscriberProvider(
            string name, 
            RedisBusOptions options,
            ConnectionMultiplexerProvider connectionMultiplexerProvider)
        {
            SubscriberName = name;
            _connectionMultiplexer = new Lazy<ConnectionMultiplexer>(() => 
                connectionMultiplexerProvider.GetConnectionMultiplexer(options));
        }

        public string SubscriberName { get; }

        /// <summary>
        /// Gets the database connection.
        /// </summary>
        public ISubscriber GetSubscriber()
        {
            return _connectionMultiplexer.Value.GetSubscriber();
        }
    }
}
