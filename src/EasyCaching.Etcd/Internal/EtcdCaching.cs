using dotnet_etcd;
using EasyCaching.Core;
using EasyCaching.Core.Serialization;
using Etcdserverpb;
using Google.Protobuf;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EasyCaching.Etcd
{
    public class EtcdCaching : IEtcdCaching
    {
        // com
        private readonly ILogger? _logger;

        private readonly IEasyCachingSerializer _serializer;
        private readonly EtcdCachingOptions _options;
        private readonly string _name;

        private readonly EtcdClient _cache;
        private readonly string _authToken;
        private readonly Grpc.Core.Metadata _metadata;

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
            this._cache = new EtcdClient(connectionString: options.Address, configureChannelOptions: (x) =>
            {
                x.Credentials = ChannelCredentials.Insecure;
            });
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
        public async Task<CacheValue<T>> GetAsync<T>(string cacheKey)
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
        public IDictionary<string, string> GetAll(string prefixKey)
        {
            return _cache.GetRangeVal(prefixKey, _metadata);
        }

        /// <summary>
        /// get rangevalues
        /// </summary>
        /// <param name="prefixKey"></param>
        /// <returns></returns>
        public async Task<IDictionary<string, string>> GetAllAsync(string prefixKey)
        {
            return await _cache.GetRangeValAsync(prefixKey, _metadata);
        }

        /// <summary>
        ///  data exists
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <returns></returns>
        public bool Exists(string cacheKey)
        {
            var data = _cache.GetVal(cacheKey, _metadata);
            return data == string.Empty ? false : true;
        }

        /// <summary>
        ///  data exists
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <returns></returns>
        public async Task<bool> ExistsAsync(string cacheKey)
        {
            var data = await _cache.GetValAsync(cacheKey, _metadata);
            return data == string.Empty ? false : true;
        }

        /// <summary>
        /// get rent leaseId
        /// </summary>
        /// <param name="ts"></param>
        /// <returns></returns>
        private long GetRentLeaseId(TimeSpan? ts)
        {
            // create rent id to bind
            CancellationTokenSource cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(_options.Timeout));
            var response = _cache.LeaseGrant(request: new LeaseGrantRequest()
            {
                TTL = (long)ts.Value.TotalMilliseconds,
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
            var response = await _cache.LeaseGrantAsync(request: new LeaseGrantRequest()
            {
                TTL = (long)ts.Value.TotalMilliseconds,
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
        public bool Set<T>(string key, T value, TimeSpan? ts)
        {
            try
            {
                long leaseId = ts.HasValue ? GetRentLeaseId(ts) : 0;
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
                var response = await _cache.PutAsync(request: request, headers: _metadata, cancellationToken: cts.Token);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,"putEphemeral(key:{},value:{}) error.",key,value);
            }
            return false;
        }

        /// <summary>
        /// delete key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public long Delete(string key)
        {
            var response = _cache.Delete(key, _metadata);
            return response.Deleted;
        }

        /// <summary>
        /// delete key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public async Task<long> DeleteAsync(string key)
        {
            var response = await _cache.DeleteAsync(key, _metadata);
            return response.Deleted;
        }

        /// <summary>
        /// delete range key
        /// </summary>
        /// <param name="prefixKey"></param>
        /// <returns></returns>
        public long DeleteRangeData(string prefixKey)
        {
            var response = _cache.DeleteRange(prefixKey, _metadata);
            return response.Deleted;
        }

        /// <summary>
        /// delete range key
        /// </summary>
        /// <param name="prefixKey"></param>
        /// <returns></returns>
        public async Task<long> DeleteRangeDataAsync(string prefixKey)
        {
            var response = await _cache.DeleteRangeAsync(prefixKey, _metadata);
            return response.Deleted;
        }

        #endregion etcd method
    }
}