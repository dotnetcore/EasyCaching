namespace EasyCaching.Memcached
{
    using EasyCaching.Core;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Cryptography;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// Default memcached caching provider.
    /// </summary>
    public class DefaultMemcachedCachingProvider : EasyCachingAbstractProvider
    {
        /// <summary>
        /// The memcached client.
        /// </summary>
        private readonly EasyCachingMemcachedClient _memcachedClient;

        /// <summary>
        /// The options.
        /// </summary>
        private readonly MemcachedOptions _options;

        /// <summary>
        /// The logger.
        /// </summary>
        private readonly ILogger _logger;

        /// <summary>
        /// The cache stats.
        /// </summary>
        private readonly CacheStats _cacheStats;

        /// <summary>
        /// The name.
        /// </summary>
        private readonly string _name;
       
        /// <summary>
        /// Initializes a new instance of the <see cref="T:EasyCaching.Memcached.DefaultMemcachedCachingProvider"/> class.
        /// </summary>
        /// <param name="name">Name.</param>
        /// <param name="memcachedClients">Memcached client.</param>
        /// <param name="options">Options.</param>
        /// <param name="loggerFactory">Logger factory.</param>
        public DefaultMemcachedCachingProvider(
            string name,
            IEnumerable<EasyCachingMemcachedClient> memcachedClients,
            MemcachedOptions options,
            ILoggerFactory loggerFactory = null)
        {
            this._name = name;
            this._memcachedClient = memcachedClients.Single(x => x.Name.Equals(this._name));
            this._options = options;
            this._logger = loggerFactory?.CreateLogger<DefaultMemcachedCachingProvider>();
            this._cacheStats = new CacheStats();

            this.ProviderName = this._name;
            this.ProviderStats = this._cacheStats;
            this.ProviderType = _options.CachingProviderType;
            this.ProviderOrder = _options.Order;
            this.ProviderMaxRdSecond = _options.MaxRdSecond;
            this.IsDistributedProvider = true;
        }

        /// <summary>
        /// Get the specified cacheKey, dataRetriever and expiration.
        /// </summary>
        /// <returns>The get.</returns>
        /// <param name="cacheKey">Cache key.</param>
        /// <param name="dataRetriever">Data retriever.</param>
        /// <param name="expiration">Expiration.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public override CacheValue<T> BaseGet<T>(string cacheKey, Func<T> dataRetriever, TimeSpan expiration)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));
            ArgumentCheck.NotNegativeOrZero(expiration, nameof(expiration));

            if (_memcachedClient.Get(this.HandleCacheKey(cacheKey)) is T result)
            {
                CacheStats.OnHit();

                if (_options.EnableLogging)
                    _logger?.LogInformation($"Cache Hit : cachekey = {cacheKey}");

                return new CacheValue<T>(result, true);
            }

            CacheStats.OnMiss();

            if (_options.EnableLogging)
                _logger?.LogInformation($"Cache Missed : cachekey = {cacheKey}");

            var flag = _memcachedClient.Store(Enyim.Caching.Memcached.StoreMode.Add, this.HandleCacheKey($"{cacheKey}_Lock"), 1, TimeSpan.FromMilliseconds(_options.LockMs));

            if (!flag)
            {
                System.Threading.Thread.Sleep(_options.SleepMs);
                return Get(cacheKey, dataRetriever, expiration);
            }

            var item = dataRetriever();
            if (item != null)
            {
                this.Set(cacheKey, item, expiration);
                _memcachedClient.Remove(this.HandleCacheKey($"{cacheKey}_Lock"));
                return new CacheValue<T>(item, true);
            }
            else
            {
                _memcachedClient.Remove(this.HandleCacheKey($"{cacheKey}_Lock"));
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
        public override async Task<CacheValue<T>> BaseGetAsync<T>(string cacheKey, Func<Task<T>> dataRetriever, TimeSpan expiration)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));
            ArgumentCheck.NotNegativeOrZero(expiration, nameof(expiration));

            var result = await _memcachedClient.GetAsync<T>(this.HandleCacheKey(cacheKey));
            if (result.Success)
            {
                CacheStats.OnHit();

                if (_options.EnableLogging)
                    _logger?.LogInformation($"Cache Hit : cachekey = {cacheKey}");

                return new CacheValue<T>(result.Value, true);
            }

            CacheStats.OnMiss();

            if (_options.EnableLogging)
                _logger?.LogInformation($"Cache Missed : cachekey = {cacheKey}");

            var flag = await _memcachedClient.StoreAsync(Enyim.Caching.Memcached.StoreMode.Add, this.HandleCacheKey($"{cacheKey}_Lock"), 1, TimeSpan.FromMilliseconds(_options.LockMs));

            if (!flag)
            {
                await Task.Delay(_options.SleepMs);
                return await GetAsync(cacheKey, dataRetriever, expiration);
            }

            var item = await dataRetriever();
            if (item != null)
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
        /// Get the specified cacheKey.
        /// </summary>
        /// <returns>The get.</returns>
        /// <param name="cacheKey">Cache key.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public override CacheValue<T> BaseGet<T>(string cacheKey)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            if (_memcachedClient.Get(this.HandleCacheKey(cacheKey)) is T result)
            {
                CacheStats.OnHit();

                if (_options.EnableLogging)
                    _logger?.LogInformation($"Cache Hit : cachekey = {cacheKey}");

                return new CacheValue<T>(result, true);
            }
            else
            {
                CacheStats.OnMiss();

                if (_options.EnableLogging)
                    _logger?.LogInformation($"Cache Missed : cachekey = {cacheKey}");

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

            var result = await _memcachedClient.GetAsync<T>(this.HandleCacheKey(cacheKey));
            if (result.Success)
            {
                CacheStats.OnHit();

                if (_options.EnableLogging)
                    _logger?.LogInformation($"Cache Hit : cachekey = {cacheKey}");

                return new CacheValue<T>(result.Value, true);
            }
            else
            {
                CacheStats.OnMiss();

                if (_options.EnableLogging)
                    _logger?.LogInformation($"Cache Missed : cachekey = {cacheKey}");

                return CacheValue<T>.NoValue;
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
                CacheStats.OnHit();

                if (_options.EnableLogging)
                    _logger?.LogInformation($"Cache Hit : cachekey = {cacheKey}");

                return result;
            }
            else
            {
                CacheStats.OnMiss();

                if (_options.EnableLogging)
                    _logger?.LogInformation($"Cache Missed : cachekey = {cacheKey}");

                return null;
            }
        }

        /// <summary>
        /// Remove the specified cacheKey.
        /// </summary>
        /// <returns>The remove.</returns>
        /// <param name="cacheKey">Cache key.</param>
        public override void BaseRemove(string cacheKey)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            _memcachedClient.Remove(this.HandleCacheKey(cacheKey));
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
        /// Set the specified cacheKey, cacheValue and expiration.
        /// </summary>
        /// <returns>The set.</returns>
        /// <param name="cacheKey">Cache key.</param>
        /// <param name="cacheValue">Cache value.</param>
        /// <param name="expiration">Expiration.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public override void BaseSet<T>(string cacheKey, T cacheValue, TimeSpan expiration)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));
            ArgumentCheck.NotNull(cacheValue, nameof(cacheValue));
            ArgumentCheck.NotNegativeOrZero(expiration, nameof(expiration));

            if (MaxRdSecond > 0)
            {
                var addSec = new Random().Next(1, MaxRdSecond);
                expiration = expiration.Add(TimeSpan.FromSeconds(addSec));
            }

            _memcachedClient.Store(Enyim.Caching.Memcached.StoreMode.Set, this.HandleCacheKey(cacheKey), cacheValue, expiration);
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
            ArgumentCheck.NotNull(cacheValue, nameof(cacheValue));
            ArgumentCheck.NotNegativeOrZero(expiration, nameof(expiration));

            if (MaxRdSecond > 0)
            {
                var addSec = new Random().Next(1, MaxRdSecond);
                expiration = expiration.Add(TimeSpan.FromSeconds(addSec));
            }

            await _memcachedClient.StoreAsync(Enyim.Caching.Memcached.StoreMode.Set, this.HandleCacheKey(cacheKey), cacheValue, expiration);
        }

        /// <summary>
        /// Exists the specified cacheKey.
        /// </summary>
        /// <returns>The exists.</returns>
        /// <param name="cacheKey">Cache key.</param>
        public override bool BaseExists(string cacheKey)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            return _memcachedClient.TryGet(this.HandleCacheKey(cacheKey), out object obj);
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
        /// Refresh the specified cacheKey, cacheValue and expiration.
        /// </summary>
        /// <param name="cacheKey">Cache key.</param>
        /// <param name="cacheValue">Cache value.</param>
        /// <param name="expiration">Expiration.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public override void BaseRefresh<T>(string cacheKey, T cacheValue, TimeSpan expiration)
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
        public override async Task BaseRefreshAsync<T>(string cacheKey, T cacheValue, TimeSpan expiration)
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
        /// <remarks>
        /// Before using the method , you should follow this link 
        /// https://github.com/memcached/memcached/wiki/ProgrammingTricks#namespacing
        /// and confirm that you use the namespacing when you set and get the cache.
        /// </remarks>
        /// <param name="prefix">Prefix of CacheKey.</param>
        public override void BaseRemoveByPrefix(string prefix)
        {
            ArgumentCheck.NotNullOrWhiteSpace(prefix, nameof(prefix));

            var oldPrefixKey = _memcachedClient.Get(prefix)?.ToString();

            var newValue = DateTime.UtcNow.Ticks.ToString();

            if (_options.EnableLogging)
                _logger?.LogInformation($"RemoveByPrefix : prefix = {prefix}");

            if (oldPrefixKey.Equals(newValue))
            {
                newValue = string.Concat(newValue, new Random().Next(9).ToString());
            }
            _memcachedClient.Store(Enyim.Caching.Memcached.StoreMode.Set, this.HandleCacheKey(prefix), newValue, new TimeSpan(0, 0, 0));
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
            await _memcachedClient.StoreAsync(Enyim.Caching.Memcached.StoreMode.Set, this.HandleCacheKey(prefix), newValue, new TimeSpan(0, 0, 0));
        }

        /// <summary>
        /// Handle the cache key of memcached limititaion
        /// </summary>
        /// <param name="cacheKey">Cache Key</param>
        /// <returns></returns>
        private string HandleCacheKey(string cacheKey)
        {
            // Memcached has a 250 character limit
            // Following memcached.h in https://github.com/memcached/memcached/
            if (cacheKey.Length >= 250)
            {
                using (SHA1 sha1 = SHA1.Create())
                {
                    byte[] data = sha1.ComputeHash(Encoding.UTF8.GetBytes(cacheKey));
                    return Convert.ToBase64String(data, Base64FormattingOptions.None);
                }
            }

            return cacheKey;
        }

        /// <summary>
        /// Sets all.
        /// </summary>
        /// <param name="values">Values.</param>
        /// <param name="expiration">Expiration.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public override void BaseSetAll<T>(IDictionary<string, T> values, TimeSpan expiration)
        {
            ArgumentCheck.NotNegativeOrZero(expiration, nameof(expiration));
            ArgumentCheck.NotNullAndCountGTZero(values, nameof(values));

            foreach (var item in values)
            {
                Set(item.Key, item.Value, expiration);
            }
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
        /// Gets all.
        /// </summary>
        /// <returns>The all.</returns>
        /// <param name="cacheKeys">Cache keys.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public override IDictionary<string, CacheValue<T>> BaseGetAll<T>(IEnumerable<string> cacheKeys)
        {
            ArgumentCheck.NotNullAndCountGTZero(cacheKeys, nameof(cacheKeys));

            var values = _memcachedClient.Get<T>(cacheKeys);
            var result = new Dictionary<string, CacheValue<T>>();

            foreach (var item in values)
            {
                if (item.Value != null)
                    result.Add(item.Key, new CacheValue<T>(item.Value, true));
                else
                    result.Add(item.Key, CacheValue<T>.NoValue);
            }

            return result;
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

            var values = await _memcachedClient.GetAsync<T>(cacheKeys);
            var result = new Dictionary<string, CacheValue<T>>();

            foreach (var item in values)
            {
                if (item.Value != null)
                    result.Add(item.Key, new CacheValue<T>(item.Value, true));
                else
                    result.Add(item.Key, CacheValue<T>.NoValue);
            }

            return result;
        }

        /// <summary>
        /// Gets the by prefix.
        /// </summary>
        /// <returns>The by prefix.</returns>
        /// <param name="prefix">Prefix.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public override IDictionary<string, CacheValue<T>> BaseGetByPrefix<T>(string prefix)
        {
            throw new NotImplementedException();
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
        /// Removes all.
        /// </summary>
        /// <param name="cacheKeys">Cache keys.</param>
        public override void BaseRemoveAll(IEnumerable<string> cacheKeys)
        {
            ArgumentCheck.NotNullAndCountGTZero(cacheKeys, nameof(cacheKeys));

            foreach (var item in cacheKeys.Distinct())
                Remove(item);
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
        /// Gets the count.
        /// </summary>
        /// <returns>The count.</returns>
        /// <param name="prefix">Prefix.</param>
        public override int BaseGetCount(string prefix = "")
        {
            if (string.IsNullOrWhiteSpace(prefix))
            {
                //Inaccurate, sometimes, memcached just causes items to expire but not free up or flush memory at once.
                return int.Parse(_memcachedClient.Stats().GetRaw("curr_items").FirstOrDefault().Value);
            }
            else
            {
                return 0;
            }
        }

        /// <summary>
        /// Flush All Cached Item.
        /// </summary>
        public override void BaseFlush()
        {
            if (_options.EnableLogging)
                _logger?.LogInformation("Memcached -- Flush");

            //not flush memory at once, just causes all items to expire
            _memcachedClient.FlushAll();
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
        /// Tries the set.
        /// </summary>
        /// <returns><c>true</c>, if set was tryed, <c>false</c> otherwise.</returns>
        /// <param name="cacheKey">Cache key.</param>
        /// <param name="cacheValue">Cache value.</param>
        /// <param name="expiration">Expiration.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public override bool BaseTrySet<T>(string cacheKey, T cacheValue, TimeSpan expiration)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));
            ArgumentCheck.NotNull(cacheValue, nameof(cacheValue));
            ArgumentCheck.NotNegativeOrZero(expiration, nameof(expiration));

            if (MaxRdSecond > 0)
            {
                var addSec = new Random().Next(1, MaxRdSecond);
                expiration = expiration.Add(TimeSpan.FromSeconds(addSec));
            }

            return _memcachedClient.Store(Enyim.Caching.Memcached.StoreMode.Add, this.HandleCacheKey(cacheKey), cacheValue, expiration);
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
            ArgumentCheck.NotNull(cacheValue, nameof(cacheValue));
            ArgumentCheck.NotNegativeOrZero(expiration, nameof(expiration));

            if (MaxRdSecond > 0)
            {
                var addSec = new Random().Next(1, MaxRdSecond);
                expiration = expiration.Add(TimeSpan.FromSeconds(addSec));
            }

            return _memcachedClient.StoreAsync(Enyim.Caching.Memcached.StoreMode.Add, this.HandleCacheKey(cacheKey), cacheValue, expiration);
        }
    }
}