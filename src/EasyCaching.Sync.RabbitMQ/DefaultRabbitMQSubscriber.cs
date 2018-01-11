namespace EasyCaching.Sync.RabbitMQ
{
    using EasyCaching.Core;
    using EasyCaching.Core.Internal;
    using System;

    /// <summary>
    /// Default rabbitMQ Subscriber.
    /// </summary>
    public class DefaultRabbitMQSubscriber: IEasyCachingSubscriber
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
