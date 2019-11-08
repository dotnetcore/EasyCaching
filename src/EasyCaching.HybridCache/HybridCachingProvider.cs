namespace EasyCaching.HybridCache
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using EasyCaching.Core;
    using EasyCaching.Core.Bus;
    using Microsoft.Extensions.Logging;
    using Polly;
    using Polly.Fallback;
    using Polly.Retry;
    using Polly.Wrap;

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

        private readonly AsyncPolicyWrap _busAsyncWrap;
        private readonly PolicyWrap _busSyncWrap;
        private readonly RetryPolicy retryPolicy;
        private readonly AsyncRetryPolicy retryAsyncPolicy;
        private readonly FallbackPolicy fallbackPolicy;
        private readonly AsyncFallbackPolicy fallbackAsyncPolicy;

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

            this._logger = loggerFactory?.CreateLogger<HybridCachingProvider>();

            // Here use the order to distinguish traditional provider
            var local = factory.GetCachingProvider(_options.LocalCacheProviderName);
            if (local.IsDistributedCache) throw new NotFoundCachingProviderException("Can not found any local caching providers.");
            else this._localCache = local;

            // Here use the order to distinguish traditional provider
            var distributed = factory.GetCachingProvider(_options.DistributedCacheProviderName);

            if (!distributed.IsDistributedCache) throw new NotFoundCachingProviderException("Can not found any distributed caching providers.");
            else this._distributedCache = distributed;

            this._bus = bus ?? NullEasyCachingBus.Instance;
            this._bus.Subscribe(_options.TopicName, OnMessage);

            this._cacheId = Guid.NewGuid().ToString("N");


            // policy

            retryAsyncPolicy = Policy.Handle<Exception>()
                    .WaitAndRetryAsync(this._options.BusRetryCount, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt - 1)));

            retryPolicy = Policy.Handle<Exception>()
                   .WaitAndRetry(this._options.BusRetryCount, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt - 1)));

            fallbackPolicy = Policy.Handle<Exception>().Fallback(() => { });

            fallbackAsyncPolicy = Policy.Handle<Exception>().FallbackAsync(ct =>
            {
                return Task.CompletedTask;
            });

            _busSyncWrap = Policy.Wrap(fallbackPolicy, retryPolicy);
            _busAsyncWrap = Policy.WrapAsync(fallbackAsyncPolicy, retryAsyncPolicy);
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

                LogMessage($"remove local cache that prefix is {prefix}");

                return;
            }

            foreach (var item in message.CacheKeys)
            {
                _localCache.Remove(item);

                LogMessage($"remove local cache that cache key is {item}");
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

            // Circuit Breaker may be more better
            try
            {
                flag = _distributedCache.Exists(cacheKey);
                return flag;
            }
            catch (Exception ex)
            {
                LogMessage($"Check cache key exists error [{cacheKey}] ", ex);
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

            // Circuit Breaker may be more better
            try
            {
                flag = await _distributedCache.ExistsAsync(cacheKey);
                return flag;
            }
            catch (Exception ex)
            {
                LogMessage($"Check cache key [{cacheKey}] exists error", ex);
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

            LogMessage($"local cache can not get the value of {cacheKey}");

            // Circuit Breaker may be more better
            try
            {
                cacheValue = _distributedCache.Get<T>(cacheKey);
            }
            catch (Exception ex)
            {
                LogMessage($"distributed cache get error, [{cacheKey}]", ex);
            }

            if (cacheValue.HasValue)
            {
                TimeSpan ts = GetExpiration(cacheKey);
               
                _localCache.Set(cacheKey, cacheValue.Value, ts);

                return cacheValue;
            }

            LogMessage($"distributed cache can not get the value of {cacheKey}");

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

            LogMessage($"local cache can not get the value of {cacheKey}");

            try
            {
                cacheValue = await _distributedCache.GetAsync<T>(cacheKey);
            }
            catch (Exception ex)
            {
                LogMessage($"distributed cache get error, [{cacheKey}]", ex);
            }

            if (cacheValue.HasValue)
            {
                TimeSpan ts = await GetExpirationAsync(cacheKey);

                await _localCache.SetAsync(cacheKey, cacheValue.Value, ts);

                return cacheValue;
            }

            LogMessage($"distributed cache can not get the value of {cacheKey}");

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
                LogMessage($"remove cache key [{cacheKey}] error", ex);
            }

            _localCache.Remove(cacheKey);

            // send message to bus 
            _busSyncWrap.Execute(() => _bus.Publish(_options.TopicName, new EasyCachingMessage { Id = _cacheId, CacheKeys = new string[] { cacheKey } }));
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
                LogMessage($"remove cache key [{cacheKey}] error", ex);
            }

            await _localCache.RemoveAsync(cacheKey);

            // send message to bus 
            await _busAsyncWrap.ExecuteAsync(async () => await _bus.PublishAsync(_options.TopicName, new EasyCachingMessage { Id = _cacheId, CacheKeys = new string[] { cacheKey } }));
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
                LogMessage($"set cache key [{cacheKey}] error", ex);
            }

            // When create/update cache, send message to bus so that other clients can remove it.
            _busSyncWrap.Execute(() => _bus.Publish(_options.TopicName, new EasyCachingMessage { Id = _cacheId, CacheKeys = new string[] { cacheKey } }));
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
                LogMessage($"set cache key [{cacheKey}] error", ex);
            }

            // When create/update cache, send message to bus so that other clients can remove it.
            await _busAsyncWrap.ExecuteAsync(async () => await _bus.PublishAsync(_options.TopicName, new EasyCachingMessage { Id = _cacheId, CacheKeys = new string[] { cacheKey } }));
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
                LogMessage($"tryset cache key [{cacheKey}] error", ex);
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
                _busSyncWrap.Execute(() => _bus.Publish(_options.TopicName, new EasyCachingMessage { Id = _cacheId, CacheKeys = new string[] { cacheKey } }));
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
                LogMessage($"tryset cache key [{cacheKey}] error", ex);
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
                await _busAsyncWrap.ExecuteAsync(async () => await _bus.PublishAsync(_options.TopicName, new EasyCachingMessage { Id = _cacheId, CacheKeys = new string[] { cacheKey } }));
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
                LogMessage($"set all from distributed provider error [{string.Join(",", value.Keys)}]", ex);
            }

            // send message to bus 
            _busSyncWrap.Execute(() => _bus.Publish(_options.TopicName, new EasyCachingMessage { Id = _cacheId, CacheKeys = value.Keys.ToArray(), IsPrefix = false }));
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
                LogMessage($"set all from distributed provider error [{string.Join(",", value.Keys)}]", ex);
            }

            // send message to bus 
            await _busAsyncWrap.ExecuteAsync(async () => await _bus.PublishAsync(_options.TopicName, new EasyCachingMessage { Id = _cacheId, CacheKeys = value.Keys.ToArray(), IsPrefix = false }));
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
                LogMessage($"remove all from distributed provider error [{string.Join(",", cacheKeys)}]", ex);
            }

            _localCache.RemoveAll(cacheKeys);

            // send message to bus in order to notify other clients.
            _busSyncWrap.Execute(() => _bus.Publish(_options.TopicName, new EasyCachingMessage { Id = _cacheId, CacheKeys = cacheKeys.ToArray() }));
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
                LogMessage($"remove all async from distributed provider error [{string.Join(",", cacheKeys)}]", ex);
            }

            await _localCache.RemoveAllAsync(cacheKeys);

            // send message to bus in order to notify other clients.
            await _busAsyncWrap.ExecuteAsync(async () => await _bus.PublishAsync(_options.TopicName, new EasyCachingMessage { Id = _cacheId, CacheKeys = cacheKeys.ToArray() }));
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

            var result = _localCache.Get<T>(cacheKey);

            if (result.HasValue)
            {
                return result;
            }

            try
            {
                result = _distributedCache.Get<T>(cacheKey, dataRetriever, expiration);
            }
            catch (Exception ex)
            {
                LogMessage($"get with data retriever from distributed provider error [{cacheKey}]", ex);
            }

            if (result.HasValue)
            {
                TimeSpan ts = GetExpiration(cacheKey);

                _localCache.Set(cacheKey, result.Value, ts);

                return result;
            }

            return CacheValue<T>.NoValue;
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

            var result = await _localCache.GetAsync<T>(cacheKey);

            if (result.HasValue)
            {
                return result;
            }

            try
            {
                result = await _distributedCache.GetAsync<T>(cacheKey, dataRetriever, expiration);
            }
            catch (Exception ex)
            {
                LogMessage($"get async with data retriever from distributed provider error [{cacheKey}]", ex);
            }

            if (result.HasValue)
            {
                TimeSpan ts = await GetExpirationAsync(cacheKey);

                _localCache.Set(cacheKey, result.Value, ts);

                return result;
            }

            return CacheValue<T>.NoValue;
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
                LogMessage($"remove by prefix [{prefix}] error", ex);
            }

            _localCache.RemoveByPrefix(prefix);

            // send message to bus 
            _busSyncWrap.Execute(() => _bus.Publish(_options.TopicName, new EasyCachingMessage { Id = _cacheId, CacheKeys = new string[] { prefix }, IsPrefix = true }));
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
                LogMessage($"remove by prefix [{prefix}] error", ex);
            }

            await _localCache.RemoveByPrefixAsync(prefix);

            // send message to bus in order to notify other clients.
            await _busAsyncWrap.ExecuteAsync(async () => await _bus.PublishAsync(_options.TopicName, new EasyCachingMessage { Id = _cacheId, CacheKeys = new string[] { prefix }, IsPrefix = true }));
        }

        /// <summary>
        /// Logs the message.
        /// </summary>
        /// <param name="message">Message.</param>
        /// <param name="ex">Ex.</param>
        private void LogMessage(string message, Exception ex = null)
        {
            if (_options.EnableLogging)
            {
                if (ex == null)
                {
                    _logger.LogDebug(message);
                }
                else
                {
                    _logger.LogError(ex, message);
                }
            }
        }

        private async Task<TimeSpan> GetExpirationAsync(string cacheKey)
        {
            TimeSpan ts = TimeSpan.Zero;

            try
            {
                ts = await _distributedCache.GetExpirationAsync(cacheKey);
            }
            catch
            {

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
            catch
            {

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

            LogMessage($"local cache can not get the value of {cacheKey}");

            try
            {
                cacheValue = await _distributedCache.GetAsync(cacheKey, type);
            }
            catch (Exception ex)
            {
                LogMessage($"distributed cache get error, [{cacheKey}]", ex);
            }

            if (cacheValue != null)
            {
                TimeSpan ts = await GetExpirationAsync(cacheKey);
              
                await _localCache.SetAsync(cacheKey, cacheValue, ts);

                return cacheValue;
            }

            LogMessage($"distributed cache can not get the value of {cacheKey}");

            return null;
        }
    }
}
