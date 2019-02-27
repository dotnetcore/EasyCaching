namespace EasyCaching.Bus.Redis
{
    using EasyCaching.Core;
    using EasyCaching.Core.Bus;
    using EasyCaching.Core.Serialization;
    using StackExchange.Redis;
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Default redis bus.
    /// </summary>
    public class DefaultRedisBus : EasyCachingAbstractBus
    {
        /// <summary>
        /// The subscriber.
        /// </summary>
        private readonly ISubscriber _subscriber;

        /// <summary>
        /// The subscriber provider.
        /// </summary>
        private readonly IRedisSubscriberProvider _subscriberProvider;

        /// <summary>
        /// The serializer.
        /// </summary>
        private readonly IEasyCachingSerializer _serializer;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:EasyCaching.Bus.Redis.DefaultRedisBus"/> class.
        /// </summary>
        /// <param name="subscriberProvider">Subscriber provider.</param>
        /// <param name="serializer">Serializer.</param>
        public DefaultRedisBus(
            IRedisSubscriberProvider subscriberProvider,
            IEasyCachingSerializer serializer)
        {
            this._subscriberProvider = subscriberProvider;
            this._serializer = serializer;
            this._subscriber = _subscriberProvider.GetSubscriber();
        }

        /// <summary>
        /// Subscribes the handle.
        /// </summary>
        /// <param name="channel">Channel.</param>
        /// <param name="value">Value.</param>
        private void OnMessage(RedisChannel channel, RedisValue value)
        {
            var message = _serializer.Deserialize<EasyCachingMessage>(value);

            BaseOnMessage(message);
        }

        /// <summary>
        /// Publish the specified topic and message.
        /// </summary>
        /// <param name="topic">Topic.</param>
        /// <param name="message">Message.</param>
        public override void BasePublish(string topic, EasyCachingMessage message)
        {
            ArgumentCheck.NotNullOrWhiteSpace(topic, nameof(topic));

            _subscriber.Publish(topic, _serializer.Serialize(message));
        }

        /// <summary>
        /// Publishs the specified topic and message async.
        /// </summary>
        /// <returns>The async.</returns>
        /// <param name="topic">Topic.</param>
        /// <param name="message">Message.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        public override async Task BasePublishAsync(string topic, EasyCachingMessage message, CancellationToken cancellationToken = default(CancellationToken))
        {
            ArgumentCheck.NotNullOrWhiteSpace(topic, nameof(topic));

            await _subscriber.PublishAsync(topic, _serializer.Serialize(message));
        }

        /// <summary>
        /// Subscribe the specified topic and action.
        /// </summary>
        /// <param name="topic">Topic.</param>
        /// <param name="action">Action.</param>
        public override void BaseSubscribe(string topic, Action<EasyCachingMessage> action)
        {
            _subscriber.Subscribe(topic, OnMessage);
        }
    }
}
