namespace EasyCaching.HybridCache
{
    using EasyCaching.Core;
    using EasyCaching.Core.Internal;
    using Microsoft.Extensions.Options;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
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
        /// Initializes a new instance of the <see cref="T:EasyCaching.HybridCache.HybridCachingProvider"/> class.
        /// </summary>
        /// <param name="optionsAccs">Options accs.</param>
        /// <param name="providers">Providers.</param>
        /// <param name="bus">Bus.</param>
        /// <param name="loggerFactory">Logger factory.</param>
        public HybridCachingProvider(
            IOptions<HybridCachingOptions> optionsAccs
            , IEnumerable<IEasyCachingProvider> providers
            , IEasyCachingBus bus = null
            , ILoggerFactory loggerFactory = null
            )
        {
            ArgumentCheck.NotNullAndCountGTZero(providers, nameof(providers));

            this._options = optionsAccs.Value;

            ArgumentCheck.NotNullOrWhiteSpace(_options.TopicName, nameof(_options.TopicName));

            this._logger = loggerFactory?.CreateLogger<HybridCachingProvider>();

            //Here use the order to distinguish traditional provider
            var local = providers.OrderBy(x => x.Order).FirstOrDefault(x => !x.IsDistributedCache);

            if (local == null) throw new NotFoundCachingProviderException("Can not found any local caching providers.");
            else this._localCache = local;

            //Here use the order to distinguish traditional provider
            var distributed = providers.OrderBy(x => x.Order).FirstOrDefault(x => x.IsDistributedCache);

            if (distributed == null) throw new NotFoundCachingProviderException("Can not found any distributed caching providers.");
            else this._distributedCache = distributed;

            this._bus = bus ?? NullEasyCachingBus.Instance;
            this._bus.Subscribe(_options.TopicName, OnMessage);
        }

        /// <summary>
        /// Ons the message.
        /// </summary>
        /// <param name="message">Message.</param>
        private void OnMessage(EasyCachingMessage message)
        {
            foreach (var item in message.CacheKeys)
            {
                _localCache.Remove(item);

                if (_options.EnableLogging)
                {
                    _logger.LogTrace($"remove local cache that cache key is {item}");
                }
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

            return _distributedCache.Exists(cacheKey);
        }

        /// <summary>
        /// Existses the specified cacheKey async.
        /// </summary>
        /// <returns>The async.</returns>
        /// <param name="cacheKey">Cache key.</param>
        public async Task<bool> ExistsAsync(string cacheKey)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            return await _distributedCache.ExistsAsync(cacheKey);
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

            if(_options.EnableLogging)
            {
                _logger.LogTrace($"local cache can not get the value of {cacheKey}");
            }

            cacheValue = _distributedCache.Get<T>(cacheKey);

            if (cacheValue.HasValue)
            {
                _localCache.Set(cacheKey, cacheValue.Value, TimeSpan.FromSeconds(60));

                return cacheValue;
            }

            if (_options.EnableLogging)
            {
                _logger.LogTrace($"distributed cache can not get the value of {cacheKey}");
            }

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

            if (_options.EnableLogging)
            {
                _logger.LogTrace($"local cache can not get the value of {cacheKey}");
            }

            cacheValue = await _distributedCache.GetAsync<T>(cacheKey);

            if (cacheValue.HasValue)
            {
                await _localCache.SetAsync(cacheKey, cacheValue.Value, TimeSpan.FromSeconds(60));

                return cacheValue;
            }

            if (_options.EnableLogging)
            {
                _logger.LogTrace($"distributed cache can not get the value of {cacheKey}");
            }

            return CacheValue<T>.NoValue;
        }

        /// <summary>
        /// Remove the specified cacheKey.
        /// </summary>
        /// <param name="cacheKey">Cache key.</param>
        public void Remove(string cacheKey)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            _distributedCache.Remove(cacheKey);
            _localCache.Remove(cacheKey);

            //send message to bus 
            _bus.Publish(_options.TopicName, new EasyCachingMessage { CacheKeys = new string[] { cacheKey } });
        }

        /// <summary>
        /// Removes the specified cacheKey async.
        /// </summary>
        /// <returns>The async.</returns>
        /// <param name="cacheKey">Cache key.</param>
        public async Task RemoveAsync(string cacheKey)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            await _distributedCache.RemoveAsync(cacheKey);
            await _localCache.RemoveAsync(cacheKey);

            //send message to bus 
            await _bus.PublishAsync(_options.TopicName, new EasyCachingMessage { CacheKeys = new string[] { cacheKey } });
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
            _distributedCache.Set(cacheKey, cacheValue, expiration);

            //send message to bus
            _bus.Publish(_options.TopicName, new EasyCachingMessage { CacheKeys = new string[] { cacheKey } });
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
            await _distributedCache.SetAsync(cacheKey, cacheValue, expiration);

            //send message to bus
            await _bus.PublishAsync(_options.TopicName, new EasyCachingMessage { CacheKeys = new string[] { cacheKey } });
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

            var flag = _distributedCache.TrySet(cacheKey, cacheValue, expiration);

            if (flag)
            {
                _localCache.Set(cacheKey, cacheValue, expiration);
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

            var flag = await _distributedCache.TrySetAsync(cacheKey, cacheValue, expiration);

            if (flag)
            {
                await _localCache.SetAsync(cacheKey, cacheValue, expiration);
            }

            return flag;
        }
    }
}
