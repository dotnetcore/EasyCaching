using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EasyCaching.Core;
using Microsoft.Extensions.Logging;

namespace EasyCaching.FasterKv
{
    public partial class DefaultFasterKvCachingProvider
    {
        public override async Task<bool> BaseExistsAsync(string cacheKey, CancellationToken cancellationToken = default)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));
            using var session = GetSession();
            var result = (await session.Session.ReadAsync(GetSpanByte(cacheKey), token: cancellationToken)).Complete();
            return result.status.Found;
        }


        public override async Task BaseFlushAsync(CancellationToken cancellationToken = default)
        {
            EnsureNotDispose();
            using var session = GetSession();
            using var iter = session.Session.Iterate();
            while (iter.GetNext(out var recordInfo))
            {
                await session.Session.DeleteAsync(ref iter.GetKey(), token: cancellationToken).ConfigureAwait(false);
            }
        }  


        public override async Task<IDictionary<string, CacheValue<T>>> BaseGetAllAsync<T>(IEnumerable<string> cacheKeys,
            CancellationToken cancellationToken = default)
        {
            ArgumentCheck.NotNullAndCountGTZero(cacheKeys, nameof(cacheKeys));
            EnsureNotDispose();
            
            using var session = GetSession();
            var dic = new Dictionary<string, CacheValue<T>>();
            foreach (var cacheKey in cacheKeys)
            {
                dic[cacheKey] = await BaseGetInternalAsync<T>(session, cacheKey, cancellationToken);
            }

            return dic;
        }

        public override async Task<CacheValue<T>> BaseGetAsync<T>(string cacheKey, Func<Task<T>> dataRetriever,
            TimeSpan expiration,
            CancellationToken cancellationToken = default)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));
            ArgumentCheck.NotNegativeOrZero(expiration, nameof(expiration));
            EnsureNotDispose();
            
            using var session = GetSession();
            var result = await BaseGetInternalAsync<T>(session, cacheKey, cancellationToken);
            if (result.HasValue)
            {
                return result;
            }
            
            var item = await dataRetriever();
            if (item is not null || _options.CacheNulls)
            {
                await SetAsync(cacheKey, item, expiration, cancellationToken);
                return new CacheValue<T>(item, true);
            }

            return CacheValue<T>.NoValue;
        }

        public override async Task<object> BaseGetAsync(string cacheKey, Type type,
            CancellationToken cancellationToken = default)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));
            EnsureNotDispose();
            
            using var session = GetSession();
            var result = await BaseGetAsync<object>(cacheKey, cancellationToken);
            return Convert.ChangeType(result.Value, type);
        }

        public override async Task<CacheValue<T>> BaseGetAsync<T>(string cacheKey,
            CancellationToken cancellationToken = default)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));
            EnsureNotDispose();
            
            using var session = GetSession();
            return await BaseGetInternalAsync<T>(session, cacheKey, cancellationToken);
        }

        public override async Task BaseRemoveAllAsync(IEnumerable<string> cacheKeys, CancellationToken cancellation = default)
        {
            ArgumentCheck.NotNullAndCountGTZero(cacheKeys, nameof(cacheKeys));
            EnsureNotDispose();

            using var session = GetSession();
            foreach (var cacheKey in cacheKeys)
            {
                await session.Session.DeleteAsync(GetSpanByte(cacheKey), token: cancellation).ConfigureAwait(false);
            }
        }

        public override async Task BaseRemoveAsync(string cacheKey, CancellationToken cancellationToken = default)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));
            EnsureNotDispose();
            
            using var session = GetSession();
            await session.Session.DeleteAsync(GetSpanByte(cacheKey), token: cancellationToken).ConfigureAwait(false);
        }


        public override async Task BaseSetAllAsync<T>(IDictionary<string, T> values, TimeSpan expiration,
            CancellationToken cancellationToken = default)
        {
            ArgumentCheck.NotNegativeOrZero(expiration, nameof(expiration));
            ArgumentCheck.NotNullAndCountGTZero(values, nameof(values));
            EnsureNotDispose();

            using var session = GetSession();
            foreach (var kp in values)
            {
                await BaseSetInternalAsync<T>(session, kp.Key, kp.Value, cancellationToken);
            }
        }

        public override async Task BaseSetAsync<T>(string cacheKey, T cacheValue, TimeSpan expiration,
            CancellationToken cancellationToken = default)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));
            ArgumentCheck.NotNull(cacheValue, nameof(cacheValue), _options.CacheNulls);
            ArgumentCheck.NotNegativeOrZero(expiration, nameof(expiration));
            EnsureNotDispose();

            using var session = GetSession();
            await BaseSetInternalAsync(session, cacheKey, cacheValue, cancellationToken);
        }

        public override async Task<bool> BaseTrySetAsync<T>(string cacheKey, T cacheValue, TimeSpan expiration,
            CancellationToken cancellationToken = default)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));
            ArgumentCheck.NotNull(cacheValue, nameof(cacheValue), _options.CacheNulls);
            ArgumentCheck.NotNegativeOrZero(expiration, nameof(expiration));
            EnsureNotDispose();

            using var session = GetSession();
            var result = await BaseGetAsync<T>(cacheKey, cancellationToken);
            if (result.HasValue == false)
            {
                await BaseSetInternalAsync<T>(session, cacheKey, cacheValue, cancellationToken);
                return true;
            }
            
            return false;
        }
        
        private async Task BaseSetInternalAsync<T>(ClientSessionWrap sessionWarp, string cacheKey, T cacheValue,
            CancellationToken cancellationToken)
        {
            _ = await (await sessionWarp.Session.UpsertAsync(GetSpanByte(cacheKey),
                    GetSpanByte(cacheValue), token: cancellationToken)
                .ConfigureAwait(false)).CompleteAsync(cancellationToken);
        }


        private async Task<CacheValue<T>> BaseGetInternalAsync<T>(ClientSessionWrap session, string cacheKey, CancellationToken cancellationToken)
        {
            var result = (await session.Session.ReadAsync(GetSpanByte(cacheKey),
                    token: cancellationToken)
                .ConfigureAwait(false)).Complete();
            if (result.status.Found)
            {
                if (_options.EnableLogging)
                    _logger?.LogInformation("Cache Hit : cacheKey = {CacheKey}", cacheKey);

                CacheStats.OnHit();

                return new CacheValue<T>(GetTValue<T>(ref result.output), true);
            }

            CacheStats.OnMiss();
            if (_options.EnableLogging)
                _logger?.LogInformation("Cache Missed : cacheKey = {CacheKey}", cacheKey);

            return CacheValue<T>.NoValue;
        }

        // Operations not currently supported by FasterKv
        #region NotSupprotOperate
        public override Task<IDictionary<string, CacheValue<T>>> BaseGetByPrefixAsync<T>(string prefix,
            CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException("BaseGetByPrefixAsync is not supported in FasterKv provider.");
        }
        
        public override Task<int> BaseGetCountAsync(string prefix = "", CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException("BaseGetCountAsync is not supported in FasterKv provider.");
        }
        
        public override Task BaseRemoveByPrefixAsync(string prefix, CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException("BaseRemoveByPrefixAsync is not supported in FasterKv provider.");
        }
        
        public override Task BaseRemoveByPatternAsync(string pattern, CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException("BaseRemoveByPatternAsync is not supported in FasterKv provider.");
        }
        
        public override Task<TimeSpan> BaseGetExpirationAsync(string cacheKey,
            CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException("BaseGetExpirationAsync is not supported in FasterKv provider.");
        }
        #endregion
    }
}