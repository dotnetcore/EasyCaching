namespace EasyCaching.HybridCache
{
    using EasyCaching.Core;
    using EasyCaching.Core.Internal;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    /// <summary>
    /// Hybrid caching provider.
    /// </summary>
    public class HybridCachingProvider : IHybridCachingProvider
    {
         private readonly IEasyCachingProvider _localCache;
         private readonly IEasyCachingProvider _distributedCache;
         private readonly IEasyCachingBus _bus;

        public HybridCachingProvider(
            IEnumerable<IEasyCachingProvider> providers
            , IEasyCachingBus bus = null
            )
        {
            ArgumentCheck.NotNullAndCountGTZero(providers, nameof(providers));

            var local = providers.OrderBy(x => x.Order).FirstOrDefault(x=>!x.IsDistributedCache);

            if (local == null) throw new Exception();
            else this._localCache = local;

            var distributed = providers.OrderBy(x => x.Order).FirstOrDefault(x => x.IsDistributedCache);

            if (distributed == null) throw new Exception();
            else this._distributedCache = distributed;

            this._bus = bus;


        }

        public bool Exists(string cacheKey)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            return _distributedCache.Exists(cacheKey);
        }

        public async Task<bool> ExistsAsync(string cacheKey)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            return await _distributedCache.ExistsAsync(cacheKey);
        }

        public CacheValue<T> Get<T>(string cacheKey)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            var cacheValue = _localCache.Get<T>(cacheKey);

            if(cacheValue.HasValue)
            {
                return cacheValue;
            }

            cacheValue = _distributedCache.Get<T>(cacheKey);

            if (cacheValue.HasValue)
            {
                _localCache.Set(cacheKey, cacheValue.Value, TimeSpan.FromSeconds(60));

                return cacheValue;
            }

            return CacheValue<T>.NoValue;
        }

        public async Task<CacheValue<T>> GetAsync<T>(string cacheKey)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            var cacheValue = await _localCache.GetAsync<T>(cacheKey);

            if (cacheValue.HasValue)
            {
                return cacheValue;
            }

            cacheValue = await _distributedCache.GetAsync<T>(cacheKey);

            if (cacheValue.HasValue)
            {
                await _localCache.SetAsync(cacheKey, cacheValue.Value, TimeSpan.FromSeconds(60));

                return cacheValue;
            }

            return CacheValue<T>.NoValue;
        }

        public void Remove(string cacheKey)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            _distributedCache.Remove(cacheKey);
            _localCache.Remove(cacheKey);

            //send message to bus 
        }

        public async Task RemoveAsync(string cacheKey)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            await _distributedCache.RemoveAsync(cacheKey);
            await _localCache.RemoveAsync(cacheKey);

            //send message to bus 
        }

        public void Set<T>(string cacheKey, T cacheValue, TimeSpan expiration)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            _localCache.Set(cacheKey, cacheValue, expiration);
            _distributedCache.Set(cacheKey, cacheValue, expiration);

            //send message to bus
        }

        public async Task SetAsync<T>(string cacheKey, T cacheValue, TimeSpan expiration)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            await _localCache.SetAsync(cacheKey, cacheValue, expiration);
            await _distributedCache.SetAsync(cacheKey, cacheValue, expiration);

            //send message to bus
        }

        public bool TrySet<T>(string cacheKey, T cacheValue, TimeSpan expiration)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            var flag = _distributedCache.TrySet(cacheKey, cacheValue, expiration);

            if(flag)
            {
                _localCache.Set(cacheKey, cacheValue, expiration);
            }

            return flag;
        }

        public async Task<bool> TrySetAsync<T>(string cacheKey, T cacheValue, TimeSpan expiration)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            var flag = await _distributedCache.TrySetAsync(cacheKey, cacheValue, expiration);

            if (flag)
            {
                await _localCache.SetAsync(cacheKey, cacheValue, expiration);
            }

            return flag;
        }
    }
}
