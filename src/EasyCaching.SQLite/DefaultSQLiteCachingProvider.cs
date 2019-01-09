namespace EasyCaching.SQLite
{
    using Dapper;
    using EasyCaching.Core;
    using EasyCaching.Core.Internal;
    using Microsoft.Data.Sqlite;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    /// <summary>
    /// SQLiteCaching provider.
    /// </summary>
    public class DefaultSQLiteCachingProvider : IEasyCachingProvider
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
        private readonly SqliteConnection _cache;

        /// <summary>
        /// The cache stats.
        /// </summary>
        private readonly CacheStats _cacheStats;

        /// <summary>
        /// The name.
        /// </summary>
        private readonly string _name;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:EasyCaching.SQLite.SQLiteCachingProvider"/> class.
        /// </summary>
        /// <param name="dbProvider">dbProvider.</param>
        public DefaultSQLiteCachingProvider(
            ISQLiteDatabaseProvider dbProvider,
            IOptionsMonitor<SQLiteOptions> options,
            ILoggerFactory loggerFactory = null)
        {
            this._dbProvider = dbProvider;
            this._options = options.CurrentValue;
            this._logger = loggerFactory?.CreateLogger<DefaultSQLiteCachingProvider>();
            this._cache = _dbProvider.GetConnection();
            this._cacheStats = new CacheStats();
            this._name = EasyCachingConstValue.DefaultRedisName;
        }

        public DefaultSQLiteCachingProvider(
            string name,
            IEnumerable< ISQLiteDatabaseProvider> dbProviders,
           SQLiteOptions options,
           ILoggerFactory loggerFactory = null)
        {
            this._dbProvider = dbProviders.FirstOrDefault(x => x.DBProviderName.Equals(name));
            this._options = options;
            this._logger = loggerFactory?.CreateLogger<DefaultSQLiteCachingProvider>();
            this._cache = _dbProvider.GetConnection();
            this._cacheStats = new CacheStats();
            this._name = name;
        }

        /// <summary>
        /// <see cref="T:EasyCaching.SQLite.SQLiteCachingProvider"/> is not distributed cache.
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
        /// <value>The max random second.</value>
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
        public string Name => this._name;

        /// <summary>
        /// Exists the specified cacheKey.
        /// </summary>
        /// <returns>The exists.</returns>
        /// <param name="cacheKey">Cache key.</param>
        public bool Exists(string cacheKey)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            var dbResult = _cache.ExecuteScalar<int>(ConstSQL.EXISTSSQL, new
            {
                cachekey = cacheKey, name = _name
            });

            return dbResult == 1;
        }

        /// <summary>
        /// Existses the specified cacheKey async.
        /// </summary>
        /// <returns>The async.</returns>
        /// <param name="cacheKey">Cache key.</param>
        public async Task<bool> ExistsAsync(string cacheKey)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            var dbResult = await _cache.ExecuteScalarAsync<int>(ConstSQL.EXISTSSQL, new
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
        public CacheValue<T> Get<T>(string cacheKey, Func<T> dataRetriever, TimeSpan expiration)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));
            ArgumentCheck.NotNegativeOrZero(expiration, nameof(expiration));

            var dbResult = _cache.Query<string>(ConstSQL.GETSQL, new
            {
                cachekey = cacheKey,
                name = _name
            }).FirstOrDefault();

            if (!string.IsNullOrWhiteSpace(dbResult))
            {
                if (_options.EnableLogging)
                    _logger?.LogInformation($"Cache Hit : cachekey = {cacheKey}");

                CacheStats.OnHit();

                return new CacheValue<T>(Newtonsoft.Json.JsonConvert.DeserializeObject<T>(dbResult), true);
            }

            CacheStats.OnMiss();

            if (_options.EnableLogging)
                _logger?.LogInformation($"Cache Missed : cachekey = {cacheKey}");

            var item = dataRetriever();

            if (item != null)
            {
                Set(cacheKey, item, expiration);
                return new CacheValue<T>(item, true);
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

            var list = await _cache.QueryAsync<string>(ConstSQL.GETSQL, new
            {
                cachekey = cacheKey,
                name = _name
            });

            var dbResult = list.FirstOrDefault();

            if (!string.IsNullOrWhiteSpace(dbResult))
            {
                if (_options.EnableLogging)
                    _logger?.LogInformation($"Cache Hit : cachekey = {cacheKey}");

                CacheStats.OnHit();

                return new CacheValue<T>(Newtonsoft.Json.JsonConvert.DeserializeObject<T>(dbResult), true);
            }

            CacheStats.OnMiss();

            if (_options.EnableLogging)
                _logger?.LogInformation($"Cache Missed : cachekey = {cacheKey}");

            var item = await dataRetriever?.Invoke();

            if (item != null)
            {
                await SetAsync(cacheKey, item, expiration);
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

            var dbResult = _cache.Query<string>(ConstSQL.GETSQL, new
            {
                cachekey = cacheKey,
                name = _name
            }).FirstOrDefault();

            if (!string.IsNullOrWhiteSpace(dbResult))
            {
                CacheStats.OnHit();

                if (_options.EnableLogging)
                    _logger?.LogInformation($"Cache Hit : cachekey = {cacheKey}");

                return new CacheValue<T>(Newtonsoft.Json.JsonConvert.DeserializeObject<T>(dbResult), true);
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
        public async Task<CacheValue<T>> GetAsync<T>(string cacheKey)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            var list = await _cache.QueryAsync<string>(ConstSQL.GETSQL, new
            {
                cachekey = cacheKey,
                name = _name
            });

            var dbResult = list.FirstOrDefault();

            if (!string.IsNullOrWhiteSpace(dbResult))
            {
                CacheStats.OnHit();
                
                if (_options.EnableLogging)
                    _logger?.LogInformation($"Cache Hit : cachekey = {cacheKey}");

                return new CacheValue<T>(Newtonsoft.Json.JsonConvert.DeserializeObject<T>(dbResult), true);
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
        public void Remove(string cacheKey)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            _cache.Execute(ConstSQL.REMOVESQL, new { cachekey = cacheKey, name = _name });
        }

        /// <summary>
        /// Removes the specified cacheKey async.
        /// </summary>
        /// <returns>The async.</returns>
        /// <param name="cacheKey">Cache key.</param>
        public async Task RemoveAsync(string cacheKey)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            await _cache.ExecuteAsync(ConstSQL.REMOVESQL, new { cachekey = cacheKey, name = _name });
        }

        /// <summary>
        /// Set the specified cacheKey, cacheValue and expiration.
        /// </summary>
        /// <returns>The set.</returns>
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
                expiration.Add(new TimeSpan(0, 0, addSec));
            }

            await _cache.ExecuteAsync(ConstSQL.SETSQL, new
            {
                cachekey = cacheKey,
                name = _name,
                cachevalue = Newtonsoft.Json.JsonConvert.SerializeObject(cacheValue),
                expiration = expiration.Ticks / 10000000
            });
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
        /// <param name="prefix">Prefix of CacheKey.</param>
        public void RemoveByPrefix(string prefix)
        {
            ArgumentCheck.NotNullOrWhiteSpace(prefix, nameof(prefix));

            if (_options.EnableLogging)
                _logger?.LogInformation($"RemoveByPrefix : prefix = {prefix}");

            _cache.Execute(ConstSQL.REMOVEBYPREFIXSQL, new { cachekey = string.Concat(prefix, "%"), name = _name });
        }

        /// <summary>
        /// Removes cached item by cachekey's prefix async.
        /// </summary>
        /// <param name="prefix">Prefix of CacheKey.</param>
        public async Task RemoveByPrefixAsync(string prefix)
        {
            ArgumentCheck.NotNullOrWhiteSpace(prefix, nameof(prefix));

            if (_options.EnableLogging)
                _logger?.LogInformation($"RemoveByPrefixAsync : prefix = {prefix}");

            await _cache.ExecuteAsync(ConstSQL.REMOVEBYPREFIXSQL, new { cachekey = string.Concat(prefix, "%"), name = _name });
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

            var tran = _cache.BeginTransaction();
            var tasks = new List<Task<int>>();

            foreach (var item in values)
            {
                tasks.Add(_cache.ExecuteAsync(ConstSQL.SETSQL, new
                {
                    cachekey = item.Key,
                    name = _name,
                    cachevalue = Newtonsoft.Json.JsonConvert.SerializeObject(item.Value),
                    expiration = expiration.Ticks / 10000000
                }, tran));
            }
            await Task.WhenAll(tasks);

            tran.Commit();
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

            var list = _cache.Query(ConstSQL.GETALLSQL, new
            {
                cachekey = cacheKeys.ToArray(),
                name = _name
            }).ToList();

            return GetDict<T>(list);
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

            var list = (await _cache.QueryAsync(ConstSQL.GETALLSQL, new
            {
                cachekey = cacheKeys.ToArray(),
                name = _name
            })).ToList();

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
        public IDictionary<string, CacheValue<T>> GetByPrefix<T>(string prefix)
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
        /// Gets the by prefix async.
        /// </summary>
        /// <returns>The by prefix async.</returns>
        /// <param name="prefix">Prefix.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public async Task<IDictionary<string, CacheValue<T>>> GetByPrefixAsync<T>(string prefix)
        {
            ArgumentCheck.NotNullOrWhiteSpace(prefix, nameof(prefix));

            var list = (await _cache.QueryAsync(ConstSQL.GETBYPREFIXSQL, new
            {
                cachekey = string.Concat(prefix, "%"),
                name = _name
            })).ToList();

            return GetDict<T>(list);
        }

        /// <summary>
        /// Removes all.
        /// </summary>
        /// <param name="cacheKeys">Cache keys.</param>
        public void RemoveAll(IEnumerable<string> cacheKeys)
        {
            ArgumentCheck.NotNullAndCountGTZero(cacheKeys, nameof(cacheKeys));

            var tran = _cache.BeginTransaction();

            foreach (var item in cacheKeys)
                _cache.Execute(ConstSQL.REMOVESQL, new { cachekey = item, name = _name }, tran);

            tran.Commit();
        }

        /// <summary>
        /// Removes all async.
        /// </summary>
        /// <returns>The all async.</returns>
        /// <param name="cacheKeys">Cache keys.</param>
        public async Task RemoveAllAsync(IEnumerable<string> cacheKeys)
        {
            ArgumentCheck.NotNullAndCountGTZero(cacheKeys, nameof(cacheKeys));

            var tran = _cache.BeginTransaction();
            var tasks = new List<Task<int>>();

            foreach (var item in cacheKeys)
                tasks.Add(_cache.ExecuteAsync(ConstSQL.REMOVESQL, new { cachekey = item, name = _name }, tran));

            await Task.WhenAll(tasks);
            tran.Commit();
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
                return _cache.ExecuteScalar<int>(ConstSQL.COUNTALLSQL,new{ name = _name });
            }
            else
            {
                return _cache.ExecuteScalar<int>(ConstSQL.COUNTPREFIXSQL, new { cachekey = string.Concat(prefix, "%"), name = _name });
            }
        }

        /// <summary>
        /// Flush All Cached Item.
        /// </summary>
        public void Flush() => _cache.Execute(ConstSQL.FLUSHSQL,new{ name = _name });

        /// <summary>
        /// Flush All Cached Item async.
        /// </summary>
        /// <returns>The async.</returns>
        public async Task FlushAsync() => await _cache.ExecuteAsync(ConstSQL.FLUSHSQL,new { name = _name });

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
                expiration.Add(new TimeSpan(0, 0, addSec));
            }

            var rows = await _cache.ExecuteAsync(ConstSQL.TRYSETSQL, new
            {
                cachekey = cacheKey,
                name = _name,
                cachevalue = Newtonsoft.Json.JsonConvert.SerializeObject(cacheValue),
                expiration = expiration.Ticks / 10000000
            });

            return rows > 0;
        }
    }
}
