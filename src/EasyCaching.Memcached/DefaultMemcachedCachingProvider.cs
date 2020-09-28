namespace EasyCaching.Memcached
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Cryptography;
    using System.Text;
    using EasyCaching.Core;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Default memcached caching provider.
    /// </summary>
    public partial class DefaultMemcachedCachingProvider : EasyCachingAbstractProvider
    {
        public const string NullValue = "{NULL}";
        
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

        private readonly ProviderInfo _info;
       
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
            this.ProviderType = CachingProviderType.Memcached;
            this.ProviderStats = this._cacheStats;
            this.ProviderMaxRdSecond = _options.MaxRdSecond;
            this.IsDistributedProvider = true;

            _info = new ProviderInfo
            {
                CacheStats = _cacheStats,
                EnableLogging = options.EnableLogging,
                IsDistributedProvider = IsDistributedProvider,
                LockMs = options.LockMs,
                MaxRdSecond = options.MaxRdSecond,
                ProviderName = ProviderName,
                ProviderType = ProviderType,
                SerializerName = options.SerializerName,
                SleepMs = options.SleepMs,
                CacheNulls = options.CacheNulls,
            };
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

            var result = BaseGet<T>(cacheKey);

            if (result.HasValue)
            {
                return result;
            }
            
            var flag = _memcachedClient.Store(Enyim.Caching.Memcached.StoreMode.Add, this.HandleCacheKey($"{cacheKey}_Lock"), 1, TimeSpan.FromMilliseconds(_options.LockMs));

            if (!flag)
            {
                System.Threading.Thread.Sleep(_options.SleepMs);
                return Get(cacheKey, dataRetriever, expiration);
            }

            var item = dataRetriever();
            if (item != null || _options.CacheNulls)
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
        /// Get the specified cacheKey.
        /// </summary>
        /// <returns>The get.</returns>
        /// <param name="cacheKey">Cache key.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public override CacheValue<T> BaseGet<T>(string cacheKey)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            var result = ConvertFromStoredValue<T>(_memcachedClient.Get(this.HandleCacheKey(cacheKey)));

            if (result.HasValue)
            {
                OnCacheHit(cacheKey);
            }
            else
            {
                OnCacheMiss(cacheKey);
            }

            return result;
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
            ArgumentCheck.NotNull(cacheValue, nameof(cacheValue), _options.CacheNulls);
            ArgumentCheck.NotNegativeOrZero(expiration, nameof(expiration));

            if (MaxRdSecond > 0)
            {
                var addSec = new Random().Next(1, MaxRdSecond);
                expiration = expiration.Add(TimeSpan.FromSeconds(addSec));
            }
            
            _memcachedClient.Store(
                Enyim.Caching.Memcached.StoreMode.Set, 
                this.HandleCacheKey(cacheKey), 
                this.ConvertToStoredValue(cacheValue), 
                expiration);
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
            _memcachedClient.Store(
                Enyim.Caching.Memcached.StoreMode.Set, 
                this.HandleCacheKey(prefix), 
                newValue, 
                new TimeSpan(0, 0, 0));
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
        
        private object ConvertToStoredValue(object cacheValue) => cacheValue ?? NullValue;

        private CacheValue<T> ConvertFromStoredValue<T>(object cacheValue)
        {
            switch (cacheValue)
            {
                case NullValue: return CacheValue<T>.Null;
                case T typedResult: return new CacheValue<T>(typedResult, true);
                default: return CacheValue<T>.NoValue;
            }
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
        /// Gets all.
        /// </summary>
        /// <returns>The all.</returns>
        /// <param name="cacheKeys">Cache keys.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public override IDictionary<string, CacheValue<T>> BaseGetAll<T>(IEnumerable<string> cacheKeys)
        {
            ArgumentCheck.NotNullAndCountGTZero(cacheKeys, nameof(cacheKeys));

            return _memcachedClient
                .Get<object>(cacheKeys)
                .ToDictionary(
                    pair => pair.Key,
                    pair => ConvertFromStoredValue<T>(pair.Value));
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
            ArgumentCheck.NotNull(cacheValue, nameof(cacheValue), _options.CacheNulls);
            ArgumentCheck.NotNegativeOrZero(expiration, nameof(expiration));

            if (MaxRdSecond > 0)
            {
                var addSec = new Random().Next(1, MaxRdSecond);
                expiration = expiration.Add(TimeSpan.FromSeconds(addSec));
            }

            return _memcachedClient.Store(
                Enyim.Caching.Memcached.StoreMode.Add, 
                this.HandleCacheKey(cacheKey), 
                ConvertToStoredValue(cacheValue), 
                expiration);
        }

        public override TimeSpan BaseGetExpiration(string cacheKey)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Get te information of this provider.
        /// </summary>
        /// <returns></returns>   
        public override ProviderInfo BaseGetProviderInfo()
        {
            return _info;
        }

        private void OnCacheHit(string cacheKey)
        {
            CacheStats.OnHit();

            if (_options.EnableLogging)
                _logger?.LogInformation($"Cache Hit : cachekey = {cacheKey}");
        }
        
        private void OnCacheMiss(string cacheKey)
        {
            CacheStats.OnMiss();

            if (_options.EnableLogging)
                _logger?.LogInformation($"Cache Missed : cachekey = {cacheKey}");
        }
    }
}