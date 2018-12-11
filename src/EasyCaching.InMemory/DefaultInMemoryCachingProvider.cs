namespace EasyCaching.InMemory
{
    using EasyCaching.Core;
    using EasyCaching.Core.Internal;
    using Microsoft.Extensions.Caching.Memory;
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
        private readonly IMemoryCache _cache;

        /// <summary>
        /// The options.
        /// </summary>
        private readonly InMemoryOptions _options;

        /// <summary>
        /// The cache keys.
        /// </summary>
        private readonly ConcurrentCollections.ConcurrentHashSet<string> _cacheKeys;

        /// <summary>
        /// The logger.
        /// </summary>
        private readonly ILogger _logger;

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

        private readonly CacheStats _cacheStats;

        private readonly string _name;

        public CacheStats CacheStats => _cacheStats;

        public string Name => _name;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:EasyCaching.InMemory.DefaultInMemoryCachingProvider"/> class.
        /// </summary>
        /// <param name="cache">Cache.</param>
        /// <param name="options">Options.</param>
        /// <param name="loggerFactory">Logger factory.</param>
        public DefaultInMemoryCachingProvider(
            IMemoryCache cache,
            IOptionsMonitor<InMemoryOptions> options,
            ILoggerFactory loggerFactory = null)
        {
            this._cache = cache;
            this._options = options.CurrentValue;
            this._logger = loggerFactory?.CreateLogger<DefaultInMemoryCachingProvider>();
            this._cacheKeys = new ConcurrentCollections.ConcurrentHashSet<string>();

            this._cacheStats = new CacheStats();
            this._name = EasyCachingConstValue.DefaultInMemoryName;
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
           IMemoryCache cache,
           IOptionsMonitor<InMemoryOptions> options,
           ILoggerFactory loggerFactory = null)
        {
            this._cache = cache;
            this._options = options.CurrentValue;
            this._logger = loggerFactory?.CreateLogger<DefaultInMemoryCachingProvider>();
            this._cacheKeys = new ConcurrentCollections.ConcurrentHashSet<string>();

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
        public CacheValue<T> Get<T>(string cacheKey, Func<T> dataRetriever, TimeSpan expiration) where T : class
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));
            ArgumentCheck.NotNegativeOrZero(expiration, nameof(expiration));

            if (_cache.Get(BuildCacheKey(Name, cacheKey)) is T result)
            {
                if (_options.EnableLogging)
                    _logger?.LogInformation($"Cache Hit : cachekey = {BuildCacheKey(Name, cacheKey)}");

                CacheStats.OnHit();

                return new CacheValue<T>(result, true);
            }

            CacheStats.OnMiss();

            if (_options.EnableLogging)
                _logger?.LogInformation($"Cache Missed : cachekey = {BuildCacheKey(Name, cacheKey)}");

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

            if (_cache.Get(BuildCacheKey(Name, cacheKey)) is T result)
            {
                if (_options.EnableLogging)
                    _logger?.LogInformation($"Cache Hit : cachekey = {BuildCacheKey(Name, cacheKey)}");

                CacheStats.OnHit();

                return new CacheValue<T>(result, true);
            }

            CacheStats.OnMiss();

            if (_options.EnableLogging)
                _logger?.LogInformation($"Cache Missed : cachekey = {BuildCacheKey(Name, cacheKey)}");

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
        public CacheValue<T> Get<T>(string cacheKey)// where T : class
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            if (_cache.Get(BuildCacheKey(Name, cacheKey)) is T result)
            {
                if (_options.EnableLogging)
                    _logger?.LogInformation($"Cache Hit : cachekey = {BuildCacheKey(Name, cacheKey)}");

                CacheStats.OnHit();

                return new CacheValue<T>(result, true);
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
        public async Task<CacheValue<T>> GetAsync<T>(string cacheKey)// where T : class
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            var result = await Task.FromResult((T)_cache.Get(BuildCacheKey(Name, cacheKey)));

            if (result != null)
            {
                if (_options.EnableLogging)
                    _logger?.LogInformation($"Cache Hit : cachekey = {BuildCacheKey(Name, cacheKey)}");

                CacheStats.OnHit();

                return new CacheValue<T>(result, true);
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

            _cacheKeys.TryRemove(BuildCacheKey(Name, cacheKey));
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
                _cacheKeys.TryRemove(BuildCacheKey(Name, cacheKey));
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
        public void Set<T>(string cacheKey, T cacheValue, TimeSpan expiration)// where T : class
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));
            ArgumentCheck.NotNull(cacheValue, nameof(cacheValue));
            ArgumentCheck.NotNegativeOrZero(expiration, nameof(expiration));

            if (MaxRdSecond > 0)
            {
                var addSec = new Random().Next(1, MaxRdSecond);
                expiration.Add(new TimeSpan(0, 0, addSec));
            }

            _cache.Set(BuildCacheKey(Name, cacheKey), cacheValue, expiration);

            _cacheKeys.Add(BuildCacheKey(Name, cacheKey));
        }


        /// <summary>
        /// Sets the specified cacheKey, cacheValue and expiration async.
        /// </summary>
        /// <returns>The async.</returns>
        /// <param name="cacheKey">Cache key.</param>
        /// <param name="cacheValue">Cache value.</param>
        /// <param name="expiration">Expiration.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public async Task SetAsync<T>(string cacheKey, T cacheValue, TimeSpan expiration)// where T : class
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));
            ArgumentCheck.NotNull(cacheValue, nameof(cacheValue));
            ArgumentCheck.NotNegativeOrZero(expiration, nameof(expiration));

            if (MaxRdSecond > 0)
            {
                var addSec = new Random().Next(1, MaxRdSecond);
                expiration.Add(new TimeSpan(0, 0, addSec));
            }

            await Task.Run(() =>
            {
                _cache.Set(BuildCacheKey(Name, cacheKey), cacheValue, expiration);
                _cacheKeys.Add(BuildCacheKey(Name, cacheKey));
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

            return _cache.TryGetValue(BuildCacheKey(Name, cacheKey), out object value);
        }

        /// <summary>
        /// Existses the specified cacheKey async.
        /// </summary>
        /// <returns>The async.</returns>
        /// <param name="cacheKey">Cache key.</param>
        public async Task<bool> ExistsAsync(string cacheKey)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            return await Task.FromResult(_cache.TryGetValue(BuildCacheKey(Name, cacheKey), out object value));
        }

        /// <summary>
        /// Refresh the specified cacheKey, cacheValue and expiration.
        /// </summary>
        /// <param name="cacheKey">Cache key.</param>
        /// <param name="cacheValue">Cache value.</param>
        /// <param name="expiration">Expiration.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public void Refresh<T>(string cacheKey, T cacheValue, TimeSpan expiration)// where T : class
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
        public async Task RefreshAsync<T>(string cacheKey, T cacheValue, TimeSpan expiration)// where T : class
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

            var keys = _cacheKeys.Where(x => x.StartsWith(prefix.Trim(), StringComparison.OrdinalIgnoreCase));

            if (_options.EnableLogging)
                _logger?.LogInformation($"RemoveByPrefix : prefix = {prefix}");

            if (keys.Any())
            {
                foreach (var item in keys)
                {
                    _cache.Remove(item);
                    _cacheKeys.TryRemove(item);
                }
            }
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

            var keys = _cacheKeys.Where(x => x.StartsWith(prefix.Trim(), StringComparison.OrdinalIgnoreCase));

            if (_options.EnableLogging)
                _logger?.LogInformation($"RemoveByPrefixAsync : prefix = {prefix}");

            if (keys.Any())
            {
                var tasks = new List<Task>();
                foreach (var item in keys)
                {
                    tasks.Add(Task.Run(() =>
                    {
                        _cache.Remove(item);
                        _cacheKeys.TryRemove(item);
                    }));
                }
                //tasks.Add(RemoveAsync(item));

                await Task.WhenAll(tasks);
            }
        }

        /// <summary>
        /// Sets all.
        /// </summary>
        /// <param name="values">Values.</param>
        /// <param name="expiration">Expiration.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public void SetAll<T>(IDictionary<string, T> values, TimeSpan expiration)// where T : class
        {
            ArgumentCheck.NotNegativeOrZero(expiration, nameof(expiration));
            ArgumentCheck.NotNullAndCountGTZero(values, nameof(values));

            foreach (var entry in values)
                this.Set(entry.Key, entry.Value, expiration);
        }

        /// <summary>
        /// Sets all async.
        /// </summary>
        /// <returns>The all async.</returns>
        /// <param name="values">Values.</param>
        /// <param name="expiration">Expiration.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public async Task SetAllAsync<T>(IDictionary<string, T> values, TimeSpan expiration)// where T : class
        {
            ArgumentCheck.NotNegativeOrZero(expiration, nameof(expiration));
            ArgumentCheck.NotNullAndCountGTZero(values, nameof(values));

            var tasks = new List<Task>();

            foreach (var entry in values)
                tasks.Add(SetAsync(entry.Key, entry.Value, expiration));

            await Task.WhenAll(tasks);
        }

        /// <summary>
        /// Gets all.
        /// </summary>
        /// <returns>The all.</returns>
        /// <param name="cacheKeys">Cache keys.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public IDictionary<string, CacheValue<T>> GetAll<T>(IEnumerable<string> cacheKeys)// where T : class
        {
            ArgumentCheck.NotNullAndCountGTZero(cacheKeys, nameof(cacheKeys));

            if (_options.EnableLogging)
                _logger?.LogInformation($"GetAll : cacheKeys = {string.Join(",", cacheKeys)}");

            var map = new Dictionary<string, CacheValue<T>>();

            foreach (string key in cacheKeys)
                map[key] = Get<T>(key);

            return map;
        }

        /// <summary>
        /// Gets all async.
        /// </summary>
        /// <returns>The all async.</returns>
        /// <param name="cacheKeys">Cache keys.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public Task<IDictionary<string, CacheValue<T>>> GetAllAsync<T>(IEnumerable<string> cacheKeys)// where T : class
        {
            ArgumentCheck.NotNullAndCountGTZero(cacheKeys, nameof(cacheKeys));

            if (_options.EnableLogging)
                _logger?.LogInformation($"GetAllAsync : cacheKeys = {string.Join(",", cacheKeys)}");

            var map = new Dictionary<string, Task<CacheValue<T>>>();

            foreach (string key in cacheKeys)
                map[key] = GetAsync<T>(key);

            return Task.WhenAll(map.Values)
                .ContinueWith<IDictionary<string, CacheValue<T>>>(t =>
                    map.ToDictionary(k => k.Key, v => v.Value.Result), TaskContinuationOptions.OnlyOnRanToCompletion);
        }

        /// <summary>
        /// Gets the by prefix.
        /// </summary>
        /// <returns>The by prefix.</returns>
        /// <param name="prefix">Prefix.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public IDictionary<string, CacheValue<T>> GetByPrefix<T>(string prefix)// where T : class
        {
            ArgumentCheck.NotNullOrWhiteSpace(prefix, nameof(prefix));

            var map = new Dictionary<string, CacheValue<T>>();

            prefix = BuildCacheKey(Name, prefix);

            if (_options.EnableLogging)
                _logger?.LogInformation($"GetByPrefix : prefix = {prefix}");

            var keys = _cacheKeys.Where(x => x.StartsWith(prefix.Trim(), StringComparison.OrdinalIgnoreCase));

            if (keys.Any())
            {
                foreach (var key in keys)
                {
                    var cacheKey = string.IsNullOrWhiteSpace(_name) ? key : key.Substring(Name.Length + 1, key.Length - Name.Length - 1);
                    map[key] = this.Get<T>(cacheKey);
                }
            }
            return map;
        }

        /// <summary>
        /// Gets the by prefix async.
        /// </summary>
        /// <returns>The by prefix async.</returns>
        /// <param name="prefix">Prefix.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public Task<IDictionary<string, CacheValue<T>>> GetByPrefixAsync<T>(string prefix)// where T : class
        {
            ArgumentCheck.NotNullOrWhiteSpace(prefix, nameof(prefix));

            prefix = BuildCacheKey(Name, prefix);

            if (_options.EnableLogging)
                _logger?.LogInformation($"GetByPrefixAsync : prefix = {prefix}");

            var keys = _cacheKeys.Where(x => x.StartsWith(prefix.Trim(), StringComparison.OrdinalIgnoreCase));

            var map = new Dictionary<string, Task<CacheValue<T>>>();

            if (keys.Any())
            {
                foreach (string key in keys)
                {
                    var cacheKey = string.IsNullOrWhiteSpace(_name) ? key : key.Substring(Name.Length + 1, key.Length - Name.Length - 1);
                    map[key] = GetAsync<T>(cacheKey);
                }
            }

            return Task.WhenAll(map.Values)
                .ContinueWith<IDictionary<string, CacheValue<T>>>(t =>
                    map.ToDictionary(k => k.Key, v => v.Value.Result), TaskContinuationOptions.OnlyOnRanToCompletion);
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

            foreach (var key in cacheKeys.Distinct())
            {
                var cacheKey = string.IsNullOrWhiteSpace(_name) ? key : key.Substring(Name.Length + 1, key.Length - Name.Length - 1);
                Remove(cacheKey);
            }
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

            var tasks = new List<Task>();
            foreach (var key in cacheKeys.Distinct())
            {
                var cacheKey = string.IsNullOrWhiteSpace(_name) ? key : key.Substring(Name.Length + 1, key.Length - Name.Length - 1);
                tasks.Add(RemoveAsync(cacheKey));
            }

            await Task.WhenAll(tasks);
        }

        /// <summary>
        /// Gets the count.
        /// </summary>
        /// <returns>The count.</returns>
        /// <param name="prefix">Prefix.</param>
        public int GetCount(string prefix = "")
        {
            return string.IsNullOrWhiteSpace(prefix)
                    ? _cacheKeys.Count
                             : _cacheKeys.Count(x => x.StartsWith(BuildCacheKey(Name, prefix.Trim()), StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Flush All Cached Item.
        /// </summary>
        public void Flush()
        {
            if (_options.EnableLogging)
                _logger?.LogInformation("Flush");

            var cacheKeys = string.IsNullOrWhiteSpace(_name)
                                  ? _cacheKeys
                                  : _cacheKeys.Where(x => x.StartsWith(_name, StringComparison.OrdinalIgnoreCase));

            foreach (var item in cacheKeys)
            {
                _cache.Remove(item);
                _cacheKeys.TryRemove(item);
            }
        }

        /// <summary>
        /// Flush All Cached Item async.
        /// </summary>
        /// <returns>The async.</returns>
        public async Task FlushAsync()
        {
            if (_options.EnableLogging)
                _logger?.LogInformation("FlushAsync");

            Flush();
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
    }
}
