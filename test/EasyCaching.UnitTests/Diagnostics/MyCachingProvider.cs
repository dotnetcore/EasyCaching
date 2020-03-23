namespace EasyCaching.UnitTests
{
    using EasyCaching.Core;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public class MyCachingProvider : EasyCachingAbstractProvider
    {
        public MyCachingProvider()
        {
            this.ProviderName = "myprovider";
            this.ProviderStats = new CacheStats();
            this.ProviderType = CachingProviderType.InMemory;
            this.ProviderMaxRdSecond = 120;
            this.IsDistributedProvider = false;
        }

        public override bool BaseExists(string cacheKey)
        {
            return true;
        }

        public override Task<bool> BaseExistsAsync(string cacheKey)
        {
            return Task.FromResult(false);
        }

        public override void BaseFlush()
        {

        }

        public override Task BaseFlushAsync()
        {
            return Task.CompletedTask;
        }

        public override CacheValue<T> BaseGet<T>(string cacheKey, Func<T> dataRetriever, TimeSpan expiration)
        {
            return CacheValue<T>.NoValue;
        }

        public override CacheValue<T> BaseGet<T>(string cacheKey)
        {
            return CacheValue<T>.NoValue;
        }

        public override IDictionary<string, CacheValue<T>> BaseGetAll<T>(IEnumerable<string> cacheKeys)
        {
            return null;
        }

        public override Task<IDictionary<string, CacheValue<T>>> BaseGetAllAsync<T>(IEnumerable<string> cacheKeys)
        {
            return null;
        }

        public override Task<CacheValue<T>> BaseGetAsync<T>(string cacheKey, Func<Task<T>> dataRetriever, TimeSpan expiration)
        {
            return Task.FromResult(CacheValue<T>.NoValue);
        }

        public override Task<object> BaseGetAsync(string cacheKey, Type type)
        {
            return Task.FromResult<object>(null);
        }

        public override Task<CacheValue<T>> BaseGetAsync<T>(string cacheKey)
        {
            return Task.FromResult(CacheValue<T>.NoValue);
        }

        public override IDictionary<string, CacheValue<T>> BaseGetByPrefix<T>(string prefix)
        {
            return null;
        }

        public override Task<IDictionary<string, CacheValue<T>>> BaseGetByPrefixAsync<T>(string prefix)
        {
            return Task.FromResult<IDictionary<string, CacheValue<T>>>(null);
        }

        public override int BaseGetCount(string prefix = "")
        {
            return 1;
        }

        public override Task<int> BaseGetCountAsync(string prefix = "")
        {
            return Task.FromResult(1);
        }

        public override TimeSpan BaseGetExpiration(string cacheKey)
        {
            return TimeSpan.FromSeconds(1);
        }

        public override Task<TimeSpan> BaseGetExpirationAsync(string cacheKey)
        {
            return Task.FromResult(TimeSpan.FromSeconds(1));
        }

        public override ProviderInfo BaseGetProviderInfo()
        {
            throw new NotImplementedException();
        }

        public override void BaseRemove(string cacheKey)
        {

        }

        public override void BaseRemoveAll(IEnumerable<string> cacheKeys)
        {

        }

        public override Task BaseRemoveAllAsync(IEnumerable<string> cacheKeys)
        {
            return Task.CompletedTask;
        }

        public override Task BaseRemoveAsync(string cacheKey)
        {
            return Task.CompletedTask;
        }

        public override void BaseRemoveByPrefix(string prefix)
        {

        }

        public override Task BaseRemoveByPrefixAsync(string prefix)
        {
            return Task.CompletedTask;
        }

        public override void BaseSet<T>(string cacheKey, T cacheValue, TimeSpan expiration)
        {

        }

        public override void BaseSetAll<T>(IDictionary<string, T> values, TimeSpan expiration)
        {

        }

        public override Task BaseSetAllAsync<T>(IDictionary<string, T> values, TimeSpan expiration)
        {
            return Task.CompletedTask;
        }

        public override Task BaseSetAsync<T>(string cacheKey, T cacheValue, TimeSpan expiration)
        {
            return Task.CompletedTask;
        }

        public override bool BaseTrySet<T>(string cacheKey, T cacheValue, TimeSpan expiration)
        {
            return false;
        }

        public override Task<bool> BaseTrySetAsync<T>(string cacheKey, T cacheValue, TimeSpan expiration)
        {
            return Task.FromResult(false);
        }
    }
}
