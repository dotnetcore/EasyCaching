namespace EasyCaching.Core
{
    using System;

    /// <summary>
    /// EasyCaching provider.
    /// </summary>
    public interface IEasyCachingProvider
    {
        /// <summary>
        /// Set the specified cacheKey, cacheValue and absoluteExpirationRelativeToNow.
        /// </summary>
        /// <returns>The set.</returns>
        /// <param name="cacheKey">Cache key.</param>
        /// <param name="cacheValue">Cache value.</param>
        /// <param name="absoluteExpirationRelativeToNow">Absolute expiration relative to now.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        void Set<T>(string cacheKey, T cacheValue, TimeSpan absoluteExpirationRelativeToNow) where T : class;

        /// <summary>
        /// Get the specified cacheKey and dataRetriever.
        /// </summary>
        /// <returns>The get.</returns>
        /// <param name="cacheKey">Cache key.</param>
        /// <param name="dataRetriever">Data retriever.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        T Get<T>(string cacheKey, Func<T> dataRetriever = null) where T : class;

        /// <summary>
        /// Set the specified cacheKey, cacheValue and absoluteExpirationRelativeToNow.
        /// </summary>
        /// <returns>The set.</returns>
        /// <param name="cacheKey">Cache key.</param>
        /// <param name="cacheValue">Cache value.</param>
        /// <param name="absoluteExpirationRelativeToNow">Absolute expiration relative to now.</param>
        void Set(string cacheKey,object cacheValue, TimeSpan absoluteExpirationRelativeToNow);

        /// <summary>
        /// Get the specified cacheKey and dataRetriever.
        /// </summary>
        /// <returns>The get.</returns>
        /// <param name="cacheKey">Cache key.</param>
        /// <param name="dataRetriever">Data retriever.</param>
        object Get(string cacheKey, Func<object> dataRetriever = null);

    }
}
