namespace EasyCaching.Core
{    
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// EasyCaching publisher.
    /// </summary>
    public interface IEasyCachingPublisher
    {
        /// <summary>
        /// Publish the specified channel and message.
        /// </summary>
        /// <returns>The publish.</returns>
        /// <param name="channel">Channel.</param>
        /// <param name="message">Message.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        void Publish(string channel, EasyCachingMessage message);

        /// <summary>
        /// Publishs the specified channel and message async.
        /// </summary>
        /// <returns>The async.</returns>
        /// <param name="channel">Channel.</param>
        /// <param name="message">Message.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        Task PublishAsync(string channel, EasyCachingMessage message);
    }
}
