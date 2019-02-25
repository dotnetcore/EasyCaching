namespace EasyCaching.Core.Diagnostics
{
    public class GetCacheEventData : EventData
    {
        public GetCacheEventData(string cacheType, string name, string operation, string[] cacheKeys)
            : base(cacheType, name, operation)
        {
            this.CacheKeys = cacheKeys;
        }

        public string[] CacheKeys { get; set; }
    }
}
