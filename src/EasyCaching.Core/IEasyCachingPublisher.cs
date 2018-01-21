namespace EasyCaching.Core
{
    using EasyCaching.Core.Internal;
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// EasyCaching publisher.
    /// </summary>
    public interface IEasyCachingPublisher
    {
        /// <summary>
        /// Publish the specified channel, cacheKey, cacheValue and expiration.
        /// </summary>
        /// <returns>The publish.</returns>
        /// <param name="channel">Channel.</param>
        /// <param name="cacheKey">Cache key.</param>
        /// <param name="cacheValue">Cache value.</param>
        /// <param name="expiration">Expiration.</param>
        void Publish<T>(string channel, string cacheKey, T cacheValue, TimeSpan expiration);

        /// <summary>
        /// Publishs the async.
        /// </summary>
        /// <returns>The async.</returns>
        /// <param name="channel">Channel.</param>
        /// <param name="cacheKey">Cache key.</param>
        /// <param name="cacheValue">Cache value.</param>
        /// <param name="expiration">Expiration.</param>
        Task PublishAsync<T>(string channel, string cacheKey, T cacheValue, TimeSpan expiration);
    }
}
