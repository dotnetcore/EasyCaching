namespace EasyCaching.Core
{
    using EasyCaching.Core.Internal;
    using System;

    public class EasyCachingMessage
    {        
        public string CacheKey { get; set; }

        public object CacheValue { get; set; }

        public TimeSpan Expiration { get; set; }

        public NotifyType NotifyType { get; set; }
    }
}
