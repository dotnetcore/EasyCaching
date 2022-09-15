namespace EasyCaching.SQLite
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
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
        /// <param name="cancellationToken">CancellationToken</param>
        public override async Task<bool> BaseExistsAsync(string cacheKey, CancellationToken cancellationToken = default)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            var dbResult = await _cache.ExecuteScalarAsync<int>(new CommandDefinition(ConstSQL.EXISTSSQL, new
            {
                cachekey = cacheKey,
                name = _name
            }, cancellationToken: cancellationToken));

            return dbResult == 1;
        }

        /// <summary>
        /// Gets the specified cacheKey, dataRetriever and expiration async.
        /// </summary>
        /// <returns>The async.</returns>
        /// <param name="cacheKey">Cache key.</param>
        /// <param name="dataRetriever">Data retriever.</param>
        /// <param name="expiration">Expiration.</param>
        /// <param name="cancellationToken">CancellationToken</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public override async Task<CacheValue<T>> BaseGetAsync<T>(string cacheKey, Func<Task<T>> dataRetriever, TimeSpan expiration, CancellationToken cancellationToken = default)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));
            ArgumentCheck.NotNegativeOrZero(expiration, nameof(expiration));

            var list = await _cache.QueryAsync<string>(new CommandDefinition(ConstSQL.GETSQL, new
            {
                cachekey = cacheKey,
                name = _name
            }, cancellationToken: cancellationToken));

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

            if (item != null || _options.CacheNulls)
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
        /// Gets the specified cacheKey async.
        /// </summary>
        /// <returns>The async.</returns>
        /// <param name="cacheKey">Cache key.</param>
        /// <param name="cancellationToken">CancellationToken</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public override async Task<CacheValue<T>> BaseGetAsync<T>(string cacheKey, CancellationToken cancellationToken = default)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            var list = await _cache.QueryAsync<string>(new CommandDefinition(ConstSQL.GETSQL, new
            {
                cachekey = cacheKey,
                name = _name
            }, cancellationToken: cancellationToken));

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
        /// Gets the count.
        /// </summary>
        /// <returns>The count.</returns>
        /// <param name="prefix">Prefix.</param>
        /// <param name="cancellationToken">CancellationToken</param>
        public override async Task<int> BaseGetCountAsync(string prefix = "", CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(prefix))
            {
                
                return await _cache.ExecuteScalarAsync<int>(new CommandDefinition(ConstSQL.COUNTALLSQL, new { name = _name }, cancellationToken: cancellationToken));
            }
            else
            {
                return await _cache.ExecuteScalarAsync<int>(new CommandDefinition(ConstSQL.COUNTPREFIXSQL, new { cachekey = string.Concat(prefix, "%"), name = _name }, cancellationToken: cancellationToken));
            }
        }

        /// <summary>
        /// Gets the specified cacheKey async.
        /// </summary>
        /// <returns>The async.</returns>
        /// <param name="cacheKey">Cache key.</param>
        /// <param name="type">Object Type.</param>
        /// <param name="cancellationToken">CancellationToken</param>
        public override async Task<object> BaseGetAsync(string cacheKey, Type type, CancellationToken cancellationToken = default)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            

            var list = await _cache.QueryAsync<string>(new CommandDefinition(ConstSQL.GETSQL, new
            {
                cachekey = cacheKey,
                name = _name
            }, cancellationToken: cancellationToken));

            var dbResult = list.FirstOrDefault();

            if (!string.IsNullOrWhiteSpace(dbResult))
            {
                CacheStats.OnHit();

                if (_options.EnableLogging)
                    _logger?.LogInformation($"Cache Hit : cachekey = {cacheKey}");

                return Newtonsoft.Json.JsonConvert.DeserializeObject(dbResult, type);
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
        /// Removes the specified cacheKey async.
        /// </summary>
        /// <returns>The async.</returns>
        /// <param name="cacheKey">Cache key.</param>
        /// <param name="cancellationToken">CancellationToken</param>
        public override async Task BaseRemoveAsync(string cacheKey, CancellationToken cancellationToken)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            await _cache.ExecuteAsync(new CommandDefinition(ConstSQL.REMOVESQL, new { cachekey = cacheKey, name = _name }, cancellationToken: cancellationToken));
        }

        /// <summary>
        /// Sets the specified cacheKey, cacheValue and expiration async.
        /// </summary>
        /// <returns>The async.</returns>
        /// <param name="cacheKey">Cache key.</param>
        /// <param name="cacheValue">Cache value.</param>
        /// <param name="expiration">Expiration.</param>
        /// <param name="cancellationToken">CancellationToken</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public override async Task BaseSetAsync<T>(string cacheKey, T cacheValue, TimeSpan expiration, CancellationToken cancellationToken = default)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));
            ArgumentCheck.NotNull(cacheValue, nameof(cacheValue), _options.CacheNulls);
            ArgumentCheck.NotNegativeOrZero(expiration, nameof(expiration));

            if (MaxRdSecond > 0)
            {
                var addSec = new Random().Next(1, MaxRdSecond);
                expiration.Add(new TimeSpan(0, 0, addSec));
            }

            await _cache.ExecuteAsync(new CommandDefinition(ConstSQL.SETSQL, new
            {
                cachekey = cacheKey,
                name = _name,
                cachevalue = Newtonsoft.Json.JsonConvert.SerializeObject(cacheValue),
                expiration = expiration.Ticks / 10000000
            }, cancellationToken: cancellationToken));
        }

        /// <summary>
        /// Removes cached item by cachekey's prefix async.
        /// </summary>
        /// <param name="prefix">Prefix of CacheKey.</param>
        /// <param name="cancellationToken">CancellationToken</param>
        public override async Task BaseRemoveByPrefixAsync(string prefix, CancellationToken cancellationToken = default)
        {
            ArgumentCheck.NotNullOrWhiteSpace(prefix, nameof(prefix));

            if (_options.EnableLogging)
                _logger?.LogInformation($"RemoveByPrefixAsync : prefix = {prefix}");

            await _cache.ExecuteAsync(new CommandDefinition(ConstSQL.REMOVEBYLIKESQL, new
            {
                cachekey = string.Concat(prefix, "%"),
                name = _name
            }, cancellationToken: cancellationToken));
        }

        /// <summary>
        /// Removes cached item by pattern async.
        /// </summary>
        /// <param name="pattern">Pattern of CacheKey.</param>
        /// <param name="cancellationToken">CancellationToken</param>
        public override async Task BaseRemoveByPatternAsync(string pattern, CancellationToken cancellationToken = default)
        {
            ArgumentCheck.NotNullOrWhiteSpace(pattern, nameof(pattern));

            if (_options.EnableLogging)
                _logger?.LogInformation($"RemoveByPatternAsync : pattern = {pattern}");

            await _cache.ExecuteAsync(new CommandDefinition(ConstSQL.REMOVEBYLIKESQL, new
            {
                cachekey = pattern.Replace('*', '%'),
                name = _name
            }, cancellationToken: cancellationToken));
        }

        /// <summary>
        /// Sets all async.
        /// </summary>
        /// <returns>The all async.</returns>
        /// <param name="values">Values.</param>
        /// <param name="expiration">Expiration.</param>
        /// <param name="cancellationToken">CancellationToken</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public override async Task BaseSetAllAsync<T>(IDictionary<string, T> values, TimeSpan expiration, CancellationToken cancellationToken = default)
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
        /// <param name="cancellationToken">CancellationToken</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public override async Task<IDictionary<string, CacheValue<T>>> BaseGetAllAsync<T>(IEnumerable<string> cacheKeys, CancellationToken cancellationToken = default)
        {
            ArgumentCheck.NotNullAndCountGTZero(cacheKeys, nameof(cacheKeys));

            var list = (await _cache.QueryAsync(new CommandDefinition(ConstSQL.GETALLSQL, new
            {
                cachekey = cacheKeys.ToArray(),
                name = _name
            }, cancellationToken: cancellationToken))).ToList();

            return GetDict<T>(list);
        }

        /// <summary>
        /// Gets the by prefix async.
        /// </summary>
        /// <returns>The by prefix async.</returns>
        /// <param name="prefix">Prefix.</param>
        /// <param name="cancellationToken">CancellationToken</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public override async Task<IDictionary<string, CacheValue<T>>> BaseGetByPrefixAsync<T>(string prefix, CancellationToken cancellationToken = default)
        {
            ArgumentCheck.NotNullOrWhiteSpace(prefix, nameof(prefix));

            var list = (await _cache.QueryAsync(new CommandDefinition(ConstSQL.GETBYPREFIXSQL, new
            {
                cachekey = string.Concat(prefix, "%"),
                name = _name
            }, cancellationToken: cancellationToken))).ToList();

            return GetDict<T>(list);
        }

        /// <summary>
        /// Removes all async.
        /// </summary>
        /// <returns>The all async.</returns>
        /// <param name="cacheKeys">Cache keys.</param>
        /// <param name="cancellationToken">CancellationToken</param>
        public override async Task BaseRemoveAllAsync(IEnumerable<string> cacheKeys, CancellationToken cancellationToken = default)
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
        /// <param name="cancellationToken">CancellationToken</param>
        /// <returns>The async.</returns>
        public override async Task BaseFlushAsync(CancellationToken cancellationToken = default) => await _cache.ExecuteAsync(new CommandDefinition(ConstSQL.FLUSHSQL, new { name = _name }, cancellationToken: cancellationToken));

        /// <summary>
        /// Tries the set async.
        /// </summary>
        /// <returns>The set async.</returns>
        /// <param name="cacheKey">Cache key.</param>
        /// <param name="cacheValue">Cache value.</param>
        /// <param name="expiration">Expiration.</param>
        /// <param name="cancellationToken">CancellationToken</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public override async Task<bool> BaseTrySetAsync<T>(string cacheKey, T cacheValue, TimeSpan expiration, CancellationToken cancellationToken = default)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));
            ArgumentCheck.NotNull(cacheValue, nameof(cacheValue), _options.CacheNulls);
            ArgumentCheck.NotNegativeOrZero(expiration, nameof(expiration));

            if (MaxRdSecond > 0)
            {
                var addSec = new Random().Next(1, MaxRdSecond);
                expiration.Add(new TimeSpan(0, 0, addSec));
            }

            var rows = await _cache.ExecuteAsync(new CommandDefinition(ConstSQL.TRYSETSQL, new
            {
                cachekey = cacheKey,
                name = _name,
                cachevalue = Newtonsoft.Json.JsonConvert.SerializeObject(cacheValue),
                expiration = expiration.Ticks / 10000000
            }, cancellationToken: cancellationToken));

            return rows > 0;
        }

        /// <summary>
        /// Get the expiration of cache key async
        /// </summary>
        /// <param name="cacheKey">cache key</param>
        /// <param name="cancellationToken">CancellationToken</param>
        /// <returns>expiration</returns>
        public override async Task<TimeSpan> BaseGetExpirationAsync(string cacheKey, CancellationToken cancellationToken = default)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            var time = await _cache.ExecuteScalarAsync<long>(new CommandDefinition(ConstSQL.GETEXPIRATIONSQL, new
            {
                cachekey = cacheKey,
                name = _name
            }, cancellationToken: cancellationToken));

            if (time <= 0) return TimeSpan.Zero;
            else return TimeSpan.FromSeconds(time);
        }     
    }
}
