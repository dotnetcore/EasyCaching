namespace EasyCaching.Core.Bus
{
    using System;

    /// <summary>
    /// EasyCaching message.
    /// </summary>
    [Serializable]
    public class EasyCachingMessage
    {        
        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        /// <value>The identifier.</value>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the cache keys.
        /// </summary>
        /// <value>The cache keys.</value>
        public string[] CacheKeys { get; set; }
    }
}
