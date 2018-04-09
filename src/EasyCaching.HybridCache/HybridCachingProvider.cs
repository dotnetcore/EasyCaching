namespace EasyCaching.HybridCache
{
    using EasyCaching.Core;
    using EasyCaching.Core.Internal;
    using System;
    using System.Collections.Generic;
    using System.Linq;
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
        /// The caching providers.
        /// </summary>
        private readonly IEnumerable<IEasyCachingProvider> _providers;

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
        /// Initializes a new instance of the <see cref="T:EasyCaching.HybridCache.HybridCachingProvider"/> class.
        /// </summary>
        /// <param name="providers">Providers.</param>
        public HybridCachingProvider(IEnumerable<IEasyCachingProvider> providers)
        {
            this._providers = providers;
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
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            var flag = false;

            foreach (var provider in _providers)
            {
                flag = provider.Exists(cacheKey);

                if(flag) break;                
            }

            //flag = _localCachingProvider.Exists(cacheKey);

            //if (!flag)
            //{
            //    flag = _distributedCachingProvider.Exists(cacheKey);
            //}

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

            foreach (var provider in _providers)
            {
                provider.Remove(cacheKey);
            }

            //_localCachingProvider.Remove(cacheKey);
            //_distributedCachingProvider.Remove(cacheKey);
        }

        /// <summary>
        /// Removes the specified cacheKey async.
        /// </summary>
        /// <returns>The async.</returns>
        /// <param name="cacheKey">Cache key.</param>
        public async Task RemoveAsync(string cacheKey)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            var tasks = new List<Task>();

            foreach (var provider in _providers)
            {
                tasks.Add(provider.RemoveAsync(cacheKey));
            }

            await Task.WhenAll(tasks);

            //await _localCachingProvider.RemoveAsync(cacheKey);
            //await _distributedCachingProvider.RemoveAsync(cacheKey);
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

            foreach (var provider in _providers)
            {
                provider.Set(cacheKey,cacheValue,expiration);
            }

            //_localCachingProvider.Set(cacheKey, cacheValue, expiration);
            //_distributedCachingProvider.Set(cacheKey, cacheValue, expiration);
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

            var tasks = new List<Task>();

            foreach (var provider in _providers)
            {
                tasks.Add( provider.SetAsync(cacheKey, cacheValue, expiration));
            }

            await Task.WhenAll(tasks);

            //await _localCachingProvider.SetAsync(cacheKey, cacheValue, expiration);
            //await _distributedCachingProvider.SetAsync(cacheKey, cacheValue, expiration);
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

            foreach (var provider in _providers)
            {
                provider.RemoveByPrefix(prefix);
            }

            //_localCachingProvider.RemoveByPrefix(prefix);
            //_distributedCachingProvider.RemoveByPrefix(prefix);
        }

        /// <summary>
        /// Removes cached item by cachekey's prefix async.
        /// </summary>
        /// <returns>The by prefix async.</returns>
        /// <param name="prefix">Prefix.</param>
        public async Task RemoveByPrefixAsync(string prefix)
        {
            ArgumentCheck.NotNullOrWhiteSpace(prefix, nameof(prefix));

            var tasks = new List<Task>();

            foreach (var provider in _providers)
            {
                tasks.Add(provider.RemoveByPrefixAsync(prefix));
            }

            await Task.WhenAll(tasks);

            //await _localCachingProvider.RemoveByPrefixAsync(prefix);
            //await _distributedCachingProvider.RemoveByPrefixAsync(prefix);
        }

        /// <summary>
        /// Sets all.
        /// </summary>
        /// <param name="values">Values.</param>
        /// <param name="expiration">Expiration.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public void SetAll<T>(IDictionary<string, T> values, TimeSpan expiration) where T : class
        {
            ArgumentCheck.NotNegativeOrZero(expiration, nameof(expiration));
            ArgumentCheck.NotNullAndCountGTZero(values, nameof(values));

            foreach (var provider in _providers)
            {
                provider.SetAll(values, expiration);
            }

            //_localCachingProvider.SetAll(values, expiration);

            //try
            //{
            //    _distributedCachingProvider.SetAll(values, expiration);
            //}
            //catch (Exception ex)
            //{
            //    System.Console.WriteLine(ex.Message);
            //}
        }

        /// <summary>
        /// Sets all async.
        /// </summary>
        /// <returns>The all async.</returns>
        /// <param name="values">Values.</param>
        /// <param name="expiration">Expiration.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public async Task SetAllAsync<T>(IDictionary<string, T> values, TimeSpan expiration) where T : class
        {
            ArgumentCheck.NotNegativeOrZero(expiration, nameof(expiration));
            ArgumentCheck.NotNullAndCountGTZero(values, nameof(values));

            var tasks = new List<Task>();

            foreach (var provider in _providers)
            {
                tasks.Add(provider.SetAllAsync(values, expiration));
            }

            await Task.WhenAll(tasks);

            //await _localCachingProvider.SetAllAsync(values, expiration);
            //try
            //{
            //    await _distributedCachingProvider.SetAllAsync(values, expiration);
            //}
            //catch (Exception ex)
            //{
            //    System.Console.WriteLine(ex.Message);
            //}
        }

        /// <summary>
        /// Gets all.
        /// </summary>
        /// <returns>The all.</returns>
        /// <param name="cacheKeys">Cache keys.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public IDictionary<string, CacheValue<T>> GetAll<T>(IEnumerable<string> cacheKeys) where T : class
        {
            ArgumentCheck.NotNullAndCountGTZero(cacheKeys, nameof(cacheKeys));

            var localDict = _localCachingProvider.GetAll<T>(cacheKeys);

            //not find in local caching.
            var localNotFindKeys = localDict.Where(x => !x.Value.HasValue).Select(x => x.Key);

            if (localNotFindKeys.Count() <= 0)
            {
                return localDict;
            }

            try
            {
                foreach (var item in localNotFindKeys)
                    localDict.Remove(item);

                var disDict = _distributedCachingProvider.GetAll<T>(localNotFindKeys);
                return localDict.Concat(disDict).ToDictionary(k => k.Key, v => v.Value);
            }
            catch (Exception ex)
            {
                System.Console.WriteLine(ex.Message);
            }

            return localDict;
        }

        /// <summary>
        /// Gets all async.
        /// </summary>
        /// <returns>The all async.</returns>
        /// <param name="cacheKeys">Cache keys.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public async Task<IDictionary<string, CacheValue<T>>> GetAllAsync<T>(IEnumerable<string> cacheKeys) where T : class
        {
            ArgumentCheck.NotNullAndCountGTZero(cacheKeys, nameof(cacheKeys));

            var localDict = await _localCachingProvider.GetAllAsync<T>(cacheKeys);

            //not find in local caching.
            var localNotFindKeys = localDict.Where(x => !x.Value.HasValue).Select(x => x.Key);

            if (localNotFindKeys.Count() <= 0)
            {
                return localDict;
            }

            try
            {
                foreach (var item in localNotFindKeys)
                    localDict.Remove(item);

                var disDict = await _distributedCachingProvider.GetAllAsync<T>(cacheKeys);
                return localDict.Concat(disDict).ToDictionary(k => k.Key, v => v.Value);
            }
            catch (Exception ex)
            {
                System.Console.WriteLine(ex.Message);
            }

            return localDict;
        }

        /// <summary>
        /// Gets the by prefix.
        /// </summary>
        /// <returns>The by prefix.</returns>
        /// <param name="prefix">Prefix.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public IDictionary<string, CacheValue<T>> GetByPrefix<T>(string prefix) where T : class
        {
            ArgumentCheck.NotNullOrWhiteSpace(prefix, nameof(prefix));

            var localDict = _localCachingProvider.GetByPrefix<T>(prefix);

            //not find in local caching.
            var localNotFindKeys = localDict.Where(x => !x.Value.HasValue).Select(x => x.Key);

            if (localNotFindKeys.Count() <= 0)
            {
                return localDict;
            }

            try
            {
                foreach (var item in localNotFindKeys)
                    localDict.Remove(item);

                var disDict = _distributedCachingProvider.GetByPrefix<T>(prefix);
                return localDict.Concat(disDict).ToDictionary(k => k.Key, v => v.Value);
            }
            catch (Exception ex)
            {
                System.Console.WriteLine(ex.Message);
            }

            return localDict;
        }

        /// <summary>
        /// Gets the by prefix async.
        /// </summary>
        /// <returns>The by prefix async.</returns>
        /// <param name="prefix">Prefix.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public async Task<IDictionary<string, CacheValue<T>>> GetByPrefixAsync<T>(string prefix) where T : class
        {
            ArgumentCheck.NotNullOrWhiteSpace(prefix, nameof(prefix));

            var localDict = await _localCachingProvider.GetByPrefixAsync<T>(prefix);

            //not find in local caching.
            var localNotFindKeys = localDict.Where(x => !x.Value.HasValue).Select(x => x.Key);

            if (localNotFindKeys.Count() <= 0)
            {
                return localDict;
            }

            try
            {
                foreach (var item in localNotFindKeys)
                    localDict.Remove(item);

                var disDict = await _distributedCachingProvider.GetByPrefixAsync<T>(prefix);
                return localDict.Concat(disDict).ToDictionary(k => k.Key, v => v.Value);
            }
            catch (Exception ex)
            {
                System.Console.WriteLine(ex.Message);
            }

            return localDict;
        }

        /// <summary>
        /// Removes all.
        /// </summary>
        /// <param name="cacheKeys">Cache keys.</param>
        public void RemoveAll(IEnumerable<string> cacheKeys)
        {
            ArgumentCheck.NotNullAndCountGTZero(cacheKeys, nameof(cacheKeys));

            foreach (var provider in _providers)
            {
                provider.RemoveAll(cacheKeys);
            }

            //_localCachingProvider.RemoveAll(cacheKeys);

            //try
            //{
            //    _distributedCachingProvider.RemoveAll(cacheKeys);
            //}
            //catch (Exception ex)
            //{
            //    System.Console.WriteLine(ex.Message);
            //}
        }

        /// <summary>
        /// Removes all async.
        /// </summary>
        /// <returns>The all async.</returns>
        /// <param name="cacheKeys">Cache keys.</param>
        public async Task RemoveAllAsync(IEnumerable<string> cacheKeys)
        {
            ArgumentCheck.NotNullAndCountGTZero(cacheKeys, nameof(cacheKeys));

            var tasks = new List<Task>();

            foreach (var provider in _providers)
            {
                tasks.Add(provider.RemoveAllAsync(cacheKeys));
            }

            await Task.WhenAll(tasks);

            //await _localCachingProvider.RemoveAllAsync(cacheKeys);

            //try
            //{
            //    await _distributedCachingProvider.RemoveAllAsync(cacheKeys);
            //}
            //catch (Exception ex)
            //{
            //    System.Console.WriteLine(ex.Message);
            //}
        }

        /// <summary>
        /// Gets the count.
        /// </summary>
        /// <returns>The count.</returns>
        /// <param name="prefix">Prefix.</param>
        public int GetCount(string prefix = "")
        {
            return Math.Max(_localCachingProvider.GetCount(prefix), _distributedCachingProvider.GetCount(prefix));
        }

        /// <summary>
        /// Flush All Cached Item.
        /// </summary>
        public void Flush()
        {
            foreach (var provider in _providers)
            {
                provider.Flush();
            }

            //_localCachingProvider.Flush();

            //try
            //{
            //    _distributedCachingProvider.Flush();
            //}
            //catch (Exception ex)
            //{
            //    System.Console.WriteLine(ex.Message);
            //}
        }

        /// <summary>
        /// Flush All Cached Item async.
        /// </summary>
        /// <returns>The async.</returns>
        public async Task FlushAsync()
        {
            var tasks = new List<Task>();

            foreach (var provider in _providers)
            {
                tasks.Add(provider.FlushAsync());
            }

            await Task.WhenAll(tasks);

            //await _localCachingProvider.FlushAsync();

            //try
            //{
            //    await _distributedCachingProvider.FlushAsync();
            //}
            //catch (Exception ex)
            //{
            //    System.Console.WriteLine(ex.Message);
            //}
        }
    }
}
