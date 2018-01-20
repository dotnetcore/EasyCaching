using System;
namespace EasyCaching.Sync.Redis
{
    using System.Threading.Tasks;
    using EasyCaching.Core;
    using EasyCaching.Core.Internal;
    using StackExchange.Redis;

    public class DefaultRedisBus : IEasyCachingBus
    {
        /// <summary>
        /// The cache.
        /// </summary>
        private readonly ISubscriber _subscriber;

        /// <summary>
        /// The db provider.
        /// </summary>
        private readonly IRedisSubscriberProvider _subscriberProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:EasyCaching.Sync.Redis.DefaultRedisSubscriber"/> class.
        /// </summary>
        /// <param name="subscriberProvider">Subscriber provider.</param>
        public DefaultRedisBus(
                  IRedisSubscriberProvider subscriberProvider)
        {
            this._subscriberProvider = subscriberProvider;

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
        public void Publish(string channel, string cacheKey, object cacheValue, TimeSpan expiration)
        {
            ArgumentCheck.NotNullOrWhiteSpace(channel, nameof(channel));

            //TODO : Handle Parameters


            _subscriber.Publish(channel, "");    
        }

        /// <summary>
        /// Publishs the async.
        /// </summary>
        /// <returns>The async.</returns>
        /// <param name="channel">Channel.</param>
        /// <param name="cacheKey">Cache key.</param>
        /// <param name="cacheValue">Cache value.</param>
        /// <param name="expiration">Expiration.</param>
        public async Task PublishAsync(string channel, string cacheKey, object cacheValue, TimeSpan expiration)
        {
            ArgumentCheck.NotNullOrWhiteSpace(channel, nameof(channel));

            //TODO : Handle Parameters

            await _subscriber.PublishAsync(channel, "");
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
        /// <param name="arg1">Arg1.</param>
        /// <param name="arg2">Arg2.</param>
        private void CacheDeleteAction(RedisChannel arg1, RedisValue arg2)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Caches the add action.
        /// </summary>
        /// <param name="arg1">Arg1.</param>
        /// <param name="arg2">Arg2.</param>
        private void CacheAddAction(RedisChannel arg1, RedisValue arg2)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Caches the update action.
        /// </summary>
        /// <param name="arg1">Arg1.</param>
        /// <param name="arg2">Arg2.</param>
        private void CacheUpdateAction(RedisChannel arg1, RedisValue arg2)
        {
            throw new NotImplementedException();
        }  
    }
}
