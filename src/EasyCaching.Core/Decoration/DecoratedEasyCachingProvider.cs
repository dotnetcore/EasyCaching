namespace EasyCaching.Core.Decoration
{
    using EasyCaching.Core;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public class DecoratedEasyCachingProvider : DecoratedEasyCachingProvider<IEasyCachingProvider>
    {
        public DecoratedEasyCachingProvider(
            string name,
            IEasyCachingProviderDecorator<IEasyCachingProvider> decorator) : base(name, decorator)
        {
        }
    }
    
    public abstract class DecoratedEasyCachingProvider<TProvider> : DecoratedEasyCachingProviderBase<TProvider>, IEasyCachingProvider
        where TProvider : class, IEasyCachingProvider
    {
        public int MaxRdSecond => Decorator.Execute(provider => provider.MaxRdSecond);
        public CachingProviderType CachingProviderType => Decorator.Execute(provider => provider.CachingProviderType);
        public CacheStats CacheStats => Decorator.Execute(provider => provider.CacheStats);

        protected DecoratedEasyCachingProvider(
            string name,
            IEasyCachingProviderDecorator<TProvider> decorator) : base(name, decorator)
        {
        }

        public IDictionary<string, CacheValue<T>> GetAll<T>(IEnumerable<string> cacheKeys)
        {
            return Decorator.Execute(provider => provider.GetAll<T>(cacheKeys));
        }

        public Task<IDictionary<string, CacheValue<T>>> GetAllAsync<T>(IEnumerable<string> cacheKeys)
        {
            return Decorator.ExecuteAsync(provider => provider.GetAllAsync<T>(cacheKeys));
        }

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

        public TimeSpan GetExpiration(string cacheKey)
        {
            return Decorator.Execute(provider => provider.GetExpiration(cacheKey));
        }

        public Task<TimeSpan> GetExpirationAsync(string cacheKey)
        {
            return Decorator.ExecuteAsync(provider => provider.GetExpirationAsync(cacheKey));
        }

        public ProviderInfo GetProviderInfo()
        {
            return Decorator.GetCachingProvider().GetProviderInfo();
        }
    }
}