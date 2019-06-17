namespace EasyCaching.Disk
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using EasyCaching.Core;

    public class DefaultDiskCachingProvider :  EasyCachingAbstractProvider
    {
        public DefaultDiskCachingProvider()
        {
        }

        public override bool BaseExists(string cacheKey)
        {
            throw new NotImplementedException();
        }

        public override Task<bool> BaseExistsAsync(string cacheKey)
        {
            throw new NotImplementedException();
        }

        public override void BaseFlush()
        {
            throw new NotImplementedException();
        }

        public override Task BaseFlushAsync()
        {
            throw new NotImplementedException();
        }

        public override CacheValue<T> BaseGet<T>(string cacheKey, Func<T> dataRetriever, TimeSpan expiration)
        {
            throw new NotImplementedException();
        }

        public override CacheValue<T> BaseGet<T>(string cacheKey)
        {
            throw new NotImplementedException();
        }

        public override IDictionary<string, CacheValue<T>> BaseGetAll<T>(IEnumerable<string> cacheKeys)
        {
            throw new NotImplementedException();
        }

        public override Task<IDictionary<string, CacheValue<T>>> BaseGetAllAsync<T>(IEnumerable<string> cacheKeys)
        {
            throw new NotImplementedException();
        }

        public override Task<CacheValue<T>> BaseGetAsync<T>(string cacheKey, Func<Task<T>> dataRetriever, TimeSpan expiration)
        {
            throw new NotImplementedException();
        }

        public override Task<object> BaseGetAsync(string cacheKey, Type type)
        {
            throw new NotImplementedException();
        }

        public override Task<CacheValue<T>> BaseGetAsync<T>(string cacheKey)
        {
            throw new NotImplementedException();
        }

        public override IDictionary<string, CacheValue<T>> BaseGetByPrefix<T>(string prefix)
        {
            throw new NotImplementedException();
        }

        public override Task<IDictionary<string, CacheValue<T>>> BaseGetByPrefixAsync<T>(string prefix)
        {
            throw new NotImplementedException();
        }

        public override int BaseGetCount(string prefix = "")
        {
            throw new NotImplementedException();
        }

        public override TimeSpan BaseGetExpiration(string cacheKey)
        {
            throw new NotImplementedException();
        }

        public override Task<TimeSpan> BaseGetExpirationAsync(string cacheKey)
        {
            throw new NotImplementedException();
        }

        public override void BaseRefresh<T>(string cacheKey, T cacheValue, TimeSpan expiration)
        {
            throw new NotImplementedException();
        }

        public override Task BaseRefreshAsync<T>(string cacheKey, T cacheValue, TimeSpan expiration)
        {
            throw new NotImplementedException();
        }

        public override void BaseRemove(string cacheKey)
        {
            throw new NotImplementedException();
        }

        public override void BaseRemoveAll(IEnumerable<string> cacheKeys)
        {
            throw new NotImplementedException();
        }

        public override Task BaseRemoveAllAsync(IEnumerable<string> cacheKeys)
        {
            throw new NotImplementedException();
        }

        public override Task BaseRemoveAsync(string cacheKey)
        {
            throw new NotImplementedException();
        }

        public override void BaseRemoveByPrefix(string prefix)
        {
            throw new NotImplementedException();
        }

        public override Task BaseRemoveByPrefixAsync(string prefix)
        {
            throw new NotImplementedException();
        }

        public override void BaseSet<T>(string cacheKey, T cacheValue, TimeSpan expiration)
        {
            throw new NotImplementedException();
        }

        public override void BaseSetAll<T>(IDictionary<string, T> values, TimeSpan expiration)
        {
            throw new NotImplementedException();
        }

        public override Task BaseSetAllAsync<T>(IDictionary<string, T> values, TimeSpan expiration)
        {
            throw new NotImplementedException();
        }

        public override Task BaseSetAsync<T>(string cacheKey, T cacheValue, TimeSpan expiration)
        {
            throw new NotImplementedException();
        }

        public override bool BaseTrySet<T>(string cacheKey, T cacheValue, TimeSpan expiration)
        {
            throw new NotImplementedException();
        }

        public override Task<bool> BaseTrySetAsync<T>(string cacheKey, T cacheValue, TimeSpan expiration)
        {
            throw new NotImplementedException();
        }
    }
}
