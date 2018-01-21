using System;
namespace EasyCaching.Core
{
    public class EasyCachingMessage
    {        
        public string CacheKey { get; set; }

        public object CacheValue { get; set; }

        public TimeSpan Expiration { get; set; }
    }
}
