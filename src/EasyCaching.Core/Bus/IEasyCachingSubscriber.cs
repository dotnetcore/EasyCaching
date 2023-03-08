namespace EasyCaching.Core.Bus
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

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
        /// <param name="reconnectAction"> Reconnect Action.</param>
        void Subscribe(string topic, Action<EasyCachingMessage> action, Action reconnectAction = null);

        /// <summary>
        /// Subscribe the specified topic and action async.
        /// </summary>
        /// <param name="topic">Topic.</param>
        /// <param name="action">Action.</param>
        /// <param name="reconnectAction"> Reconnect Action.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        Task SubscribeAsync(string topic, Action<EasyCachingMessage> action, Action reconnectAction = null, CancellationToken cancellationToken = default(CancellationToken));
    }
}
