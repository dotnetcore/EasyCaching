using EasyCaching.Core;
using EasyCaching.Core.Serialization;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EasyCaching.Etcd
{
    public sealed partial class DefaultEtcdCachingProvider : EasyCachingAbstractProvider, IDisposable
    {
        // name
        private readonly string _name;

        private bool _disposed;

        // com
        private readonly ILogger? _logger;

        private readonly IEasyCachingSerializer _serializer;
        private readonly EtcdCachingOptions _options;

        private readonly IEtcdCaching _cache;

        /// <summary>
        /// The cache stats.
        /// </summary>
        private readonly CacheStats _cacheStats;

        private readonly ProviderInfo _info;

        public DefaultEtcdCachingProvider(
            string name,
            IEnumerable<IEtcdCaching> cache,
            EtcdCachingOptions options,
            IEnumerable<IEasyCachingSerializer> serializers,
            ILoggerFactory? loggerFactory = null)
        {
            ArgumentCheck.NotNull(options, nameof(options));
            ArgumentCheck.NotNull(serializers, nameof(serializers));

            _name = name;

            _options = options;
            _logger = loggerFactory?.CreateLogger<DefaultEtcdCachingProvider>();

            _cache = cache.Single(x => x.ProviderName == _name);

            var serName = !string.IsNullOrWhiteSpace(options.SerializerName) ? options.SerializerName : name;
            _serializer = serializers.FirstOrDefault(x => x.Name.Equals(serName)) ??
                          throw new EasyCachingNotFoundException(string.Format(
                              EasyCachingConstValue.NotFoundSerExceptionMessage,
                              serName));

            _cacheStats = new CacheStats();
            ProviderName = _name;
            ProviderType = CachingProviderType.Etcd;
            ProviderStats = _cacheStats;
            ProviderMaxRdSecond = _options.MaxRdSecond;
            IsDistributedProvider = false;
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

            var result = _cache.Get<T>(cacheKey);
            if (result.HasValue)
            {
                if (_options.EnableLogging)
                    _logger?.LogInformation($"Cache Hit : cachekey = {cacheKey}");

                CacheStats.OnHit();

                return result;
            }

            CacheStats.OnMiss();

            if (_options.EnableLogging)
                _logger?.LogInformation($"Cache Missed : cachekey = {cacheKey}");

            if (!_cache.Set<string>($"{cacheKey}_Lock", "1", TimeSpan.FromMilliseconds(_options.LockMs)))
            {
                System.Threading.Thread.Sleep(_options.SleepMs);
                return Get(cacheKey, dataRetriever, expiration);
            }

            try
            {
                var res = dataRetriever();

                if (res != null || _options.CacheNulls)
                {
                    Set(cacheKey, res, expiration);
                    //remove mutex key
                    _cache.Delete($"{cacheKey}_Lock");

                    return new CacheValue<T>(res, true);
                }
                else
                {
                    //remove mutex key
                    _cache.Delete($"{cacheKey}_Lock");
                    return CacheValue<T>.NoValue;
                }
            }
            catch
            {
                //remove mutex key
                _cache.Delete($"{cacheKey}_Lock");
                throw;
            }
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

            var result = _cache.Get<T>(cacheKey);
            if (result.HasValue)
            {
                if (_options.EnableLogging)
                    _logger?.LogInformation($"Cache Hit : cachekey = {cacheKey}");

                CacheStats.OnHit();

                return result;
            }
            else
            {
                if (_options.EnableLogging)
                    _logger?.LogInformation($"Cache Missed : cachekey = {cacheKey}");

                CacheStats.OnMiss();

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

            _cache.Delete(cacheKey);
        }

        /// <summary>
        /// Set the specified cacheKey, cacheValue and expiration.
        /// </summary>
        /// <returns>The set.</returns>
        /// <param name="cacheKey">Cache key.</param>
        /// <param name="cacheValue">Cache value.</param>
        /// <param name="expiration">expiration.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public override void BaseSet<T>(string cacheKey, T cacheValue, TimeSpan expiration)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));
            ArgumentCheck.NotNull(cacheValue, nameof(cacheValue), _options.CacheNulls);
            ArgumentCheck.NotNegativeOrZero(expiration, nameof(expiration));

            if (MaxRdSecond > 0)
            {
                var addSec = new Random().Next(1, MaxRdSecond);
                expiration = expiration.Add(TimeSpan.FromMilliseconds(addSec));
            }

            //var valExpiration = expiration.Seconds <= 1 ? expiration : TimeSpan.FromSeconds(expiration.Seconds / 2);
            //var val = new CacheValue<T>(cacheValue, true, valExpiration);
            _cache.Set<T>(cacheKey, cacheValue, expiration);
        }

        /// <summary>
        /// Exists the specified cacheKey.
        /// </summary>
        /// <returns>The exists.</returns>
        /// <param name="cacheKey">Cache key.</param>
        public override bool BaseExists(string cacheKey)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            return _cache.Exists(cacheKey);
        }

        /// <summary>
        /// Removes cached item by cachekey's prefix.
        /// </summary>
        /// <param name="prefix">Prefix.</param>
        public override void BaseRemoveByPrefix(string prefix)
        {
            ArgumentCheck.NotNullOrWhiteSpace(prefix, nameof(prefix));

            var count = _cache.DeleteRangeData(prefix);

            if (_options.EnableLogging)
                _logger?.LogInformation($"RemoveByPrefix : prefix = {prefix} , count = {count}");
        }

        /// <summary>
        /// Removes cached items by pattern async.
        /// </summary>
        /// <returns>The by prefix async.</returns>
        /// <param name="pattern">Pattern.</param>
        public override void BaseRemoveByPattern(string pattern)
        {
            ArgumentCheck.NotNullOrWhiteSpace(pattern, nameof(pattern));

            throw new NotSupportedException("BaseRemoveByPattern is not supported in Etcd provider.");
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

            foreach (var item in values)
            {
                _cache.Set<T>(item.Key, item.Value, expiration);
            }
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

            if (_options.EnableLogging)
                _logger?.LogInformation($"GetAll : cacheKeys = {string.Join(",", cacheKeys)}");

            Dictionary<string, CacheValue<T>> result = new Dictionary<string, CacheValue<T>>();
            foreach (var item in cacheKeys)
            {
                var value = BaseGet<T>(item);
                result.Add(item, value);
            }
            return result;
        }

        /// <summary>
        /// Get all cacheKey by prefix.
        /// </summary>
        /// <param name="prefix">Prefix.</param>
        /// <returns>Get all cacheKey by prefix.</returns>
        public override IEnumerable<string> BaseGetAllKeysByPrefix(string prefix)
        {
            if (_options.EnableLogging)
                _logger?.LogInformation("GetAllKeys");

            var dicData = _cache.GetAll(prefix);
            List<string> result = new List<string>();
            foreach (var item in dicData)
            {
                result.Add(item.Key);
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

            if (_options.EnableLogging)
                _logger?.LogInformation($"GetByPrefix : prefix = {prefix}");

            var dicData = _cache.GetAll(prefix);
            Dictionary<string, CacheValue<T>> result = new Dictionary<string, CacheValue<T>>();
            foreach (var item in dicData)
            {
                result.Add(item.Key, new CacheValue<T>(_serializer.Deserialize<T>(Encoding.UTF8.GetBytes(item.Value)), true));
            }
            return result;
        }

        /// <summary>
        /// Removes all.
        /// </summary>
        /// <param name="cacheKeys">Cache keys.</param>
        public override void BaseRemoveAll(IEnumerable<string> cacheKeys)
        {
            ArgumentCheck.NotNullAndCountGTZero(cacheKeys, nameof(cacheKeys));

            if (_options.EnableLogging)
                _logger?.LogInformation($"RemoveAll : cacheKeys = {string.Join(",", cacheKeys)}");

            foreach (var item in cacheKeys)
            {
                _cache.Delete(item);
            }
        }

        /// <summary>
        /// Gets the count.
        /// </summary>
        /// <returns>The count.</returns>
        /// <param name="prefix">Prefix.</param>
        public override int BaseGetCount(string prefix = "")
        {
            var dicData = _cache.GetAll(prefix);
            return dicData != null ? dicData.Count : 0;
        }

        /// <summary>
        /// Flush All Cached Item.
        /// </summary>
        public override void BaseFlush()
        {
            if (_options.EnableLogging)
                _logger?.LogInformation("Flush");

            var dicData = _cache.GetAll("");
            if (dicData != null)
            {
                List<string> listKeys = new List<string>(dicData.Count);
                foreach (var item in dicData)
                {
                    listKeys.Add(item.Key);
                }
                if(listKeys.Count > 0)
                 BaseRemoveAll(listKeys);
            }
            // throw new NotSupportedException("BaseFlush is not supported in Etcd provider.");
        }

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

            //var val = new CacheValue<T>(cacheValue, true, expiration);
            return _cache.Set<T>(cacheKey, cacheValue, expiration);
        }

        /// <summary>
        /// Get the expiration of cache key
        /// </summary>
        /// <param name="cacheKey">cache key</param>
        /// <returns>expiration</returns>
        public override TimeSpan BaseGetExpiration(string cacheKey)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));
            //_cache.LeaseTimeToLive(new LeaseTimeToLiveRequest()
            //{
            //     Keys
            //})
            //return _cache.GetExpiration(cacheKey);
            throw new NotSupportedException("BaseGetExpiration is not supported in Etcd provider.");
        }

        /// <summary>
        /// Get te information of this provider.
        /// </summary>
        /// <returns></returns>
        public override ProviderInfo BaseGetProviderInfo() => _info;

        public override object BaseGetDatabase() => _cache;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool _)
        {
            if (_disposed)
                return;

            _disposed = true;
        }
    }
}