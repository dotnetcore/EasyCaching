namespace EasyCaching.Core.Decoration
{
    using EasyCaching.Core;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public class DecoratedEasyCachingProvider : IEasyCachingProvider
    {
        private IEasyCachingProviderDecorator<IEasyCachingProvider> Decorator { get; }

        public string Name { get; }

        public DecoratedEasyCachingProvider(
            string name,
            IEasyCachingProviderDecorator<IEasyCachingProvider> decorator)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Decorator = decorator;
        }

        public void Set<T>(string cacheKey, T cacheValue, TimeSpan expiration)
        {
            Decorator.Execute(provider => provider.Set(cacheKey, cacheValue, expiration));
        }

        public Task SetAsync<T>(string cacheKey, T cacheValue, TimeSpan expiration)
        {
            return Decorator.ExecuteAsync(provider => provider.SetAsync(cacheKey, cacheValue, expiration));
        }

        public IDictionary<string, CacheValue<T>> GetAll<T>(IEnumerable<string> cacheKeys)
        {
            return Decorator.Execute(provider => provider.GetAll<T>(cacheKeys));
        }

        public Task<IDictionary<string, CacheValue<T>>> GetAllAsync<T>(IEnumerable<string> cacheKeys)
        {
            return Decorator.ExecuteAsync(provider => provider.GetAllAsync<T>(cacheKeys));
        }

        public CacheValue<T> Get<T>(string cacheKey)
        {
            return Decorator.Execute(provider => provider.Get<T>(cacheKey));
        }

        public Task<CacheValue<T>> GetAsync<T>(string cacheKey)
        {
            return Decorator.ExecuteAsync(provider => provider.GetAsync<T>(cacheKey));
        }

        public void Remove(string cacheKey)
        {
            Decorator.Execute(provider => provider.Remove(cacheKey));
        }

        public Task RemoveAsync(string cacheKey)
        {
            return Decorator.ExecuteAsync(provider => provider.RemoveAsync(cacheKey));
        }

        public Task<bool> ExistsAsync(string cacheKey)
        {
            return Decorator.ExecuteAsync(provider => provider.ExistsAsync(cacheKey));
        }

        public bool Exists(string cacheKey)
        {
            return Decorator.Execute(provider => provider.Exists(cacheKey));
        }

        public bool TrySet<T>(string cacheKey, T cacheValue, TimeSpan expiration)
        {
            return Decorator.Execute(provider => provider.TrySet(cacheKey, cacheValue, expiration));
        }

        public Task<bool> TrySetAsync<T>(string cacheKey, T cacheValue, TimeSpan expiration)
        {
            return Decorator.ExecuteAsync(provider => provider.TrySetAsync(cacheKey, cacheValue, expiration));
        }

        public void SetAll<T>(IDictionary<string, T> value, TimeSpan expiration)
        {
            Decorator.Execute(provider => provider.SetAll(value, expiration));
        }

        public Task SetAllAsync<T>(IDictionary<string, T> value, TimeSpan expiration)
        {
            return Decorator.ExecuteAsync(provider => provider.SetAllAsync(value, expiration));
        }

        public void RemoveAll(IEnumerable<string> cacheKeys)
        {
            Decorator.Execute(provider => provider.RemoveAll(cacheKeys));
        }

        public Task RemoveAllAsync(IEnumerable<string> cacheKeys)
        {
            return Decorator.ExecuteAsync(provider => provider.RemoveAllAsync(cacheKeys));
        }

        public CacheValue<T> Get<T>(string cacheKey, Func<T> dataRetriever, TimeSpan expiration)
        {
            return Decorator.Execute(provider => provider.Get(cacheKey, dataRetriever, expiration));
        }

        public Task<CacheValue<T>> GetAsync<T>(string cacheKey, Func<Task<T>> dataRetriever, TimeSpan expiration)
        {
            return Decorator.ExecuteAsync(provider => provider.GetAsync(cacheKey, dataRetriever, expiration));
        }

        public void RemoveByPrefix(string prefix)
        {
            Decorator.Execute(provider => provider.RemoveByPrefix(prefix));
        }

        public Task RemoveByPrefixAsync(string prefix)
        {
            return Decorator.ExecuteAsync(provider => provider.RemoveByPrefixAsync(prefix));
        }

        public Task<object> GetAsync(string cacheKey, Type type)
        {
            return Decorator.ExecuteAsync(provider => provider.GetAsync(cacheKey, type));
        }

        public TimeSpan GetExpiration(string cacheKey)
        {
            return Decorator.Execute(provider => provider.GetExpiration(cacheKey));
        }

        public Task<TimeSpan> GetExpirationAsync(string cacheKey)
        {
            return Decorator.Execute(provider => provider.GetExpirationAsync(cacheKey));
        }

        public int MaxRdSecond => Decorator.Execute(provider => provider.MaxRdSecond);
        public CachingProviderType CachingProviderType => Decorator.Execute(provider => provider.CachingProviderType);
        public CacheStats CacheStats => Decorator.Execute(provider => provider.CacheStats);

        public IDictionary<string, CacheValue<T>> GetByPrefix<T>(string prefix)
        {
            return Decorator.Execute(provider => provider.GetByPrefix<T>(prefix));
        }

        public Task<IDictionary<string, CacheValue<T>>> GetByPrefixAsync<T>(string prefix)
        {
            return Decorator.ExecuteAsync(provider => provider.GetByPrefixAsync<T>(prefix));
        }

        public int GetCount(string prefix = "")
        {
            return Decorator.Execute(provider => provider.GetCount(prefix));
        }

        public Task<int> GetCountAsync(string prefix = "")
        {
            return Decorator.ExecuteAsync(provider => provider.GetCountAsync(prefix));
        }

        public void Flush()
        {
            Decorator.Execute(provider => provider.Flush());
        }

        public Task FlushAsync()
        {
            return Decorator.ExecuteAsync(provider => provider.FlushAsync());
        }

        public ProviderInfo GetProviderInfo()
        {
            return Decorator.GetCachingProvider().GetProviderInfo();
        }
    }
}