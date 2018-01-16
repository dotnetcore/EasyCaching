namespace EasyCaching.Redis
{
    using EasyCaching.Core;
    using EasyCaching.Core.Internal;
    using StackExchange.Redis;
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// Default redis caching provider.
    /// </summary>
    public class DefaultRedisCachingProvider : IEasyCachingProvider
    {
        /// <summary>
        /// The cache.
        /// </summary>
        private readonly IDatabase _cache;

        /// <summary>
        /// The db provider.
        /// </summary>
        private readonly IRedisDatabaseProvider _dbProvider;

        /// <summary>
        /// The serializer.
        /// </summary>
        private readonly IEasyCachingSerializer _serializer;

        /// <summary>
        /// <see cref="T:EasyCaching.Redis.DefaultRedisCachingProvider"/> 
        /// is not distributed cache.
        /// </summary>
        public bool IsDistributedCache => false;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:EasyCaching.Redis.DefaultRedisCachingProvider"/> class.
        /// </summary>
        /// <param name="dbProvider">DB Provider.</param>
        /// <param name="serializer">Serializer.</param>
        public DefaultRedisCachingProvider(
            IRedisDatabaseProvider dbProvider,
            IEasyCachingSerializer serializer)
        {
            ArgumentCheck.NotNull(dbProvider, nameof(dbProvider));
            ArgumentCheck.NotNull(serializer, nameof(serializer));

            _dbProvider = dbProvider;
            _serializer = serializer;
            _cache = _dbProvider.GetDatabase();
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

            var result = _cache.StringGet(cacheKey);
            if (!result.IsNull)
            {
                var value = _serializer.Deserialize<T>(result);
                return new CacheValue<T>(value, true);
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

            var result = await _cache.StringGetAsync(cacheKey);
            if (!result.IsNull)
            {
                var value = _serializer.Deserialize<T>(result);
                return new CacheValue<T>(value, true);
            }

            var item = await dataRetriever?.Invoke();
            if (item != null)
            {
                await SetAsync(cacheKey, item, expiration);
                return new CacheValue<T>(item, true);
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

            var result = _cache.StringGet(cacheKey);
            if (!result.IsNull)
            {
                var value = _serializer.Deserialize<T>(result);
                return new CacheValue<T>(value, true);
            }
            else
            {
                return CacheValue<T>.NoValue;
            }           
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

            var result = await _cache.StringGetAsync(cacheKey);
            if (!result.IsNull)
            {
                var value = _serializer.Deserialize<T>(result);
                return new CacheValue<T>(value, true);
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

            _cache.KeyDelete(cacheKey);
        }

        /// <summary>
        /// Removes the specified cacheKey async.
        /// </summary>
        /// <returns>The async.</returns>
        /// <param name="cacheKey">Cache key.</param>
        public async Task RemoveAsync(string cacheKey)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            await _cache.KeyDeleteAsync(cacheKey);
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

            _cache.StringSet(
                cacheKey,
                _serializer.Serialize(cacheValue),
                expiration);
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

            await _cache.StringSetAsync(
                    cacheKey,
                    _serializer.Serialize(cacheValue),
                    expiration);
        }

        /// <summary>
        /// Exists the specified cacheKey.
        /// </summary>
        /// <returns>The exists.</returns>
        /// <param name="cacheKey">Cache key.</param>
        public bool Exists(string cacheKey)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            return _cache.KeyExists(cacheKey);
        }

        /// <summary>
        /// Existses the specified cacheKey async.
        /// </summary>
        /// <returns>The async.</returns>
        /// <param name="cacheKey">Cache key.</param>
        public async Task<bool> ExistsAsync(string cacheKey)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            return await _cache.KeyExistsAsync(cacheKey);
        }             
    }
}
