namespace EasyCaching.Bus.RabbitMQ
{
    using global::RabbitMQ.Client;
    using Microsoft.Extensions.ObjectPool;
    using Microsoft.Extensions.Options;

    public class ModelPooledObjectPolicy : IPooledObjectPolicy<IModel>
    {
          
        private readonly RabbitMQBusOptions _options;        

        private readonly IConnection _connection;

        public ModelPooledObjectPolicy(IOptions<RabbitMQBusOptions> optionsAccs)
        {
            _options = optionsAccs.Value;
            _connection = GetConnection();
        }

        private IConnection GetConnection()
        {
            var factory = new ConnectionFactory()
            {
                HostName = _options.HostName,
                UserName = _options.UserName,
                Port = _options.Port,
                Password = _options.Password,
                VirtualHost = _options.VirtualHost,
                RequestedConnectionTimeout = System.TimeSpan.FromMilliseconds(_options.RequestedConnectionTimeout),
                SocketReadTimeout = System.TimeSpan.FromMilliseconds(_options.SocketReadTimeout),
                SocketWriteTimeout = System.TimeSpan.FromMilliseconds(_options.SocketWriteTimeout)
            };

            return factory.CreateConnection();
        }

        public IModel Create()
        {
            return _connection.CreateModel();
        }

        public bool Return(IModel obj)
        {
            if (obj.IsOpen)
            {
                return true;
            }
            else
            {
                obj?.Dispose();
                return false;
            }
        }
    }
}
