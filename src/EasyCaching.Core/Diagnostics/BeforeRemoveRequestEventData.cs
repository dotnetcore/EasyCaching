namespace EasyCaching.Core.Diagnostics
{
    using System;

    public class BeforeRemoveRequestEventData : EventData
    {
        public BeforeRemoveRequestEventData(string cacheType, string name, string operation, string[] cacheKeys)
            : base(cacheType, name, operation)
        {
            this.CacheKeys = cacheKeys;
        }

        public string[] CacheKeys { get; set; }
    }
}
