namespace EasyCaching.Core.Bus
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Null easy caching bus.
    /// </summary>
    public class NullEasyCachingBus : IEasyCachingBus
    {
        /// <summary>
        /// The instance.
        /// </summary>
        public static readonly NullEasyCachingBus Instance = new NullEasyCachingBus();

        /// <summary>
        /// Releases all resource used by the <see cref="T:EasyCaching.Core.Bus.NullEasyCachingBus"/> object.
        /// </summary>
        /// <remarks>Call <see cref="Dispose"/> when you are finished using the
        /// <see cref="T:EasyCaching.Core.Bus.NullEasyCachingBus"/>. The <see cref="Dispose"/> method leaves the
        /// <see cref="T:EasyCaching.Core.Bus.NullEasyCachingBus"/> in an unusable state. After calling
        /// <see cref="Dispose"/>, you must release all references to the
        /// <see cref="T:EasyCaching.Core.Bus.NullEasyCachingBus"/> so the garbage collector can reclaim the memory that
        /// the <see cref="T:EasyCaching.Core.Bus.NullEasyCachingBus"/> was occupying.</remarks>
        public void Dispose() { }
               
        /// <summary>
        /// Publish the specified topic and message.
        /// </summary>
        /// <param name="topic">Topic.</param>
        /// <param name="message">Message.</param>
        public void Publish(string topic, EasyCachingMessage message)
        {

        }

        /// <summary>
        /// Publish the specified topic and message async.
        /// </summary>
        /// <returns>The async.</returns>
        /// <param name="topic">Topic.</param>
        /// <param name="message">Message.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        public Task PublishAsync(string topic, EasyCachingMessage message, CancellationToken cancellationToken = default(CancellationToken))
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Subscribe the specified topic and action.
        /// </summary>
        /// <param name="topic">Topic.</param>
        /// <param name="action">Action.</param>
        public void Subscribe(string topic, Action<EasyCachingMessage> action)
        {

        }
    }
}
