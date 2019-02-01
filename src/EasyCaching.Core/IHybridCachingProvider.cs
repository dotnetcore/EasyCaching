using System;
using System.Threading.Tasks;

namespace EasyCaching.Core
{
    /// <summary>
    /// Hybrid caching provider.
    /// </summary>
    public interface IHybridCachingProvider //: IEasyCachingProvider 
    {
        void Set<T>(string cacheKey, T cacheValue, TimeSpan expiration);

        Task SetAsync<T>(string cacheKey, T cacheValue, TimeSpan expiration);

        CacheValue<T> Get<T>(string cacheKey);

        Task<CacheValue<T>> GetAsync<T>(string cacheKey);

        void Remove(string cacheKey);

        Task RemoveAsync(string cacheKey);

        Task<bool> ExistsAsync(string cacheKey);

        bool Exists(string cacheKey);

        bool TrySet<T>(string cacheKey, T cacheValue, TimeSpan expiration);

        Task<bool> TrySetAsync<T>(string cacheKey, T cacheValue, TimeSpan expiration);
    }
}
