using EasyCaching.Core.DistributedLock;

namespace EasyCaching.Core
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using EasyCaching.Core.Configurations;
    using EasyCaching.Core.Diagnostics;

    public abstract class EasyCachingAbstractProvider : IEasyCachingProvider
    {
        protected static readonly DiagnosticListener s_diagnosticListener =
                    new DiagnosticListener(EasyCachingDiagnosticListenerExtensions.DiagnosticListenerName);

        private readonly IDistributedLockFactory _lockFactory;
        private readonly BaseProviderOptions _options;

        protected string ProviderName { get; set; }
        protected bool IsDistributedProvider { get; set; }
        protected int ProviderMaxRdSecond { get; set; }
        protected CachingProviderType ProviderType { get; set; }
        protected CacheStats ProviderStats { get; set; }

        public string Name => this.ProviderName;
        public bool IsDistributedCache => this.IsDistributedProvider;
        public bool UseLock => _lockFactory != null;
        public int MaxRdSecond => this.ProviderMaxRdSecond;
        public CachingProviderType CachingProviderType => this.ProviderType;
        public CacheStats CacheStats => this.ProviderStats;

        public object Database => BaseGetDatabase();

        protected EasyCachingAbstractProvider() { }

        protected EasyCachingAbstractProvider(IDistributedLockFactory lockFactory, BaseProviderOptions options)
        {
            _lockFactory = lockFactory;
            _options = options;
        }

        public abstract object BaseGetDatabase();
        public abstract bool BaseExists(string cacheKey);
        public abstract Task<bool> BaseExistsAsync(string cacheKey, CancellationToken cancellationToken = default);
        public abstract void BaseFlush();
        public abstract Task BaseFlushAsync(CancellationToken cancellationToken = default);
        public abstract CacheValue<T> BaseGet<T>(string cacheKey, Func<T> dataRetriever, TimeSpan expiration);
        public abstract CacheValue<T> BaseGet<T>(string cacheKey);
        public abstract IEnumerable<string> BaseGetAllKeysByPrefix(string prefix);
        public abstract Task<IEnumerable<string>> BaseGetAllKeysByPrefixAsync(string prefix, CancellationToken cancellationToken = default);
        public abstract IDictionary<string, CacheValue<T>> BaseGetAll<T>(IEnumerable<string> cacheKeys);
        public abstract Task<IDictionary<string, CacheValue<T>>> BaseGetAllAsync<T>(IEnumerable<string> cacheKeys, CancellationToken cancellationToken = default);
        public abstract Task<CacheValue<T>> BaseGetAsync<T>(string cacheKey, Func<Task<T>> dataRetriever, TimeSpan expiration, CancellationToken cancellationToken = default);
        public abstract Task<object> BaseGetAsync(string cacheKey, Type type, CancellationToken cancellationToken = default);
        public abstract Task<CacheValue<T>> BaseGetAsync<T>(string cacheKey, CancellationToken cancellationToken = default);
        public abstract IDictionary<string, CacheValue<T>> BaseGetByPrefix<T>(string prefix);
        public abstract Task<IDictionary<string, CacheValue<T>>> BaseGetByPrefixAsync<T>(string prefix, CancellationToken cancellationToken = default);
        public abstract int BaseGetCount(string prefix = "");
        public abstract Task<int> BaseGetCountAsync(string prefix = "", CancellationToken cancellationToken = default);
        public abstract void BaseRemove(string cacheKey);
        public abstract void BaseRemoveAll(IEnumerable<string> cacheKeys);
        public abstract Task BaseRemoveAllAsync(IEnumerable<string> cacheKeys, CancellationToken cancellation = default);
        public abstract Task BaseRemoveAsync(string cacheKey, CancellationToken cancellationToken = default);
        public abstract void BaseRemoveByPrefix(string prefix);
        public abstract Task BaseRemoveByPrefixAsync(string prefix, CancellationToken cancellationToken = default);
        public abstract void BaseRemoveByPattern(string pattern);
        public abstract Task BaseRemoveByPatternAsync(string pattern, CancellationToken cancellationToken = default);
        public abstract void BaseSet<T>(string cacheKey, T cacheValue, TimeSpan expiration);
        public abstract void BaseSetAll<T>(IDictionary<string, T> values, TimeSpan expiration);
        public abstract Task BaseSetAllAsync<T>(IDictionary<string, T> values, TimeSpan expiration, CancellationToken cancellationToken = default);
        public abstract Task BaseSetAsync<T>(string cacheKey, T cacheValue, TimeSpan expiration, CancellationToken cancellationToken = default);
        public abstract bool BaseTrySet<T>(string cacheKey, T cacheValue, TimeSpan expiration);
        public abstract Task<bool> BaseTrySetAsync<T>(string cacheKey, T cacheValue, TimeSpan expiration, CancellationToken cancellationToken = default);

        public abstract TimeSpan BaseGetExpiration(string cacheKey);
        public abstract Task<TimeSpan> BaseGetExpirationAsync(string cacheKey, CancellationToken cancellationToken = default);
        public abstract ProviderInfo BaseGetProviderInfo();

        public bool Exists(string cacheKey)
        {
            var operationId = s_diagnosticListener.WriteExistsCacheBefore(new BeforeExistsRequestEventData(CachingProviderType.ToString(), Name, nameof(Exists), cacheKey));
            bool result;
            try
            {
                result = BaseExists(cacheKey);
            }
            catch (Exception ex)
            {
                s_diagnosticListener.WriteExistsCacheError(operationId, ex);
                throw;
            }
            s_diagnosticListener.WriteExistsCacheAfter(operationId);
            return result;
        }

        public async Task<bool> ExistsAsync(string cacheKey, CancellationToken cancellationToken = default)
        {
            var operationId = s_diagnosticListener.WriteExistsCacheBefore(new BeforeExistsRequestEventData(CachingProviderType.ToString(), Name, nameof(ExistsAsync), cacheKey));
            bool result;
            try
            {
                result = await BaseExistsAsync(cacheKey, cancellationToken);
            }
            catch (Exception ex)
            {
                s_diagnosticListener.WriteExistsCacheError(operationId, ex);
                throw;
            }
            s_diagnosticListener.WriteExistsCacheAfter(operationId);
            return result;
        }

        public void Flush()
        {
            var operationId = s_diagnosticListener.WriteFlushCacheBefore(new EventData(CachingProviderType.ToString(), Name, nameof(Flush)));
            try
            {
                BaseFlush();
            }
            catch (Exception ex)
            {
                s_diagnosticListener.WriteFlushCacheError(operationId, ex);
                throw;
            }
            s_diagnosticListener.WriteFlushCacheAfter(operationId);
        }

        public async Task FlushAsync(CancellationToken cancellationToken = default)
        {
            var operationId = s_diagnosticListener.WriteFlushCacheBefore(new EventData(CachingProviderType.ToString(), Name, nameof(FlushAsync)));
            try
            {
                await BaseFlushAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                s_diagnosticListener.WriteFlushCacheError(operationId, ex);
                throw;
            }
            s_diagnosticListener.WriteFlushCacheAfter(operationId);
        }

        public CacheValue<T> Get<T>(string cacheKey, Func<T> dataRetriever, TimeSpan expiration)
        {
            var operationId = s_diagnosticListener.WriteGetCacheBefore(new BeforeGetRequestEventData(CachingProviderType.ToString(), Name, nameof(Get), new[] { cacheKey }, expiration));
            bool doNotEnterFinally = false;
            try
            {
                if (_lockFactory == null) return BaseGet<T>(cacheKey, dataRetriever, expiration);

                var value = BaseGet<T>(cacheKey);
                if (value.HasValue) return value;

                using (var @lock = _lockFactory.CreateLock(Name, $"{cacheKey}_Lock"))
                {
                    if (!@lock.Lock(_options.SleepMs)) throw new TimeoutException();

                    value = BaseGet<T>(cacheKey);
                    if (value.HasValue) return value;

                    var item = dataRetriever();
                    if (item != null || _options.CacheNulls)
                    {
                        BaseSet(cacheKey, item, expiration);

                        return new CacheValue<T>(item, true);
                    }
                    else
                    {
                        return CacheValue<T>.NoValue;
                    }
                }
            }
            catch (Exception ex)
            {
                doNotEnterFinally = true;
                s_diagnosticListener.WriteGetCacheError(operationId, ex);
                throw;
            }
            finally
            {
                if (!doNotEnterFinally)
                {
                    s_diagnosticListener.WriteGetCacheAfter(operationId);
                }
            }
        }

        public CacheValue<T> Get<T>(string cacheKey)
        {
            var operationId = s_diagnosticListener.WriteGetCacheBefore(new BeforeGetRequestEventData(CachingProviderType.ToString(), Name, nameof(Get), new[] { cacheKey }));
            CacheValue<T> result;
            try
            {
                result = BaseGet<T>(cacheKey);
            }
            catch (Exception ex)
            {
                s_diagnosticListener.WriteGetCacheError(operationId, ex);
                throw;
            }
            s_diagnosticListener.WriteGetCacheAfter(operationId);
            return result;
        }

        public IEnumerable<string> GetAllKeysByPrefix(string prefix)
        {
            var operationId = s_diagnosticListener.WriteGetCacheBefore(new BeforeGetRequestEventData(CachingProviderType.ToString(), Name, nameof(GetAllKeysByPrefix), new[] { prefix }));
            IEnumerable<string> result;
            try
            {
                result = BaseGetAllKeysByPrefix(prefix);
            }
            catch (Exception ex)
            {
                s_diagnosticListener.WriteGetCacheError(operationId, ex);
                throw;
            }
            s_diagnosticListener.WriteGetCacheAfter(operationId);
            return result;
        }

        public async Task<IEnumerable<string>> GetAllKeysByPrefixAsync(string prefix, CancellationToken cancellationToken = default)
        {
            var operationId = s_diagnosticListener.WriteGetCacheBefore(new BeforeGetRequestEventData(CachingProviderType.ToString(), Name, nameof(GetAllKeysByPrefixAsync), new[] { prefix }));
            IEnumerable<string> result;
            try
            {
                result = await BaseGetAllKeysByPrefixAsync(prefix);
            }
            catch (Exception ex)
            {
                s_diagnosticListener.WriteGetCacheError(operationId, ex);
                throw;
            }
            s_diagnosticListener.WriteGetCacheAfter(operationId);
            return result;
        }

        public IDictionary<string, CacheValue<T>> GetAll<T>(IEnumerable<string> cacheKeys)
        {
            var operationId = s_diagnosticListener.WriteGetCacheBefore(new BeforeGetRequestEventData(CachingProviderType.ToString(), Name, nameof(GetAll), cacheKeys.ToArray()));
            IDictionary<string, CacheValue<T>> result;
            try
            {
                result = BaseGetAll<T>(cacheKeys);
            }
            catch (Exception ex)
            {
                s_diagnosticListener.WriteGetCacheError(operationId, ex);
                throw;
            }
            s_diagnosticListener.WriteGetCacheAfter(operationId);
            return result;
        }

        public async Task<IDictionary<string, CacheValue<T>>> GetAllAsync<T>(IEnumerable<string> cacheKeys, CancellationToken cancellationToken = default)
        {
            var operationId = s_diagnosticListener.WriteGetCacheBefore(new BeforeGetRequestEventData(CachingProviderType.ToString(), Name, nameof(GetAllAsync), cacheKeys.ToArray()));
            IDictionary<string, CacheValue<T>> result;
            try
            {
                result = await BaseGetAllAsync<T>(cacheKeys, cancellationToken);
            }
            catch (Exception ex)
            {
                s_diagnosticListener.WriteGetCacheError(operationId, ex);
                throw;
            }
            s_diagnosticListener.WriteGetCacheAfter(operationId);
            return result;
        }

        public async Task<CacheValue<T>> GetAsync<T>(string cacheKey, Func<Task<T>> dataRetriever, TimeSpan expiration, CancellationToken cancellationToken = default)
        {
            var operationId = s_diagnosticListener.WriteGetCacheBefore(new BeforeGetRequestEventData(CachingProviderType.ToString(), Name, nameof(GetAsync), new[] { cacheKey }, expiration));
            bool doNotEnterFinally = false;
            try
            {
                if (_lockFactory == null) return await BaseGetAsync<T>(cacheKey, dataRetriever, expiration, cancellationToken);

                var value = await BaseGetAsync<T>(cacheKey);
                if (value.HasValue) return value;

                var @lock = _lockFactory.CreateLock(Name, $"{cacheKey}_Lock");
                try
                {
                    if (!await @lock.LockAsync(_options.SleepMs)) throw new TimeoutException();

                    value = await BaseGetAsync<T>(cacheKey, cancellationToken);
                    if (value.HasValue) return value;

                    var task = dataRetriever();
                    if (!task.IsCompleted &&
                        await Task.WhenAny(task, Task.Delay(_options.LockMs)) != task)
                        throw new TimeoutException();

                    var item = await task;
                    if (item != null || _options.CacheNulls)
                    {
                        await BaseSetAsync(cacheKey, item, expiration, cancellationToken);

                        return new CacheValue<T>(item, true);
                    }
                    else
                    {
                        return CacheValue<T>.NoValue;
                    }
                }
                finally
                {
                    await @lock.DisposeAsync();
                }
            }
            catch (Exception ex)
            {
                doNotEnterFinally = true;
                s_diagnosticListener.WriteGetCacheError(operationId, ex);
                throw;
            }
            finally
            {
                if (!doNotEnterFinally)
                {
                    s_diagnosticListener.WriteGetCacheAfter(operationId);
                }
            }
        }

        public async Task<object> GetAsync(string cacheKey, Type type, CancellationToken cancellationToken = default)
        {
            var operationId = s_diagnosticListener.WriteGetCacheBefore(new BeforeGetRequestEventData(CachingProviderType.ToString(), Name, "GetAsync_Type", new[] { cacheKey }));
            object result;
            try
            {
                result = await BaseGetAsync(cacheKey, type, cancellationToken);
            }
            catch (Exception ex)
            {
                s_diagnosticListener.WriteGetCacheError(operationId, ex);
                throw;
            }
            s_diagnosticListener.WriteGetCacheAfter(operationId);
            return result;
        }

        public async Task<CacheValue<T>> GetAsync<T>(string cacheKey, CancellationToken cancellationToken = default)
        {
            var operationId = s_diagnosticListener.WriteGetCacheBefore(new BeforeGetRequestEventData(CachingProviderType.ToString(), Name, nameof(GetAsync), new[] { cacheKey }));
            CacheValue<T> result;
            try
            {
                result = await BaseGetAsync<T>(cacheKey, cancellationToken);
            }
            catch (Exception ex)
            {
                s_diagnosticListener.WriteGetCacheError(operationId, ex);
                throw;
            }
            s_diagnosticListener.WriteGetCacheAfter(operationId);
            return result;
        }

        public IDictionary<string, CacheValue<T>> GetByPrefix<T>(string prefix)
        {
            var operationId = s_diagnosticListener.WriteGetCacheBefore(new BeforeGetRequestEventData(CachingProviderType.ToString(), Name, nameof(GetByPrefix), new[] { prefix }));
            IDictionary<string, CacheValue<T>> result;
            try
            {
                result = BaseGetByPrefix<T>(prefix);
            }
            catch (Exception ex)
            {
                s_diagnosticListener.WriteGetCacheError(operationId, ex);
                throw;
            }
            s_diagnosticListener.WriteGetCacheAfter(operationId);
            return result;
        }

        public async Task<IDictionary<string, CacheValue<T>>> GetByPrefixAsync<T>(string prefix, CancellationToken cancellationToken = default)
        {
            var operationId = s_diagnosticListener.WriteGetCacheBefore(new BeforeGetRequestEventData(CachingProviderType.ToString(), Name, nameof(GetByPrefixAsync), new[] { prefix }));
            IDictionary<string, CacheValue<T>> result;
            try
            {
                result = await BaseGetByPrefixAsync<T>(prefix, cancellationToken);
            }
            catch (Exception ex)
            {
                s_diagnosticListener.WriteGetCacheError(operationId, ex);
                throw;
            }
            s_diagnosticListener.WriteGetCacheAfter(operationId);
            return result;
        }

        public int GetCount(string prefix = "")
        {
            return BaseGetCount(prefix);
        }

        public async Task<int> GetCountAsync(string prefix = "", CancellationToken cancellationToken = default)
        {
            return await BaseGetCountAsync(prefix, cancellationToken);
        }

        public void Remove(string cacheKey)
        {
            var operationId = s_diagnosticListener.WriteRemoveCacheBefore(new BeforeRemoveRequestEventData(CachingProviderType.ToString(), Name, nameof(Remove), new[] { cacheKey }));
            try
            {
                BaseRemove(cacheKey);
            }
            catch (Exception ex)
            {
                s_diagnosticListener.WriteRemoveCacheError(operationId, ex);
                throw;
            }
            s_diagnosticListener.WriteRemoveCacheAfter(operationId);
        }

        public void RemoveAll(IEnumerable<string> cacheKeys)
        {
            var operationId = s_diagnosticListener.WriteRemoveCacheBefore(new BeforeRemoveRequestEventData(CachingProviderType.ToString(), Name, nameof(RemoveAll), cacheKeys.ToArray()));
            try
            {
                BaseRemoveAll(cacheKeys);
            }
            catch (Exception ex)
            {
                s_diagnosticListener.WriteRemoveCacheError(operationId, ex);
                throw;
            }
            s_diagnosticListener.WriteRemoveCacheAfter(operationId);
        }

        public async Task RemoveAllAsync(IEnumerable<string> cacheKeys, CancellationToken cancellationToken = default)
        {
            var operationId = s_diagnosticListener.WriteRemoveCacheBefore(new BeforeRemoveRequestEventData(CachingProviderType.ToString(), Name, nameof(RemoveAllAsync), cacheKeys.ToArray()));
            try
            {
                await BaseRemoveAllAsync(cacheKeys, cancellationToken);
            }
            catch (Exception ex)
            {
                s_diagnosticListener.WriteRemoveCacheError(operationId, ex);
                throw;
            }
            s_diagnosticListener.WriteRemoveCacheAfter(operationId);
        }

        public async Task RemoveAsync(string cacheKey, CancellationToken cancellationToken = default)
        {
            var operationId = s_diagnosticListener.WriteRemoveCacheBefore(new BeforeRemoveRequestEventData(CachingProviderType.ToString(), Name, nameof(RemoveAsync), new[] { cacheKey }));
            try
            {
                await BaseRemoveAsync(cacheKey, cancellationToken);
            }
            catch (Exception ex)
            {
                s_diagnosticListener.WriteRemoveCacheError(operationId, ex);
                throw;
            }
            s_diagnosticListener.WriteRemoveCacheAfter(operationId);
        }

        public void RemoveByPrefix(string prefix)
        {
            var operationId = s_diagnosticListener.WriteRemoveCacheBefore(new BeforeRemoveRequestEventData(CachingProviderType.ToString(), Name, nameof(RemoveByPrefix), new[] { prefix }));
            try
            {
                BaseRemoveByPrefix(prefix);
            }
            catch (Exception ex)
            {
                s_diagnosticListener.WriteRemoveCacheError(operationId, ex);
                throw;
            }
            s_diagnosticListener.WriteRemoveCacheAfter(operationId);
        }

        public async Task RemoveByPrefixAsync(string prefix, CancellationToken cancellationToken = default)
        {
            var operationId = s_diagnosticListener.WriteRemoveCacheBefore(new BeforeRemoveRequestEventData(CachingProviderType.ToString(), Name, nameof(RemoveByPrefixAsync), new[] { prefix }));
            try
            {
                await BaseRemoveByPrefixAsync(prefix, cancellationToken);
            }
            catch (Exception ex)
            {
                s_diagnosticListener.WriteRemoveCacheError(operationId, ex);
                throw;
            }
            s_diagnosticListener.WriteRemoveCacheAfter(operationId);
        }

        public void RemoveByPattern(string pattern)
        {
            var operationId = s_diagnosticListener.WriteRemoveCacheBefore(
                new BeforeRemoveRequestEventData(CachingProviderType.ToString(), Name, nameof(RemoveByPattern),
                    new[] { pattern }));
            try
            {
                BaseRemoveByPattern(pattern);
            }
            catch (Exception ex)
            {
                s_diagnosticListener.WriteRemoveCacheError(operationId, ex);
                throw;
            }
            s_diagnosticListener.WriteRemoveCacheAfter(operationId);
        }

        public async Task RemoveByPatternAsync(string pattern, CancellationToken cancellationToken = default)
        {
            var operationId = s_diagnosticListener.WriteRemoveCacheBefore(
                new BeforeRemoveRequestEventData(CachingProviderType.ToString(), Name, nameof(RemoveByPatternAsync),
                    new[] { pattern }));
            try
            {
                await BaseRemoveByPatternAsync(pattern, cancellationToken);
            }
            catch (Exception ex)
            {
                s_diagnosticListener.WriteRemoveCacheError(operationId, ex);
                throw;
            }
            s_diagnosticListener.WriteRemoveCacheAfter(operationId);
        }

        public void Set<T>(string cacheKey, T cacheValue, TimeSpan expiration)
        {
            var operationId = s_diagnosticListener.WriteSetCacheBefore(new BeforeSetRequestEventData(CachingProviderType.ToString(), Name, nameof(Set), new Dictionary<string, object> { { cacheKey, cacheValue } }, expiration));
            try
            {
                BaseSet(cacheKey, cacheValue, expiration);
            }
            catch (Exception ex)
            {
                s_diagnosticListener.WriteSetCacheError(operationId, ex);
                throw;
            }
            s_diagnosticListener.WriteSetCacheAfter(operationId);
        }

        public void SetAll<T>(IDictionary<string, T> value, TimeSpan expiration)
        {
            var operationId = s_diagnosticListener.WriteSetCacheBefore(new BeforeSetRequestEventData(CachingProviderType.ToString(), Name, nameof(SetAll), value.ToDictionary(k => k.Key, v => (object)v.Value), expiration));
            try
            {
                BaseSetAll(value, expiration);
            }
            catch (Exception ex)
            {
                s_diagnosticListener.WriteSetCacheError(operationId, ex);
                throw;
            }
            s_diagnosticListener.WriteSetCacheAfter(operationId);
        }

        public async Task SetAllAsync<T>(IDictionary<string, T> value, TimeSpan expiration, CancellationToken cancellationToken = default)
        {
            var operationId = s_diagnosticListener.WriteSetCacheBefore(new BeforeSetRequestEventData(CachingProviderType.ToString(), Name, nameof(SetAllAsync), value.ToDictionary(k => k.Key, v => (object)v.Value), expiration));
            try
            {
                await BaseSetAllAsync(value, expiration, cancellationToken);
            }
            catch (Exception ex)
            {
                s_diagnosticListener.WriteSetCacheError(operationId, ex);
                throw;
            }
            s_diagnosticListener.WriteSetCacheAfter(operationId);
        }

        public async Task SetAsync<T>(string cacheKey, T cacheValue, TimeSpan expiration, CancellationToken cancellationToken = default)
        {
            var operationId = s_diagnosticListener.WriteSetCacheBefore(new BeforeSetRequestEventData(CachingProviderType.ToString(), Name, nameof(SetAsync), new Dictionary<string, object> { { cacheKey, cacheValue } }, expiration));
            try
            {
                await BaseSetAsync(cacheKey, cacheValue, expiration, cancellationToken);
            }
            catch (Exception ex)
            {
                s_diagnosticListener.WriteSetCacheError(operationId, ex);
                throw;
            }
            s_diagnosticListener.WriteSetCacheAfter(operationId);
        }

        public bool TrySet<T>(string cacheKey, T cacheValue, TimeSpan expiration)
        {
            var operationId = s_diagnosticListener.WriteSetCacheBefore(new BeforeSetRequestEventData(CachingProviderType.ToString(), Name, nameof(TrySet), new Dictionary<string, object> { { cacheKey, cacheValue } }, expiration));
            bool result;
            try
            {
                result = BaseTrySet(cacheKey, cacheValue, expiration);
            }
            catch (Exception ex)
            {
                s_diagnosticListener.WriteSetCacheError(operationId, ex);
                throw;
            }
            s_diagnosticListener.WriteSetCacheAfter(operationId);
            return result;
        }

        public async Task<bool> TrySetAsync<T>(string cacheKey, T cacheValue, TimeSpan expiration, CancellationToken cancellationToken = default)
        {
            var operationId = s_diagnosticListener.WriteSetCacheBefore(new BeforeSetRequestEventData(CachingProviderType.ToString(), Name, nameof(TrySetAsync), new Dictionary<string, object> { { cacheKey, cacheValue } }, expiration));
            bool result;
            try
            {
                result = await BaseTrySetAsync(cacheKey, cacheValue, expiration, cancellationToken);
            }
            catch (Exception ex)
            {
                s_diagnosticListener.WriteSetCacheError(operationId, ex);
                throw;
            }
            s_diagnosticListener.WriteSetCacheAfter(operationId);
            return result;
        }

        public TimeSpan GetExpiration(string cacheKey)
        {
            return BaseGetExpiration(cacheKey);
        }

        public async Task<TimeSpan> GetExpirationAsync(string cacheKey, CancellationToken cancellationToken = default)
        {
            return await BaseGetExpirationAsync(cacheKey, cancellationToken);
        }

        public ProviderInfo GetProviderInfo()
        {
            return BaseGetProviderInfo();
        }

        protected SearchKeyPattern ProcessSearchKeyPattern(string pattern)
        {
            var postfix = pattern.StartsWith("*");
            var prefix = pattern.EndsWith("*");

            var contains = postfix && prefix;

            if (contains)
            {
                return SearchKeyPattern.Contains;
            }

            if (postfix)
            {
                return SearchKeyPattern.Postfix;
            }

            if (prefix)
            {
                return SearchKeyPattern.Prefix;
            }

            return SearchKeyPattern.Exact;
        }

        protected string HandleSearchKeyPattern(string pattern)
        {
            return pattern.Replace("*", string.Empty);
        }
    }
}
