namespace EasyCaching.SQLite
{
    using Dapper;
    using EasyCaching.Core;
    using EasyCaching.Core.Internal;
    using Microsoft.Data.Sqlite;
    using System;
    using System.Linq;
    using System.Threading.Tasks;

    /// <summary>
    /// SQLiteCaching provider.
    /// </summary>
    public class SQLiteCachingProvider : IEasyCachingProvider
    {
        /// <summary>
        /// The cache.
        /// </summary>
        private ISQLiteDatabaseProvider _dbProvider;

        /// <summary>
        /// The cache.
        /// </summary>
        private SqliteConnection _cache;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:EasyCaching.SQLite.SQLiteCachingProvider"/> class.
        /// </summary>
        /// <param name="dbProvider">dbProvider.</param>
        public SQLiteCachingProvider(ISQLiteDatabaseProvider dbProvider)
        {
            this._dbProvider = dbProvider;
            this._cache = _dbProvider.GetConnection();
        }

        /// <summary>
        /// <see cref="T:EasyCaching.SQLite.SQLiteCachingProvider"/> is not distributed cache.
        /// </summary>
        /// <value><c>true</c> if is distributed cache; otherwise, <c>false</c>.</value>
        public bool IsDistributedCache => false;

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
                cachekey = cacheKey
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
                cachekey = cacheKey
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
        public CacheValue<T> Get<T>(string cacheKey, Func<T> dataRetriever, TimeSpan expiration) where T : class
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));
            ArgumentCheck.NotNegativeOrZero(expiration, nameof(expiration));

            var dbResult = _cache.Query<string>(ConstSQL.GETSQL, new
            {
                cachekey = cacheKey
            }).FirstOrDefault();

            if(!string.IsNullOrWhiteSpace(dbResult))
            {
                return new CacheValue<T>(Newtonsoft.Json.JsonConvert.DeserializeObject<T>(dbResult),true);
            }

            var item = dataRetriever?.Invoke();

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
        public async Task<CacheValue<T>> GetAsync<T>(string cacheKey, Func<Task<T>> dataRetriever, TimeSpan expiration) where T : class
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));
            ArgumentCheck.NotNegativeOrZero(expiration, nameof(expiration));

            var list = await _cache.QueryAsync<string>(ConstSQL.GETSQL, new
            {
                cachekey = cacheKey
            });

            var dbResult = list.FirstOrDefault();

            if (!string.IsNullOrWhiteSpace(dbResult))
            {
                return new CacheValue<T>(Newtonsoft.Json.JsonConvert.DeserializeObject<T>(dbResult), true);
            }

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
        public CacheValue<T> Get<T>(string cacheKey) where T : class
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            var dbResult = _cache.Query<string>(ConstSQL.GETSQL, new
            {
                cachekey = cacheKey
            }).FirstOrDefault();

            if (!string.IsNullOrWhiteSpace(dbResult))
            {
                return new CacheValue<T>(Newtonsoft.Json.JsonConvert.DeserializeObject<T>(dbResult), true);
            }
            else
            {
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
           
            var list = await _cache.QueryAsync<string>(ConstSQL.GETSQL, new
            {
                cachekey = cacheKey
            });

            var dbResult = list.FirstOrDefault();

            if (!string.IsNullOrWhiteSpace(dbResult))
            {
                return new CacheValue<T>(Newtonsoft.Json.JsonConvert.DeserializeObject<T>(dbResult), true);
            }
            else
            {
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

            _cache.Execute(ConstSQL.REMOVESQL,new { cachekey = cacheKey });
        }

        /// <summary>
        /// Removes the specified cacheKey async.
        /// </summary>
        /// <returns>The async.</returns>
        /// <param name="cacheKey">Cache key.</param>
        public async Task RemoveAsync(string cacheKey)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            await _cache.ExecuteAsync(ConstSQL.REMOVESQL, new { cachekey = cacheKey });
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

            _cache.Execute(ConstSQL.SETSQL,new
            {
                cachekey = cacheKey,
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
        public async Task SetAsync<T>(string cacheKey, T cacheValue, TimeSpan expiration) where T : class
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));
            ArgumentCheck.NotNull(cacheValue, nameof(cacheValue));
            ArgumentCheck.NotNegativeOrZero(expiration, nameof(expiration));

            await _cache.ExecuteAsync(ConstSQL.SETSQL, new
            {
                cachekey = cacheKey,
                cachevalue = Newtonsoft.Json.JsonConvert.SerializeObject(cacheValue),
                expiration = expiration.Ticks / 10000000
            });
        }
    }
}
