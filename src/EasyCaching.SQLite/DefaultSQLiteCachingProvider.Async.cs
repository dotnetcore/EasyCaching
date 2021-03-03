namespace EasyCaching.SQLite
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Dapper;
    using EasyCaching.Core;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// SQLiteCaching provider.
    /// </summary>
    public partial class DefaultSQLiteCachingProvider : EasyCachingAbstractProvider
    {
        /// <summary>
        /// Existses the specified cacheKey async.
        /// </summary>
        /// <returns>The async.</returns>
        /// <param name="cacheKey">Cache key.</param>
        public override async Task<bool> BaseExistsAsync(string cacheKey)
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

            var list = await _cache.QueryAsync<string>(ConstSQL.GETSQL, new
            {
                cachekey = cacheKey,
                name = _name
            });

            var dbResult = list.FirstOrDefault();
            var result = Deserialize<T>(cacheKey, dbResult);
            TrackCacheStats(cacheKey, result);

            if (result.HasValue)
                return result;

            var item = await dataRetriever?.Invoke();

            if (item != null || _options.CacheNulls)
            {
                await SetAsync(cacheKey, item, expiration);
                result = new CacheValue<T>(item, true);
            }
            else
            {
                result = CacheValue<T>.NoValue;
            }

            return result;
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

            var list = await _cache.QueryAsync<string>(ConstSQL.GETSQL, new
            {
                cachekey = cacheKey,
                name = _name
            });

            var dbResult = list.FirstOrDefault();
            var result = Deserialize<T>(cacheKey, dbResult);
            TrackCacheStats(cacheKey, result);

            return result;
        }

        /// <summary>
        /// Gets the count.
        /// </summary>
        /// <returns>The count.</returns>
        /// <param name="prefix">Prefix.</param>
        public override async Task<int> BaseGetCountAsync(string prefix = "")
        {
            if (string.IsNullOrWhiteSpace(prefix))
            {
                return await _cache.ExecuteScalarAsync<int>(ConstSQL.COUNTALLSQL, new { name = _name });
            }
            else
            {
                return await _cache.ExecuteScalarAsync<int>(ConstSQL.COUNTPREFIXSQL, new { cachekey = string.Concat(prefix, "%"), name = _name });
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

            var list = await _cache.QueryAsync<string>(ConstSQL.GETSQL, new
            {
                cachekey = cacheKey,
                name = _name
            });

            var dbResult = list.FirstOrDefault();

            if (!string.IsNullOrWhiteSpace(dbResult))
            {
                OnCacheHit(cacheKey);

                return Newtonsoft.Json.JsonConvert.DeserializeObject(dbResult, type);
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

            await _cache.ExecuteAsync(ConstSQL.REMOVESQL, new { cachekey = cacheKey, name = _name });
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
        /// Removes cached item by cachekey's prefix async.
        /// </summary>
        /// <param name="prefix">Prefix of CacheKey.</param>
        public override async Task BaseRemoveByPrefixAsync(string prefix)
        {
            ArgumentCheck.NotNullOrWhiteSpace(prefix, nameof(prefix));

            Logger?.LogInformation("RemoveByPrefixAsync : prefix = {0}", prefix);

            await _cache.ExecuteAsync(ConstSQL.REMOVEBYPREFIXSQL, new { cachekey = string.Concat(prefix, "%"), name = _name });
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
        /// Gets all async.
        /// </summary>
        /// <returns>The all async.</returns>
        /// <param name="cacheKeys">Cache keys.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public override async Task<IDictionary<string, CacheValue<T>>> BaseGetAllAsync<T>(IEnumerable<string> cacheKeys)
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
        /// Gets the by prefix async.
        /// </summary>
        /// <returns>The by prefix async.</returns>
        /// <param name="prefix">Prefix.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public override async Task<IDictionary<string, CacheValue<T>>> BaseGetByPrefixAsync<T>(string prefix)
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
        /// Removes all async.
        /// </summary>
        /// <returns>The all async.</returns>
        /// <param name="cacheKeys">Cache keys.</param>
        public override async Task BaseRemoveAllAsync(IEnumerable<string> cacheKeys)
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
        /// Flush All Cached Item async.
        /// </summary>
        /// <returns>The async.</returns>
        public override async Task BaseFlushAsync() => await _cache.ExecuteAsync(ConstSQL.FLUSHSQL, new { name = _name });

        /// <summary>
        /// Tries the set async.
        /// </summary>
        /// <returns>The set async.</returns>
        /// <param name="cacheKey">Cache key.</param>
        /// <param name="cacheValue">Cache value.</param>
        /// <param name="expiration">Expiration.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public override async Task<bool> BaseTrySetAsync<T>(string cacheKey, T cacheValue, TimeSpan expiration)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));
            ArgumentCheck.NotNull(cacheValue, nameof(cacheValue), _options.CacheNulls);
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

        /// <summary>
        /// Get the expiration of cache key async
        /// </summary>
        /// <param name="cacheKey">cache key</param>
        /// <returns>expiration</returns>
        public override async Task<TimeSpan> BaseGetExpirationAsync(string cacheKey)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            var time = await _cache.ExecuteScalarAsync<long>(ConstSQL.GETEXPIRATIONSQL, new
            {
                cachekey = cacheKey,
                name = _name
            });

            if (time <= 0) return TimeSpan.Zero;
            else return TimeSpan.FromSeconds(time);
        }     
    }
}
