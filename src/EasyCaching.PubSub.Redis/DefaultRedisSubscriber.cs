namespace EasyCaching.PubSub.Redis
{
    using EasyCaching.Core;
    using EasyCaching.Core.Internal;
    using System;

    /// <summary>
    /// Default redis subscriber.
    /// </summary>
    public class DefaultRedisSubscriber : IEasyCachingSubscriber
    {
        /// <summary>
        /// Subscribe the specified channel with notifyType.
        /// </summary>
        /// <returns>The subscribe.</returns>
        /// <param name="channel">Channel.</param>
        /// <param name="notifyType">Notify type.</param>
        public void Subscribe(string channel, NotifyType notifyType)
        {
            throw new NotImplementedException();
        }
    }
}
