namespace EasyCaching.LiteDB
{
    using EasyCaching.Core;
    using global::LiteDB;
    using Microsoft.Extensions.Logging;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// LiteDBCaching provider.
    /// </summary>
    public partial class DefaultLiteDBCachingProvider : EasyCachingAbstractProvider
    {
        /// <summary>
        /// The cache.
        /// </summary>
        private ILiteDBDatabaseProvider _dbProvider;

        /// <summary>
        /// The options.
        /// </summary>
        private readonly LiteDBOptions _options;

        /// <summary>
        /// The logger.
        /// </summary>
        private readonly ILogger _logger;

        private readonly LiteDatabase _litedb;

        /// <summary>
        /// The cache.
        /// </summary>
        private readonly ILiteCollection<CacheItem> _cache;

        /// <summary>
        /// The cache stats.
        /// </summary>
        private readonly CacheStats _cacheStats;

        /// <summary>
        /// The name.
        /// </summary>
        private readonly string _name;

        private readonly ProviderInfo _info;

        public DefaultLiteDBCachingProvider(
            string name,
            IEnumerable<ILiteDBDatabaseProvider> dbProviders,
            LiteDBOptions options,
           ILoggerFactory loggerFactory = null)
        {
            this._dbProvider = dbProviders.Single(x => x.DBProviderName.Equals(name));
            this._options = options;
            this._logger = loggerFactory?.CreateLogger<DefaultLiteDBCachingProvider>();
            this._litedb = _dbProvider.GetConnection();
            this._cache = _litedb.GetCollection<CacheItem>(name);
            this._cacheStats = new CacheStats();
            this._name = name;

            this.ProviderName = this._name;
            this.ProviderType = CachingProviderType.LiteDB;
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
                CacheNulls = options.CacheNulls
            };

            InitDb();
        }

        /// <summary>
        /// init database
        /// </summary>
        private void InitDb()
        {
            lock (_litedb)
            {
                _litedb.Checkpoint();
                _litedb.Rebuild();
                lock (_cache)
                {
                    _cache.EnsureIndex(c => c.cachekey);
                }
            }

        }

        /// <summary>
        /// Exists the specified cacheKey.
        /// </summary>
        /// <returns>The exists.</returns>
        /// <param name="cacheKey">Cache key.</param>
        public override bool BaseExists(string cacheKey)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            var dbResult = _cache.Count(fc => fc.cachekey == cacheKey && fc.expiration > DateTimeOffset.Now.ToUnixTimeSeconds());

            return dbResult > 0;
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
            
            var item = dataRetriever();

            if (item != null || _options.CacheNulls)
            {
                Set(cacheKey, item, expiration);
                result = new CacheValue<T>(item, true);
            }

            return result;
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

            var cacheItem = _cache.FindOne(c => c.cachekey == cacheKey && c.expiration > DateTimeOffset.Now.ToUnixTimeSeconds());

