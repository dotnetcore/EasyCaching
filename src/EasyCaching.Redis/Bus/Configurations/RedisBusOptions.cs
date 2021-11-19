namespace EasyCaching.Redis
{
    using EasyCaching.Core.Bus;
    using EasyCaching.Core.Configurations;
    using EasyCaching.Core.Decoration;

    /// <summary>
    /// Redis bus options.
    /// </summary>
    public class RedisBusOptions : IBusOptions
    {
        public string ConnectionString { get; set; }
        
        /// <summary>
        /// Gets or sets the serializer name that should be use in this bus.
        /// </summary>
        public string SerializerName { get; set; }

        public BusFactoryDecorator BusFactoryDecorator { get; set; }
    }
}
