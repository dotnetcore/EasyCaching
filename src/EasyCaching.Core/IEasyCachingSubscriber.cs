namespace EasyCaching.Core
{
    using EasyCaching.Core.Internal;
    using System.Threading.Tasks;

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

        /// <summary>
        /// Subscribes the specified channel with notifyType async.
        /// </summary>
        /// <returns>The async.</returns>
        /// <param name="channel">Channel.</param>
        /// <param name="notifyType">Notify type.</param>
        Task SubscribeAsync(string channel, NotifyType notifyType);
    }
}
