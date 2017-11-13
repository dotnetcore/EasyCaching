namespace EasyCaching.Core
{
    using System;

    /// <summary>
    /// Cache entry.
    /// </summary>
    public class CacheEntry
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:EasyCaching.Core.CacheEntry"/> class.
        /// </summary>
        /// <param name="cacheKey">Cache key.</param>
        /// <param name="cacheValue">Cache value.</param>
        /// <param name="absoluteExpirationRelativeToNow">Absolute expiration relative to now.</param>
        public CacheEntry(string cacheKey, object cacheValue, TimeSpan absoluteExpirationRelativeToNow)
        {
            if (string.IsNullOrWhiteSpace(cacheKey))
                throw new ArgumentNullException(nameof(cacheKey));

            if (cacheValue == null)
                throw new ArgumentNullException(nameof(cacheValue));

            if (absoluteExpirationRelativeToNow <= TimeSpan.Zero)
                throw new ArgumentOutOfRangeException(
                    nameof(absoluteExpirationRelativeToNow), 
                    absoluteExpirationRelativeToNow, 
                    "The relative expiration value must be positive.");

            this.CacheKey = cacheKey;
            this.CacheValue = cacheValue;
            this.AbsoluteExpirationRelativeToNow = absoluteExpirationRelativeToNow;
        }

        /// <summary>
        /// Gets the cache key.
        /// </summary>
        /// <value>The cache key.</value>
        public string CacheKey { get; private set; }

        /// <summary>
        /// Gets the cache value.
        /// </summary>
        /// <value>The cache value.</value>
        public object CacheValue { get; private set; }

        /// <summary>
        /// Gets the absolute expiration relative to now.
        /// </summary>
        /// <value>The absolute expiration relative to now.</value>
        public TimeSpan AbsoluteExpirationRelativeToNow { get; private set; }
    }
}
