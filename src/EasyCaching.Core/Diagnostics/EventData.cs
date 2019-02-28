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
}
