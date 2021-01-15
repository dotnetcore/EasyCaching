namespace EasyCaching.Core.Decoration
{
    using EasyCaching.Core.Configurations;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    
    public partial class NullCachingProvider : EasyCachingAbstractProvider, IRedisAndEasyCachingProvider
    {
        private readonly ProviderInfo _info;
        private readonly BaseProviderOptions _options;

        public NullCachingProvider(string name, BaseProviderOptions options)
        {
            this._options = options;
            
            this.ProviderName = name;
            this.ProviderType = CachingProviderType.InMemory;
            this.ProviderStats = new CacheStats();
            this.ProviderMaxRdSecond = _options.MaxRdSecond;
            this.IsDistributedProvider = false;

            _info = new ProviderInfo
            {
                CacheStats = ProviderStats,
                EnableLogging = options.EnableLogging,
                IsDistributedProvider = IsDistributedProvider,
                LockMs = options.LockMs,
                MaxRdSecond = options.MaxRdSecond,
                ProviderName = ProviderName,
                ProviderType = ProviderType,
                SerializerName = options.SerializerName,
                SleepMs = options.SleepMs,
                Serializer = null,
                CacheNulls = options.CacheNulls
            };
        }
        
        public override bool BaseExists(string cacheKey) => false;

        public override void BaseFlush()
        {
        }

        public override CacheValue<T> BaseGet<T>(string cacheKey, Func<T> dataRetriever, TimeSpan expiration)
        {
            var value = dataRetriever();
            return value != null || _options.CacheNulls 
                ? new CacheValue<T>(value, hasValue: true) 
                : CacheValue<T>.NoValue;
        }
        
        public override async Task<CacheValue<T>> BaseGetAsync<T>(string cacheKey, Func<Task<T>> dataRetriever, TimeSpan expiration)
        {
            var value = await dataRetriever();
            return value != null || _options.CacheNulls 
                ? new CacheValue<T>(value, hasValue: true) 
                : CacheValue<T>.NoValue;
        }

        public override CacheValue<T> BaseGet<T>(string cacheKey) => CacheValue<T>.NoValue;

        public override IDictionary<string, CacheValue<T>> BaseGetAll<T>(IEnumerable<string> cacheKeys) =>
            cacheKeys.ToDictionary(key => key, key => CacheValue<T>.NoValue);

        public override Task<object> BaseGetAsync(string cacheKey, Type type) => Task.FromResult<object>(null);

        public override IDictionary<string, CacheValue<T>> BaseGetByPrefix<T>(string prefix) => new Dictionary<string, CacheValue<T>>();

        public override int BaseGetCount(string prefix = "") => 0;

        public override void BaseRemove(string cacheKey)
        {
        }

        public override void BaseRemoveAll(IEnumerable<string> cacheKeys)
        {
        }

        public override void BaseRemoveByPrefix(string prefix)
        {
        }

        public override void BaseSet<T>(string cacheKey, T cacheValue, TimeSpan expiration)
        {
        }

        public override void BaseSetAll<T>(IDictionary<string, T> values, TimeSpan expiration)
        {
        }

        public override bool BaseTrySet<T>(string cacheKey, T cacheValue, TimeSpan expiration) => true;

        public override TimeSpan BaseGetExpiration(string cacheKey) => TimeSpan.Zero;
        
         public override Task<bool> BaseExistsAsync(string cacheKey) => Task.FromResult(BaseExists(cacheKey));

        public override Task BaseFlushAsync()
        {
            BaseFlush();
            return Task.CompletedTask;
        }

        public override Task<CacheValue<T>> BaseGetAsync<T>(string cacheKey) => Task.FromResult(BaseGet<T>(cacheKey));

        public override Task<IDictionary<string, CacheValue<T>>> BaseGetAllAsync<T>(IEnumerable<string> cacheKeys) =>
            Task.FromResult(BaseGetAll<T>(cacheKeys));

        public override Task<IDictionary<string, CacheValue<T>>> BaseGetByPrefixAsync<T>(string prefix) => Task.FromResult(BaseGetByPrefix<T>(prefix));

        public override Task<int> BaseGetCountAsync(string prefix = "") => Task.FromResult(BaseGetCount(prefix));

        public override Task BaseRemoveAsync(string cacheKey)
        {
            BaseRemove(cacheKey);
            return Task.CompletedTask;
        }

        public override Task BaseRemoveAllAsync(IEnumerable<string> cacheKeys)
        {
            BaseRemoveAll(cacheKeys);
            return Task.CompletedTask;
        }
        public override Task BaseRemoveByPrefixAsync(string prefix)
        {
            BaseRemoveByPrefix(prefix);
            return Task.CompletedTask;
        }
        
        public override Task BaseSetAsync<T>(string cacheKey, T cacheValue, TimeSpan expiration)
        {
            BaseSet(cacheKey, cacheValue, expiration);
            return Task.CompletedTask;
        }

        public override Task BaseSetAllAsync<T>(IDictionary<string, T> values, TimeSpan expiration)
        {
            BaseSetAll(values, expiration);
            return Task.CompletedTask;
        }

        public override Task<bool> BaseTrySetAsync<T>(string cacheKey, T cacheValue, TimeSpan expiration) =>
            Task.FromResult(BaseTrySet(cacheKey, cacheValue, expiration));

        public override Task<TimeSpan> BaseGetExpirationAsync(string cacheKey) => Task.FromResult<TimeSpan>(BaseGetExpiration(cacheKey));

        public override ProviderInfo BaseGetProviderInfo() => _info;
    }
}