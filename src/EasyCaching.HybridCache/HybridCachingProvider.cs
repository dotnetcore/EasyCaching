namespace EasyCaching.HybridCache
{
    using EasyCaching.Core;
    using EasyCaching.Core.Internal;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    /// <summary>
    /// Hybrid caching provider.
    /// </summary>
    public class HybridCachingProvider : IHybridCachingProvider
    {
        /// <summary>
        /// The local caching provider.
        /// </summary>
        private IEasyCachingProvider _localCachingProvider;

        /// <summary>
        /// The distributed caching provider.
        /// </summary>
        private IEasyCachingProvider _distributedCachingProvider;

        /// <summary>
        /// The service accessor.
        /// </summary>
        private readonly Func<string, IEasyCachingProvider> _serviceAccessor;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:EasyCaching.HybridCache.HybridCachingProvider"/> class.
        /// </summary>
        /// <param name="serviceAccessor">Service accessor.</param>
        public HybridCachingProvider(Func<string, IEasyCachingProvider> serviceAccessor)
        {
            _serviceAccessor = serviceAccessor;

            this._localCachingProvider = _serviceAccessor(HybridCachingKeyType.LocalKey);
            this._distributedCachingProvider = _serviceAccessor(HybridCachingKeyType.DistributedKey);
        }

        /// <summary>
        /// Gets a value indicating whether this <see cref="T:EasyCaching.HybridCache.HybridCachingProvider"/> is
        /// distributed cache.
        /// </summary>
        /// <value><c>true</c> if is distributed cache; otherwise, <c>false</c>.</value>
        public bool IsDistributedCache => true;

        /// <summary>
        /// Exists the specified cacheKey.
        /// </summary>
        /// <returns>The exists.</returns>
        /// <param name="cacheKey">Cache key.</param>
        public bool Exists(string cacheKey)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey,nameof(cacheKey));

            var flag = false;
            
            flag = _localCachingProvider.Exists(cacheKey);

            if(!flag)
            {
                flag = _distributedCachingProvider.Exists(cacheKey);
            }

            return flag;
        }

        /// <summary>
        /// Existses the specified cacheKey async.
        /// </summary>
        /// <returns>The async.</returns>
        /// <param name="cacheKey">Cache key.</param>
        public async Task<bool> ExistsAsync(string cacheKey)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            var flag = false;

            flag = await _localCachingProvider.ExistsAsync(cacheKey);

            if (!flag)
            {
                flag = await _distributedCachingProvider.ExistsAsync(cacheKey);
            }

            return flag;
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

            var value = _localCachingProvider.Get<T>(cacheKey);

            if (value.HasValue)
            {
                return value;
            }

            value = _distributedCachingProvider.Get<T>(cacheKey);

            if (value.HasValue)
            {
                return value;
            }

            var item = dataRetriever?.Invoke();
            if (item != null)
            {
                Set(cacheKey, item, expiration);
                return new CacheValue<T>(item, true);
            }        
            else
            {
                //TODO : Set a null value to cache!!

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

            var value = _localCachingProvider.Get<T>(cacheKey);

            if (value.HasValue)
            {
                return value;
            }

            value = _distributedCachingProvider.Get<T>(cacheKey);

            if (value.HasValue)
            {
                return value;
            }
            else
            {
                //TODO : Set a null value to cache!!

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

            var value = await _localCachingProvider.GetAsync<T>(cacheKey);

            if (value.HasValue)
            {
                return value;
            }

            value = await _distributedCachingProvider.GetAsync<T>(cacheKey);

            if (value.HasValue)
            {
                return value;
            }

            var item = await dataRetriever?.Invoke();
            if (item != null)
            {
                await SetAsync(cacheKey, item, expiration);
                return new CacheValue<T>(item, true);
            }
            else
            {
                //TODO : Set a null value to cache!!

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

            var value = await _localCachingProvider.GetAsync<T>(cacheKey);

            if (value.HasValue)
            {
                return value;
            }

            value = await _distributedCachingProvider.GetAsync<T>(cacheKey);

            if (value.HasValue)
            {
                return value;
            }
            else
            {
                //TODO : Set a null value to cache!!

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

            _localCachingProvider.Remove(cacheKey);
            _distributedCachingProvider.Remove(cacheKey);
        }

        /// <summary>
        /// Removes the specified cacheKey async.
        /// </summary>
        /// <returns>The async.</returns>
        /// <param name="cacheKey">Cache key.</param>
        public async Task RemoveAsync(string cacheKey)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            await _localCachingProvider.RemoveAsync(cacheKey);
            await _distributedCachingProvider.RemoveAsync(cacheKey);
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

            _localCachingProvider.Set(cacheKey,cacheValue,expiration);
            _distributedCachingProvider.Set(cacheKey, cacheValue, expiration);
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

            await _localCachingProvider.SetAsync(cacheKey, cacheValue, expiration);
            await _distributedCachingProvider.SetAsync(cacheKey, cacheValue, expiration);
        }

        /// <summary>
        /// Refresh the specified cacheKey, cacheValue and expiration.
        /// </summary>
        /// <param name="cacheKey">Cache key.</param>
        /// <param name="cacheValue">Cache value.</param>
        /// <param name="expiration">Expiration.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public void Refresh<T>(string cacheKey, T cacheValue, TimeSpan expiration) where T : class
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));
            ArgumentCheck.NotNull(cacheValue, nameof(cacheValue));
            ArgumentCheck.NotNegativeOrZero(expiration, nameof(expiration));

            this.Remove(cacheKey);
            this.Set(cacheKey, cacheValue, expiration);
        }

        /// <summary>
        /// Refreshs the specified cacheKey, cacheValue and expiration.
        /// </summary>
        /// <returns>The async.</returns>
        /// <param name="cacheKey">Cache key.</param>
        /// <param name="cacheValue">Cache value.</param>
        /// <param name="expiration">Expiration.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public async Task RefreshAsync<T>(string cacheKey, T cacheValue, TimeSpan expiration) where T : class
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));
            ArgumentCheck.NotNull(cacheValue, nameof(cacheValue));
            ArgumentCheck.NotNegativeOrZero(expiration, nameof(expiration));

            await this.RemoveAsync(cacheKey);
            await this.SetAsync(cacheKey, cacheValue, expiration);
        }

        /// <summary>
        /// Removes cached item by cachekey's prefix.
        /// </summary>
        /// <returns>The by prefix async.</returns>
        /// <param name="prefix">Prefix.</param>
        public void RemoveByPrefix(string prefix)
        {
            ArgumentCheck.NotNullOrWhiteSpace(prefix, nameof(prefix));

            _localCachingProvider.RemoveByPrefix(prefix);
            _distributedCachingProvider.RemoveByPrefix(prefix);
        }

        /// <summary>
        /// Removes cached item by cachekey's prefix async.
        /// </summary>
        /// <returns>The by prefix async.</returns>
        /// <param name="prefix">Prefix.</param>
        public async Task RemoveByPrefixAsync(string prefix)
        {
            ArgumentCheck.NotNullOrWhiteSpace(prefix, nameof(prefix));

            await _localCachingProvider.RemoveByPrefixAsync(prefix);
            await _distributedCachingProvider.RemoveByPrefixAsync(prefix);
        }

        public void SetAll<T>(IDictionary<string, T> value, TimeSpan expiration) where T : class
        {
            throw new NotImplementedException();
        }

        public Task SetAllAsync<T>(IDictionary<string, T> value, TimeSpan expiration) where T : class
        {
            throw new NotImplementedException();
        }

        public IDictionary<string, CacheValue<T>> GetAll<T>(IEnumerable<string> cacheKeys) where T : class
        {
            throw new NotImplementedException();
        }

        public Task<IDictionary<string, CacheValue<T>>> GetAllAsync<T>(IEnumerable<string> cacheKeys) where T : class
        {
            throw new NotImplementedException();
        }

        public IEnumerable<CacheValue<T>> GetByPrefix<T>(string prefix) where T : class
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<CacheValue<T>>> GetByPrefixAsync<T>(string prefix) where T : class
        {
            throw new NotImplementedException();
        }

        public void RemoveAll(IEnumerable<string> cacheKeys)
        {
            throw new NotImplementedException();
        }

        public Task RemoveAllAsync(IEnumerable<string> cacheKeys)
        {
            throw new NotImplementedException();
        }
    }
}
