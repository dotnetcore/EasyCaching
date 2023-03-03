//using dotnet_etcd;
//using EasyCaching.Core;
//using EasyCaching.Core.Serialization;
//using Microsoft.Extensions.Logging;
//using System;
//using System.Collections.Concurrent;
//using System.Collections.Generic;
//using System.Linq;
//using System.Runtime.CompilerServices;
//using System.Threading;
//using static Etcdserverpb.KV;

//namespace EasyCaching.Etcd
//{
//    public sealed partial class DefaultEtcdCachingProvider : EasyCachingAbstractProvider, IDisposable
//    {
//        // name
//        private readonly string _name;

//        private bool _disposed;

//        // com
//        private readonly ILogger? _logger;

//        private readonly IEasyCachingSerializer _serializer;
//        private readonly EtcdCachingOptions _options;

//        private readonly EtcdClient _cache;
//        private readonly string _authToken;
//        private readonly Grpc.Core.Metadata _metadata;
//        private long _globalLeaseId;

//        /// <summary>
//        /// The cache stats.
//        /// </summary>
//        private readonly CacheStats _cacheStats;

//        private readonly ProviderInfo _info;

//        public DefaultEtcdCachingProvider(
//            string name,
//            EtcdCachingOptions options,
//            IEnumerable<IEasyCachingSerializer> serializers,
//            ILoggerFactory? loggerFactory = null)
//        {
//            ArgumentCheck.NotNull(options, nameof(options));
//            ArgumentCheck.NotNull(serializers, nameof(serializers));

//            _name = name;

//            _options = options;
//            _logger = loggerFactory?.CreateLogger<DefaultEtcdCachingProvider>();

//            //init etcd client            
//            this._cache = new EtcdClient(options.Address);
//            //auth
//            if (!string.IsNullOrEmpty(options.UserName) && !string.IsNullOrEmpty(options.Password))
//            {
//                var authRes = this._cache.Authenticate(new Etcdserverpb.AuthenticateRequest()
//                {
//                    Name = options.UserName,
//                    Password = options.Password,
//                });
//                _authToken = authRes.Token;
//                _metadata = new Grpc.Core.Metadata() { new Grpc.Core.Metadata.Entry("token", _authToken) };
//            }
//            InitLease(options);

//            var serName = !string.IsNullOrWhiteSpace(options.SerializerName) ? options.SerializerName : name;
//            _serializer = serializers.FirstOrDefault(x => x.Name.Equals(serName)) ??
//                          throw new EasyCachingNotFoundException(string.Format(
//                              EasyCachingConstValue.NotFoundSerExceptionMessage,
//                              serName));

//            _cacheStats = new CacheStats();
//            ProviderName = _name;
//            ProviderType = CachingProviderType.Etcd;
//            ProviderStats = _cacheStats;
//            ProviderMaxRdSecond = _options.MaxRdSecond;
//            IsDistributedProvider = false;
//            _info = new ProviderInfo
//            {
//                CacheStats = _cacheStats,
//                EnableLogging = options.EnableLogging,
//                IsDistributedProvider = IsDistributedProvider,
//                LockMs = options.LockMs,
//                MaxRdSecond = options.MaxRdSecond,
//                ProviderName = ProviderName,
//                ProviderType = ProviderType,
//                SerializerName = options.SerializerName,
//                SleepMs = options.SleepMs,
//                CacheNulls = options.CacheNulls
//            };
//        }

//        /// <summary>
//        ///  rent
//        /// </summary>
//        private void InitLease(EtcdCachingOptions options)
//        {
//            try
//            {
//                // create rent id to bind
//                var response = this._cache.LeaseGrant(new Etcdserverpb.LeaseGrantRequest()
//                {
//                    TTL = options.TTL
//                });
//                this._globalLeaseId = response.ID;
//                var tokenSource = new CancellationTokenSource();
//                this._cache.LeaseKeepAlive(this._globalLeaseId, tokenSource.Token);
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError("Init Lease error", ex);
//            }
//        }

//        /// <summary>
//        /// get data
//        /// </summary>
//        /// <param name="cacheKey"></param>
//        /// <returns></returns>
//        public T GetVal<T>(string cacheKey)
//        {
//            var data = _cache.GetVal(cacheKey, _metadata);
//            if (data != null)
//            {
//               var segmentData = _serializer.SerializeObject(data);
//                _serializer.
//            }
//            return null;
//        }

//        /// <summary>
//        /// Get the specified cacheKey, dataRetriever and expiration.
//        /// </summary>
//        /// <returns>The get.</returns>
//        /// <param name="cacheKey">Cache key.</param>
//        /// <param name="dataRetriever">Data retriever.</param>
//        /// <param name="expiration">Expiration.</param>
//        /// <typeparam name="T">The 1st type parameter.</typeparam>
//        public override CacheValue<T> BaseGet<T>(string cacheKey, Func<T> dataRetriever, TimeSpan expiration)
//        {
//            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));
//            ArgumentCheck.NotNegativeOrZero(expiration, nameof(expiration));

