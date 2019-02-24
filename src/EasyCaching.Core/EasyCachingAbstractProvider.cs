namespace EasyCaching.Core
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading.Tasks;
    using EasyCaching.Core.Diagnostics;

    public abstract class EasyCachingAbstractProvider : IEasyCachingProvider
    {
        protected static readonly DiagnosticListener s_diagnosticListener =
                    new DiagnosticListener(EasyCachingDiagnosticListenerExtensions.DiagnosticListenerName);

        protected string ProviderName { get; set; }
        protected bool IsDistributedProvider { get; set; }
        protected int ProviderOrder { get; set; }
        protected int ProviderMaxRdSecond { get; set; }
        protected CachingProviderType ProviderType { get; set; }
        protected CacheStats ProviderStats { get; set; }

        public string Name => this.ProviderName;
        public bool IsDistributedCache => this.IsDistributedProvider;
        public int Order => this.ProviderOrder;
        public int MaxRdSecond => this.ProviderMaxRdSecond;
        public CachingProviderType CachingProviderType => this.ProviderType;
        public CacheStats CacheStats => this.ProviderStats;

        public abstract bool BaseExists(string cacheKey);
        public abstract Task<bool> BaseExistsAsync(string cacheKey);
        public abstract void BaseFlush();
        public abstract Task BaseFlushAsync();
        public abstract CacheValue<T> BaseGet<T>(string cacheKey, Func<T> dataRetriever, TimeSpan expiration);
        public abstract CacheValue<T> BaseGet<T>(string cacheKey);
        public abstract IDictionary<string, CacheValue<T>> BaseGetAll<T>(IEnumerable<string> cacheKeys);
        public abstract Task<IDictionary<string, CacheValue<T>>> BaseGetAllAsync<T>(IEnumerable<string> cacheKeys);
        public abstract Task<CacheValue<T>> BaseGetAsync<T>(string cacheKey, Func<Task<T>> dataRetriever, TimeSpan expiration);
        public abstract Task<object> BaseGetAsync(string cacheKey, Type type);
        public abstract Task<CacheValue<T>> BaseGetAsync<T>(string cacheKey);
        public abstract IDictionary<string, CacheValue<T>> BaseGetByPrefix<T>(string prefix);
        public abstract Task<IDictionary<string, CacheValue<T>>> BaseGetByPrefixAsync<T>(string prefix);
        public abstract int BaseGetCount(string prefix = "");
        public abstract void BaseRefresh<T>(string cacheKey, T cacheValue, TimeSpan expiration);
        public abstract Task BaseRefreshAsync<T>(string cacheKey, T cacheValue, TimeSpan expiration);
        public abstract void BaseRemove(string cacheKey);
        public abstract void BaseRemoveAll(IEnumerable<string> cacheKeys);
        public abstract Task BaseRemoveAllAsync(IEnumerable<string> cacheKeys);
        public abstract Task BaseRemoveAsync(string cacheKey);
        public abstract void BaseRemoveByPrefix(string prefix);
        public abstract Task BaseRemoveByPrefixAsync(string prefix);
        public abstract void BaseSet<T>(string cacheKey, T cacheValue, TimeSpan expiration);
        public abstract void BaseSetAll<T>(IDictionary<string, T> values, TimeSpan expiration);
        public abstract Task BaseSetAllAsync<T>(IDictionary<string, T> values, TimeSpan expiration);
        public abstract Task BaseSetAsync<T>(string cacheKey, T cacheValue, TimeSpan expiration);
        public abstract bool BaseTrySet<T>(string cacheKey, T cacheValue, TimeSpan expiration);
        public abstract Task<bool> BaseTrySetAsync<T>(string cacheKey, T cacheValue, TimeSpan expiration);

        public bool Exists(string cacheKey)
        {
            var flag = BaseExists(cacheKey);
            s_diagnosticListener.WriteExistsCache(new ExistsCacheEventData(CachingProviderType.ToString(), Name, nameof(Exists), cacheKey, flag));
            return flag;
        }

        public async Task<bool> ExistsAsync(string cacheKey)
        {
            var flag = await BaseExistsAsync(cacheKey);
            s_diagnosticListener.WriteExistsCache(new ExistsCacheEventData(CachingProviderType.ToString(), Name, nameof(ExistsAsync), cacheKey, flag));
            return flag;
        }

        public void Flush()
        {
            s_diagnosticListener.WriteFlushCache(new EventData(CachingProviderType.ToString(), Name, nameof(Flush)));
            BaseFlush();
        }

        public async Task FlushAsync()
        {
            s_diagnosticListener.WriteFlushCache(new EventData(CachingProviderType.ToString(), Name, nameof(FlushAsync)));
            await BaseFlushAsync();
        }

        public CacheValue<T> Get<T>(string cacheKey, Func<T> dataRetriever, TimeSpan expiration)
        {

            s_diagnosticListener.WriteGetCache(new GetCacheEventData(CachingProviderType.ToString(), Name, nameof(Get), new[] { cacheKey}));
            return BaseGet(cacheKey, dataRetriever, expiration);
        }

        public CacheValue<T> Get<T>(string cacheKey)
        {
            s_diagnosticListener.WriteGetCache(new GetCacheEventData(CachingProviderType.ToString(), Name, nameof(Get), new[] { cacheKey }));
            return BaseGet<T>(cacheKey);
        }

        public IDictionary<string, CacheValue<T>> GetAll<T>(IEnumerable<string> cacheKeys)
        {
            s_diagnosticListener.WriteGetCache(new GetCacheEventData(CachingProviderType.ToString(), Name, nameof(GetAll), cacheKeys.ToArray()));
            return BaseGetAll<T>(cacheKeys);
        }

        public async Task<IDictionary<string, CacheValue<T>>> GetAllAsync<T>(IEnumerable<string> cacheKeys)
        {
            s_diagnosticListener.WriteGetCache(new GetCacheEventData(CachingProviderType.ToString(), Name, nameof(GetAllAsync), cacheKeys.ToArray()));
            return await BaseGetAllAsync<T>(cacheKeys);
        }

        public async Task<CacheValue<T>> GetAsync<T>(string cacheKey, Func<Task<T>> dataRetriever, TimeSpan expiration)
        {
            s_diagnosticListener.WriteGetCache(new GetCacheEventData(CachingProviderType.ToString(), Name, nameof(Get), new[] { cacheKey }));
            return await BaseGetAsync<T>(cacheKey, dataRetriever, expiration);
        }

        public async Task<object> GetAsync(string cacheKey, Type type)
        {
            s_diagnosticListener.WriteGetCache(new GetCacheEventData(CachingProviderType.ToString(), Name, nameof(Get), new[] { cacheKey }));
            return await BaseGetAsync(cacheKey, type);
        }

        public async Task<CacheValue<T>> GetAsync<T>(string cacheKey)
        {
            s_diagnosticListener.WriteGetCache(new GetCacheEventData(CachingProviderType.ToString(), Name, nameof(Get), new[] { cacheKey }));
            return await BaseGetAsync<T>(cacheKey);
        }

        public IDictionary<string, CacheValue<T>> GetByPrefix<T>(string prefix)
        {
            s_diagnosticListener.WriteGetCache(new GetCacheEventData(CachingProviderType.ToString(), Name, nameof(GetByPrefix), new[] { prefix }));
            return BaseGetByPrefix<T>(prefix);
        }

        public async Task<IDictionary<string, CacheValue<T>>> GetByPrefixAsync<T>(string prefix)
        {
            s_diagnosticListener.WriteGetCache(new GetCacheEventData(CachingProviderType.ToString(), Name, nameof(GetByPrefix), new[] { prefix }));
            return await BaseGetByPrefixAsync<T>(prefix);
        }

        public int GetCount(string prefix = "")
        {
            var count = BaseGetCount(prefix);
            s_diagnosticListener.WriteGetCount(new GetCountEventData(CachingProviderType.ToString(), Name, nameof(GetCount), prefix, count));
            return count;
        }

        public void Refresh<T>(string cacheKey, T cacheValue, TimeSpan expiration)
        {
            s_diagnosticListener.WriteSetCache(new SetCacheEventData(CachingProviderType.ToString(), Name, nameof(Refresh), cacheKey, cacheValue, expiration));
            BaseRefresh(cacheKey, cacheValue, expiration);
        }

        public async Task RefreshAsync<T>(string cacheKey, T cacheValue, TimeSpan expiration)
        {
            s_diagnosticListener.WriteSetCache(new SetCacheEventData(CachingProviderType.ToString(), Name, nameof(RefreshAsync), cacheKey, cacheValue, expiration));
            await BaseRefreshAsync(cacheKey, cacheValue, expiration);
        }

        public void Remove(string cacheKey)
        {
            s_diagnosticListener.WriteRemoveCache(new RemoveCacheEventData(CachingProviderType.ToString(), Name, nameof(Remove),new[] { cacheKey}));
            BaseRemove(cacheKey);
        }

        public void RemoveAll(IEnumerable<string> cacheKeys)
        {
            s_diagnosticListener.WriteRemoveCache(new RemoveCacheEventData(CachingProviderType.ToString(), Name, nameof(RemoveAll), cacheKeys.ToArray()));
            BaseRemoveAll(cacheKeys);
        }

        public async Task RemoveAllAsync(IEnumerable<string> cacheKeys)
        {
            s_diagnosticListener.WriteRemoveCache(new RemoveCacheEventData(CachingProviderType.ToString(), Name, nameof(RemoveAllAsync), cacheKeys.ToArray()));
            await BaseRemoveAllAsync(cacheKeys);
        }

        public async Task RemoveAsync(string cacheKey)
        {
            s_diagnosticListener.WriteRemoveCache(new RemoveCacheEventData(CachingProviderType.ToString(), Name, nameof(RemoveAsync), new[] { cacheKey }));
            await BaseRemoveAsync(cacheKey);
        }

        public void RemoveByPrefix(string prefix)
        {
            s_diagnosticListener.WriteRemoveCache(new RemoveCacheEventData(CachingProviderType.ToString(), Name, nameof(RemoveByPrefix), new[] { prefix }));
            BaseRemoveByPrefix(prefix);
        }

        public async Task RemoveByPrefixAsync(string prefix)
        {
            s_diagnosticListener.WriteRemoveCache(new RemoveCacheEventData(CachingProviderType.ToString(), Name, nameof(RemoveByPrefixAsync), new[] { prefix }));
            await BaseRemoveByPrefixAsync(prefix);
        }

        public void Set<T>(string cacheKey, T cacheValue, TimeSpan expiration)
        {
            s_diagnosticListener.WriteSetCache(new SetCacheEventData(CachingProviderType.ToString(), Name, nameof(Set), cacheKey, cacheValue, expiration));
            BaseSet(cacheKey, cacheValue, expiration);
        }

        public void SetAll<T>(IDictionary<string, T> value, TimeSpan expiration)
        {
            s_diagnosticListener.WriteSetAll(new SetAllEventData(CachingProviderType.ToString(), Name, nameof(SetAll), value, expiration));
            BaseSetAll(value, expiration);
        }

        public async Task SetAllAsync<T>(IDictionary<string, T> value, TimeSpan expiration)
        {
            s_diagnosticListener.WriteSetAll(new SetAllEventData(CachingProviderType.ToString(), Name, nameof(SetAllAsync), value, expiration));
            await BaseSetAllAsync(value, expiration);
        }

        public async Task SetAsync<T>(string cacheKey, T cacheValue, TimeSpan expiration)
        {
            s_diagnosticListener.WriteSetCache(new SetCacheEventData(CachingProviderType.ToString(), Name, nameof(GetAsync), cacheKey, cacheValue, expiration));
            await BaseSetAsync(cacheKey, cacheValue, expiration);
        }

        public bool TrySet<T>(string cacheKey, T cacheValue, TimeSpan expiration)
        {
            s_diagnosticListener.WriteSetCache(new SetCacheEventData(CachingProviderType.ToString(), Name, nameof(TrySet), cacheKey, cacheValue, expiration));
            return BaseTrySet(cacheKey, cacheValue, expiration);
        }

        public async Task<bool> TrySetAsync<T>(string cacheKey, T cacheValue, TimeSpan expiration)
        {
            s_diagnosticListener.WriteSetCache(new SetCacheEventData(CachingProviderType.ToString(), Name, nameof(TrySetAsync), cacheKey, cacheValue, expiration));
            return await BaseTrySetAsync(cacheKey, cacheValue, expiration);
        }
    }
}
