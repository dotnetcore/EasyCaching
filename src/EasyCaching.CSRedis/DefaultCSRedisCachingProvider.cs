namespace EasyCaching.CSRedis
{
    using EasyCaching.Core;
    using EasyCaching.Core.Internal;
    using EasyCaching.Core.Serialization;
    using global::CSRedis;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public partial class DefaultCSRedisCachingProvider : IRedisCachingProvider //: IEasyCachingProvider
    {
        /// <summary>
        /// The cache.
        /// </summary>
        private readonly EasyCachingCSRedisClient _cache;

        /// <summary>
        /// The serializer.
        /// </summary>
        private readonly IEasyCachingSerializer _serializer;

        /// <summary>
        /// The logger.
        /// </summary>
        private readonly ILogger _logger;

        /// <summary>
        /// The options.
        /// </summary>
        private readonly RedisOptions _options;

        /// <summary>
        /// The cache stats.
        /// </summary>
        private readonly CacheStats _cacheStats;

        /// <summary>
        /// The name.
        /// </summary>
        private readonly string _name;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:EasyCaching.CSRedis.DefaultCSRedisCachingProvider"/> class.
        /// </summary>
        /// <param name="name">Name.</param>
        /// <param name="clients">Clients.</param>
        /// <param name="serializer">Serializer.</param>
        /// <param name="options">Options.</param>
        /// <param name="loggerFactory">Logger factory.</param>
        public DefaultCSRedisCachingProvider(
           string name,
           IEnumerable<EasyCachingCSRedisClient> clients,
           IEasyCachingSerializer serializer,
           IOptionsMonitor<RedisOptions> options,
           ILoggerFactory loggerFactory = null)
        {
            this._name = name;
            this._serializer = serializer;
            this._options = options.CurrentValue;
            this._logger = loggerFactory?.CreateLogger<DefaultCSRedisCachingProvider>();
            this._cache = clients.FirstOrDefault(x => x.Name.Equals(_name));
            this._cacheStats = new CacheStats();
        }

        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>The name.</value>
        public string Name => this._name;

        /// <summary>
        /// Gets a value indicating whether this <see cref="T:EasyCaching.CSRedis.DefaultCSRedisCachingProvider"/> is
        /// distributed cache.
        /// </summary>
        /// <value><c>true</c> if is distributed cache; otherwise, <c>false</c>.</value>
        public bool IsDistributedCache => true;

        /// <summary>
        /// Gets the order.
        /// </summary>
        /// <value>The order.</value>
        public int Order => this._options.Order;

        /// <summary>
        /// Gets the max rd second.
        /// </summary>
        /// <value>The max rd second.</value>
        public int MaxRdSecond => this._options.MaxRdSecond;

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
        /// Exists the specified cacheKey.
        /// </summary>
        /// <returns>The exists.</returns>
        /// <param name="cacheKey">Cache key.</param>
        public bool Exists(string cacheKey)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            return _cache.Exists(cacheKey);
        }

        /// <summary>
        /// Existses the async.
        /// </summary>
        /// <returns>The async.</returns>
        /// <param name="cacheKey">Cache key.</param>
        public async Task<bool> ExistsAsync(string cacheKey)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            return await _cache.ExistsAsync(cacheKey);
        }

        /// <summary>
        /// Flush this instance.
        /// </summary>
        public void Flush()
        {
            if (_options.EnableLogging)
                _logger?.LogInformation("Redis -- Flush");

            _cache.NodesServerManager.FlushDb();
        }

        /// <summary>
        /// Flushs the async.
        /// </summary>
        /// <returns>The async.</returns>
        public async Task FlushAsync()
        {
            if (_options.EnableLogging)
                _logger?.LogInformation("Redis -- FlushAsync");

            await _cache.NodesServerManager.FlushDbAsync();
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

            var result = _cache.Get<byte[]>(cacheKey);
            if (result != null)
            {
                CacheStats.OnHit();

                if (_options.EnableLogging)
                    _logger?.LogInformation($"Cache Hit : cachekey = {cacheKey}");

                var value = _serializer.Deserialize<T>(result);
                return new CacheValue<T>(value, true);
            }

            CacheStats.OnMiss();

            if (_options.EnableLogging)
                _logger?.LogInformation($"Cache Missed : cachekey = {cacheKey}");

            if (!_cache.Set($"{cacheKey}_Lock", 1, TimeSpan.FromMilliseconds(_options.LockMs).Seconds, RedisExistence.Nx))
            {
                System.Threading.Thread.Sleep(_options.SleepMs);
                return Get(cacheKey, dataRetriever, expiration);
            }

            var item = dataRetriever();
            if (item != null)
            {
                Set(cacheKey, item, expiration);
                //remove mutex key
                _cache.Del($"{cacheKey}_Lock");
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
        public CacheValue<T> Get<T>(string cacheKey)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            var result = _cache.Get<byte[]>(cacheKey);
            if (result != null)
            {
                CacheStats.OnHit();

                if (_options.EnableLogging)
                    _logger?.LogInformation($"Cache Hit : cachekey = {cacheKey}");

                var value = _serializer.Deserialize<T>(result);
                return new CacheValue<T>(value, true);
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
        /// Gets all.
        /// </summary>
        /// <returns>The all.</returns>
        /// <param name="cacheKeys">Cache keys.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public IDictionary<string, CacheValue<T>> GetAll<T>(IEnumerable<string> cacheKeys)
        {
            ArgumentCheck.NotNullAndCountGTZero(cacheKeys, nameof(cacheKeys));

            var result = new Dictionary<string, CacheValue<T>>();

            //maybe we should use mget here based on redis mode
            //multiple keys may trigger `don't hash to the same slot`

            foreach (var item in cacheKeys)
            {
                var cachedValue = _cache.Get<byte[]>(item);
                if (cachedValue != null)
                    result.Add(item, new CacheValue<T>(_serializer.Deserialize<T>(cachedValue), true));
                else
                    result.Add(item, CacheValue<T>.NoValue);
            }

            return result;
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

            var result = new Dictionary<string, CacheValue<T>>();

            //maybe we should use mget here based on redis mode
            //multiple keys may trigger `don't hash to the same slot`

            foreach (var item in cacheKeys)
            {
                var cachedValue = await _cache.GetAsync<byte[]>(item);
                if (cachedValue != null)
                    result.Add(item, new CacheValue<T>(_serializer.Deserialize<T>(cachedValue), true));
                else
                    result.Add(item, CacheValue<T>.NoValue);
            }

            return result;
        }

        /// <summary>
        /// Gets the async.
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

            var result = await _cache.GetAsync<byte[]>(cacheKey);
            if (result != null)
            {
                CacheStats.OnHit();

                if (_options.EnableLogging)
                    _logger?.LogInformation($"Cache Hit : cachekey = {cacheKey}");

                var value = _serializer.Deserialize<T>(result);
                return new CacheValue<T>(value, true);
            }

            CacheStats.OnMiss();

            if (_options.EnableLogging)
                _logger?.LogInformation($"Cache Missed : cachekey = {cacheKey}");

            var flag = await _cache.SetAsync($"{cacheKey}_Lock", 1, TimeSpan.FromMilliseconds(_options.LockMs).Seconds, RedisExistence.Nx);

            if (!flag)
            {
                await Task.Delay(_options.SleepMs);
                return await GetAsync(cacheKey, dataRetriever, expiration);
            }

            var item = await dataRetriever();
            if (item != null)
            {
                await SetAsync(cacheKey, item, expiration);
                //remove mutex key
                await _cache.DelAsync($"{cacheKey}_Lock");
                return new CacheValue<T>(item, true);
            }
            else
            {
                return CacheValue<T>.NoValue;
            }
        }

        /// <summary>
        /// Gets the async.
        /// </summary>
        /// <returns>The async.</returns>
        /// <param name="cacheKey">Cache key.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public async Task<CacheValue<T>> GetAsync<T>(string cacheKey)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            var result = await _cache.GetAsync<byte[]>(cacheKey);
            if (result != null)
            {
                CacheStats.OnHit();

                if (_options.EnableLogging)
                    _logger?.LogInformation($"Cache Hit : cachekey = {cacheKey}");

                var value = _serializer.Deserialize<T>(result);
                return new CacheValue<T>(value, true);
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
        /// Handles the prefix of CacheKey.
        /// </summary>
        /// <param name="prefix">Prefix of CacheKey.</param>
        /// <exception cref="ArgumentException"></exception>
        private string HandlePrefix(string prefix)
        {
            // Forbid
            if (prefix.Equals("*"))
                throw new ArgumentException("the prefix should not equal to *");

            // Don't start with *
            prefix = new System.Text.RegularExpressions.Regex("^\\*+").Replace(prefix, "");

            // End with *
            if (!prefix.EndsWith("*", StringComparison.OrdinalIgnoreCase))
                prefix = string.Concat(prefix, "*");

            return prefix;
        }

        /// <summary>
        /// Searchs the redis keys.
        /// </summary>
        /// <returns>The redis keys.</returns>
        /// <param name="pattern">Pattern.</param>
        private string[] SearchRedisKeys(string pattern)
        {
            var keys = new List<string>();

            long nextCursor = 0;
            do
            {
                var scanResult = _cache.Scan( nextCursor,  pattern, 500);
                nextCursor = scanResult.Cursor;
                var items = scanResult.Items;
                keys.AddRange(items);
            }
            while (nextCursor != 0);

            return keys.Distinct().ToArray();                      
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

            prefix = this.HandlePrefix(prefix);

            var redisKeys = this.SearchRedisKeys(prefix);

            var result = new Dictionary<string, CacheValue<T>>();

            foreach (var item in redisKeys)
            {
                var cachedValue = _cache.Get<byte[]>(item);
                if (cachedValue!=null)
                    result.Add(item, new CacheValue<T>(_serializer.Deserialize<T>(cachedValue), true));
                else
                    result.Add(item, CacheValue<T>.NoValue);
            }

            return result;
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

            prefix = this.HandlePrefix(prefix);

            var redisKeys = this.SearchRedisKeys(prefix);

            var result = new Dictionary<string, CacheValue<T>>();

            foreach (var item in redisKeys)
            {
                var cachedValue = await _cache.GetAsync<byte[]>(item);
                if (cachedValue != null)
                    result.Add(item, new CacheValue<T>(_serializer.Deserialize<T>(cachedValue), true));
                else
                    result.Add(item, CacheValue<T>.NoValue);
            }

            return result;
        }

       /// <summary>
       /// Gets the count.
       /// </summary>
       /// <returns>The count.</returns>
       /// <param name="prefix">Prefix.</param>
        public int GetCount(string prefix = "")
        {
            if (string.IsNullOrWhiteSpace(prefix))
            {
                var allCount = 0L;

                var servers = _cache.NodesServerManager.DbSize();

                foreach (var item in servers)
                {
                    allCount += item.value;
                }

                return (int)allCount;                              
            }

            return this.SearchRedisKeys(this.HandlePrefix(prefix)).Length;
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
        /// Refreshs the async.
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
        /// Remove the specified cacheKey.
        /// </summary>
        /// <param name="cacheKey">Cache key.</param>
        public void Remove(string cacheKey)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            _cache.Del(cacheKey);
        }

        /// <summary>
        /// Removes all.
        /// </summary>
        /// <param name="cacheKeys">Cache keys.</param>
        public void RemoveAll(IEnumerable<string> cacheKeys)
        {
            ArgumentCheck.NotNullAndCountGTZero(cacheKeys, nameof(cacheKeys));

            foreach (var item in cacheKeys)
            {
                _cache.Del(item);
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

            var tasks = new List<Task<long>>();

            foreach (var item in cacheKeys)
            {
                tasks.Add(_cache.DelAsync(item));
            }

            await Task.WhenAll(tasks);
        }

        /// <summary>
        /// Removes the async.
        /// </summary>
        /// <returns>The async.</returns>
        /// <param name="cacheKey">Cache key.</param>
        public async Task RemoveAsync(string cacheKey)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            await _cache.DelAsync(cacheKey);
        }

        public void RemoveByPrefix(string prefix)
        {
            ArgumentCheck.NotNullOrWhiteSpace(prefix, nameof(prefix));

            prefix = this.HandlePrefix(prefix);

            if (_options.EnableLogging)
                _logger?.LogInformation($"RemoveByPrefix : prefix = {prefix}");

            var redisKeys = this.SearchRedisKeys(prefix);

            foreach (var item in redisKeys)
            {
                _cache.Del(item);
            }
        }

        /// <summary>
        /// Removes the by prefix async.
        /// </summary>
        /// <returns>The by prefix async.</returns>
        /// <param name="prefix">Prefix.</param>
        public async Task RemoveByPrefixAsync(string prefix)
        {
            ArgumentCheck.NotNullOrWhiteSpace(prefix, nameof(prefix));

            prefix = this.HandlePrefix(prefix);

            if (_options.EnableLogging)
                _logger?.LogInformation($"RemoveByPrefixAsync : prefix = {prefix}");

            var redisKeys = this.SearchRedisKeys(prefix);

            var tasks = new List<Task<long>>();

            foreach (var item in redisKeys)
            {
               tasks.Add(  _cache.DelAsync(item));
            }

            await Task.WhenAll(tasks);
        }

        /// <summary>
        /// Set the specified cacheKey, cacheValue and expiration.
        /// </summary>
        /// <param name="cacheKey">Cache key.</param>
        /// <param name="cacheValue">Cache value.</param>
        /// <param name="expiration">Expiration.</param>
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

            _cache.Set(
                cacheKey,
                _serializer.Serialize(cacheValue),
                expiration.Seconds
                );
        }

        /// <summary>
        /// Sets all.
        /// </summary>
        /// <param name="value">Value.</param>
        /// <param name="expiration">Expiration.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public void SetAll<T>(IDictionary<string, T> value, TimeSpan expiration)
        {
            //whether to use pipe based on redis mode 
            if (MaxRdSecond > 0)
            {
                var addSec = new Random().Next(1, MaxRdSecond);
                expiration = expiration.Add(TimeSpan.FromSeconds(addSec));
            }

            foreach (var item in value)
            {
                _cache.Set(item.Key, _serializer.Serialize(item.Value), expiration.Seconds);
            }
        }

        /// <summary>
        /// Sets all async.
        /// </summary>
        /// <returns>The all async.</returns>
        /// <param name="value">Value.</param>
        /// <param name="expiration">Expiration.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public async Task SetAllAsync<T>(IDictionary<string, T> value, TimeSpan expiration)
        {
            //whether to use pipe based on redis mode 
            var tasks = new List<Task<bool>>();

            if (MaxRdSecond > 0)
            {
                var addSec = new Random().Next(1, MaxRdSecond);
                expiration = expiration.Add(TimeSpan.FromSeconds(addSec));
            }

            foreach (var item in value)
            {
                tasks.Add(_cache.SetAsync(item.Key, _serializer.Serialize(item.Value), expiration.Seconds));
            }

            await Task.WhenAll(tasks);
        }

        /// <summary>
        /// Sets the async.
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

            var val = _serializer.Serialize(cacheValue);

            await _cache.SetAsync(
                cacheKey,
                val,
                expiration.Seconds
                );
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

            if (MaxRdSecond > 0)
            {
                var addSec = new Random().Next(1, MaxRdSecond);
                expiration = expiration.Add(TimeSpan.FromSeconds(addSec));
            }

            return _cache.Set(
                cacheKey,
                _serializer.Serialize(cacheValue),
                expiration.Seconds,
                RedisExistence.Nx
                );
        }

        /// <summary>
        /// Tries the set async.
        /// </summary>
        /// <returns>The set async.</returns>
        /// <param name="cacheKey">Cache key.</param>
        /// <param name="cacheValue">Cache value.</param>
        /// <param name="expiration">Expiration.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public async Task<bool> TrySetAsync<T>(string cacheKey, T cacheValue, TimeSpan expiration)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));
            ArgumentCheck.NotNull(cacheValue, nameof(cacheValue));
            ArgumentCheck.NotNegativeOrZero(expiration, nameof(expiration));

            if (MaxRdSecond > 0)
            {
                var addSec = new Random().Next(1, MaxRdSecond);
                expiration = expiration.Add(TimeSpan.FromSeconds(addSec));
            }

            return await _cache.SetAsync(
                cacheKey,
                _serializer.Serialize(cacheValue),
                expiration.Seconds,
                RedisExistence.Nx
                );
        }
    }
}
