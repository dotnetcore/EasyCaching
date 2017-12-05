namespace EasyCaching.Memory
{
    using System;
    using EasyCaching.Core;
    using Microsoft.Extensions.Caching.Memory;

    /// <summary>
    /// MemoryCaching provider.
    /// </summary>
    public class MemoryCachingProvider : IEasyCachingProvider
    {
        /// <summary>
        /// The MemoryCache.
        /// </summary>
        private readonly IMemoryCache _cache;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:EasyCaching.Memory.MemoryCachingProvider"/> class.
        /// </summary>
        /// <param name="cache">Microsoft MemoryCache.</param>
        public MemoryCachingProvider(IMemoryCache cache)
        {
            this._cache = cache;
        }

        /// <summary>
        /// Get the specified cacheKey.
        /// </summary>
        /// <returns>The get.</returns>
        /// <param name="cacheKey">Cache key.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public T Get<T>(string cacheKey, Func<T> dataRetriever = null) where T : class
        {
            if (string.IsNullOrWhiteSpace(cacheKey))
                throw new ArgumentNullException(nameof(cacheKey));

            var result = _cache.Get(cacheKey) as T;

            if (result != null)
                return result;

            if(dataRetriever!=null)
            {
                result = dataRetriever.Invoke();
                Set(cacheKey, result, TimeSpan.FromMinutes(10));
            }

            return result;
        }

        /// <summary>
        /// Get the specified cacheKey.
        /// </summary>
        /// <returns>The get.</returns>
        /// <param name="cacheKey">Cache key.</param>
        public object Get(string cacheKey, Func<object> dataRetriever = null)
        {
            if (string.IsNullOrWhiteSpace(cacheKey))
                throw new ArgumentNullException(nameof(cacheKey));

            var result = _cache.Get(cacheKey);

            if (result != null)
                return result;

            if (dataRetriever != null)
            {
                result = dataRetriever.Invoke();
                Set(cacheKey, result, TimeSpan.FromMinutes(10));
            }

            return result;
        }

        /// <summary>
        /// Set the specified cacheKey, cacheValue and absoluteExpirationRelativeToNow.
        /// </summary>
        /// <returns>The set.</returns>
        /// <param name="cacheKey">Cache key.</param>
        /// <param name="cacheValue">Cache value.</param>
        /// <param name="absoluteExpirationRelativeToNow">Absolute expiration relative to now.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public void Set<T>(string cacheKey,T cacheValue,TimeSpan absoluteExpirationRelativeToNow) where T : class 
        {
            _cache.Set(cacheKey, cacheValue, absoluteExpirationRelativeToNow);
        }

        /// <summary>
        /// Set the specified cacheKey, cacheValue and absoluteExpirationRelativeToNow.
        /// </summary>
        /// <returns>The set.</returns>
        /// <param name="cacheKey">Cache key.</param>
        /// <param name="cacheValue">Cache value.</param>
        /// <param name="absoluteExpirationRelativeToNow">Absolute expiration relative to now.</param>
        public void Set(string cacheKey, object cacheValue, TimeSpan absoluteExpirationRelativeToNow)
        {
            _cache.Set(cacheKey, cacheValue, absoluteExpirationRelativeToNow);
        }
    }
}
