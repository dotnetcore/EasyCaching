namespace EasyCaching.Core
{
    using System;

    [Serializable]
    public class EasyCachingMessage
    {        
        /// <summary>
        /// Gets or sets the cache keys.
        /// </summary>
        /// <value>The cache keys.</value>
        public string[] CacheKeys { get; set; }
    }
}
