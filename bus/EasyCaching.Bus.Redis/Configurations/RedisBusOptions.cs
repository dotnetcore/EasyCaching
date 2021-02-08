namespace EasyCaching.Bus.Redis
{
    using EasyCaching.Core.Bus;
    using EasyCaching.Core.Configurations;
    using EasyCaching.Core.Decoration;

    /// <summary>
    /// Redis bus options.
    /// </summary>
    public class RedisBusOptions : BaseRedisOptions, IBusOptions
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

        public BusFactoryDecorator BusFactoryDecorator { get; set; }
    }
}
