namespace EasyCaching.PubSub.Redis
{
    using EasyCaching.Core;
    using EasyCaching.Core.Internal;
    using System;

    public class RedisPublisher : IEasyCachingPublisher
    {
        public void Notify(NotifyType notifyType, string cacheKey, object cacheValue, TimeSpan expiration)
        {
            throw new NotImplementedException();
        }
    }
}
