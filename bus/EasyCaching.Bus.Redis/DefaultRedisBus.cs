namespace EasyCaching.Bus.Redis
{
    using EasyCaching.Core;
    using EasyCaching.Core.Bus;
    using EasyCaching.Core.Serialization;
    using StackExchange.Redis;
    using System;
    using System.Collections.Generic;
    using System.Linq;
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
        /// The name.
        /// </summary>
        private readonly string _name;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:EasyCaching.Bus.Redis.DefaultRedisBus"/> class.
        /// </summary>
        /// <param name="name">Name.</param>
        /// <param name="subscriberProviders">Subscriber provider.</param>
        /// <param name="busOptions">bus Options.</param>
        /// <param name="serializers">Serializers.</param>
        public DefaultRedisBus(
            string name
            , IEnumerable<IRedisSubscriberProvider> subscriberProviders
            , RedisBusOptions busOptions
            , IEnumerable<IEasyCachingSerializer> serializers)
        {
            this._name = name;
            this.BusName = name;
            this._subscriberProvider = subscriberProviders.Single(x => x.SubscriberName.Equals(name));
            this._serializer = !string.IsNullOrWhiteSpace(busOptions.SerializerName)
                ? serializers.Single(x => x.Name.Equals(busOptions.SerializerName))
                : serializers.Single(x => x.Name.Equals(EasyCachingConstValue.DefaultSerializerName));
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
