namespace EasyCaching.Bus.RabbitMQ
{
    using System;
    using System.Collections.Concurrent;
    using System.Threading;
    using global::RabbitMQ.Client;

    /// <summary>
    /// Connection channel pool.
    /// </summary>
    public class ConnectionChannelPool : IConnectionChannelPool, IDisposable
    {
        /// <summary>
        /// The default size of the pool.
        /// </summary>
        private const int DefaultPoolSize = 15;
        /// <summary>
        /// The connection activator.
        /// </summary>
        private readonly Func<IConnection> _connectionActivator;
        /// <summary>
        /// The pool.
        /// </summary>
        private readonly ConcurrentQueue<IModel> _pool = new ConcurrentQueue<IModel>();
        /// <summary>
        /// The connection.
        /// </summary>
        private IConnection _connection;
        /// <summary>
        /// The count.
        /// </summary>
        private int _count;
        /// <summary>
        /// The size of the max.
        /// </summary>
        private int _maxSize;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:EasyCaching.Bus.RabbitMQ.ConnectionChannelPool"/> class.
        /// </summary>
        /// <param name="options">Options.</param>
        public ConnectionChannelPool(RabbitMQBusOptions options)
        {
            _maxSize = DefaultPoolSize;

            _connectionActivator = CreateConnection(options);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        IModel IConnectionChannelPool.Rent()
        {
            return Rent();
        }

        /// <summary>
        /// Easies the caching. bus. rabbit MQ . IC onnection channel pool. return.
        /// </summary>
        /// <returns><c>true</c>, if caching. bus. rabbit MQ . IC onnection channel pool. return was easyed, <c>false</c> otherwise.</returns>
        /// <param name="connection">Connection.</param>
        bool IConnectionChannelPool.Return(IModel connection)
        {
            return Return(connection);
        }

        /// <summary>
        /// Gets the connection.
        /// </summary>
        /// <returns>The connection.</returns>
        public IConnection GetConnection()
        {
            if (_connection != null && _connection.IsOpen)
                return _connection;
            _connection = _connectionActivator();
            _connection.ConnectionShutdown += RabbitMQ_ConnectionShutdown;
            return _connection;
        }

        /// <summary>
        /// Releases all resource used by the <see cref="T:EasyCaching.Bus.RabbitMQ.ConnectionChannelPool"/> object.
        /// </summary>
        /// <remarks>Call <see cref="Dispose"/> when you are finished using the
        /// <see cref="T:EasyCaching.Bus.RabbitMQ.ConnectionChannelPool"/>. The <see cref="Dispose"/> method leaves the
        /// <see cref="T:EasyCaching.Bus.RabbitMQ.ConnectionChannelPool"/> in an unusable state. After calling
        /// <see cref="Dispose"/>, you must release all references to the
        /// <see cref="T:EasyCaching.Bus.RabbitMQ.ConnectionChannelPool"/> so the garbage collector can reclaim the
        /// memory that the <see cref="T:EasyCaching.Bus.RabbitMQ.ConnectionChannelPool"/> was occupying.</remarks>
        public void Dispose()
        {
            _maxSize = 0;

            while (_pool.TryDequeue(out var context))
                context.Dispose();
        }

        /// <summary>
        /// Creates the connection.
        /// </summary>
        /// <returns>The connection.</returns>
        /// <param name="options">Options.</param>
        private static Func<IConnection> CreateConnection(RabbitMQBusOptions options)
        {
            var factory = new ConnectionFactory
            {
                HostName = options.HostName,
                UserName = options.UserName,
                Port = options.Port,
                Password = options.Password,
                VirtualHost = options.VirtualHost,
                RequestedConnectionTimeout = options.RequestedConnectionTimeout,
                SocketReadTimeout = options.SocketReadTimeout,
                SocketWriteTimeout = options.SocketWriteTimeout
            };

            return () => factory.CreateConnection();
        }

        /// <summary>
        /// Rabbits the mq connection shutdown.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">E.</param>
        private void RabbitMQ_ConnectionShutdown(object sender, ShutdownEventArgs e)
        {            
            Console.WriteLine("closed!");
        }

        /// <summary>
        /// Rent this instance.
        /// </summary>
        /// <returns>The rent.</returns>
        public virtual IModel Rent()
        {
            if (_pool.TryDequeue(out var model))
            {
                Interlocked.Decrement(ref _count);

                return model;
            }

            model = GetConnection().CreateModel();

            return model;
        }

        /// <summary>
        /// Return the specified connection.
        /// </summary>
        /// <returns>The return.</returns>
        /// <param name="connection">Connection.</param>
        public virtual bool Return(IModel connection)
        {
            if (Interlocked.Increment(ref _count) <= _maxSize)
            {
                _pool.Enqueue(connection);

                return true;
            }

            Interlocked.Decrement(ref _count);

            return false;
        }
    }

}