//            var result = _cache.GetValAsync(cacheKey, _metadata)(cacheKey);
//            if (result.HasValue)
//            {
//                if (_options.EnableLogging)
//                    _logger?.LogInformation($"Cache Hit : cachekey = {cacheKey}");

//                CacheStats.OnHit();

//                return result;
//            }

//            CacheStats.OnMiss();

//            if (_options.EnableLogging)
//                _logger?.LogInformation($"Cache Missed : cachekey = {cacheKey}");

//            if (!_cache.Add($"{cacheKey}_Lock", 1, TimeSpan.FromMilliseconds(_options.LockMs)))
//            {
//                System.Threading.Thread.Sleep(_options.SleepMs);
//                return Get(cacheKey, dataRetriever, expiration);
//            }

//            try
//            {
//                var res = dataRetriever();

//                if (res != null || _options.CacheNulls)
//                {
//                    Set(cacheKey, res, expiration);
//                    //remove mutex key
//                    _cache.Remove($"{cacheKey}_Lock");

//                    return new CacheValue<T>(res, true);
//                }
//                else
//                {
//                    //remove mutex key
//                    _cache.Remove($"{cacheKey}_Lock");
//                    return CacheValue<T>.NoValue;
//                }
//            }
//            catch
//            {
//                //remove mutex key
//                _cache.Remove($"{cacheKey}_Lock");
//                throw;
//            }
//        }

//        /// <summary>
//        /// Get the specified cacheKey.
//        /// </summary>
//        /// <returns>The get.</returns>
//        /// <param name="cacheKey">Cache key.</param>
//        /// <typeparam name="T">The 1st type parameter.</typeparam>
//        public override CacheValue<T> BaseGet<T>(string cacheKey)
//        {
//            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

//            var result = _cache.Get<T>(cacheKey);
//            if (result.HasValue)
//            {
//                if (_options.EnableLogging)
//                    _logger?.LogInformation($"Cache Hit : cachekey = {cacheKey}");

//                CacheStats.OnHit();

//                return result;
//            }
//            else
//            {
//                if (_options.EnableLogging)
//                    _logger?.LogInformation($"Cache Missed : cachekey = {cacheKey}");

//                CacheStats.OnMiss();

//                return CacheValue<T>.NoValue;
//            }
//        }

//        /// <summary>
//        /// Remove the specified cacheKey.
//        /// </summary>
//        /// <returns>The remove.</returns>
//        /// <param name="cacheKey">Cache key.</param>
//        public override void BaseRemove(string cacheKey)
//        {
//            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

//            _cache.Remove(cacheKey);
//        }


//        /// <summary>
//        /// Set the specified cacheKey, cacheValue and expiration.
//        /// </summary>
//        /// <returns>The set.</returns>
//        /// <param name="cacheKey">Cache key.</param>
//        /// <param name="cacheValue">Cache value.</param>
//        /// <param name="expiration">expiration.</param>
//        /// <typeparam name="T">The 1st type parameter.</typeparam>
//        public override void BaseSet<T>(string cacheKey, T cacheValue, TimeSpan expiration)
//        {
//            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));
//            ArgumentCheck.NotNull(cacheValue, nameof(cacheValue), _options.CacheNulls);
//            ArgumentCheck.NotNegativeOrZero(expiration, nameof(expiration));

//            if (MaxRdSecond > 0)
//            {
//                var addSec = new Random().Next(1, MaxRdSecond);
//                expiration = expiration.Add(TimeSpan.FromSeconds(addSec));
//            }

//            //var valExpiration = expiration.Seconds <= 1 ? expiration : TimeSpan.FromSeconds(expiration.Seconds / 2);
//            //var val = new CacheValue<T>(cacheValue, true, valExpiration);
//            _cache.Set(cacheKey, cacheValue, expiration);
//        }

//        /// <summary>
//        /// Exists the specified cacheKey.
//        /// </summary>
//        /// <returns>The exists.</returns>
//        /// <param name="cacheKey">Cache key.</param>
//        public override bool BaseExists(string cacheKey)
//        {
//            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

//            return _cache.Exists(cacheKey);
//        }

//        /// <summary>
//        /// Removes cached item by cachekey's prefix.
//        /// </summary>
//        /// <param name="prefix">Prefix.</param>
//        public override void BaseRemoveByPrefix(string prefix)
//        {
//            ArgumentCheck.NotNullOrWhiteSpace(prefix, nameof(prefix));

//            var count = _cache.RemoveByPrefix(prefix);

//            if (_options.EnableLogging)
//                _logger?.LogInformation($"RemoveByPrefix : prefix = {prefix} , count = {count}");
//        }

//        /// <summary>
//        /// Removes cached items by pattern async.
//        /// </summary>
//        /// <returns>The by prefix async.</returns>
//        /// <param name="pattern">Pattern.</param>
//        public override void BaseRemoveByPattern(string pattern)
//        {
//            ArgumentCheck.NotNullOrWhiteSpace(pattern, nameof(pattern));

//            var searchPattern = this.ProcessSearchKeyPattern(pattern);
//            var searchKey = this.HandleSearchKeyPattern(pattern);

//            var count = _cache.RemoveByPattern(searchKey, searchPattern);

