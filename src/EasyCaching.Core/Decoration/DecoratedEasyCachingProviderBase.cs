namespace EasyCaching.Core.Decoration
{
    using EasyCaching.Core;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public abstract class DecoratedEasyCachingProviderBase<TProvider> : IEasyCachingProviderBase
        where TProvider : class, IEasyCachingProviderBase
    {
        protected IEasyCachingProviderDecorator<TProvider> Decorator { get; }

        public string Name { get; }

        protected DecoratedEasyCachingProviderBase(
            string name,
            IEasyCachingProviderDecorator<TProvider> decorator)
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
    }
}