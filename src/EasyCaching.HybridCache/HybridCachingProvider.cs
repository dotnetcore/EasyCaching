namespace EasyCaching.HybridCache
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using EasyCaching.Core;
    using EasyCaching.Core.Bus;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Hybrid caching provider.
    /// </summary>
    public class HybridCachingProvider : IHybridCachingProvider
    {
        /// <summary>
        /// The local cache.
        /// </summary>
        private readonly IEasyCachingProvider _localCache;
        /// <summary>
        /// The distributed cache.
        /// </summary>
        private readonly IEasyCachingProvider _distributedCache;
        /// <summary>
        /// The bus.
        /// </summary>
        private readonly IEasyCachingBus _bus;
        /// <summary>
        /// The options.
        /// </summary>
        private readonly HybridCachingOptions _options;
        /// <summary>
        /// The logger.
        /// </summary>
        private readonly ILogger _logger;
        /// <summary>
        /// The cache identifier.
        /// </summary>
        private readonly string _cacheId;
        /// <summary>
        /// The name.
        /// </summary>
        private readonly string _name;
        
        public string Name => _name;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:EasyCaching.HybridCache.HybridCachingProvider"/> class.
        /// </summary>
        /// <param name="name">Name.</param>
        /// <param name="optionsAccs">Options accs.</param>
        /// <param name="factory">Providers factory</param>
        /// <param name="bus">Bus.</param>
        /// <param name="loggerFactory">Logger factory.</param>
        public HybridCachingProvider(
            string name
            , HybridCachingOptions optionsAccs
            , IEasyCachingProviderFactory factory
            , IEasyCachingBus bus = null
            , ILoggerFactory loggerFactory = null
            )
        {
            ArgumentCheck.NotNull(factory, nameof(factory));

            this._name = name;
            this._options = optionsAccs;

            ArgumentCheck.NotNullOrWhiteSpace(_options.TopicName, nameof(_options.TopicName));

            if (optionsAccs.EnableLogging)
            {
                this._logger = loggerFactory.CreateLogger<HybridCachingProvider>();
            }

            // Here use the order to distinguish traditional provider
            this._localCache = factory.GetCachingProvider(_options.LocalCacheProviderName);

            // Here use the order to distinguish traditional provider
            this._distributedCache = factory.GetCachingProvider(_options.DistributedCacheProviderName);

            this._bus = bus ?? NullEasyCachingBus.Instance;
            this._bus.Subscribe(_options.TopicName, OnMessage);

            this._cacheId = Guid.NewGuid().ToString("N");
        }

        /// <summary>
        /// Ons the message.
        /// </summary>
        /// <param name="message">Message.</param>
        private void OnMessage(EasyCachingMessage message)
        {
            // each clients will recive the message, current client should ignore.
            if (!string.IsNullOrWhiteSpace(message.Id) && message.Id.Equals(_cacheId, StringComparison.OrdinalIgnoreCase))
                return;

            // remove by prefix
            if (message.IsPrefix)
            {
                var prefix = message.CacheKeys.First();

                _localCache.RemoveByPrefix(prefix);

                _logger?.LogDebug("remove local cache that prefix is {0}", prefix);

                return;
            }

            foreach (var item in message.CacheKeys)
            {
                _localCache.Remove(item);

                _logger?.LogDebug("remove local cache that cache key is {0}", item);
            }
        }

        /// <summary>
        /// Exists the specified cacheKey.
        /// </summary>
        /// <returns>The exists.</returns>
        /// <param name="cacheKey">Cache key.</param>
        public bool Exists(string cacheKey)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));
            bool flag;

            try
            {
                flag = _distributedCache.Exists(cacheKey);
                return flag;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Check cache key exists error [{0}] ", cacheKey);
            }

            flag = _localCache.Exists(cacheKey);

            return flag;
        }

        /// <summary>
        /// Existses the specified cacheKey async.
        /// </summary>
        /// <returns>The async.</returns>
        /// <param name="cacheKey">Cache key.</param>
        public async Task<bool> ExistsAsync(string cacheKey)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            bool flag;

            try
            {
                flag = await _distributedCache.ExistsAsync(cacheKey);
                return flag;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Check cache key [{0}] exists error", cacheKey);
            }

            flag = await _localCache.ExistsAsync(cacheKey);
            return flag;
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

            var cacheValue = _localCache.Get<T>(cacheKey);

            if (cacheValue.HasValue)
            {
                return cacheValue;
            }

            _logger?.LogDebug("local cache can not get the value of {0}", cacheKey);

            try
            {
                cacheValue = _distributedCache.Get<T>(cacheKey);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "distributed cache get error, [{0}]", cacheKey);
            }

            if (cacheValue.HasValue)
            {
                TimeSpan ts = GetExpiration(cacheKey);
               
                _localCache.Set(cacheKey, cacheValue.Value, ts);

                return cacheValue;
            }

            _logger?.LogDebug("distributed cache can not get the value of {0}", cacheKey);

            return CacheValue<T>.NoValue;
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

            var cacheValue = await _localCache.GetAsync<T>(cacheKey);

            if (cacheValue.HasValue)
            {
                return cacheValue;
            }

            _logger?.LogDebug("local cache can not get the value of {0}", cacheKey);

            try
            {
                cacheValue = await _distributedCache.GetAsync<T>(cacheKey);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "distributed cache get error, [{0}]", cacheKey);
            }

            if (cacheValue.HasValue)
            {
                TimeSpan ts = await GetExpirationAsync(cacheKey);

                await _localCache.SetAsync(cacheKey, cacheValue.Value, ts);

                return cacheValue;
            }

            _logger?.LogDebug("distributed cache can not get the value of {0}", cacheKey);

            return CacheValue<T>.NoValue;
        }

        /// <summary>
        /// Remove the specified cacheKey.
        /// </summary>
        /// <param name="cacheKey">Cache key.</param>
        public void Remove(string cacheKey)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            try
            {
                // distributed cache at first
                _distributedCache.Remove(cacheKey);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "remove cache key [{0}] error", cacheKey);
            }

            _localCache.Remove(cacheKey);

            // send message to bus 
            _bus.Publish(_options.TopicName, new EasyCachingMessage { Id = _cacheId, CacheKeys = new string[] { cacheKey } });
        }

        /// <summary>
        /// Removes the specified cacheKey async.
        /// </summary>
        /// <returns>The async.</returns>
        /// <param name="cacheKey">Cache key.</param>
        public async Task RemoveAsync(string cacheKey)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            try
            {
                // distributed cache at first
                await _distributedCache.RemoveAsync(cacheKey);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "remove cache key [{0}] error", cacheKey);
            }

            await _localCache.RemoveAsync(cacheKey);

            // send message to bus 
            await _bus.PublishAsync(_options.TopicName, new EasyCachingMessage { Id = _cacheId, CacheKeys = new string[] { cacheKey } });
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

            _localCache.Set(cacheKey, cacheValue, expiration);

            try
            {
                _distributedCache.Set(cacheKey, cacheValue, expiration);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "set cache key [{0}] error", cacheKey);
            }

            // When create/update cache, send message to bus so that other clients can remove it.
            _bus.Publish(_options.TopicName, new EasyCachingMessage { Id = _cacheId, CacheKeys = new string[] { cacheKey } });
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

            await _localCache.SetAsync(cacheKey, cacheValue, expiration);

            try
            {
                await _distributedCache.SetAsync(cacheKey, cacheValue, expiration);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "set cache key [{0}] error", cacheKey);
            }

            // When create/update cache, send message to bus so that other clients can remove it.
            await _bus.PublishAsync(_options.TopicName, new EasyCachingMessage { Id = _cacheId, CacheKeys = new string[] { cacheKey } });
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

            bool distributedError = false;
            bool flag = false;

            try
            {
                flag = _distributedCache.TrySet(cacheKey, cacheValue, expiration);
            }
            catch (Exception ex)
            {
                distributedError = true;
                _logger?.LogError(ex, "tryset cache key [{0}] error", cacheKey);
            }

            if (flag && !distributedError)
            {
                // When we TrySet succeed in distributed cache, we should Set this cache to local cache.
                // It's mainly to prevent the cache value was changed
                _localCache.Set(cacheKey, cacheValue, expiration);
            }

            // distributed cache occur error, have a try with local cache
            if (distributedError)
            {
                flag = _localCache.TrySet(cacheKey, cacheValue, expiration);
            }

            if (flag)
            {
                // Here should send message to bus due to cache was set successfully.
                _bus.Publish(_options.TopicName, new EasyCachingMessage { Id = _cacheId, CacheKeys = new string[] { cacheKey } });
            }

            return flag;
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

            bool distributedError = false;
            bool flag = false;

            try
            {
                flag = await _distributedCache.TrySetAsync(cacheKey, cacheValue, expiration);
            }
            catch (Exception ex)
            {
                distributedError = true;
                _logger?.LogError(ex, "tryset cache key [{0}] error", cacheKey);
            }

            if (flag && !distributedError)
            {
                // When we TrySet succeed in distributed cache, we should Set this cache to local cache.
                // It's mainly to prevent the cache value was changed
                await _localCache.SetAsync(cacheKey, cacheValue, expiration);
            }

            // distributed cache occur error, have a try with local cache
            if (distributedError)
            {
                flag = await _localCache.TrySetAsync(cacheKey, cacheValue, expiration);
            }

            if (flag)
            {
                // Here should send message to bus due to cache was set successfully.
                await _bus.PublishAsync(_options.TopicName, new EasyCachingMessage { Id = _cacheId, CacheKeys = new string[] { cacheKey } });
            }

            return flag;
        }

        /// <summary>
        /// Sets all.
        /// </summary>
        /// <param name="value">Value.</param>
        /// <param name="expiration">Expiration.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public void SetAll<T>(IDictionary<string, T> value, TimeSpan expiration)
        {
            _localCache.SetAll(value, expiration);

            try
            {
                _distributedCache.SetAll(value, expiration);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "set all from distributed provider error [{0}]", string.Join(",", value.Keys));
            }

            // send message to bus 
            _bus.Publish(_options.TopicName, new EasyCachingMessage { Id = _cacheId, CacheKeys = value.Keys.ToArray(), IsPrefix = false });
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
            await _localCache.SetAllAsync(value, expiration);

            try
            {
                await _distributedCache.SetAllAsync(value, expiration);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "set all from distributed provider error [{0}]", string.Join(",", value.Keys));
            }

            // send message to bus 
            await _bus.PublishAsync(_options.TopicName, new EasyCachingMessage { Id = _cacheId, CacheKeys = value.Keys.ToArray(), IsPrefix = false });
        }

        /// <summary>
        /// Removes all.
        /// </summary>
        /// <param name="cacheKeys">Cache keys.</param>
        public void RemoveAll(IEnumerable<string> cacheKeys)
        {
            ArgumentCheck.NotNullAndCountGTZero(cacheKeys, nameof(cacheKeys));

            try
            {
                _distributedCache.RemoveAllAsync(cacheKeys);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "remove all from distributed provider error [{0}]", string.Join(",", cacheKeys));
            }

            _localCache.RemoveAll(cacheKeys);

            // send message to bus in order to notify other clients.
            _bus.Publish(_options.TopicName, new EasyCachingMessage { Id = _cacheId, CacheKeys = cacheKeys.ToArray() });
        }

        /// <summary>
        /// Removes all async.
        /// </summary>
        /// <returns>The all async.</returns>
        /// <param name="cacheKeys">Cache keys.</param>
        public async Task RemoveAllAsync(IEnumerable<string> cacheKeys)
        {
            ArgumentCheck.NotNullAndCountGTZero(cacheKeys, nameof(cacheKeys));

            try
            {
                await _distributedCache.RemoveAllAsync(cacheKeys);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "remove all async from distributed provider error [{0}]", string.Join(",", cacheKeys));
            }

            await _localCache.RemoveAllAsync(cacheKeys);

            // send message to bus in order to notify other clients.
            await _bus.PublishAsync(_options.TopicName, new EasyCachingMessage { Id = _cacheId, CacheKeys = cacheKeys.ToArray() });
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

            var gotValueFromDistributedCache = false;
            
            var result = _localCache.Get<T>(
                cacheKey, 
                () =>
                {
                    var dataRetrieverFailed = false;
                    try
                    {
                        gotValueFromDistributedCache = true;
                        var resultFromDistributedCache = _distributedCache.Get<T>(
                            cacheKey, 
                            () =>
                            {
                                gotValueFromDistributedCache = false;
                                try
                                {
                                    return dataRetriever();
                                }
                                catch (Exception)
                                {
                                    dataRetrieverFailed = true;
                                    throw;
                                }
                            }, 
                            expiration);

                        return resultFromDistributedCache.Value;
                    }
                    catch (Exception ex) when(!dataRetrieverFailed)
                    {
                        gotValueFromDistributedCache = false;
                        _logger?.LogError(ex, "get with data retriever from distributed provider error [{0}]", cacheKey);
                        return dataRetriever();
                    }
                },
                expiration);

            if (gotValueFromDistributedCache && result.HasValue)
            {
                var ts = GetExpiration(cacheKey);

                _localCache.Set(cacheKey, result.Value, ts);
            }

            return result;
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

            var gotValueFromDistributedCache = false;
            
            var result = await _localCache.GetAsync<T>(
                cacheKey, 
                async () =>
                {
                    var dataRetrieverFailed = false;
                    try
                    {
                        gotValueFromDistributedCache = true;
                        var resultFromDistributedCache = await _distributedCache.GetAsync<T>(
                            cacheKey, 
                            async () =>
                            {
                                gotValueFromDistributedCache = false;
                                try
                                {
                                    return await dataRetriever();
                                }
                                catch (Exception)
                                {
                                    dataRetrieverFailed = true;
                                    throw;
                                }
                            }, 
                            expiration);

                        return resultFromDistributedCache.Value;
                    }
                    catch (Exception ex) when(!dataRetrieverFailed)
                    {
                        gotValueFromDistributedCache = false;
                        _logger?.LogError(ex, "get with data retriever from distributed provider error [{0}]", cacheKey);
                        return await dataRetriever();
                    }
                },
                expiration);

            if (gotValueFromDistributedCache && result.HasValue)
            {
                var ts = await GetExpirationAsync(cacheKey);

                _localCache.Set(cacheKey, result.Value, ts);
            }

            return result;
        }

        /// <summary>
        /// Removes the by prefix.
        /// </summary>
        /// <param name="prefix">Prefix.</param>
        public void RemoveByPrefix(string prefix)
        {
            ArgumentCheck.NotNullOrWhiteSpace(prefix, nameof(prefix));

            try
            {
                _distributedCache.RemoveByPrefix(prefix);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "remove by prefix [{0}] error", prefix);
            }

            _localCache.RemoveByPrefix(prefix);

            // send message to bus 
            _bus.Publish(_options.TopicName, new EasyCachingMessage { Id = _cacheId, CacheKeys = new string[] { prefix }, IsPrefix = true });
        }

        /// <summary>
        /// Removes the by prefix async.
        /// </summary>
        /// <returns>The by prefix async.</returns>
        /// <param name="prefix">Prefix.</param>
        public async Task RemoveByPrefixAsync(string prefix)
        {
            ArgumentCheck.NotNullOrWhiteSpace(prefix, nameof(prefix));

            try
            {
                await _distributedCache.RemoveByPrefixAsync(prefix);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "remove by prefix [{0}] error", prefix);
            }

            await _localCache.RemoveByPrefixAsync(prefix);

            // send message to bus in order to notify other clients.
            await _bus.PublishAsync(_options.TopicName, new EasyCachingMessage { Id = _cacheId, CacheKeys = new string[] { prefix }, IsPrefix = true });
        }

        private async Task<TimeSpan> GetExpirationAsync(string cacheKey)
        {
            TimeSpan ts = TimeSpan.Zero;

            try
            {
                ts = await _distributedCache.GetExpirationAsync(cacheKey);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error getting expiration for cache key = '{0}'.", cacheKey);
            }

            if (ts <= TimeSpan.Zero)
            {
                ts = TimeSpan.FromSeconds(_options.DefaultExpirationForTtlFailed);
            }

            return ts;
        }

        private TimeSpan GetExpiration(string cacheKey)
        {
            TimeSpan ts = TimeSpan.Zero;

            try
            {
                ts = _distributedCache.GetExpiration(cacheKey);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error getting expiration for cache key = '{0}'.", cacheKey);
            }

            if (ts <= TimeSpan.Zero)
            {
                ts = TimeSpan.FromSeconds(_options.DefaultExpirationForTtlFailed);
            }

            return ts;
        }

        public async Task<object> GetAsync(string cacheKey, Type type)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            var cacheValue = await _localCache.GetAsync(cacheKey, type);

            if (cacheValue != null)
            {
                return cacheValue;
            }

            _logger?.LogDebug("local cache can not get the value of {0}", cacheKey);

            try
            {
                cacheValue = await _distributedCache.GetAsync(cacheKey, type);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "distributed cache get error, [{0}]", cacheKey);
            }

            if (cacheValue != null)
            {
                TimeSpan ts = await GetExpirationAsync(cacheKey);
              
                await _localCache.SetAsync(cacheKey, cacheValue, ts);

                return cacheValue;
            }

            _logger?.LogDebug("distributed cache can not get the value of {0}", cacheKey);

            return null;
        }
    }
}
