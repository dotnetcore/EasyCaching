namespace EasyCaching.Sync.RabbitMQ
{
    using EasyCaching.Core;
    using EasyCaching.Core.Internal;    
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// Default RabbitMQ Bus.
    /// </summary>
    public class DefaultRabbitMQBus : IEasyCachingBus
    {
        public DefaultRabbitMQBus()
        {
        }

        public void Publish<T>(string channel, string cacheKey, T cacheValue, TimeSpan expiration)
        {
            throw new NotImplementedException();
        }

        public Task PublishAsync<T>(string channel, string cacheKey, T cacheValue, TimeSpan expiration)
        {
            throw new NotImplementedException();
        }

        public void Subscribe(string channel, NotifyType notifyType)
        {
            throw new NotImplementedException();
        }

        public Task SubscribeAsync(string channel, NotifyType notifyType)
        {
            throw new NotImplementedException();
        }
    }
}
