namespace EasyCaching.Core.Diagnostics
{
    using System;
    
    public class EventData
    {
        public EventData(string cacheType, string name, string operation)
        {
            this.CacheType = cacheType;
            this.Name = name;
            this.Operation = operation;
        }

        public string CacheType { get; set; }

        public string Name { get; set; }

        public string Operation { get; set; }
    }

    public class RemoveCacheEventData : EventData
    {
        public RemoveCacheEventData(string cacheType, string name, string operation, string[] cacheKeys)
            : base(cacheType, name, operation)
        {
            this.CacheKeys = cacheKeys;
        }

        public string[] CacheKeys { get; set; }
    }

    public class GetCacheEventData : EventData
    {
        public GetCacheEventData(string cacheType, string name, string operation, string[] cacheKeys)
            : base(cacheType, name, operation)
        {
            this.CacheKeys = cacheKeys;
        }

        public string[] CacheKeys { get; set; }
    }

    public class GetCountEventData : EventData
    {
        public GetCountEventData(string cacheType, string name, string operation, string prefix, long count)
            : base(cacheType, name, operation)
        {
            this.Prefix = prefix;
            this.Count = count;
        }

        public string Prefix { get; set; }

        public long Count { get; set; }
    }

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

    public class SetAllEventData : EventData
    {
        public SetAllEventData(string cacheType, string name, string operation, object values, TimeSpan expiration)
            : base(cacheType, name, operation)
        {
            this.Values = values;
            this.Expiration = expiration;
        }

        public object Values { get; set; }

        public TimeSpan Expiration { get; set; }
    }

    public class ExistsCacheEventData : EventData
    {
        public ExistsCacheEventData(string cacheType, string name, string operation, string cacheKey, bool flag)
            : base(cacheType, name, operation)
        {
            this.CacheKey = cacheKey;
            this.Flag = flag;
        }

        public string CacheKey { get; set; }

        public bool Flag { get; set; }
    }
}
