using dotnet_etcd;
using EasyCaching.Core;
using EasyCaching.Core.Serialization;
using Etcdserverpb;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using V3Lockpb;

namespace EasyCaching.Etcd
{
    public class EtcdCaching : IEtcdCaching
    {
        private readonly ILogger? _logger;
        private readonly IEasyCachingSerializer _serializer;
        private readonly EtcdCachingOptions _options;
        private readonly string _name;

        private readonly EtcdClient _etcdClient;
        private readonly string _authToken;
        private readonly Metadata _metadata;

        public EtcdCaching(
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
            this._etcdClient = new EtcdClient(connectionString: options.Address, configureChannelOptions: (x) =>
            {
                x.Credentials = ChannelCredentials.Insecure;
                x.LoggerFactory = loggerFactory;
            });
            //auth
            if (!string.IsNullOrEmpty(options.UserName) && !string.IsNullOrEmpty(options.Password))
            {
                var authRes = this._etcdClient.Authenticate(new AuthenticateRequest()
                {
                    Name = options.UserName,
                    Password = options.Password,
                });
                _authToken = authRes.Token;
                _metadata = new Metadata() { new Metadata.Entry("token", _authToken) };
            }

            var serName = !string.IsNullOrWhiteSpace(options.SerializerName) ? options.SerializerName : name;
            _serializer = serializers.FirstOrDefault(x => x.Name.Equals(serName)) ??
                          throw new EasyCachingNotFoundException(string.Format(
                              EasyCachingConstValue.NotFoundSerExceptionMessage,
                              serName));
        }

        public string ProviderName => this._name;

        #region etcd method

        /// <summary>
        /// get data
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <returns></returns>
        public CacheValue<T> Get<T>(string cacheKey)
        {
            var data = _etcdClient.GetVal(cacheKey, _metadata);
            return string.IsNullOrWhiteSpace(data)
                    ? CacheValue<T>.Null
                    : new CacheValue<T>(_serializer.Deserialize<T>(Encoding.UTF8.GetBytes(data)), true);
        }

        /// <summary>
        /// get data
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <returns></returns>
        public async Task<CacheValue<T>> GetAsync<T>(string cacheKey)
        {
            var data = await _etcdClient.GetValAsync(cacheKey, _metadata);
            return string.IsNullOrWhiteSpace(data)
                    ? CacheValue<T>.Null
                    : new CacheValue<T>(_serializer.Deserialize<T>(Encoding.UTF8.GetBytes(data)), true);
        }

        /// <summary>
        /// get rangevalues
        /// </summary>
        /// <param name="prefixKey"></param>
        /// <returns></returns>
        public IDictionary<string, string> GetAll(string prefixKey)
        {
            return _etcdClient.GetRangeVal(prefixKey, _metadata);
        }

        /// <summary>
        /// get rangevalues
        /// </summary>
        /// <param name="prefixKey"></param>
        /// <returns></returns>
        public async Task<IDictionary<string, string>> GetAllAsync(string prefixKey)
        {
            return await _etcdClient.GetRangeValAsync(prefixKey, _metadata);
        }

        /// <summary>
        ///  data exists
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <returns></returns>
        public bool Exists(string cacheKey)
        {
            var data = _etcdClient.GetVal(cacheKey, _metadata);
            return data == string.Empty ? false : true;
        }

        /// <summary>
        ///  data exists
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <returns></returns>
        public async Task<bool> ExistsAsync(string cacheKey)
        {
            var data = await _etcdClient.GetValAsync(cacheKey, _metadata);
            return data == string.Empty ? false : true;
        }

        /// <summary>
        /// get rent leaseId
        /// </summary>
        /// <param name="ts"></param>
        /// <returns></returns>
        private long GetRentLeaseId(TimeSpan? ts, CancellationTokenSource cts)
        {
            // create rent id to bind
            var response = _etcdClient.LeaseGrant(request: new LeaseGrantRequest()
            {
                TTL = (long)(ts.Value.TotalMilliseconds < 1000 ? 1: ts.Value.TotalMilliseconds / 1000),
            }, cancellationToken: cts.Token);
            return response.ID;
        }

