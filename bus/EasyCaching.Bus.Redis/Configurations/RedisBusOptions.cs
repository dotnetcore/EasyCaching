namespace EasyCaching.Bus.Redis
{
    using EasyCaching.Core.Configurations;

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
    }
}
