namespace EasyCaching.Bus.RabbitMQ
{    
    using global::RabbitMQ.Client;

    /// <summary>
    /// Connection channel pool.
    /// </summary>
    public interface IConnectionChannelPool
    {
        /// <summary>
        /// Gets the connection.
        /// </summary>
        /// <returns>The connection.</returns>
        IConnection GetConnection();

        /// <summary>
        /// Rent this instance.
        /// </summary>
        /// <returns>The rent.</returns>
        IModel Rent();

        /// <summary>
        /// Return the specified context.
        /// </summary>
        /// <returns>The return.</returns>
        /// <param name="context">Context.</param>
        bool Return(IModel context);
    }
}
