namespace EasyCaching.Core.Diagnostics
{
    using System;

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
}
