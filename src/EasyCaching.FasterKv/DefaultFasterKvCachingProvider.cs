using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using EasyCaching.Core;
using EasyCaching.Core.Serialization;
using EasyCaching.FasterKv.Configurations;
using FASTER.core;
using Microsoft.Extensions.Logging;

namespace EasyCaching.FasterKv
{
    public sealed partial class DefaultFasterKvCachingProvider : EasyCachingAbstractProvider, IDisposable
    {
        // name
        private readonly string _name;
        private bool _disposed;

        // com
        private readonly ILogger? _logger;
        private readonly IEasyCachingSerializer _serializer;
        private readonly FasterKvCachingOptions _options;

        // faster
        private readonly FasterKV<SpanByte, SpanByte> _fasterKv;
        private readonly IDevice? _logDevice;
        private readonly
            ConcurrentQueue<ClientSession<
                SpanByte, SpanByte, SpanByte, SpanByteAndMemory, StoreContext, StoreFunctions>> _sessionPool;

        /// <summary>
        /// The cache stats.
        /// </summary>
        private readonly CacheStats _cacheStats;

        private readonly ProviderInfo _info;

        ~DefaultFasterKvCachingProvider()
        {
            Dispose(false);
        }

        public DefaultFasterKvCachingProvider(
            string name,
            FasterKvCachingOptions options,
            IEnumerable<IEasyCachingSerializer> serializers,
            ILoggerFactory? loggerFactory = null)
        {
            ArgumentCheck.NotNull(options, nameof(options));
            ArgumentCheck.NotNull(serializers, nameof(serializers));

            _name = name;

            _options = options;
            _logger = loggerFactory?.CreateLogger<DefaultFasterKvCachingProvider>();

            if (_options.CustomStore is null)
            {
                var logSetting = options.GetLogSettings(name);
                _logDevice = logSetting.LogDevice;
                _fasterKv = new FasterKV<SpanByte, SpanByte>(_options.IndexCount, logSetting,
                    loggerFactory: loggerFactory);
            }
            else
            {
                _fasterKv = _options.CustomStore;
            }

            _sessionPool =
                new ConcurrentQueue<ClientSession<SpanByte, SpanByte, SpanByte, SpanByteAndMemory, StoreContext,
                    StoreFunctions>>();

            var serName = !string.IsNullOrWhiteSpace(options.SerializerName) ? options.SerializerName : name;
            _serializer = serializers.FirstOrDefault(x => x.Name.Equals(serName)) ??
                          throw new EasyCachingNotFoundException(string.Format(
                              EasyCachingConstValue.NotFoundSerExceptionMessage,
                              serName));

            _cacheStats = new CacheStats();
            ProviderName = _name;
            ProviderType = CachingProviderType.FasterKv;
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

        public override object BaseGetDatabase()
        {
            EnsureNotDispose();
            return _fasterKv;
        }

        public override bool BaseExists(string cacheKey)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));
            EnsureNotDispose();
            
