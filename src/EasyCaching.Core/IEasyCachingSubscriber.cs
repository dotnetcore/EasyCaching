namespace EasyCaching.Core
{
    using EasyCaching.Core.Internal;

    /// <summary>
    /// EasyCaching subscriber.
    /// </summary>
    public interface IEasyCachingSubscriber
    {
        /// <summary>
        /// Subscribe the specified channel with notifyType.
        /// </summary>
        /// <param name="channel">Channel.</param>
        /// <param name="notifyType">Notify type.</param>
        void Subscribe(string channel, NotifyType notifyType);
    }
}
