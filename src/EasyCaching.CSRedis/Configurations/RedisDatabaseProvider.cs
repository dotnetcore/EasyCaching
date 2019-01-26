namespace EasyCaching.CSRedis
{    
    using global::CSRedis;
    using Microsoft.Extensions.Options;

    public class RedisDatabaseProvider : IRedisDatabaseProvider
    {
        /// <summary>
        /// The options.
        /// </summary>
        private readonly CSRedisDBOptions _options;

        /// <summary>
        /// The client.
        /// </summary>
        private readonly CSRedisClient _client;

        /// <summary>
        /// The name.
        /// </summary>
        private readonly string _name;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:EasyCaching.CSRedis.RedisDatabaseProvider"/> class.
        /// </summary>
        /// <param name="options">Options.</param>
        public RedisDatabaseProvider(IOptionsMonitor<RedisOptions> options)
        {
            _options = options.CurrentValue.DBConfig;
            _client = new CSRedisClient(_options.NodeRule, _options.ConnectionStrings.ToArray());
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:EasyCaching.CSRedis.RedisDatabaseProvider"/> class.
        /// </summary>
        /// <param name="name">Name.</param>
        /// <param name="options">Options.</param>
        public RedisDatabaseProvider(string name, RedisOptions options)
        {
            _name = name;
            _options = options.DBConfig;
            _client = new CSRedisClient(_options.NodeRule, _options.ConnectionStrings.ToArray());
        }

        /// <summary>
        /// Gets the name of the DBP rovider.
        /// </summary>
        /// <value>The name of the DBP rovider.</value>
        public string DBProviderName => this._name;

        /// <summary>
        /// Gets the client.
        /// </summary>
        /// <returns>The client.</returns>
        public CSRedisClient GetClient()
        {
            return _client;
        }
    }
}
