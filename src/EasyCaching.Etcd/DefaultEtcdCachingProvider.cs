using dotnet_etcd;
using EasyCaching.Core;
using EasyCaching.Core.Serialization;
using Etcdserverpb;
using Google.Protobuf;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

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

        private readonly EtcdClient _cache;
        private readonly string _authToken;
        private readonly Grpc.Core.Metadata _metadata;

        /// <summary>
        /// The cache stats.
        /// </summary>
        private readonly CacheStats _cacheStats;

        private readonly ProviderInfo _info;

        public DefaultEtcdCachingProvider(
            string name,
            EtcdCachingOptions options,
            IEnumerable<IEasyCachingSerializer> serializers,
            ILoggerFactory? loggerFactory = null)
        {
            ArgumentCheck.NotNull(options, nameof(options));
            ArgumentCheck.NotNull(serializers, nameof(serializers));

            _name = name;

            _options = options;
            _logger = loggerFactory?.CreateLogger<DefaultEtcdCachingProvider>();

            //init etcd client
            this._cache = new EtcdClient(connectionString: options.Address,serverName:options.ServerName, configureChannelOptions: null);
            //auth
            if (!string.IsNullOrEmpty(options.UserName) && !string.IsNullOrEmpty(options.Password))
            {
                var authRes = this._cache.Authenticate(new Etcdserverpb.AuthenticateRequest()
                {
                    Name = options.UserName,
                    Password = options.Password,
                });
                _authToken = authRes.Token;
                _metadata = new Grpc.Core.Metadata() { new Grpc.Core.Metadata.Entry("token", _authToken) };
            }

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

        #region etcd method

        /// <summary>
        /// get data
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <returns></returns>
        private CacheValue<T> GetVal<T>(string cacheKey)
        {
            var data = _cache.GetVal(cacheKey, _metadata);
            return string.IsNullOrWhiteSpace(data)
                    ? CacheValue<T>.Null
                    : new CacheValue<T>(_serializer.Deserialize<T>(Encoding.UTF8.GetBytes(data)), true);
        }

        /// <summary>
        /// get data
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <returns></returns>
        private async Task<CacheValue<T>> GetValAsync<T>(string cacheKey)
        {
            var data = await _cache.GetValAsync(cacheKey, _metadata);
            return string.IsNullOrWhiteSpace(data)
                    ? CacheValue<T>.Null
                    : new CacheValue<T>(_serializer.Deserialize<T>(Encoding.UTF8.GetBytes(data)), true);
        }

        /// <summary>
        /// get rangevalues
        /// </summary>
        /// <param name="prefixKey"></param>
        /// <returns></returns>
        private IDictionary<string, string> GetRangeVals(string prefixKey)
        {
            return _cache.GetRangeVal(prefixKey, _metadata);
        }

        /// <summary>
        /// get rangevalues
        /// </summary>
        /// <param name="prefixKey"></param>
        /// <returns></returns>
        private async Task<IDictionary<string, string>> GetRangeValsAsync(string prefixKey)
        {
            return await _cache.GetRangeValAsync(prefixKey, _metadata);
        }

        /// <summary>
        ///  data exists
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <returns></returns>
        private bool GetDataExists(string cacheKey)
        {
            var data = _cache.GetVal(cacheKey, _metadata);
            return data == string.Empty ? false : true;
        }

        /// <summary>
        ///  data exists
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <returns></returns>
        private async Task<bool> GetDataExistsAsync(string cacheKey)
        {
            var data = await _cache.GetValAsync(cacheKey, _metadata);
            return data == string.Empty ? false : true;
        }

        /// <summary>
        /// get rent leaseId
        /// </summary>
        /// <param name="ts"></param>
        /// <returns></returns>
        private long GetRentLeaseId(TimeSpan ts)
        {
            // create rent id to bind
            CancellationTokenSource cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(_options.Timeout));
            var response = _cache.LeaseGrant(request: new LeaseGrantRequest()
            {
                TTL = (long)ts.TotalMilliseconds,
            }, cancellationToken: cts.Token);
            return response.ID;
        }

        /// <summary>
        /// get rent leaseId
        /// </summary>
        /// <param name="ts"></param>
        /// <returns></returns>
        private async Task<long> GetRentLeaseIdAsync(TimeSpan ts)
        {
            // create rent id to bind
            CancellationTokenSource cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(_options.Timeout));
            var response = await _cache.LeaseGrantAsync(request: new LeaseGrantRequest()
            {
                TTL = (long)ts.TotalMilliseconds,
            }, cancellationToken: cts.Token);
            return response.ID;
        }

        /// <summary>
        /// put ke-val with leaseId
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="ts"></param>
        /// <returns></returns>
        private bool AddEphemeralData<T>(string key, T value, TimeSpan ts)
        {
            try
            {
                long leaseId = GetRentLeaseId(ts);
                CancellationTokenSource cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(_options.Timeout));
                PutRequest request = new PutRequest()
                {
                    Key = ByteString.CopyFromUtf8(key),
                    Value = ByteString.CopyFrom(_serializer.Serialize(value)),
                    Lease = leaseId
                };
                var response = _cache.Put(request: request, headers: _metadata, cancellationToken: cts.Token);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError("putEphemeral(key:{},value:{}) error.", key, value, ex);
            }
            return false;
        }

        /// <summary>
        /// put ke-val with leaseId
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="ts"></param>
        /// <returns></returns>
        private async Task<bool> AddEphemeralDataAsync<T>(string key, T value, TimeSpan ts)
        {
            try
            {
                long leaseId = await GetRentLeaseIdAsync(ts);
                CancellationTokenSource cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(_options.Timeout));
                PutRequest request = new PutRequest()
                {
                    Key = ByteString.CopyFromUtf8(key),
                    Value = ByteString.CopyFrom(_serializer.Serialize(value)),
                    Lease = leaseId
                };
                var response = await _cache.PutAsync(request: request, headers: _metadata, cancellationToken: cts.Token);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError("putEphemeral(key:{},value:{}) error.", key, value, ex);
            }
            return false;
        }

        /// <summary>
        /// delete key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        private long DeleteData(string key)
        {
            var response = _cache.Delete(key, _metadata);
            return response.Deleted;
        }

        /// <summary>
        /// delete key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        private async Task<long> DeleteDataAsync(string key)
        {
            var response = await _cache.DeleteAsync(key, _metadata);
            return response.Deleted;
        }

        /// <summary>
        /// delete key
        /// </summary>
        /// <param name="prefixKey"></param>
        /// <returns></returns>
        private long DeleteRangeData(string prefixKey)
        {
            var response = _cache.Delete(prefixKey, _metadata);
            return response.Deleted;
        }

        /// <summary>
        /// delete key
        /// </summary>
        /// <param name="prefixKey"></param>
        /// <returns></returns>
        private async Task<long> DeleteRangeDataAsync(string prefixKey)
        {
            var response = await _cache.DeleteAsync(prefixKey, _metadata);
            return response.Deleted;
        }

        #endregion etcd method

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

            var result = GetVal<T>(cacheKey);
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

            if (!AddEphemeralData<string>($"{cacheKey}_Lock", "1", TimeSpan.FromMilliseconds(_options.LockMs)))
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
                    DeleteData($"{cacheKey}_Lock");

                    return new CacheValue<T>(res, true);
                }
                else
                {
                    //remove mutex key
                    DeleteData($"{cacheKey}_Lock");
                    return CacheValue<T>.NoValue;
                }
            }
            catch
            {
                //remove mutex key
                DeleteData($"{cacheKey}_Lock");
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

            var result = GetVal<T>(cacheKey);
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

            DeleteData(cacheKey);
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
            AddEphemeralData<T>(cacheKey, cacheValue, expiration);
        }

        /// <summary>
        /// Exists the specified cacheKey.
        /// </summary>
        /// <returns>The exists.</returns>
        /// <param name="cacheKey">Cache key.</param>
        public override bool BaseExists(string cacheKey)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            return GetDataExists(cacheKey);
        }

        /// <summary>
        /// Removes cached item by cachekey's prefix.
        /// </summary>
        /// <param name="prefix">Prefix.</param>
        public override void BaseRemoveByPrefix(string prefix)
        {
            ArgumentCheck.NotNullOrWhiteSpace(prefix, nameof(prefix));

            var count = DeleteRangeData(prefix);

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
                AddEphemeralData<T>(item.Key, item.Value, expiration);
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

            var dicData = GetRangeVals(prefix);
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

            var dicData = GetRangeVals(prefix);
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
                DeleteData(item);
            }
        }

        /// <summary>
        /// Gets the count.
        /// </summary>
        /// <returns>The count.</returns>
        /// <param name="prefix">Prefix.</param>
        public override int BaseGetCount(string prefix = "")
        {
            var dicData = GetRangeVals(prefix);
            return dicData != null ? dicData.Count : 0;
        }

        /// <summary>
        /// Flush All Cached Item.
        /// </summary>
        public override void BaseFlush()
        {
            if (_options.EnableLogging)
                _logger?.LogInformation("Flush");
            throw new NotSupportedException("BaseFlush is not supported in Etcd provider.");
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
            return AddEphemeralData<T>(cacheKey, cacheValue, expiration);
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

            _cache.Dispose();
            _disposed = true;
        }
    }
}