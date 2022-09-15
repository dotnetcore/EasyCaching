namespace EasyCaching.LiteDB
{
    using EasyCaching.Core;
    using Microsoft.Extensions.Logging;
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// LiteDBCaching provider.
    /// </summary>
    public partial class DefaultLiteDBCachingProvider : EasyCachingAbstractProvider
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

            return await Task.Run(() => BaseExists(cacheKey), cancellationToken);
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
            return await Task.Run(() =>
                        {
                                return BaseGet(cacheKey, () =>
                                {
                                    return dataRetriever.Invoke().GetAwaiter().GetResult();
                                }, expiration);
                        }, cancellationToken);
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
            return await Task.Run(() => BaseGet<T>(cacheKey), cancellationToken);
        }

        /// <summary>
        /// Gets the count.
        /// </summary>
        /// <returns>The count.</returns>
        /// <param name="prefix">Prefix.</param>
        /// <param name="cancellationToken">CancellationToken</param>
        public override async Task<int> BaseGetCountAsync(string prefix = "", CancellationToken cancellationToken = default)
        {
            return await Task.Run(() => BaseGetCount(prefix), cancellationToken);
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
            return await Task.Run(() => System.Convert.ChangeType(BaseGet<object>(cacheKey).Value, type), cancellationToken);
        }

        /// <summary>
        /// Removes the specified cacheKey async.
        /// </summary>
        /// <returns>The async.</returns>
        /// <param name="cacheKey">Cache key.</param>
        /// <param name="cancellationToken">CancellationToken</param>
        public override async Task BaseRemoveAsync(string cacheKey, CancellationToken cancellationToken = default)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            await Task.Run(() => BaseRemove(cacheKey), cancellationToken);
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
            await Task.Run(() => BaseSet<T>(cacheKey, cacheValue, expiration), cancellationToken);
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

            await Task.Run(() => BaseRemoveByPrefix(prefix), cancellationToken);
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

            await Task.Run(() => BaseRemoveByPattern(pattern), cancellationToken);
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
            await Task.Run(() => BaseSetAll(values, expiration), cancellationToken);
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
            return await Task.Run(() => BaseGetAll<T>(cacheKeys), cancellationToken);
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

            return await Task.Run(() => BaseGetByPrefix<T>(prefix), cancellationToken);
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
            await Task.Run(() => BaseRemoveAll(cacheKeys), cancellationToken);
        }

        /// <summary>
        /// Flush All Cached Item async.
        /// </summary>
        /// <returns>The async.</returns>
        /// <param name="cancellationToken">CancellationToken</param>
        public override async Task BaseFlushAsync(CancellationToken cancellationToken = default) => await Task.Run(BaseFlush, cancellationToken);

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
            return await Task.Run(() => BaseTrySet(cacheKey, cacheValue, expiration), cancellationToken);
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
            return await Task.Run(() => BaseGetExpiration(cacheKey), cancellationToken);
        }
    }
}
