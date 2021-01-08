namespace EasyCaching.SQLite
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Dapper;
    using EasyCaching.Core;
    using Microsoft.Data.Sqlite;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// SQLiteCaching provider.
    /// </summary>
    public partial class DefaultSQLiteCachingProvider : EasyCachingAbstractProvider
    {
        /// <summary>
        /// The cache.
        /// </summary>
        private ISQLiteDatabaseProvider _dbProvider;

        /// <summary>
        /// The options.
        /// </summary>
        private readonly SQLiteOptions _options;

        /// <summary>
        /// The logger.
        /// </summary>
        private readonly ILogger _logger;

        /// <summary>
        /// The cache.
        /// </summary>
        private SqliteConnection _cache => _dbProvider.GetConnection();

        /// <summary>
        /// The cache stats.
        /// </summary>
        private readonly CacheStats _cacheStats;

        /// <summary>
        /// The name.
        /// </summary>
        private readonly string _name;

        private readonly ProviderInfo _info;

        public DefaultSQLiteCachingProvider(
            string name,
            IEnumerable<ISQLiteDatabaseProvider> dbProviders,
            SQLiteOptions options,
           ILoggerFactory loggerFactory = null)
        {
            this._dbProvider = dbProviders.Single(x => x.DBProviderName.Equals(name));
            this._options = options;
            this._logger = loggerFactory?.CreateLogger<DefaultSQLiteCachingProvider>();
            this._cacheStats = new CacheStats();
            this._name = name;

            this.ProviderName = this._name;
            this.ProviderType = CachingProviderType.SQLite;
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

            InitDb(_dbProvider);
        }

        /// <summary>
        /// init database
        /// </summary>
        /// <param name="dbProvider"></param>
        private void InitDb(ISQLiteDatabaseProvider dbProvider)
        {            
            var conn = dbProvider.GetConnection();

            if (conn.State == System.Data.ConnectionState.Closed)
            {
                conn.Open();
            }

            conn.Execute(ConstSQL.CREATESQL);            
        }

        /// <summary>
        /// Exists the specified cacheKey.
        /// </summary>
        /// <returns>The exists.</returns>
        /// <param name="cacheKey">Cache key.</param>
        public override bool BaseExists(string cacheKey)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            var dbResult = _cache.ExecuteScalar<int>(ConstSQL.EXISTSSQL, new
            {
                cachekey = cacheKey,
                name = _name
            });

            return dbResult == 1;
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

            if (!result.HasValue)
            {
                var item = dataRetriever();

                if (item != null || _options.CacheNulls)
                {
                    Set(cacheKey, item, expiration);
                    result = new CacheValue<T>(item, true);
                }
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

            var dbResult = _cache.Query<string>(ConstSQL.GETSQL, new
            {
                cachekey = cacheKey,
                name = _name
            }).FirstOrDefault();

            if (!string.IsNullOrWhiteSpace(dbResult) || _options.CacheNulls)
            {
                CacheStats.OnHit();

                if (_options.EnableLogging)
                    _logger?.LogInformation($"Cache Hit : cachekey = {cacheKey}");

                return string.IsNullOrWhiteSpace(dbResult) 
                    ? CacheValue<T>.Null
                    : new CacheValue<T>(Newtonsoft.Json.JsonConvert.DeserializeObject<T>(dbResult), true);
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

            _cache.Execute(ConstSQL.REMOVESQL, new { cachekey = cacheKey, name = _name });
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

            _cache.Execute(ConstSQL.SETSQL, new
            {
                cachekey = cacheKey,
                name = _name,
                cachevalue = Newtonsoft.Json.JsonConvert.SerializeObject(cacheValue),
                expiration = expiration.Ticks / 10000000
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

            _cache.Execute(ConstSQL.REMOVEBYPREFIXSQL, new { cachekey = string.Concat(prefix, "%"), name = _name });
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

            var tran = _cache.BeginTransaction();

            foreach (var item in values)
            {
                _cache.Execute(ConstSQL.SETSQL, new
                {
                    cachekey = item.Key,
                    name = _name,
                    cachevalue = Newtonsoft.Json.JsonConvert.SerializeObject(item.Value),
                    expiration = expiration.Ticks / 10000000
                }, tran);
            }

            tran.Commit();
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

            var list = _cache.Query(ConstSQL.GETALLSQL, new
            {
                cachekey = cacheKeys.ToArray(),
                name = _name
            }).ToList();

            return GetDict<T>(list);
        }

        /// <summary>
        /// Gets the dict.
        /// </summary>
        /// <returns>The dict.</returns>
        /// <param name="list">List.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        private IDictionary<string, CacheValue<T>> GetDict<T>(List<dynamic> list)
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

            var list = _cache.Query(ConstSQL.GETBYPREFIXSQL, new
            {
                cachekey = string.Concat(prefix, "%"),
                name = _name
            }).ToList();

            return GetDict<T>(list);
        }
    
        /// <summary>
        /// Removes all.
        /// </summary>
        /// <param name="cacheKeys">Cache keys.</param>
        public override void BaseRemoveAll(IEnumerable<string> cacheKeys)
        {
            ArgumentCheck.NotNullAndCountGTZero(cacheKeys, nameof(cacheKeys));

            var tran = _cache.BeginTransaction();

            foreach (var item in cacheKeys)
                _cache.Execute(ConstSQL.REMOVESQL, new { cachekey = item, name = _name }, tran);

            tran.Commit();
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
                return _cache.ExecuteScalar<int>(ConstSQL.COUNTALLSQL, new { name = _name });
            }
            else
            {
                return _cache.ExecuteScalar<int>(ConstSQL.COUNTPREFIXSQL, new { cachekey = string.Concat(prefix, "%"), name = _name });
            }
        }

        /// <summary>
        /// Flush All Cached Item.
        /// </summary>
        public override void BaseFlush() => _cache.Execute(ConstSQL.FLUSHSQL, new { name = _name });

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

            var rows = _cache.Execute(ConstSQL.TRYSETSQL, new
            {
                cachekey = cacheKey,
                name = _name,
                cachevalue = Newtonsoft.Json.JsonConvert.SerializeObject(cacheValue),
                expiration = expiration.Ticks / 10000000
            });

            return rows > 0;
        }

        /// <summary>
        /// Get the expiration of cache key
        /// </summary>
        /// <param name="cacheKey">cache key</param>
        /// <returns>expiration</returns>
        public override TimeSpan BaseGetExpiration(string cacheKey)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            var time = _cache.ExecuteScalar<long>(ConstSQL.GETEXPIRATIONSQL, new
            {
                cachekey = cacheKey,
                name = _name
            });

            if (time <= 0) return TimeSpan.Zero;
            else return TimeSpan.FromSeconds(time);
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
