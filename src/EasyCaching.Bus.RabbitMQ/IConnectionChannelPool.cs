namespace EasyCaching.Bus.RabbitMQ
{    
    using global::RabbitMQ.Client;

    public interface IConnectionChannelPool
    {
        IConnection GetConnection();

        IModel Rent();

        bool Return(IModel context);
    }
}
