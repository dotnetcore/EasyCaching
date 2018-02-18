namespace EasyCaching.Bus.RabbitMQ
{
    using EasyCaching.Core;
    using EasyCaching.Core.Internal;
    using System;
    using System.Threading.Tasks;
    using System.Collections.Generic;
    using global::RabbitMQ.Client.Events;
    using global::RabbitMQ.Client;

    /// <summary>
    /// Default RabbitMQ Bus.
    /// </summary>
    public class DefaultRabbitMQBus : IEasyCachingBus
    {
        /// <summary>
        /// The connection channel pool.
        /// </summary>
        private readonly IConnectionChannelPool _connectionChannelPool;

        /// <summary>
        /// The rabbitMQ Bus options.
        /// </summary>
        private readonly RabbitMQBusOptions _rabbitMQBusOptions;

        /// <summary>
        /// The serializer.
        /// </summary>
        private readonly IEasyCachingSerializer _serializer;

        /// <summary>
        /// The local caching provider.
        /// </summary>
        private readonly IEasyCachingProvider _localCachingProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:EasyCaching.Bus.RabbitMQ.DefaultRabbitMQBus"/> class.
        /// </summary>
        /// <param name="connectionChannelPool">Connection channel pool.</param>
        /// <param name="rabbitMQOptions">Rabbit MQO ptions.</param>
        /// <param name="serializer">Serializer.</param>
        /// <param name="localCachingProvider">Local caching provider.</param>
        public DefaultRabbitMQBus(
            IConnectionChannelPool connectionChannelPool,
            RabbitMQBusOptions rabbitMQOptions,
            IEasyCachingSerializer serializer,
            IEasyCachingProvider localCachingProvider)
        {
            this._rabbitMQBusOptions = rabbitMQOptions;
            this._connectionChannelPool = connectionChannelPool;
            this._serializer = serializer;
            this._localCachingProvider = localCachingProvider;
        }
             
        /// <summary>
        /// Publish the specified channel and message.
        /// </summary>
        /// <returns>The publish.</returns>
        /// <param name="channel">Channel.</param>
        /// <param name="message">Message.</param>
        public void Publish(string channel, EasyCachingMessage message)
        {
            var _publisherChannel = _connectionChannelPool.Rent();
            try
            {                          
                var body = _serializer.Serialize(message);

                _publisherChannel.ExchangeDeclare(_rabbitMQBusOptions.TopicExchangeName, ExchangeType.Topic, true, false, null);
                _publisherChannel.BasicPublish(_rabbitMQBusOptions.TopicExchangeName, channel, false, null, body);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                var returned = _connectionChannelPool.Return(_publisherChannel);
                if (!returned)
                    _publisherChannel.Dispose();
            }
        }

        /// <summary>
        /// Publishs the async.
        /// </summary>
        /// <returns>The async.</returns>
        /// <param name="channel">Channel.</param>
        /// <param name="message">Message.</param>
        public Task PublishAsync(string channel, EasyCachingMessage message)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Subscribe the specified channel.
        /// </summary>
        /// <returns>The subscribe.</returns>
        /// <param name="channel">Channel.</param>
        public void Subscribe(string channel)
        {
            var _connection = _connectionChannelPool.GetConnection();

            var _subscriberChannel = _connection.CreateModel();

            _subscriberChannel.ExchangeDeclare(_rabbitMQBusOptions.TopicExchangeName, ExchangeType.Topic, true, true, null);

            var arguments = new Dictionary<string, object> {
                { "x-message-ttl", _rabbitMQBusOptions.QueueMessageExpires }
            };

            _subscriberChannel.QueueDeclare("easycaching.queue", true, false, false, arguments);

            var consumer = new EventingBasicConsumer(_subscriberChannel);
            consumer.Received += OnConsumerReceived;
            //consumer.Shutdown += OnConsumerShutdown;

            _subscriberChannel.BasicConsume("easycaching.queue", false, string.Empty, false, false, null, consumer);
        }

        /// <summary>
        /// Subscribes the async.
        /// </summary>
        /// <returns>The async.</returns>
        /// <param name="channel">Channel.</param>
        public Task SubscribeAsync(string channel)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Consumers the received.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">E.</param>
        private void OnConsumerReceived(object sender, BasicDeliverEventArgs e)
        {
            var message = _serializer.Deserialize<EasyCachingMessage>(e.Body); 

            switch (message.NotifyType)
            {
                case NotifyType.Add:
                    _localCachingProvider.Set(message.CacheKey, message.CacheValue, message.Expiration);
                    break;
                case NotifyType.Update:
                    _localCachingProvider.Refresh(message.CacheKey, message.CacheValue, message.Expiration);
                    break;
                case NotifyType.Delete:
                    _localCachingProvider.Remove(message.CacheKey);
                    break;
            }
        }
    }
}
