namespace EasyCaching.Memcached
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using EasyCaching.Core;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Default memcached caching provider.
    /// </summary>
    public partial class DefaultMemcachedCachingProvider : EasyCachingAbstractProvider
    {       
        /// <summary>
        /// Gets the specified cacheKey, dataRetriever and expiration async.
        /// </summary>
        /// <returns>The async.</returns>
        /// <param name="cacheKey">Cache key.</param>
        /// <param name="dataRetriever">Data retriever.</param>
        /// <param name="expiration">Expiration.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public override async Task<CacheValue<T>> BaseGetAsync<T>(string cacheKey, Func<Task<T>> dataRetriever, TimeSpan expiration)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));
            ArgumentCheck.NotNegativeOrZero(expiration, nameof(expiration));

            var result = await BaseGetAsync<T>(cacheKey);

            if (result.HasValue)
            {
                return result;
            }
            
            var flag = await _memcachedClient.StoreAsync(Enyim.Caching.Memcached.StoreMode.Add,
                this.HandleCacheKey($"{cacheKey}_Lock"), 1, TimeSpan.FromMilliseconds(_options.LockMs));

            if (!flag)
            {
                await Task.Delay(_options.SleepMs);
                return await GetAsync(cacheKey, dataRetriever, expiration);
            }

            var item = await dataRetriever();
            if (item != null || _options.CacheNulls)
            {
                await this.SetAsync(cacheKey, item, expiration);
                await _memcachedClient.RemoveAsync(this.HandleCacheKey($"{cacheKey}_Lock"));
                return new CacheValue<T>(item, true);
            }
            else
            {
                await _memcachedClient.RemoveAsync(this.HandleCacheKey($"{cacheKey}_Lock"));
                return CacheValue<T>.NoValue;
            }
        }
      
        /// <summary>
        /// Gets the specified cacheKey async.
        /// </summary>
        /// <returns>The async.</returns>
        /// <param name="cacheKey">Cache key.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public override async Task<CacheValue<T>> BaseGetAsync<T>(string cacheKey)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            var result = await _memcachedClient.GetAsync<object>(this.HandleCacheKey(cacheKey));
            
            if (result.Success)
            {
                OnCacheHit(cacheKey);

                return NullValue.Equals(result.Value) 
                    ? CacheValue<T>.Null 
                    : new CacheValue<T>((T)result.Value, true);
            }
            else
            {
                OnCacheMiss(cacheKey);
                return CacheValue<T>.NoValue;
            }
        }

        /// <summary>
        /// Gets the count.
        /// </summary>
        /// <returns>The count.</returns>
        /// <param name="prefix">Prefix.</param>
        public override Task<int> BaseGetCountAsync(string prefix = "")
        {
            if (string.IsNullOrWhiteSpace(prefix))
            {
                //Inaccurate, sometimes, memcached just causes items to expire but not free up or flush memory at once.
                return Task.FromResult(int.Parse(_memcachedClient.Stats().GetRaw("curr_items").FirstOrDefault().Value));
            }
            else
            {
                return Task.FromResult(0);
            }
        }

        /// <summary>
        /// Gets the specified cacheKey async.
        /// </summary>
        /// <returns>The async.</returns>
        /// <param name="cacheKey">Cache key.</param>
        /// <param name="type">Object Type.</param>
        public override async Task<object> BaseGetAsync(string cacheKey, Type type)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            var result = await Task.FromResult(_memcachedClient.Get(this.HandleCacheKey(cacheKey)));
            if (result != null)
            {
                OnCacheHit(cacheKey);
                return result;
            }
            else
            {
                OnCacheMiss(cacheKey);
                return null;
            }
        }

        /// <summary>
        /// Removes the specified cacheKey async.
        /// </summary>
        /// <returns>The async.</returns>
        /// <param name="cacheKey">Cache key.</param>
        public override async Task BaseRemoveAsync(string cacheKey)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            await _memcachedClient.RemoveAsync(this.HandleCacheKey(cacheKey));
        }

        /// <summary>
        /// Sets the specified cacheKey, cacheValue and expiration async.
        /// </summary>
        /// <returns>The async.</returns>
        /// <param name="cacheKey">Cache key.</param>
        /// <param name="cacheValue">Cache value.</param>
        /// <param name="expiration">Expiration.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public override async Task BaseSetAsync<T>(string cacheKey, T cacheValue, TimeSpan expiration)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));
            ArgumentCheck.NotNull(cacheValue, nameof(cacheValue), _options.CacheNulls);
            ArgumentCheck.NotNegativeOrZero(expiration, nameof(expiration));

            if (MaxRdSecond > 0)
            {
                var addSec = new Random().Next(1, MaxRdSecond);
                expiration = expiration.Add(TimeSpan.FromSeconds(addSec));
            }

            await _memcachedClient.StoreAsync(
                Enyim.Caching.Memcached.StoreMode.Set, 
                this.HandleCacheKey(cacheKey), 
                this.ConvertToStoredValue(cacheValue), 
                expiration);
        }

        /// <summary>
        /// Existses the specified cacheKey async.
        /// </summary>
        /// <returns>The async.</returns>
        /// <param name="cacheKey">Cache key.</param>
        public override async Task<bool> BaseExistsAsync(string cacheKey)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            return await Task.FromResult(_memcachedClient.TryGet(this.HandleCacheKey(cacheKey), out object obj));
        }
                 
        /// <summary>
        /// Removes cached item by cachekey's prefix async.
        /// </summary>
        /// <remarks>
        /// Before using the method , you should follow this link 
        /// https://github.com/memcached/memcached/wiki/ProgrammingTricks#namespacing
        /// and confirm that you use the namespacing when you set and get the cache.
        /// </remarks>
        /// <param name="prefix">Prefix of CacheKey.</param>
        /// <returns></returns>
        public override async Task BaseRemoveByPrefixAsync(string prefix)
        {
            ArgumentCheck.NotNullOrWhiteSpace(prefix, nameof(prefix));

            var oldPrefixKey = _memcachedClient.Get(prefix)?.ToString();

            var newValue = DateTime.UtcNow.Ticks.ToString();

            if (_options.EnableLogging)
                _logger?.LogInformation($"RemoveByPrefixAsync : prefix = {prefix}");

            if (oldPrefixKey.Equals(newValue))
            {
                newValue = string.Concat(newValue, new Random().Next(9).ToString());
            }
            await _memcachedClient.StoreAsync(
                Enyim.Caching.Memcached.StoreMode.Set, 
                this.HandleCacheKey(prefix), 
                newValue, 
                new TimeSpan(0, 0, 0));
        }

        /// <summary>
        /// Sets all async.
        /// </summary>
        /// <returns>The all async.</returns>
        /// <param name="values">Values.</param>
        /// <param name="expiration">Expiration.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public override async Task BaseSetAllAsync<T>(IDictionary<string, T> values, TimeSpan expiration)
        {
            ArgumentCheck.NotNegativeOrZero(expiration, nameof(expiration));
            ArgumentCheck.NotNullAndCountGTZero(values, nameof(values));

            var tasks = new List<Task>();
            foreach (var item in values)
            {
                tasks.Add(SetAsync(item.Key, item.Value, expiration));
            }
            await Task.WhenAll(tasks);
        }
  
        /// <summary>
        /// Gets all async.
        /// </summary>
        /// <returns>The all async.</returns>
        /// <param name="cacheKeys">Cache keys.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public override async Task<IDictionary<string, CacheValue<T>>> BaseGetAllAsync<T>(IEnumerable<string> cacheKeys)
        {
            ArgumentCheck.NotNullAndCountGTZero(cacheKeys, nameof(cacheKeys));

            var values = await _memcachedClient.GetAsync<object>(cacheKeys);

            return values
                .ToDictionary(
                    pair => pair.Key,
                    pair => ConvertFromStoredValue<T>(pair.Value));
        }
      
        /// <summary>
        /// Gets the by prefix async.
        /// </summary>
        /// <returns>The by prefix async.</returns>
        /// <param name="prefix">Prefix.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public override Task<IDictionary<string, CacheValue<T>>> BaseGetByPrefixAsync<T>(string prefix)
        {
            throw new NotImplementedException();
        }
      
        /// <summary>
        /// Removes all async.
        /// </summary>
        /// <returns>The all async.</returns>
        /// <param name="cacheKeys">Cache keys.</param>
        public override async Task BaseRemoveAllAsync(IEnumerable<string> cacheKeys)
        {
            ArgumentCheck.NotNullAndCountGTZero(cacheKeys, nameof(cacheKeys));

            var tasks = new List<Task>();
            foreach (var item in cacheKeys.Distinct())
                tasks.Add(RemoveAsync(item));

            await Task.WhenAll(tasks);
        }

        /// <summary>
        /// Flush All Cached Item async.
        /// </summary>
        /// <returns>The async.</returns>
        public override async Task BaseFlushAsync()
        {
            if (_options.EnableLogging)
                _logger?.LogInformation("Memcached -- FlushAsync");

            await _memcachedClient.FlushAllAsync();
        }
     
        /// <summary>
        /// Tries the set async.
        /// </summary>
        /// <returns>The set async.</returns>
        /// <param name="cacheKey">Cache key.</param>
        /// <param name="cacheValue">Cache value.</param>
        /// <param name="expiration">Expiration.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public override Task<bool> BaseTrySetAsync<T>(string cacheKey, T cacheValue, TimeSpan expiration)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));
            ArgumentCheck.NotNull(cacheValue, nameof(cacheValue), _options.CacheNulls);
            ArgumentCheck.NotNegativeOrZero(expiration, nameof(expiration));

            if (MaxRdSecond > 0)
            {
                var addSec = new Random().Next(1, MaxRdSecond);
                expiration = expiration.Add(TimeSpan.FromSeconds(addSec));
            }

            return _memcachedClient.StoreAsync(
                Enyim.Caching.Memcached.StoreMode.Add, 
                this.HandleCacheKey(cacheKey),
                ConvertToStoredValue(cacheValue),
                expiration);
        }

        public override Task<TimeSpan> BaseGetExpirationAsync(string cacheKey)
        {
            throw new NotImplementedException();
        }
    }
}