namespace EasyCaching.Redis
{
    using EasyCaching.Core;
    using EasyCaching.Core.Internal;
    using StackExchange.Redis;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    /// <summary>
    /// Default redis caching provider.
    /// </summary>
    public class DefaultRedisCachingProvider : IEasyCachingProvider
    {
        /// <summary>
        /// The cache.
        /// </summary>
        private readonly IDatabase _cache;

        /// <summary>
        /// The servers.
        /// </summary>
        private readonly IEnumerable<IServer> _servers;

        /// <summary>
        /// The db provider.
        /// </summary>
        private readonly IRedisDatabaseProvider _dbProvider;

        /// <summary>
        /// The serializer.
        /// </summary>
        private readonly IEasyCachingSerializer _serializer;

        /// <summary>
        /// <see cref="T:EasyCaching.Redis.DefaultRedisCachingProvider"/> 
        /// is distributed cache.
        /// </summary>
        public bool IsDistributedCache => false;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:EasyCaching.Redis.DefaultRedisCachingProvider"/> class.
        /// </summary>
        /// <param name="dbProvider">DB Provider.</param>
        /// <param name="serializer">Serializer.</param>
        public DefaultRedisCachingProvider(
            IRedisDatabaseProvider dbProvider,
            IEasyCachingSerializer serializer)
        {
            ArgumentCheck.NotNull(dbProvider, nameof(dbProvider));
            ArgumentCheck.NotNull(serializer, nameof(serializer));

            _dbProvider = dbProvider;
            _serializer = serializer;
            _cache = _dbProvider.GetDatabase();
            _servers = _dbProvider.GetServerList();
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

            var result = _cache.StringGet(cacheKey);
            if (!result.IsNull)
            {
                var value = _serializer.Deserialize<T>(result);
                return new CacheValue<T>(value, true);
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

            var result = await _cache.StringGetAsync(cacheKey);
            if (!result.IsNull)
            {
                var value = _serializer.Deserialize<T>(result);
                return new CacheValue<T>(value, true);
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

            var result = _cache.StringGet(cacheKey);
            if (!result.IsNull)
            {
                var value = _serializer.Deserialize<T>(result);
                return new CacheValue<T>(value, true);
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

            var result = await _cache.StringGetAsync(cacheKey);
            if (!result.IsNull)
            {
                var value = _serializer.Deserialize<T>(result);
                return new CacheValue<T>(value, true);
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

            _cache.KeyDelete(cacheKey);
        }

        /// <summary>
        /// Removes the specified cacheKey async.
        /// </summary>
        /// <returns>The async.</returns>
        /// <param name="cacheKey">Cache key.</param>
        public async Task RemoveAsync(string cacheKey)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            await _cache.KeyDeleteAsync(cacheKey);
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

            _cache.StringSet(
                cacheKey,
                _serializer.Serialize(cacheValue),
                expiration);
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

            await _cache.StringSetAsync(
                    cacheKey,
                    _serializer.Serialize(cacheValue),
                    expiration);
        }

        /// <summary>
        /// Exists the specified cacheKey.
        /// </summary>
        /// <returns>The exists.</returns>
        /// <param name="cacheKey">Cache key.</param>
        public bool Exists(string cacheKey)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            return _cache.KeyExists(cacheKey);
        }

        /// <summary>
        /// Existses the specified cacheKey async.
        /// </summary>
        /// <returns>The async.</returns>
        /// <param name="cacheKey">Cache key.</param>
        public async Task<bool> ExistsAsync(string cacheKey)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            return await _cache.KeyExistsAsync(cacheKey);
        }

        /// <summary>
        /// Refresh the specified cacheKey, cacheValue and expiration.
        /// </summary>
        /// <param name="cacheKey">Cache key.</param>
        /// <param name="cacheValue">Cache value.</param>
        /// <param name="expiration">Expiration.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public void Refresh<T>(string cacheKey, T cacheValue, TimeSpan expiration) where T : class
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
        public async Task RefreshAsync<T>(string cacheKey, T cacheValue, TimeSpan expiration) where T : class
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

            prefix = this.HandlePrefix(prefix);

            foreach (var server in _servers)
            {
                this.HandleKeyDelWithTran(server, prefix);
            }
        }

        /// <summary>
        /// Removes cached item by cachekey's prefix async.
        /// </summary>
        /// <param name="prefix">Prefix of CacheKey.</param>
        public async Task RemoveByPrefixAsync(string prefix)
        {
            ArgumentCheck.NotNullOrWhiteSpace(prefix, nameof(prefix));

            prefix = this.HandlePrefix(prefix);

            foreach (var server in _servers)
            {
                await this.HandleKeyDelWithTranAsync(server, prefix);
            }
        }

        /// <summary>
        /// Delete keys of Redis using transaction.
        /// </summary>
        /// <param name="server">Server.</param>
        /// <param name="prefix">Prefix.</param>
        private void HandleKeyDelWithTran(IServer server, string prefix)
        {
            int count = 1;
            do
            {
                var keys = server.Keys(pattern: prefix, pageSize: 100);
                count = keys.Count();
                if (count > 0)
                {
                    var tran = _cache.CreateTransaction();

                    tran.KeyDeleteAsync(keys.ToArray());

                    tran.Execute(CommandFlags.FireAndForget);
                }
            }
            while (count <= 0);
        }

        /// <summary>
        /// Delete keys of Redis using transaction async.
        /// </summary>
        /// <returns>The key del with tran async.</returns>
        /// <param name="server">Server.</param>
        /// <param name="prefix">Prefix.</param>
        private async Task HandleKeyDelWithTranAsync(IServer server, string prefix)
        {
            int count = 1;
            do
            {
                var keys = server.Keys(pattern: prefix, pageSize: 100);
                count = keys.Count();
                if (count > 0)
                {
                    var tran = _cache.CreateTransaction();

                    Task delTask = tran.KeyDeleteAsync(keys.ToArray());

                    Task execTask = tran.ExecuteAsync(CommandFlags.FireAndForget);

                    await Task.WhenAll(delTask, execTask);

                    //await tran.KeyDeleteAsync(keys.ToArray());

                    //await tran.ExecuteAsync(CommandFlags.FireAndForget);
                }
            }
            while (count <= 0);
        }

        /// <summary>
        /// Handles the prefix of CacheKey.
        /// </summary>
        /// <param name="prefix">Prefix of CacheKey.</param>
        /// <exception cref="ArgumentException">
        private string HandlePrefix(string prefix)
        {
            // Forbid
            if (prefix.Equals("*"))
                throw new ArgumentException("the prefix should not to *");

            // Don't start with *
            prefix = new System.Text.RegularExpressions.Regex("^\\*+").Replace(prefix, "");

            // End with *
            if (!prefix.EndsWith("*", StringComparison.OrdinalIgnoreCase))
                prefix = string.Concat(prefix, "*");

            return prefix;
        }
    }
}
