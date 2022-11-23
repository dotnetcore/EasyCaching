﻿namespace EasyCaching.UnitTests
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using EasyCaching.Core;
    using System.Linq;
    
    public class FakeLocalCachingProvider : IEasyCachingProvider
    {
        public string Name => "local";

        public bool IsDistributedCache => false;

        public bool UseLock => false;

        public int Order => 1;

        public int MaxRdSecond => 0;

        public CachingProviderType CachingProviderType => CachingProviderType.InMemory;

        public CacheStats CacheStats => new CacheStats();

        public object Database => null;

        public bool Exists(string cacheKey)
        {
            return true;
        }

        public Task<bool> ExistsAsync(string cacheKey, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(true);
        }

        public void Flush()
        {

        }

        public Task FlushAsync(CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public CacheValue<T> Get<T>(string cacheKey, Func<T> dataRetriever, TimeSpan expiration)
        {
            return new CacheValue<T>(default(T), true);
        }

        public CacheValue<T> Get<T>(string cacheKey)
        {
            return new CacheValue<T>(default(T), true);
        }
        
        public IEnumerable<string> GetAllKeysByPrefix(string prefix)
        {
            return Enumerable.Empty<string>();
        }

        public Task<IEnumerable<string>> GetAllKeysByPrefixAsync(string prefix, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Enumerable.Empty<string>());
        }

        public IDictionary<string, CacheValue<T>> GetAll<T>(IEnumerable<string> cacheKeys)
        {
            return new Dictionary<string, CacheValue<T>>();
        }

        public Task<IDictionary<string, CacheValue<T>>> GetAllAsync<T>(IEnumerable<string> cacheKeys, CancellationToken cancellationToken = default)
        {
            IDictionary<string, CacheValue<T>> dict = new Dictionary<string, CacheValue<T>>();
            return Task.FromResult(dict);
        }

        public Task<CacheValue<T>> GetAsync<T>(string cacheKey, Func<Task<T>> dataRetriever, TimeSpan expiration, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new CacheValue<T>(default(T), true));
        }

        public Task<object> GetAsync(string cacheKey, Type type, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<object>(null);
        }

        public Task<CacheValue<T>> GetAsync<T>(string cacheKey, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new CacheValue<T>(default(T), true));
        }

        public IDictionary<string, CacheValue<T>> GetByPrefix<T>(string prefix)
        {
            return new Dictionary<string, CacheValue<T>>();
        }

        public Task<IDictionary<string, CacheValue<T>>> GetByPrefixAsync<T>(string prefix, CancellationToken cancellationToken = default)
        {
            IDictionary<string, CacheValue<T>> dict = new Dictionary<string, CacheValue<T>>();
            return Task.FromResult(dict);
        }

        public int GetCount(string prefix = "")
        {
            return 1;
        }

        public Task<int> GetCountAsync(string prefix = "", CancellationToken cancellationToken = default)
        {
            return Task.FromResult(1);
        }

        public TimeSpan GetExpiration(string cacheKey)
        {
            return TimeSpan.FromSeconds(1);
        }

        public Task<TimeSpan> GetExpirationAsync(string cacheKey, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(TimeSpan.FromSeconds(1));
        }

        public ProviderInfo GetProviderInfo()
        {
            throw new NotImplementedException();
        }

        public void Refresh<T>(string cacheKey, T cacheValue, TimeSpan expiration)
        {

        }

        public Task RefreshAsync<T>(string cacheKey, T cacheValue, TimeSpan expiration, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public void Remove(string cacheKey)
        {

        }

        public void RemoveAll(IEnumerable<string> cacheKeys)
        {

        }

        public Task RemoveAllAsync(IEnumerable<string> cacheKeys, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public Task RemoveAsync(string cacheKey, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public void RemoveByPrefix(string prefix)
        {

        }

        public Task RemoveByPatternAsync(string pattern, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public void RemoveByPattern(string pattern)
        {
        }

        public Task RemoveByPrefixAsync(string prefix, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public void Set<T>(string cacheKey, T cacheValue, TimeSpan expiration)
        {

        }

        public void SetAll<T>(IDictionary<string, T> value, TimeSpan expiration)
        {

        }

        public Task SetAllAsync<T>(IDictionary<string, T> value, TimeSpan expiration, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public Task SetAsync<T>(string cacheKey, T cacheValue, TimeSpan expiration, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public bool TrySet<T>(string cacheKey, T cacheValue, TimeSpan expiration)
        {
            return true;
        }

        public Task<bool> TrySetAsync<T>(string cacheKey, T cacheValue, TimeSpan expiration, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(true);
        }
    }
}
