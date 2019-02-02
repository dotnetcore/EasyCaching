namespace EasyCaching.Core.Bus
{
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// EasyCaching publisher.
    /// </summary>
    public interface IEasyCachingPublisher
    {
        /// <summary>
        /// Publish the specified topic and message.
        /// </summary>
        /// <param name="topic">Topic.</param>
        /// <param name="message">Message.</param>
        void Publish(string topic, EasyCachingMessage message);

        /// <summary>
        /// Publishs the specified topic and message.
        /// </summary>
        /// <returns>The async.</returns>
        /// <param name="topic">Topic.</param>
        /// <param name="message">Message.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        Task PublishAsync(string topic, EasyCachingMessage message, CancellationToken cancellationToken = default(CancellationToken));
    }
}
