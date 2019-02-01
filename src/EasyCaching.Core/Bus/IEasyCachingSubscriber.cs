namespace EasyCaching.Core
{
    using System;

    /// <summary>
    /// EasyCaching subscriber.
    /// </summary>
    public interface IEasyCachingSubscriber
    {
        void Subscribe(string topic, Action<EasyCachingMessage> action);
    }
}
