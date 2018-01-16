namespace EasyCaching.InMemory
{
    using EasyCaching.Core;
    using EasyCaching.Core.Internal;
    using Microsoft.Extensions.Caching.Memory;
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// MemoryCaching provider.
    /// </summary>
    public class InMemoryCachingProvider : IEasyCachingProvider
    {
        /// <summary>
        /// The MemoryCache.
        /// </summary>
        private readonly IMemoryCache _cache;

        /// <summary>
        /// <see cref="T:EasyCaching.InMemory.InMemoryCachingProvider"/> 
        /// is not distributed cache.
        /// </summary>
        /// <value><c>true</c> if is distributed cache; otherwise, <c>false</c>.</value>
        public bool IsDistributedCache => false;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:EasyCaching.Memory.MemoryCachingProvider"/> class.
        /// </summary>
        /// <param name="cache">Microsoft MemoryCache.</param>
        public InMemoryCachingProvider(IMemoryCache cache)
        {
            this._cache = cache;
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

            var result = _cache.Get(cacheKey) as T;

            if (result != null)
                return new CacheValue<T>(result, true);

            result = dataRetriever?.Invoke();

            if (result != null)
            {
                Set(cacheKey, result, expiration);
                return new CacheValue<T>(result, true);
            }
            else
            {
                return CacheValue<T>.NoValue;
            }
        }

        /// <summary>
        /// Gets the specified cacheKey, dataRetriever and expiration async.
        /// </summary>
        /// <returns>The async.</returns>
        /// <param name="cacheKey">Cache key.</param>
        /// <param name="dataRetriever">Data retriever.</param>
        /// <param name="expiration">Expiration.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public async Task<CacheValue<T>> GetAsync<T>(string cacheKey, Func<Task<T>> dataRetriever, TimeSpan expiration) where T : class
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));
            ArgumentCheck.NotNegativeOrZero(expiration, nameof(expiration));

            var result = _cache.Get(cacheKey) as T;

            if (result != null)
                return new CacheValue<T>(result, true);

            result = await dataRetriever?.Invoke();

            if (result != null)
            {
                Set(cacheKey, result, expiration);
                return new CacheValue<T>(result, true);
            }
            else
            {
                return CacheValue<T>.NoValue;
            }
        }

        /// <summary>
        /// Get the specified cacheKey.
        /// </summary>
        /// <returns>The get.</returns>
        /// <param name="cacheKey">Cache key.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public CacheValue<T> Get<T>(string cacheKey) where T : class
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            var result = _cache.Get(cacheKey) as T;

            if (result != null)
                return new CacheValue<T>(result, true);
            else
                return CacheValue<T>.NoValue;
        }

        /// <summary>
        /// Gets the specified cacheKey async.
        /// </summary>
        /// <returns>The async.</returns>
        /// <param name="cacheKey">Cache key.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public async Task<CacheValue<T>> GetAsync<T>(string cacheKey) where T : class
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            var result = await Task.Run(() => { return _cache.Get(cacheKey) as T; });

            if (result != null)
                return new CacheValue<T>(result, true);
            else
                return CacheValue<T>.NoValue;
        }

        /// <summary>
        /// Remove the specified cacheKey.
        /// </summary>
        /// <returns>The remove.</returns>
        /// <param name="cacheKey">Cache key.</param>
        public void Remove(string cacheKey)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            _cache.Remove(cacheKey);
        }

        /// <summary>
        /// Removes the specified cacheKey async.
        /// </summary>
        /// <returns>The async.</returns>
        /// <param name="cacheKey">Cache key.</param>
        public async Task RemoveAsync(string cacheKey)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            await Task.Run(() => _cache.Remove(cacheKey));
        }

        /// <summary>
        /// Set the specified cacheKey, cacheValue and expiration.
        /// </summary>
        /// <returns>The set.</returns>
        /// <param name="cacheKey">Cache key.</param>
        /// <param name="cacheValue">Cache value.</param>
        /// <param name="expiration">expiration.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public void Set<T>(string cacheKey, T cacheValue, TimeSpan expiration) where T : class
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));
            ArgumentCheck.NotNull(cacheValue, nameof(cacheValue));
            ArgumentCheck.NotNegativeOrZero(expiration, nameof(expiration));

            _cache.Set(cacheKey, cacheValue, expiration);
        }


        /// <summary>
        /// Sets the specified cacheKey, cacheValue and expiration async.
        /// </summary>
        /// <returns>The async.</returns>
        /// <param name="cacheKey">Cache key.</param>
        /// <param name="cacheValue">Cache value.</param>
        /// <param name="expiration">Expiration.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public async Task SetAsync<T>(string cacheKey, T cacheValue, TimeSpan expiration) where T : class
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));
            ArgumentCheck.NotNull(cacheValue, nameof(cacheValue));
            ArgumentCheck.NotNegativeOrZero(expiration, nameof(expiration));

            await Task.Run(() => _cache.Set(cacheKey, cacheValue, expiration));
        }

        /// <summary>
        /// Exists the specified cacheKey.
        /// </summary>
        /// <returns>The exists.</returns>
        /// <param name="cacheKey">Cache key.</param>
        public bool Exists(string cacheKey)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            return _cache.TryGetValue(cacheKey, out object value);
        }

        /// <summary>
        /// Existses the specified cacheKey async.
        /// </summary>
        /// <returns>The async.</returns>
        /// <param name="cacheKey">Cache key.</param>
        public async Task<bool> ExistsAsync(string cacheKey)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            return await Task.Run(() => { return _cache.TryGetValue(cacheKey, out object value); });
        }
    }
}
