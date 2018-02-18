namespace EasyCaching.Bus.Redis
{    
    using EasyCaching.Core;
    using EasyCaching.Core.Internal;
    using StackExchange.Redis;
    using System.Threading.Tasks;

    public class DefaultRedisBus : IEasyCachingBus
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
        /// The local caching provider.
        /// </summary>
        private readonly IEasyCachingProvider _localCachingProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:EasyCaching.Sync.Redis.DefaultRedisSubscriber"/> class.
        /// </summary>
        /// <param name="subscriberProvider">Subscriber provider.</param>
        public DefaultRedisBus(
            IRedisSubscriberProvider subscriberProvider,
            IEasyCachingSerializer serializer,
            IEasyCachingProvider localCachingProvider)
        {
            this._subscriberProvider = subscriberProvider;
            this._serializer = serializer;
            this._localCachingProvider = localCachingProvider;

            this._subscriber = _subscriberProvider.GetSubscriber();
        }

        /// <summary>
        /// Subscribe the specified channel and notifyType.
        /// </summary>
        /// <returns>The subscribe.</returns>
        /// <param name="channel">Channel.</param>
        public void Subscribe(string channel)
        {
            _subscriber.Subscribe(channel,SubscribeHandle);
        }

        /// <summary>
        /// Subscribes the handle.
        /// </summary>
        /// <param name="channel">Channel.</param>
        /// <param name="value">Value.</param>
        private void SubscribeHandle(RedisChannel channel, RedisValue value)
        {
            var message = _serializer.Deserialize<EasyCachingMessage>(value);

            switch(message.NotifyType)
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

        /// <summary>
        /// Subscribes the async.
        /// </summary>
        /// <returns>The async.</returns>
        /// <param name="channel">Channel.</param>
        public async Task SubscribeAsync(string channel)
        {
            await _subscriber.SubscribeAsync(channel, SubscribeHandle);           
        }

        /// <summary>
        /// Publish the specified channel and message.
        /// </summary>
        /// <returns>The publish.</returns>
        /// <param name="channel">Channel.</param>
        /// <param name="message">Message.</param>
        public void Publish(string channel, EasyCachingMessage message)
        {
            ArgumentCheck.NotNullOrWhiteSpace(channel, nameof(channel));

            _subscriber.Publish(channel, _serializer.Serialize(message));
        }

        /// <summary>
        /// Publishs the async.
        /// </summary>
        /// <returns>The async.</returns>
        /// <param name="channel">Channel.</param>
        /// <param name="message">Message.</param>
        public async Task PublishAsync(string channel, EasyCachingMessage message)
        {
            ArgumentCheck.NotNullOrWhiteSpace(channel, nameof(channel));

            await _subscriber.PublishAsync(channel, _serializer.Serialize(message));
        }
    }
}
