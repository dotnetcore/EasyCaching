namespace EasyCaching.Core.Bus
{
    using System;

    /// <summary>
    /// EasyCaching subscriber.
    /// </summary>
    public interface IEasyCachingSubscriber
    {
        /// <summary>
        /// Subscribe the specified topic and action.
        /// </summary>
        /// <param name="topic">Topic.</param>
        /// <param name="action">Action.</param>
        void Subscribe(string topic, Action<EasyCachingMessage> action);
    }
}