        /// <summary>
        /// get rent leaseId
        /// </summary>
        /// <param name="ts"></param>
        /// <returns></returns>
        private async Task<long> GetRentLeaseIdAsync(TimeSpan? ts)
        {
            // create rent id to bind
            CancellationTokenSource cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(_options.Timeout));
            var response = await _etcdClient.LeaseGrantAsync(request: new LeaseGrantRequest()
            {
                TTL = (long)(ts.Value.TotalMilliseconds < 1000 ? 1 : ts.Value.TotalMilliseconds / 1000),
            }, deadline: DateTime.UtcNow.AddMilliseconds(_options.Timeout), cancellationToken: cts.Token);
            return response.ID;
        }

        /// <summary>
        /// put ke-val with leaseId
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="ts"></param>
        /// <returns></returns>
        public bool Set<T>(string key, T value, TimeSpan? ts)
        {
            try
            {
                CancellationTokenSource cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(_options.Timeout));
                long leaseId = ts.HasValue ? GetRentLeaseId(ts,cts) : 0;
                PutRequest request = new PutRequest()
                {
                    Key = ByteString.CopyFromUtf8(key),
                    Value = ByteString.CopyFrom(_serializer.Serialize(value)),
                    Lease = leaseId
                };
                var response = _etcdClient.Put(request: request, headers: _metadata, cancellationToken: cts.Token);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "putEphemeral(key:{},value:{}) error.", key, value);
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
        public async Task<bool> SetAsync<T>(string key, T value, TimeSpan? ts)
        {
            try
            {
                long leaseId = ts.HasValue ? await GetRentLeaseIdAsync(ts) : 0;
                CancellationTokenSource cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(_options.Timeout));
                PutRequest request = new PutRequest()
                {
                    Key = ByteString.CopyFromUtf8(key),
                    Value = ByteString.CopyFrom(_serializer.Serialize(value)),
                    Lease = leaseId
                };
                var response = await _etcdClient.PutAsync(request: request, headers: _metadata, cancellationToken: cts.Token);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,"putEphemeral(key:{},value:{}) error.",key,value);
            }
            return false;
        }

        /// <summary>
        /// Lock
        /// </summary>
        /// <param name="key"></param>
        /// <param name="ts"></param>
        /// <returns></returns>
        public bool Lock(string key, TimeSpan? ts)
        {
            try
            {
                CancellationTokenSource cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(_options.Timeout));
                long leaseId = ts.HasValue ? GetRentLeaseId(ts,cts) : 0;
                LockRequest request = new LockRequest()
                {
                    Name = ByteString.CopyFromUtf8(key),
                    Lease = leaseId
                };
                var response = _etcdClient.Lock(request: request, headers: _metadata, deadline: DateTime.UtcNow.AddMilliseconds(_options.Timeout), cancellationToken: cts.Token);
                if (response?.Key == null || response.Key.IsEmpty)
                {
                    return false;
                }
                return true;
            }
            catch (RpcException ex) when (ex.StatusCode == StatusCode.DeadlineExceeded)
            {
                _logger.LogError(ex, "Lock DeadlineExceeded (key:{}) error.", key);
            }
            catch (RpcException ex) when (ex.StatusCode == StatusCode.FailedPrecondition)
            {
                _logger.LogError(ex, "Lock FailedPrecondition (key:{}) error.", key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lock(key:{}) error.", key);
            }
            return false;
        }

        /// <summary>
        /// LockAsync
        /// </summary>
        /// <param name="key"></param>
        /// <param name="ts"></param>
        /// <returns></returns>
        public async Task<bool> LockAsync(string key, TimeSpan? ts)
        {
            try
            {
                CancellationTokenSource cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(_options.Timeout));
                long leaseId = ts.HasValue ? GetRentLeaseId(ts,cts) : 0;
                LockRequest request = new LockRequest()
                {
                    Name = ByteString.CopyFromUtf8(key),
                    Lease = leaseId
                };
                var response = await _etcdClient.LockAsync(request: request, headers: _metadata, deadline: DateTime.UtcNow.AddMilliseconds(_options.Timeout), cancellationToken: cts.Token);
                if (response?.Key == null || response.Key.IsEmpty)
                {
                    return false;
                }
                return true;
            }
            catch (RpcException ex) when (ex.StatusCode == StatusCode.DeadlineExceeded)
            {
                _logger.LogError(ex, "LockAsync DeadlineExceeded (key:{}) error.", key);
            }
            catch (RpcException ex) when (ex.StatusCode == StatusCode.FailedPrecondition)
            {
                _logger.LogError(ex, "LockAsync FailedPrecondition (key:{}) error.", key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "LockAsync(key:{}) error.", key);
            }
            return false;
        }

      /// <summary>
      /// UnLock
      /// releaseLock
      /// </summary>
      /// <param name="key"></param>
      /// <returns></returns>
        public bool UnLock(string key)
        {
            try
            {
                CancellationTokenSource cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(_options.Timeout));
                var response = _etcdClient.Unlock(key, headers: _metadata, deadline: DateTime.UtcNow.AddMilliseconds(_options.Timeout), cancellationToken: cts.Token);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "UnLock(key:{}) error.", key);
            }
            return false;
        }

        /// <summary>
        /// UnLockAsync
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public async Task<bool> UnLockAsnyc(string key)
        {
            try
            {
                CancellationTokenSource cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(_options.Timeout));
                var response = await _etcdClient.UnlockAsync(key, headers: _metadata, deadline: DateTime.UtcNow.AddMilliseconds(_options.Timeout), cancellationToken: cts.Token);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "UnLockAsync(key:{}) error.", key);
            }
            return false;
        }

        /// <summary>
        /// get key expireTTL
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public long GetExpireTTL(string key)
        {
            try
            {
                CancellationTokenSource cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(_options.Timeout));
                var rangeResponse = _etcdClient.GetRange(key, headers: _metadata, cancellationToken: cts.Token);
                if (rangeResponse != null && rangeResponse.Kvs != null && rangeResponse.Kvs.Count > 0)
                {
                    var leaseId = rangeResponse.Kvs[0].Lease;
                    var leaseTimeToLiveResponse = _etcdClient.LeaseTimeToLive(new LeaseTimeToLiveRequest
                    {
                        ID = leaseId,
                        Keys = true
                    });

                    var remainingTtlSeconds = leaseTimeToLiveResponse.TTL;
                    return remainingTtlSeconds;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetExpireMsTTL(key:{}) error.", key);
            }
            return 0;
        }

        /// <summary>
        /// get key expireTTL
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public async Task<long> GetExpireTTLAsync(string key)
        {
            try
            {
                CancellationTokenSource cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(_options.Timeout));
                var rangeResponse = await _etcdClient.GetRangeAsync(key, headers: _metadata, cancellationToken: cts.Token);
                if (rangeResponse != null && rangeResponse.Kvs != null && rangeResponse.Kvs.Count > 0)
                {
                    var leaseId = rangeResponse.Kvs[0].Lease;
                    var leaseTimeToLiveResponse = await _etcdClient.LeaseTimeToLiveAsync(new LeaseTimeToLiveRequest
                    {
                        ID = leaseId,
                        Keys = true
                    });

                    var remainingTtlSeconds = leaseTimeToLiveResponse.TTL;
                    return remainingTtlSeconds ;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetExpireMsTTLAsync(key:{}) error.", key);
            }
            return 0;
        }

        /// <summary>
        /// delete key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public long Delete(string key)
        {
            var response = _etcdClient.Delete(key, _metadata);
            return response.Deleted;
        }

        /// <summary>
        /// delete key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public async Task<long> DeleteAsync(string key)
        {
            var response = await _etcdClient.DeleteAsync(key, _metadata);
            return response.Deleted;
        }

        /// <summary>
        /// delete range key
        /// </summary>
        /// <param name="prefixKey"></param>
        /// <returns></returns>
        public long DeleteRangeData(string prefixKey)
        {
            var response = _etcdClient.DeleteRange(prefixKey, _metadata);
            return response.Deleted;
        }

        /// <summary>
        /// delete range key
        /// </summary>
        /// <param name="prefixKey"></param>
        /// <returns></returns>
        public async Task<long> DeleteRangeDataAsync(string prefixKey)
        {
            var response = await _etcdClient.DeleteRangeAsync(prefixKey, _metadata);
            return response.Deleted;
        }

        #endregion etcd method
    }
}