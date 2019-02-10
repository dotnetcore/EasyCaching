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
        public string RouteKey { get; set; } = "rmq.queue.undurable.easycaching.subscriber.*";

        /// <summary>
        /// Gets or sets the name of the queue.
        /// </summary>
        /// <value>The name of the queue.</value>
        public string QueueName { get; set; } = "";
    }
}
