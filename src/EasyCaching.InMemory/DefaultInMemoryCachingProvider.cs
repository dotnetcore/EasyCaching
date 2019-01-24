namespace EasyCaching.InMemory
{
    using EasyCaching.Core;
    using EasyCaching.Core.Internal;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    /// <summary>
    /// MemoryCaching provider.
    /// </summary>
    public class DefaultInMemoryCachingProvider : IEasyCachingProvider
    {
        /// <summary>
        /// The MemoryCache.
        /// </summary>
        private readonly IInMemoryCaching _cache;

        /// <summary>
        /// The options.
        /// </summary>
        private readonly InMemoryOptions _options;

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
        /// <see cref="T:EasyCaching.InMemory.InMemoryCachingProvider"/> 
        /// is not distributed cache.
        /// </summary>
        /// <value><c>true</c> if is distributed cache; otherwise, <c>false</c>.</value>
        public bool IsDistributedCache => false;

        /// <summary>
        /// Gets the order.
        /// </summary>
        /// <value>The order.</value>
        public int Order => _options.Order;

        /// <summary>
        /// Gets the max random second.
        /// </summary>
        /// <value>The max rd second.</value>
        public int MaxRdSecond => _options.MaxRdSecond;

        /// <summary>
        /// Gets the type of the caching provider.
        /// </summary>
        /// <value>The type of the caching provider.</value>
        public CachingProviderType CachingProviderType => _options.CachingProviderType;

        /// <summary>
        /// Gets the cache stats.
        /// </summary>
        /// <value>The cache stats.</value>
        public CacheStats CacheStats => _cacheStats;

        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>The name.</value>
        public string Name => _name;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:EasyCaching.InMemory.DefaultInMemoryCachingProvider"/> class.
        /// </summary>
        /// <param name="cache">Cache.</param>
        /// <param name="options">Options.</param>
        /// <param name="loggerFactory">Logger factory.</param>
        public DefaultInMemoryCachingProvider(
            IInMemoryCaching cache,
            IOptionsMonitor<InMemoryOptions> options,
            ILoggerFactory loggerFactory = null)
        {
            this._cache = cache;
            this._options = options.CurrentValue;
            this._logger = loggerFactory?.CreateLogger<DefaultInMemoryCachingProvider>();
            //this._cacheKeys = new ConcurrentCollections.ConcurrentHashSet<string>();

            this._cacheStats = new CacheStats();
            //this._name = EasyCachingConstValue.DefaultInMemoryName;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:EasyCaching.InMemory.DefaultInMemoryCachingProvider"/> class.
        /// </summary>
        /// <param name="name">Name.</param>
        /// <param name="cache">Cache.</param>
        /// <param name="options">Options.</param>
        /// <param name="loggerFactory">Logger factory.</param>
        public DefaultInMemoryCachingProvider(
           string name,
           IInMemoryCaching cache,
           IOptionsMonitor<InMemoryOptions> options,
           ILoggerFactory loggerFactory = null)
        {
            this._cache = cache;
            this._options = options.CurrentValue;
            this._logger = loggerFactory?.CreateLogger<DefaultInMemoryCachingProvider>();
            // this._cacheKeys = new ConcurrentCollections.ConcurrentHashSet<string>();

            this._cacheStats = new CacheStats();
            this._name = name;
        }

        /// <summary>
        /// Get the specified cacheKey, dataRetriever and expiration.
        /// </summary>
        /// <returns>The get.</returns>
        /// <param name="cacheKey">Cache key.</param>
        /// <param name="dataRetriever">Data retriever.</param>
        /// <param name="expiration">Expiration.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public CacheValue<T> Get<T>(string cacheKey, Func<T> dataRetriever, TimeSpan expiration)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));
            ArgumentCheck.NotNegativeOrZero(expiration, nameof(expiration));

            ////mutex key
            //Lock(cacheKey);

            var result = _cache.Get<T>(BuildCacheKey(Name, cacheKey));
            if (result.HasValue)
            {
                if (_options.EnableLogging)
                    _logger?.LogInformation($"Cache Hit : cachekey = {BuildCacheKey(Name, cacheKey)}");

                CacheStats.OnHit();

                return result;
            }

            CacheStats.OnMiss();

            if (_options.EnableLogging)
                _logger?.LogInformation($"Cache Missed : cachekey = {BuildCacheKey(Name, cacheKey)}");
                
            if (! _cache.Add($"{cacheKey}_Lock", 1, TimeSpan.FromMilliseconds(_options.LockMs)))
            {
                System.Threading.Thread.Sleep(_options.SleepMs);
                return Get(cacheKey, dataRetriever, expiration);
            }

            var res = dataRetriever();

            if (res != null)
            {
                Set(cacheKey, res, expiration);
                //remove mutex key
                _cache.Remove($"{cacheKey}_Lock");

                return new CacheValue<T>(res, true);
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
        public async Task<CacheValue<T>> GetAsync<T>(string cacheKey, Func<Task<T>> dataRetriever, TimeSpan expiration)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));
            ArgumentCheck.NotNegativeOrZero(expiration, nameof(expiration));

            var result = _cache.Get<T>(BuildCacheKey(Name, cacheKey));
            if (result.HasValue)
            {
                if (_options.EnableLogging)
                    _logger?.LogInformation($"Cache Hit : cachekey = {BuildCacheKey(Name, cacheKey)}");

                CacheStats.OnHit();

                return result;
            }

            CacheStats.OnMiss();

            if (_options.EnableLogging)
                _logger?.LogInformation($"Cache Missed : cachekey = {BuildCacheKey(Name, cacheKey)}");

            if (!_cache.Add($"{cacheKey}_Lock", 1, TimeSpan.FromMilliseconds(_options.LockMs)))
            {
                //wait for some ms
                await Task.Delay(_options.SleepMs);
                return await GetAsync(cacheKey, dataRetriever, expiration);
            }

            var res = await dataRetriever();

            if (res != null)
            {
                await SetAsync(cacheKey, res, expiration);
                //remove mutex key
                _cache.Remove($"{cacheKey}_Lock");

                return new CacheValue<T>(res, true);
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
        public CacheValue<T> Get<T>(string cacheKey)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            var result = _cache.Get<T>(BuildCacheKey(Name, cacheKey));
            if (result.HasValue)
            {
                if (_options.EnableLogging)
                    _logger?.LogInformation($"Cache Hit : cachekey = {BuildCacheKey(Name, cacheKey)}");

                CacheStats.OnHit();

                return result;
            }
            else
            {
                if (_options.EnableLogging)
                    _logger?.LogInformation($"Cache Missed : cachekey = {BuildCacheKey(Name, cacheKey)}");

                CacheStats.OnMiss();

                return CacheValue<T>.NoValue;
            }
        }

        /// <summary>
        /// Gets the specified cacheKey async.
        /// </summary>
        /// <returns>The async.</returns>
        /// <param name="cacheKey">Cache key.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public async Task<CacheValue<T>> GetAsync<T>(string cacheKey)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            var result = await Task.FromResult(_cache.Get<T>(BuildCacheKey(Name, cacheKey)));

            if (result.HasValue)
            {
                if (_options.EnableLogging)
                    _logger?.LogInformation($"Cache Hit : cachekey = {BuildCacheKey(Name, cacheKey)}");

                CacheStats.OnHit();

                return result;
            }
            else
            {
                if (_options.EnableLogging)
                    _logger?.LogInformation($"Cache Missed : cachekey = {BuildCacheKey(Name, cacheKey)}");

                CacheStats.OnMiss();

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

            _cache.Remove(BuildCacheKey(Name, cacheKey));
        }

        /// <summary>
        /// Removes the specified cacheKey async.
        /// </summary>
        /// <returns>The async.</returns>
        /// <param name="cacheKey">Cache key.</param>
        public async Task RemoveAsync(string cacheKey)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            await Task.Run(() =>
            {
                _cache.Remove(BuildCacheKey(Name, cacheKey));
            });
        }

        /// <summary>
        /// Set the specified cacheKey, cacheValue and expiration.
        /// </summary>
        /// <returns>The set.</returns>
        /// <param name="cacheKey">Cache key.</param>
        /// <param name="cacheValue">Cache value.</param>
        /// <param name="expiration">expiration.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public void Set<T>(string cacheKey, T cacheValue, TimeSpan expiration)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));
            ArgumentCheck.NotNull(cacheValue, nameof(cacheValue));
            ArgumentCheck.NotNegativeOrZero(expiration, nameof(expiration));

            if (MaxRdSecond > 0)
            {
                var addSec = new Random().Next(1, MaxRdSecond);
                expiration = expiration.Add(TimeSpan.FromSeconds(addSec));
            }

            //var valExpiration = expiration.Seconds <= 1 ? expiration : TimeSpan.FromSeconds(expiration.Seconds / 2);
            //var val = new CacheValue<T>(cacheValue, true, valExpiration);
            _cache.Set(BuildCacheKey(Name, cacheKey), cacheValue, expiration);
        }

        /// <summary>
        /// Sets the specified cacheKey, cacheValue and expiration async.
        /// </summary>
        /// <returns>The async.</returns>
        /// <param name="cacheKey">Cache key.</param>
        /// <param name="cacheValue">Cache value.</param>
        /// <param name="expiration">Expiration.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public async Task SetAsync<T>(string cacheKey, T cacheValue, TimeSpan expiration)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));
            ArgumentCheck.NotNull(cacheValue, nameof(cacheValue));
            ArgumentCheck.NotNegativeOrZero(expiration, nameof(expiration));

            if (MaxRdSecond > 0)
            {
                var addSec = new Random().Next(1, MaxRdSecond);
                expiration = expiration.Add(TimeSpan.FromSeconds(addSec));
            }

            await Task.Run(() =>
            {
                //var valExpiration = expiration.Seconds <= 1 ? expiration : TimeSpan.FromSeconds(expiration.Seconds / 2);
                //var val = new CacheValue<T>(cacheValue, true, valExpiration);
                _cache.Set(BuildCacheKey(Name, cacheKey), cacheValue, expiration);
            });
        }

        /// <summary>
        /// Exists the specified cacheKey.
        /// </summary>
        /// <returns>The exists.</returns>
        /// <param name="cacheKey">Cache key.</param>
        public bool Exists(string cacheKey)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            return _cache.Exists(BuildCacheKey(Name, cacheKey));
        }

        /// <summary>
        /// Existses the specified cacheKey async.
        /// </summary>
        /// <returns>The async.</returns>
        /// <param name="cacheKey">Cache key.</param>
        public async Task<bool> ExistsAsync(string cacheKey)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            return await Task.FromResult(_cache.Exists(BuildCacheKey(Name, cacheKey)));
        }

        /// <summary>
        /// Refresh the specified cacheKey, cacheValue and expiration.
        /// </summary>
        /// <param name="cacheKey">Cache key.</param>
        /// <param name="cacheValue">Cache value.</param>
        /// <param name="expiration">Expiration.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public void Refresh<T>(string cacheKey, T cacheValue, TimeSpan expiration)
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
        public async Task RefreshAsync<T>(string cacheKey, T cacheValue, TimeSpan expiration)
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
        /// <param name="prefix">Prefix.</param>
        public void RemoveByPrefix(string prefix)
        {
            ArgumentCheck.NotNullOrWhiteSpace(prefix, nameof(prefix));

            prefix = BuildCacheKey(Name, prefix);

            var count = _cache.RemoveByPrefix(prefix);

            if (_options.EnableLogging)
                _logger?.LogInformation($"RemoveByPrefix : prefix = {prefix} , count = {count}");
        }

        /// <summary>
        /// Removes cached item by cachekey's prefix async.
        /// </summary>
        /// <returns>The by prefix async.</returns>
        /// <param name="prefix">Prefix.</param>
        public async Task RemoveByPrefixAsync(string prefix)
        {
            ArgumentCheck.NotNullOrWhiteSpace(prefix, nameof(prefix));

            prefix = BuildCacheKey(Name, prefix);

            var count = await Task.Run(() => _cache.RemoveByPrefix(prefix));

            if (_options.EnableLogging)
                _logger?.LogInformation($"RemoveByPrefixAsync : prefix = {prefix} , count = {count}");
        }

        /// <summary>
        /// Sets all.
        /// </summary>
        /// <param name="values">Values.</param>
        /// <param name="expiration">Expiration.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public void SetAll<T>(IDictionary<string, T> values, TimeSpan expiration)
        {
            ArgumentCheck.NotNegativeOrZero(expiration, nameof(expiration));
            ArgumentCheck.NotNullAndCountGTZero(values, nameof(values));

            var newDict = new Dictionary<string, T>();

            foreach (var item in values)
            {
                //var valExpiration = expiration.Seconds <= 1 ? expiration : TimeSpan.FromSeconds(expiration.Seconds / 2);
                //var val = new CacheValue<T>(item.Value, true, valExpiration);
                newDict.Add(BuildCacheKey(this._name, item.Key), item.Value);
            }

            _cache.SetAll(newDict, expiration);
        }

        /// <summary>
        /// Sets all async.
        /// </summary>
        /// <returns>The all async.</returns>
        /// <param name="values">Values.</param>
        /// <param name="expiration">Expiration.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public async Task SetAllAsync<T>(IDictionary<string, T> values, TimeSpan expiration)
        {
            ArgumentCheck.NotNegativeOrZero(expiration, nameof(expiration));
            ArgumentCheck.NotNullAndCountGTZero(values, nameof(values));

            var newDict = new Dictionary<string, T>();

            foreach (var item in values)
            {
                //var valExpiration = expiration.Seconds <= 1 ? expiration : TimeSpan.FromSeconds(expiration.Seconds / 2);
                //var val = new CacheValue<T>(item.Value, true, valExpiration);
                newDict.Add(BuildCacheKey(this._name, item.Key), item.Value);
            }

            await Task.Run(() => _cache.SetAll(newDict, expiration));
        }

        /// <summary>
        /// Gets all.
        /// </summary>
        /// <returns>The all.</returns>
        /// <param name="cacheKeys">Cache keys.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public IDictionary<string, CacheValue<T>> GetAll<T>(IEnumerable<string> cacheKeys)
        {
            ArgumentCheck.NotNullAndCountGTZero(cacheKeys, nameof(cacheKeys));

            if (_options.EnableLogging)
                _logger?.LogInformation($"GetAll : cacheKeys = {string.Join(",", cacheKeys)}");

            var keys = new List<string>();

            foreach (var item in cacheKeys)
            {
                keys.Add(BuildCacheKey(this._name, item));
            }

            return _cache.GetAll<T>(keys);
        }

        /// <summary>
        /// Gets all async.
        /// </summary>
        /// <returns>The all async.</returns>
        /// <param name="cacheKeys">Cache keys.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public async Task<IDictionary<string, CacheValue<T>>> GetAllAsync<T>(IEnumerable<string> cacheKeys)
        {
            ArgumentCheck.NotNullAndCountGTZero(cacheKeys, nameof(cacheKeys));

            if (_options.EnableLogging)
                _logger?.LogInformation($"GetAllAsync : cacheKeys = {string.Join(",", cacheKeys)}");

            var keys = new List<string>();

            foreach (var item in cacheKeys)
            {
                keys.Add(BuildCacheKey(this._name, item));
            }

            return await Task.FromResult(_cache.GetAll<T>(keys));
        }

        /// <summary>
        /// Gets the by prefix.
        /// </summary>
        /// <returns>The by prefix.</returns>
        /// <param name="prefix">Prefix.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public IDictionary<string, CacheValue<T>> GetByPrefix<T>(string prefix)
        {
            ArgumentCheck.NotNullOrWhiteSpace(prefix, nameof(prefix));

            var map = new Dictionary<string, CacheValue<T>>();

            prefix = BuildCacheKey(Name, prefix);

            if (_options.EnableLogging)
                _logger?.LogInformation($"GetByPrefix : prefix = {prefix}");

            return _cache.GetByPrefix<T>(prefix);
        }

        /// <summary>
        /// Gets the by prefix async.
        /// </summary>
        /// <returns>The by prefix async.</returns>
        /// <param name="prefix">Prefix.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public async Task<IDictionary<string, CacheValue<T>>> GetByPrefixAsync<T>(string prefix)
        {
            ArgumentCheck.NotNullOrWhiteSpace(prefix, nameof(prefix));
            var map = new Dictionary<string, CacheValue<T>>();
            prefix = BuildCacheKey(Name, prefix);

            if (_options.EnableLogging)
                _logger?.LogInformation($"GetByPrefixAsync : prefix = {prefix}");

            return await Task.FromResult(_cache.GetByPrefix<T>(prefix));
        }

        /// <summary>
        /// Removes all.
        /// </summary>
        /// <param name="cacheKeys">Cache keys.</param>
        public void RemoveAll(IEnumerable<string> cacheKeys)
        {
            ArgumentCheck.NotNullAndCountGTZero(cacheKeys, nameof(cacheKeys));

            cacheKeys = cacheKeys.Select(x => BuildCacheKey(this._name, x));

            if (_options.EnableLogging)
                _logger?.LogInformation($"RemoveAll : cacheKeys = {string.Join(",", cacheKeys)}");

            _cache.RemoveAll(cacheKeys);
        }

        /// <summary>
        /// Removes all async.
        /// </summary>
        /// <returns>The all async.</returns>
        /// <param name="cacheKeys">Cache keys.</param>
        public async Task RemoveAllAsync(IEnumerable<string> cacheKeys)
        {
            ArgumentCheck.NotNullAndCountGTZero(cacheKeys, nameof(cacheKeys));

            cacheKeys = cacheKeys.Select(x => BuildCacheKey(Name, x));

            if (_options.EnableLogging)
                _logger?.LogInformation($"RemoveAllAsync : cacheKeys = {string.Join(",", cacheKeys)}");

            await Task.Run(() => _cache.RemoveAll(cacheKeys));
        }

        /// <summary>
        /// Gets the count.
        /// </summary>
        /// <returns>The count.</returns>
        /// <param name="prefix">Prefix.</param>
        public int GetCount(string prefix = "")
        {
            return _cache.GetCount(prefix);
        }

        /// <summary>
        /// Flush All Cached Item.
        /// </summary>
        public void Flush()
        {
            if (_options.EnableLogging)
                _logger?.LogInformation("Flush");

            _cache.Clear(_name);
        }

        /// <summary>
        /// Flush All Cached Item async.
        /// </summary>
        /// <returns>The async.</returns>
        public async Task FlushAsync()
        {
            if (_options.EnableLogging)
                _logger?.LogInformation("FlushAsync");

            _cache.Clear(_name);
            await Task.CompletedTask;
        }

        /// <summary>
        /// Builds the cache key.
        /// </summary>
        /// <returns>The cache key.</returns>
        /// <param name="prividerName">Privider name.</param>
        /// <param name="cacheKey">Cache key.</param>
        private string BuildCacheKey(string prividerName, string cacheKey)
        {
            return string.IsNullOrWhiteSpace(prividerName)
                         ? cacheKey
                         : $"{prividerName}-{cacheKey}";
        }

        /// <summary>
        /// Tries the set.
        /// </summary>
        /// <returns><c>true</c>, if set was tryed, <c>false</c> otherwise.</returns>
        /// <param name="cacheKey">Cache key.</param>
        /// <param name="cacheValue">Cache value.</param>
        /// <param name="expiration">Expiration.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public bool TrySet<T>(string cacheKey, T cacheValue, TimeSpan expiration)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));
            ArgumentCheck.NotNull(cacheValue, nameof(cacheValue));
            ArgumentCheck.NotNegativeOrZero(expiration, nameof(expiration));

            //var val = new CacheValue<T>(cacheValue, true, expiration);
            return _cache.Add(cacheKey, cacheValue, expiration);
        }

        /// <summary>
        /// Tries the set async.
        /// </summary>
        /// <returns>The set async.</returns>
        /// <param name="cacheKey">Cache key.</param>
        /// <param name="cacheValue">Cache value.</param>
        /// <param name="expiration">Expiration.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public Task<bool> TrySetAsync<T>(string cacheKey, T cacheValue, TimeSpan expiration)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));
            ArgumentCheck.NotNull(cacheValue, nameof(cacheValue));
            ArgumentCheck.NotNegativeOrZero(expiration, nameof(expiration));

            //var val = new CacheValue<T>(cacheValue, true, expiration);
            return Task.FromResult(_cache.Add(cacheKey, cacheValue, expiration));
        }
    }
}