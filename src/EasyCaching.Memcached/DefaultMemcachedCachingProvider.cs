namespace EasyCaching.Memcached
{
    using System;
    using EasyCaching.Core;
    using Enyim.Caching;

    /// <summary>
    /// Default memcached caching provider.
    /// </summary>
    public class DefaultMemcachedCachingProvider : IEasyCachingProvider
    {
        /// <summary>
        /// The memcached client.
        /// </summary>
        private readonly IMemcachedClient _memcachedClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:EasyCaching.Memcached.DefaultMemcachedCachingProvider"/> class.
        /// </summary>
        /// <param name="memcachedClient">Memcached client.</param>
        public DefaultMemcachedCachingProvider(IMemcachedClient memcachedClient)
        {
            this._memcachedClient = memcachedClient;
        }

        /// <summary>
        /// Get the specified cacheKey, dataRetriever and expiration.
        /// </summary>
        /// <returns>The get.</returns>
        /// <param name="cacheKey">Cache key.</param>
        /// <param name="dataRetriever">Data retriever.</param>
        /// <param name="expiration">Expiration.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public T Get<T>(string cacheKey, Func<T> dataRetriever, TimeSpan expiration) where T : class
        {
            var result = _memcachedClient.Get<T>(cacheKey);

            if (result != null)
                return result;

            result = dataRetriever.Invoke();
            Set(cacheKey, result, expiration);

            return result;
        }

        /// <summary>
        /// Get the specified cacheKey, dataRetriever and expiration.
        /// </summary>
        /// <returns>The get.</returns>
        /// <param name="cacheKey">Cache key.</param>
        /// <param name="dataRetriever">Data retriever.</param>
        /// <param name="expiration">Expiration.</param>
        public object Get(string cacheKey, Func<object> dataRetriever, TimeSpan expiration)
        {
            var result = _memcachedClient.Get(cacheKey);

            if (result != null)
                return result;

            result = dataRetriever.Invoke();
            Set(cacheKey, result, expiration);

            return result;
        }

        /// <summary>
        /// Remove the specified cacheKey.
        /// </summary>
        /// <returns>The remove.</returns>
        /// <param name="cacheKey">Cache key.</param>
        public void Remove(string cacheKey)
        {
            _memcachedClient.Remove(cacheKey);
        }

        /// <summary>
        /// Set the specified cacheKey, cacheValue and expiration.
        /// </summary>
        /// <returns>The set.</returns>
        /// <param name="cacheKey">Cache key.</param>
        /// <param name="cacheValue">Cache value.</param>
        /// <param name="expiration">Expiration.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public void Set<T>(string cacheKey, T cacheValue, TimeSpan expiration) where T : class
        {
            _memcachedClient.Add(cacheKey, cacheValue, expiration.Seconds);
        }

        /// <summary>
        /// Set the specified cacheKey, cacheValue and expiration.
        /// </summary>
        /// <returns>The set.</returns>
        /// <param name="cacheKey">Cache key.</param>
        /// <param name="cacheValue">Cache value.</param>
        /// <param name="expiration">Expiration.</param>
        public void Set(string cacheKey, object cacheValue, TimeSpan expiration)
        {
            _memcachedClient.Add(cacheKey, cacheValue, expiration.Seconds);
        }
    }
}
