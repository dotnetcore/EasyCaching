namespace EasyCaching.Core
{
    /// <summary>
    /// EasyCaching provider.
    /// </summary>
    public interface IEasyCachingProvider
    {
        /// <summary>
        /// Set the specified cacheEntry.
        /// </summary>
        /// <returns></returns>
        /// <param name="cacheEntry">Cache entry.</param>
        void Set(CacheEntry cacheEntry);

        /// <summary>
        /// Get the specified cacheKey.
        /// </summary>
        /// <returns>The cache value.</returns>
        /// <param name="cacheKey">Cache key.</param>
        object Get(string cacheKey);
    }
}
