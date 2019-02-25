namespace EasyCaching.Core.Diagnostics
{
    public class RemoveCacheEventData : EventData
    {
        public RemoveCacheEventData(string cacheType, string name, string operation, string[] cacheKeys)
            : base(cacheType, name, operation)
        {
            this.CacheKeys = cacheKeys;
        }

        public string[] CacheKeys { get; set; }
    }
}
