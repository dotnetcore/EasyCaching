namespace EasyCaching.Core
{    
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
        void Subscribe(string channel);

        /// <summary>
        /// Subscribes the specified channel with notifyType async.
        /// </summary>
        /// <returns>The async.</returns>
        /// <param name="channel">Channel.</param>
        Task SubscribeAsync(string channel);
    }
}
