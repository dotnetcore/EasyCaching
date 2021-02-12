namespace EasyCaching.Redis
{
    using EasyCaching.Core.Configurations;

    /// <summary>
    /// Redis cache options.
    /// </summary>
    public class RedisDBOptions : BaseRedisOptions
    {
        /// <summary>
        /// Gets or sets the Redis database index the cache will use.
        /// </summary>
        /// <value>
        /// The database.
        /// </value>
        public int Database { get; set; } = 0;

        /// <summary>
        /// Specifies the time in milliseconds that the system should allow for asynchronous operations (defaults to SyncTimeout)
        /// </summary>
        public int AsyncTimeout { get; set; }

        /// <summary>
        /// Specifies the time in milliseconds that the system should allow for synchronous operations (defaults to 5 seconds)
        /// </summary>
        public int SyncTimeout { get; set; }
    }
}
