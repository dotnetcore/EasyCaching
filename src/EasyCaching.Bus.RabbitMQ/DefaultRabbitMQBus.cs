namespace EasyCaching.Bus.RabbitMQ
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using EasyCaching.Core.Bus;
    using EasyCaching.Core.Serialization;
    using global::RabbitMQ.Client;
    using global::RabbitMQ.Client.Events;
    using Microsoft.Extensions.ObjectPool;

    /// <summary>
    /// Default RabbitMQ Bus.
    /// </summary>
    public class DefaultRabbitMQBus : IEasyCachingBus
    {
        /// <summary>
        /// The subscriber connection.
        /// </summary>
        private readonly IConnection _subConnection;

        /// <summary>
        /// The publish connection pool.
        /// </summary>
        private readonly ObjectPool<IConnection> _pubConnectionPool;

        /// <summary>
        /// The handler.
        /// </summary>
        private Action<EasyCachingMessage> _handler;

        /// <summary>
        /// The rabbitMQ Bus options.
        /// </summary>
        private readonly RabbitMQBusOptions _options;

        /// <summary>
        /// The serializer.
        /// </summary>
        private readonly IEasyCachingSerializer _serializer;

        /// <summary>
        /// The identifier.
        /// </summary>
        private readonly string _busId;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:EasyCaching.Bus.RabbitMQ.DefaultRabbitMQBus"/> class.
        /// </summary>
        /// <param name="_objectPolicy">Object policy.</param>
        /// <param name="rabbitMQOptions">RabbitMQ Options.</param>
        /// <param name="serializer">Serializer.</param>
        public DefaultRabbitMQBus(
            IPooledObjectPolicy<IConnection> _objectPolicy
            ,RabbitMQBusOptions rabbitMQOptions
            ,IEasyCachingSerializer serializer)
        {
            this._options = rabbitMQOptions;
            this._serializer = serializer;

            var factory = new ConnectionFactory
            {
                HostName = _options.HostName,
                UserName = _options.UserName,
                Port = _options.Port,
                Password = _options.Password,
                VirtualHost = _options.VirtualHost,
                RequestedConnectionTimeout = _options.RequestedConnectionTimeout,
                SocketReadTimeout = _options.SocketReadTimeout,
                SocketWriteTimeout = _options.SocketWriteTimeout
            };

            _subConnection = factory.CreateConnection();

            _pubConnectionPool = new DefaultObjectPool<IConnection>(_objectPolicy);

            _busId = Guid.NewGuid().ToString("N");
        }

        /// <summary>
        /// Publish the specified topic and message.
        /// </summary>
        /// <param name="topic">Topic.</param>
        /// <param name="message">Message.</param>
        public void Publish(string topic, EasyCachingMessage message)
        {
            var conn = _pubConnectionPool.Get();

            try
            {
                var body = _serializer.Serialize(message);
                var model = conn.CreateModel();

                model.ExchangeDeclare(_options.TopicExchangeName, ExchangeType.Topic, true, false, null);
                model.BasicPublish(_options.TopicExchangeName, topic, false, null, body);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                _pubConnectionPool.Return(conn);
            }
        }

        /// <summary>
        /// Publish the specified topic and message async.
        /// </summary>
        /// <returns>The async.</returns>
        /// <param name="topic">Topic.</param>
        /// <param name="message">Message.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        public Task PublishAsync(string topic, EasyCachingMessage message, CancellationToken cancellationToken = default(CancellationToken))
        {
            var conn = _pubConnectionPool.Get();
            try
            {
                var body = _serializer.Serialize(message);
                var model = conn.CreateModel();

                model.ExchangeDeclare(_options.TopicExchangeName, ExchangeType.Topic, true, false, null);
                model.BasicPublish(_options.TopicExchangeName, topic, false, null, body);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                _pubConnectionPool.Return(conn);
            }
            return Task.CompletedTask;
        }
              
        /// <summary>
        /// Subscribe the specified topic and action.
        /// </summary>
        /// <param name="topic">Topic.</param>
        /// <param name="action">Action.</param>
        public void Subscribe(string topic, Action<EasyCachingMessage> action)
        {
            _handler = action;
            var queueName = string.Empty;
            if(string.IsNullOrWhiteSpace(_options.QueueName))
            {
                queueName = $"rmq.queue.undurable.easycaching.subscriber.{_busId}";
            }
            else
            {
                queueName = _options.QueueName;
            }

            Task.Factory.StartNew(() => 
            {
                var model = _subConnection.CreateModel();
                model.ExchangeDeclare(_options.TopicExchangeName, ExchangeType.Topic, true, false, null);
                model.QueueDeclare(queueName, false, false, true, null);
                // bind the queue with the exchange.
                model.QueueBind(_options.TopicExchangeName, queueName, _options.RouteKey);
                var consumer = new EventingBasicConsumer(model);
                consumer.Received += OnMessage;
                consumer.Shutdown += OnConsumerShutdown;

                model.BasicConsume(queueName, true, consumer);

            }, TaskCreationOptions.LongRunning);
        }

        /// <summary>
        /// Ons the consumer shutdown.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">E.</param>
        private void OnConsumerShutdown(object sender, ShutdownEventArgs e)
        {
            Console.WriteLine($"{e.ReplyText}");
        }

        /// <summary>
        /// Ons the message.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">E.</param>
        private void OnMessage(object sender, BasicDeliverEventArgs e)
        {
            var message = _serializer.Deserialize<EasyCachingMessage>(e.Body);

            _handler?.Invoke(message);
        }
    }
}