            using var sessionWarp = GetSession();
            var key = GetSpanByte(cacheKey);
            var result = sessionWarp.Session.Read(key);
            if (result.status.IsPending)
                sessionWarp.Session.CompletePending(true);
            return result.status.Found;
        }


        public override void BaseFlush()
        {
            EnsureNotDispose();
            using var session = GetSession();
            using var iter = session.Session.Iterate();
            while (iter.GetNext(out var recordInfo))
            {
                session.Session.Delete(ref iter.GetKey());
            }
        }


        public override CacheValue<T> BaseGet<T>(string cacheKey, Func<T> dataRetriever, TimeSpan expiration)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));
            ArgumentCheck.NotNegativeOrZero(expiration, nameof(expiration));
            EnsureNotDispose();

            using var session = GetSession();
            var result = BaseGetInternal<T>(cacheKey, session);
            if (result.HasValue)
            {
                return result;
            }

            var item = dataRetriever();
            if (item is not null || _options.CacheNulls)
            {
                Set(cacheKey, item, expiration);
                return new CacheValue<T>(item, true);
            }

            return CacheValue<T>.NoValue;
        }

        public override CacheValue<T> BaseGet<T>(string cacheKey)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));
            EnsureNotDispose();
            
            using var session = GetSession();
            return BaseGetInternal<T>(cacheKey, session);
        }

        public override IDictionary<string, CacheValue<T>> BaseGetAll<T>(IEnumerable<string> cacheKeys)
        {
            ArgumentCheck.NotNullAndCountGTZero(cacheKeys, nameof(cacheKeys));
            EnsureNotDispose();
            
            using var session = GetSession();
            var dic = new Dictionary<string, CacheValue<T>>();
            foreach (var cacheKey in cacheKeys)
            {
                dic[cacheKey] = BaseGetInternal<T>(cacheKey, session);
            }

            return dic;
        }


        public override void BaseRemove(string cacheKey)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));
            EnsureNotDispose();
            
            using var session = GetSession();
            // ignore result
            _ = session.Session.Delete(GetSpanByte(cacheKey));
        }

        public override void BaseRemoveAll(IEnumerable<string> cacheKeys)
        {
            ArgumentCheck.NotNullAndCountGTZero(cacheKeys, nameof(cacheKeys));
            EnsureNotDispose();
            
            using var session = GetSession();
            foreach (var cacheKey in cacheKeys)
            {
                _ = session.Session.Delete(GetSpanByte(cacheKey));
            }
        }


        public override void BaseSet<T>(string cacheKey, T cacheValue, TimeSpan expiration)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));
            ArgumentCheck.NotNull(cacheValue, nameof(cacheValue), _options.CacheNulls);
            ArgumentCheck.NotNegativeOrZero(expiration, nameof(expiration));
            
            EnsureNotDispose();
            
            using var sessionWarp = GetSession();
            BaseSetInternal(sessionWarp, cacheKey, cacheValue);
        }

        public override void BaseSetAll<T>(IDictionary<string, T> values, TimeSpan expiration)
        {
            ArgumentCheck.NotNegativeOrZero(expiration, nameof(expiration));
            ArgumentCheck.NotNullAndCountGTZero(values, nameof(values));

            using var session = GetSession();
            foreach (var kp in values)
            {
                BaseSetInternal(session, kp.Key, kp.Value);
            }
        }

        public override bool BaseTrySet<T>(string cacheKey, T cacheValue, TimeSpan expiration)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));
            ArgumentCheck.NotNull(cacheValue, nameof(cacheValue), _options.CacheNulls);
            ArgumentCheck.NotNegativeOrZero(expiration, nameof(expiration));
            EnsureNotDispose();

            using var session = GetSession();
            var result = BaseGet<T>(cacheKey);
            if (result.HasValue == false)
            {
                BaseSetInternal(session, cacheKey, cacheValue);
                return true;
            }
            
            return false;
        }


        public override ProviderInfo BaseGetProviderInfo()
        {
            return _info;
        }

        
        

        private CacheValue<T> BaseGetInternal<T>(string cacheKey, ClientSessionWrap session)
        {
            var context = new StoreContext();
            var result = session.Session.Read(GetSpanByte(cacheKey), context);
            if (result.status.IsPending)
            {
                session.Session.CompletePending(true);
                context.FinalizeRead(out result.status, out result.output);
            }

            if (result.status.Found)
            {
                CacheStats.OnHit();
                if (_options.EnableLogging)
                    _logger?.LogInformation("Cache Hit : cacheKey = {CacheKey}", cacheKey);
                var value = GetTValue<T>(ref result.output);
                return new CacheValue<T>(value, true);
            }

            CacheStats.OnMiss();
            if (_options.EnableLogging)
                _logger?.LogInformation("Cache Missed : cacheKey = {CacheKey}", cacheKey);

            return CacheValue<T>.NoValue;
        }

        private void BaseSetInternal<T>(ClientSessionWrap sessionWarp, string cacheKey, T cacheValue)
        {
            var key = GetSpanByte(cacheKey);
            var value = GetSpanByte(cacheValue);
            _ = sessionWarp.Session.Upsert(key, value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private SpanByte GetSpanByte<T>(T value)
        {
            var bytes = _serializer.Serialize(value);
            bytes.AsSpan().GetPinnableReference();
            return SpanByte.FromFixedSpan(bytes);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private T GetTValue<T>(ref SpanByteAndMemory span)
        {
            return _serializer.Deserialize<T>(span.Memory.Memory.ToArray());
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void EnsureNotDispose()
        {
            if (_disposed) throw new ObjectDisposedException(nameof(DefaultFasterKvCachingProvider));
        }

        /// <summary>
        /// Get ClientSession from pool
        /// </summary>
        /// <returns></returns>
        private ClientSessionWrap GetSession()
        {
            if (_sessionPool.TryDequeue(out var session) == false)
            {
                session = _fasterKv.For(new StoreFunctions()).NewSession<StoreFunctions>();
            }

            return new ClientSessionWrap(session, _sessionPool);
        }

        // Operations not currently supported by FasterKv
        #region NotSupprotOperate

        public override TimeSpan BaseGetExpiration(string cacheKey)
        {
            throw new NotSupportedException("BaseGetExpiration is not supported in FasterKv provider.");
        }

        public override IDictionary<string, CacheValue<T>> BaseGetByPrefix<T>(string prefix)
        {
            throw new NotSupportedException("BaseGetByPrefix is not supported in FasterKv provider.");
        }

        public override int BaseGetCount(string prefix = "")
        {
            throw new NotSupportedException("BaseGetCount is not supported in FasterKv provider.");
        }

        public override void BaseRemoveByPrefix(string prefix)
        {
            throw new NotSupportedException("BaseRemoveByPrefix is not supported in FasterKv provider.");
        }

        public override void BaseRemoveByPattern(string pattern)
        {
            throw new NotSupportedException("BaseRemoveByPattern is not supported in FasterKv provider.");
        }

        #endregion

        private void Dispose(bool _)
        {
            if (_disposed)
                return;
            
            foreach (var session in _sessionPool)
            {
                session.Dispose();
            }

            _logDevice?.Dispose();
            if (_options.CustomStore != _fasterKv)
            {
                _fasterKv.Dispose();   
            }
            _disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}