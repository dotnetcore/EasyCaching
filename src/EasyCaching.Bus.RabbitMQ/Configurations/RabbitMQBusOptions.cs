namespace EasyCaching.Bus.RabbitMQ
{
    using EasyCaching.Core.Configurations;

    /// <summary>
    /// RabbitMQ Bus options.
    /// </summary>
    public class RabbitMQBusOptions : BaseRabbitMQOptions
    {
        /// <summary>
        /// Gets or sets the route key.
        /// </summary>
        /// <value>The route key.</value>
        public string RouteKey { get; set; } = "easycaching.subscriber.*";
    }
}