            if (cacheItem != null || _options.CacheNulls)
            {
                if (_options.EnableLogging)
                    _logger?.LogInformation($"Cache Hit : cachekey = {cacheKey}");

                CacheStats.OnHit();

                return string.IsNullOrWhiteSpace(cacheItem?.cachevalue) 
                    ? CacheValue<T>.Null 
                    : new CacheValue<T>(Newtonsoft.Json.JsonConvert.DeserializeObject<T>(cacheItem.cachevalue), true);
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
        /// Remove the specified cacheKey.
        /// </summary>
        /// <returns>The remove.</returns>
        /// <param name="cacheKey">Cache key.</param>
        public override void BaseRemove(string cacheKey)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));
            _cache.DeleteMany(c => c.cachekey == cacheKey);
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
                expiration.Add(new TimeSpan(0, 0, addSec));
            }
            _cache.Upsert(new CacheItem
            {
                cachekey = cacheKey,
                name = _name,
                cachevalue = Newtonsoft.Json.JsonConvert.SerializeObject(cacheValue),
                expiration = DateTimeOffset.UtcNow.Add(expiration).ToUnixTimeSeconds()
            });
        }

        /// <summary>
        /// Removes cached item by cachekey's prefix.
        /// </summary>
        /// <param name="prefix">Prefix of CacheKey.</param>
        public override void BaseRemoveByPrefix(string prefix)
        {
            ArgumentCheck.NotNullOrWhiteSpace(prefix, nameof(prefix));

            if (_options.EnableLogging)
                _logger?.LogInformation($"RemoveByPrefix : prefix = {prefix}");

            _cache.DeleteMany(c => c.cachekey.StartsWith(prefix));
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
            _litedb.BeginTrans();
            try
            {
                foreach (var item in values)
                {
                    _cache.Upsert(new CacheItem
                    {
                        cachekey = item.Key,
                        name = _name,
                        cachevalue = Newtonsoft.Json.JsonConvert.SerializeObject(item.Value),
                        expiration = DateTimeOffset.UtcNow.Add(expiration).ToUnixTimeSeconds()
                    });
                }
                _litedb.Commit();
            }
            catch (Exception)
            {
                _litedb.Rollback();
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
            var lst = cacheKeys.ToList();
            var list = _cache.Find(c => lst.Contains(c.cachekey) && c.expiration > DateTimeOffset.Now.ToUnixTimeSeconds()).ToList();
            return GetDict<T>(list);
        }

        /// <summary>
        /// Gets the dict.
        /// </summary>
        /// <returns>The dict.</returns>
        /// <param name="list">List.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        private IDictionary<string, CacheValue<T>> GetDict<T>(List<CacheItem> list)
        {
            var result = new Dictionary<string, CacheValue<T>>();
            foreach (var item in list)
            {
                if (!string.IsNullOrWhiteSpace(item.cachekey))
                    result.Add(item.cachekey, new CacheValue<T>(Newtonsoft.Json.JsonConvert.DeserializeObject<T>(item.cachevalue), true));
                else
                    result.Add(item.cachekey, CacheValue<T>.NoValue);
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
            ArgumentCheck.NotNullOrWhiteSpace(prefix, nameof(prefix));
            var list = _cache.Find(c => c.cachekey.StartsWith(prefix) && c.expiration > DateTimeOffset.Now.ToUnixTimeSeconds()).ToList();
            return GetDict<T>(list);
        }

        /// <summary>
        /// Removes all.
        /// </summary>
        /// <param name="cacheKeys">Cache keys.</param>
        public override void BaseRemoveAll(IEnumerable<string> cacheKeys)
        {
            ArgumentCheck.NotNullAndCountGTZero(cacheKeys, nameof(cacheKeys));
            var lst = cacheKeys.ToList();
            // _litedb.BeginTrans();
            _cache.DeleteMany(c => lst.Contains(c.cachekey));
            // _litedb.Commit();
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
                return _cache.Count(c =>  c.expiration > DateTimeOffset.Now.ToUnixTimeSeconds());
            }
            else
            {
                return _cache.Count(c => c.cachekey.StartsWith(prefix) && c.expiration > DateTimeOffset.Now.ToUnixTimeSeconds());
            }
        }

        /// <summary>
        /// Flush All Cached Item.
        /// </summary>
        public override void BaseFlush() => _cache.DeleteAll();

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
                expiration.Add(new TimeSpan(0, 0, addSec));
            }
         
            var r = _cache.FindOne(c => c.cachekey == cacheKey &&   c.expiration > DateTimeOffset.Now.ToUnixTimeSeconds());
            bool result = false;
            if (r == null)
            {
                var rows = _cache.Insert(new CacheItem
                {
                    cachekey = cacheKey,
                    name = _name,
                    cachevalue = Newtonsoft.Json.JsonConvert.SerializeObject(cacheValue),
                    expiration = DateTimeOffset.UtcNow.Add(expiration).ToUnixTimeSeconds()
                });
                result = rows != null;
            }
            return result;
        }

        /// <summary>
        /// Get the expiration of cache key
        /// </summary>
        /// <param name="cacheKey">cache key</param>
        /// <returns>expiration</returns>
        public override TimeSpan BaseGetExpiration(string cacheKey)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            var time = _cache.FindOne(c => c.cachekey == cacheKey )?.expiration;
            return time == null ? TimeSpan.Zero : DateTimeOffset.FromUnixTimeSeconds((long)time).Subtract(DateTimeOffset.UtcNow);
        }

        /// <summary>
        /// Get te information of this provider.
        /// </summary>
        /// <returns></returns>
        public override ProviderInfo BaseGetProviderInfo()
        {
            return _info;
        }
    }
}