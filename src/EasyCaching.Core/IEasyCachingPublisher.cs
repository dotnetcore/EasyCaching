namespace EasyCaching.Core
{
    using EasyCaching.Core.Internal;
    using System;

    /// <summary>
    /// EasyCaching publisher.
    /// </summary>
    public interface IEasyCachingPublisher
    {
        /// <summary>
        /// Notify the specified cacheKey, cacheValue and expiration with notifyType.
        /// </summary>
        /// <param name="notifyType">Notify type.</param>
        /// <param name="cacheKey">Cache key.</param>
        /// <param name="cacheValue">Cache value.</param>
        /// <param name="expiration">Expiration.</param>
        void Notify(NotifyType notifyType, string cacheKey, object cacheValue, TimeSpan expiration);
    }
}
