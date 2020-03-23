namespace EasyCaching.UnitTests
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using EasyCaching.Core;

    public class FakeDistributedCachingProvider : IEasyCachingProvider
    {    
        public string Name => "distributed";

        public bool IsDistributedCache => true;

        public int Order => 1;

        public int MaxRdSecond => 0;

        public CachingProviderType CachingProviderType => CachingProviderType.Redis;

        public CacheStats CacheStats => new CacheStats();

        public virtual bool Exists(string cacheKey)
        {
            return true;
        }

        public virtual Task<bool> ExistsAsync(string cacheKey)
        {
            return Task.FromResult(true);
        }

        public virtual void Flush()
        {

        }

        public virtual Task FlushAsync()
        {
            return Task.CompletedTask;
        }

        public virtual CacheValue<T> Get<T>(string cacheKey, Func<T> dataRetriever, TimeSpan expiration)
        {
            return new CacheValue<T>(default(T), true);
        }

        public virtual CacheValue<T> Get<T>(string cacheKey)
        {
            return new CacheValue<T>(default(T), true);
        }

        public virtual IDictionary<string, CacheValue<T>> GetAll<T>(IEnumerable<string> cacheKeys)
        {
            return new Dictionary<string, CacheValue<T>>();
        }

        public virtual Task<IDictionary<string, CacheValue<T>>> GetAllAsync<T>(IEnumerable<string> cacheKeys)
        {
            IDictionary<string, CacheValue<T>> dict = new Dictionary<string, CacheValue<T>>();
            return Task.FromResult(dict);
        }

        public virtual Task<CacheValue<T>> GetAsync<T>(string cacheKey, Func<Task<T>> dataRetriever, TimeSpan expiration)
        {
            return Task.FromResult(new CacheValue<T>(default(T), true));
        }

        public virtual Task<object> GetAsync(string cacheKey, Type type)
        {
            return Task.FromResult<object>(null);
        }

        public virtual Task<CacheValue<T>> GetAsync<T>(string cacheKey)
        {
            return Task.FromResult(new CacheValue<T>(default(T), true));
        }

        public virtual IDictionary<string, CacheValue<T>> GetByPrefix<T>(string prefix)
        {
            return new Dictionary<string, CacheValue<T>>();
        }

        public virtual Task<IDictionary<string, CacheValue<T>>> GetByPrefixAsync<T>(string prefix)
        {
            IDictionary<string, CacheValue<T>> dict = new Dictionary<string, CacheValue<T>>();
            return Task.FromResult(dict);
        }

        public virtual int GetCount(string prefix = "")
        {
            return 1;
        }

        public Task<int> GetCountAsync(string prefix = "")
        {
            return Task.FromResult(1);
        }

        public virtual TimeSpan GetExpiration(string cacheKey)
        {
            return TimeSpan.FromSeconds(1);
        }

        public virtual Task<TimeSpan> GetExpirationAsync(string cacheKey)
        {
            return Task.FromResult(TimeSpan.FromSeconds(1));
        }

        public ProviderInfo GetProviderInfo()
        {
            throw new NotImplementedException();
        }

        public virtual void Refresh<T>(string cacheKey, T cacheValue, TimeSpan expiration)
        {

        }

        public virtual Task RefreshAsync<T>(string cacheKey, T cacheValue, TimeSpan expiration)
        {
            return Task.CompletedTask;
        }

        public virtual void Remove(string cacheKey)
        {

        }

        public virtual void RemoveAll(IEnumerable<string> cacheKeys)
        {

        }

        public virtual Task RemoveAllAsync(IEnumerable<string> cacheKeys)
        {
            return Task.CompletedTask;
        }

        public virtual Task RemoveAsync(string cacheKey)
        {
            return Task.CompletedTask;
        }

        public virtual void RemoveByPrefix(string prefix)
        {

        }

        public virtual Task RemoveByPrefixAsync(string prefix)
        {
            return Task.CompletedTask;
        }

        public virtual void Set<T>(string cacheKey, T cacheValue, TimeSpan expiration)
        {

        }

        public virtual void SetAll<T>(IDictionary<string, T> value, TimeSpan expiration)
        {

        }

        public virtual Task SetAllAsync<T>(IDictionary<string, T> value, TimeSpan expiration)
        {
            return Task.CompletedTask;
        }

        public virtual Task SetAsync<T>(string cacheKey, T cacheValue, TimeSpan expiration)
        {
            return Task.CompletedTask;
        }

        public virtual bool TrySet<T>(string cacheKey, T cacheValue, TimeSpan expiration)
        {
            return true;
        }

        public virtual Task<bool> TrySetAsync<T>(string cacheKey, T cacheValue, TimeSpan expiration)
        {
            return Task.FromResult(true);
        }
    }
}
