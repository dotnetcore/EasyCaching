namespace EasyCaching.Sync.Redis
{
    using System;
    using System.Threading.Tasks;
    using EasyCaching.Core;
    using EasyCaching.Core.Internal;
    using StackExchange.Redis;

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
        /// Publish the specified channel, cacheKey, cacheValue and expiration.
        /// </summary>
        /// <returns>The publish.</returns>
        /// <param name="channel">Channel.</param>
        /// <param name="cacheKey">Cache key.</param>
        /// <param name="cacheValue">Cache value.</param>
        /// <param name="expiration">Expiration.</param>
        public void Publish<T>(string channel, string cacheKey, T cacheValue, TimeSpan expiration)
        {
            ArgumentCheck.NotNullOrWhiteSpace(channel, nameof(channel));

            //TODO : Handle Parameters
            EasyCachingMessage message = new EasyCachingMessage()
            {
                CacheKey = cacheKey,
                CacheValue = cacheValue,
                Expiration = expiration
            };

            _subscriber.Publish(channel, _serializer.Serialize(message));
        }

        /// <summary>
        /// Publishs the async.
        /// </summary>
        /// <returns>The async.</returns>
        /// <param name="channel">Channel.</param>
        /// <param name="cacheKey">Cache key.</param>
        /// <param name="cacheValue">Cache value.</param>
        /// <param name="expiration">Expiration.</param>
        public async Task PublishAsync<T>(string channel, string cacheKey, T cacheValue, TimeSpan expiration)
        {
            ArgumentCheck.NotNullOrWhiteSpace(channel, nameof(channel));

            //TODO : Handle Parameters
            EasyCachingMessage message = new EasyCachingMessage()
            {
                CacheKey = cacheKey,
                CacheValue = cacheValue,
                Expiration = expiration
            };

            await _subscriber.PublishAsync(channel, _serializer.Serialize(message));
        }

        /// <summary>
        /// Subscribe the specified channel and notifyType.
        /// </summary>
        /// <returns>The subscribe.</returns>
        /// <param name="channel">Channel.</param>
        /// <param name="notifyType">Notify type.</param>
        public void Subscribe(string channel, NotifyType notifyType)
        {
            switch (notifyType)
            {
                case NotifyType.Add:
                    _subscriber.Subscribe(channel, CacheAddAction);
                    break;
                case NotifyType.Update:
                    _subscriber.Subscribe(channel, CacheUpdateAction);
                    break;
                case NotifyType.Delete:
                    _subscriber.Subscribe(channel, CacheDeleteAction);
                    break;
            }
        }

        /// <summary>
        /// Subscribes the async.
        /// </summary>
        /// <returns>The async.</returns>
        /// <param name="channel">Channel.</param>
        /// <param name="notifyType">Notify type.</param>
        public async Task SubscribeAsync(string channel, NotifyType notifyType)
        {
            switch (notifyType)
            {
                case NotifyType.Add:
                    await _subscriber.SubscribeAsync(channel, CacheAddAction);
                    break;
                case NotifyType.Update:
                    await _subscriber.SubscribeAsync(channel, CacheUpdateAction);
                    break;
                case NotifyType.Delete:
                    await _subscriber.SubscribeAsync(channel, CacheDeleteAction);
                    break;
            }
        }

        /// <summary>
        /// Caches the delete action.
        /// </summary>
        /// <param name="channel">Channel.</param>
        /// <param name="value">Value.</param>
        private void CacheDeleteAction(RedisChannel channel, RedisValue value)
        {
            var message = _serializer.Deserialize<EasyCachingMessage>(value);

            //TODO : remove local cache
            _localCachingProvider.Remove(message.CacheKey);
        }

        /// <summary>
        /// Caches the add action.
        /// </summary>
        /// <param name="channel">Channel.</param>
        /// <param name="value">Value.</param>
        private void CacheAddAction(RedisChannel channel, RedisValue value)
        {
            var message = _serializer.Deserialize<EasyCachingMessage>(value);

            //TODO : add local cache
            _localCachingProvider.Set(message.CacheKey, message.CacheValue, message.Expiration);
        }

        /// <summary>
        /// Caches the update action.
        /// </summary>
        /// <param name="channel">Channel.</param>
        /// <param name="value">Value.</param>
        private void CacheUpdateAction(RedisChannel channel, RedisValue value)
        {
            var message = _serializer.Deserialize<EasyCachingMessage>(value);

            //TODO : update local cache
            _localCachingProvider.Remove(message.CacheKey);
            _localCachingProvider.Set(message.CacheKey, message.CacheValue, message.Expiration);
        }
    }
}