//            if (_options.EnableLogging)
//                _logger?.LogInformation($"RemoveByPattern : pattern = {pattern} , count = {count}");
//        }

//        /// <summary>
//        /// Sets all.
//        /// </summary>
//        /// <param name="values">Values.</param>
//        /// <param name="expiration">Expiration.</param>
//        /// <typeparam name="T">The 1st type parameter.</typeparam>
//        public override void BaseSetAll<T>(IDictionary<string, T> values, TimeSpan expiration)
//        {
//            ArgumentCheck.NotNegativeOrZero(expiration, nameof(expiration));
//            ArgumentCheck.NotNullAndCountGTZero(values, nameof(values));

//            _cache.SetAll(values, expiration);
//        }

//        /// <summary>
//        /// Gets all.
//        /// </summary>
//        /// <returns>The all.</returns>
//        /// <param name="cacheKeys">Cache keys.</param>
//        /// <typeparam name="T">The 1st type parameter.</typeparam>
//        public override IDictionary<string, CacheValue<T>> BaseGetAll<T>(IEnumerable<string> cacheKeys)
//        {
//            ArgumentCheck.NotNullAndCountGTZero(cacheKeys, nameof(cacheKeys));

//            if (_options.EnableLogging)
//                _logger?.LogInformation($"GetAll : cacheKeys = {string.Join(",", cacheKeys)}");

//            return _cache.GetAll<T>(cacheKeys);
//        }

//        /// <summary>
//        /// Get all cacheKey by prefix.
//        /// </summary>
//        /// <param name="prefix">Prefix.</param>
//        /// <returns>Get all cacheKey by prefix.</returns>
//        public override IEnumerable<string> BaseGetAllKeysByPrefix(string prefix)
//        {
//            if (_options.EnableLogging)
//                _logger?.LogInformation("GetAllKeys");

//            return _cache.GetAllKeys(prefix);
//        }

//        /// <summary>
//        /// Gets the by prefix.
//        /// </summary>
//        /// <returns>The by prefix.</returns>
//        /// <param name="prefix">Prefix.</param>
//        /// <typeparam name="T">The 1st type parameter.</typeparam>
//        public override IDictionary<string, CacheValue<T>> BaseGetByPrefix<T>(string prefix)
//        {
//            ArgumentCheck.NotNullOrWhiteSpace(prefix, nameof(prefix));

//            var map = new Dictionary<string, CacheValue<T>>();

//            if (_options.EnableLogging)
//                _logger?.LogInformation($"GetByPrefix : prefix = {prefix}");

//            return _cache.GetByPrefix<T>(prefix);
//        }

//        /// <summary>
//        /// Removes all.
//        /// </summary>
//        /// <param name="cacheKeys">Cache keys.</param>
//        public override void BaseRemoveAll(IEnumerable<string> cacheKeys)
//        {
//            ArgumentCheck.NotNullAndCountGTZero(cacheKeys, nameof(cacheKeys));

//            if (_options.EnableLogging)
//                _logger?.LogInformation($"RemoveAll : cacheKeys = {string.Join(",", cacheKeys)}");

//            _cache.RemoveAll(cacheKeys);
//        }

//        /// <summary>
//        /// Gets the count.
//        /// </summary>
//        /// <returns>The count.</returns>
//        /// <param name="prefix">Prefix.</param>
//        public override int BaseGetCount(string prefix = "")
//        {
//            return _cache.GetCount(prefix);
//        }

//        /// <summary>
//        /// Flush All Cached Item.
//        /// </summary>
//        public override void BaseFlush()
//        {
//            if (_options.EnableLogging)
//                _logger?.LogInformation("Flush");

//            _cache.Clear();
//        }

//        /// <summary>
//        /// Tries the set.
//        /// </summary>
//        /// <returns><c>true</c>, if set was tryed, <c>false</c> otherwise.</returns>
//        /// <param name="cacheKey">Cache key.</param>
//        /// <param name="cacheValue">Cache value.</param>
//        /// <param name="expiration">Expiration.</param>
//        /// <typeparam name="T">The 1st type parameter.</typeparam>
//        public override bool BaseTrySet<T>(string cacheKey, T cacheValue, TimeSpan expiration)
//        {
//            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));
//            ArgumentCheck.NotNull(cacheValue, nameof(cacheValue), _options.CacheNulls);
//            ArgumentCheck.NotNegativeOrZero(expiration, nameof(expiration));

//            //var val = new CacheValue<T>(cacheValue, true, expiration);
//            return _cache.Add(cacheKey, cacheValue, expiration);
//        }

//        /// <summary>
//        /// Get the expiration of cache key
//        /// </summary>
//        /// <param name="cacheKey">cache key</param>
//        /// <returns>expiration</returns>
//        public override TimeSpan BaseGetExpiration(string cacheKey)
//        {
//            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

//            return _cache.GetExpiration(cacheKey);
//        }

//        /// <summary>
//        /// Get te information of this provider.
//        /// </summary>
//        /// <returns></returns>
//        public override ProviderInfo BaseGetProviderInfo() => _info;

//        public override object BaseGetDatabase() => _cache;
//    }
//}