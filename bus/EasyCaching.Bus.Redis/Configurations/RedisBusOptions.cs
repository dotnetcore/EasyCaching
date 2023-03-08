namespace EasyCaching.Bus.Redis
{
    using EasyCaching.Core.Configurations;
    using StackExchange.Redis;

    /// <summary>
    /// Redis bus options.
    /// </summary>
    public class RedisBusOptions : BaseRedisOptions
    {
        /// <summary>
        /// Gets or sets the database.
        /// </summary>
        /// <value>The database.</value>
        public int Database { get; set; } = 0;

        /// <summary>
        /// Gets or sets the serializer name that should be use in this bus.
        /// </summary>
        public string SerializerName { get; set; }

        /// <summary>
        /// Gets or sets the Redis database ConfigurationOptions will use.
        /// </summary>
        public ConfigurationOptions ConfigurationOptions { get; set; }
    }
}
