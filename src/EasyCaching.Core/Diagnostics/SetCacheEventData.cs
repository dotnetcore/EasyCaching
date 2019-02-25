namespace EasyCaching.Core.Diagnostics
{
    using System;

    public class SetCacheEventData : EventData
    {
        public SetCacheEventData(string cacheType, string name, string operation, string cacheKey, object cacheValue, TimeSpan expiration)
            : base(cacheType, name, operation)
        {
            this.CacheKey = cacheKey;
            this.CacheValue = cacheValue;
            this.Expiration = expiration;
        }

        public string CacheKey { get; set; }

        public object CacheValue { get; set; }

        public TimeSpan Expiration { get; set; }
    }

}
