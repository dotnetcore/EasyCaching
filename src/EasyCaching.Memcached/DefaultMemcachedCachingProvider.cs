namespace EasyCaching.Memcached
{
    using EasyCaching.Core;
    using EasyCaching.Core.Internal;
    using Enyim.Caching;
    using System;

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
        public CacheValue<T> Get<T>(string cacheKey, Func<T> dataRetriever, TimeSpan expiration) where T : class
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));
            ArgumentCheck.NotNegativeOrZero(expiration, nameof(expiration));

            var result = _memcachedClient.Get(cacheKey) as T;
            if (result != null)
            {
                return new CacheValue<T>(result, true);
            }

            var item = dataRetriever?.Invoke();
            if (item != null)
            {
                Set(cacheKey, item, expiration);
                return new CacheValue<T>(item, true);
            }
            else
            {
                return CacheValue<T>.NoValue;
            }
        }

        /// <summary>
        /// Remove the specified cacheKey.
        /// </summary>
        /// <returns>The remove.</returns>
        /// <param name="cacheKey">Cache key.</param>
        public void Remove(string cacheKey)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

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
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));
            ArgumentCheck.NotNull(cacheValue, nameof(cacheValue));
            ArgumentCheck.NotNegativeOrZero(expiration, nameof(expiration));

            _memcachedClient.Add(cacheKey, cacheValue, expiration.Seconds);
        }
             
        /// <summary>
        /// Exists the specified cacheKey.
        /// </summary>
        /// <returns>The exists.</returns>
        /// <param name="cacheKey">Cache key.</param>
        public bool Exists(string cacheKey)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            return _memcachedClient.TryGet(cacheKey, out object obj);
        }
    }
}
