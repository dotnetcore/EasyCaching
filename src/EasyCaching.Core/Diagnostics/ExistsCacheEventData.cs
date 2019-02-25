namespace EasyCaching.Core.Diagnostics
{
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
