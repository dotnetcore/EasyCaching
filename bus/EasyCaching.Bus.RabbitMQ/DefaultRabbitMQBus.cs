namespace EasyCaching.Bus.RabbitMQ
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using EasyCaching.Core;
    using EasyCaching.Core.Bus;
    using EasyCaching.Core.Serialization;
    using global::RabbitMQ.Client;
    using global::RabbitMQ.Client.Events;
    using Microsoft.Extensions.ObjectPool;
    using Microsoft.Extensions.Options;

    /// <summary>
    /// Default RabbitMQ Bus.
    /// </summary>
    public class DefaultRabbitMQBus : EasyCachingAbstractBus
    {
        /// <summary>
        /// The subscriber connection.
        /// </summary>
        private readonly IConnection _subConnection;

        /// <summary>
        /// The publish channel pool.
        /// </summary>
        private readonly ObjectPool<IModel> _pubChannelPool;

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
            IPooledObjectPolicy<IModel> _objectPolicy
            , IOptions<RabbitMQBusOptions> rabbitMQOptions
            , IEasyCachingSerializer serializer)
        {
            this._options = rabbitMQOptions.Value;
            this._serializer = serializer;

            var factory = new ConnectionFactory
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

            _subConnection = factory.CreateConnection();

            _pubChannelPool = new DefaultObjectPool<IModel>(_objectPolicy);
            
            _busId = Guid.NewGuid().ToString("N");

            BusName = "easycachingbus";
        }

        /// <summary>
        /// Publish the specified topic and message.
        /// </summary>
        /// <param name="topic">Topic.</param>
        /// <param name="message">Message.</param>
        public override void BasePublish(string topic, EasyCachingMessage message)
        {
            var channel = _pubChannelPool.Get();

            try
            {
                var body = _serializer.Serialize(message);

                channel.ExchangeDeclare(_options.TopicExchangeName, ExchangeType.Topic, true, false, null);
                channel.BasicPublish(_options.TopicExchangeName, topic, false, null, body);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                _pubChannelPool.Return(channel);
            }
        }

        /// <summary>
        /// Publish the specified topic and message async.
        /// </summary>
        /// <returns>The async.</returns>
        /// <param name="topic">Topic.</param>
        /// <param name="message">Message.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        public override Task BasePublishAsync(string topic, EasyCachingMessage message, CancellationToken cancellationToken = default(CancellationToken))
        {
            var channel = _pubChannelPool.Get();
            try
            {
                var body = _serializer.Serialize(message);

                channel.ExchangeDeclare(_options.TopicExchangeName, ExchangeType.Topic, true, false, null);
                channel.BasicPublish(_options.TopicExchangeName, topic, false, null, body);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                _pubChannelPool.Return(channel);
            }
            return Task.CompletedTask;
        }

        /// <summary>
        /// Subscribe the specified topic and action.
        /// </summary>
        /// <param name="topic">Topic.</param>
        /// <param name="action">Action.</param>
        public override void BaseSubscribe(string topic, Action<EasyCachingMessage> action)
        {
            var queueName = string.Empty;
            if (string.IsNullOrWhiteSpace(_options.QueueName))
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
                model.QueueBind(queueName, _options.TopicExchangeName, topic);
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
            var message = _serializer.Deserialize<EasyCachingMessage>(e.Body.ToArray());

            BaseOnMessage(message);
        }
    }
}
