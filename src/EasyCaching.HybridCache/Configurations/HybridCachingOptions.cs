namespace EasyCaching.HybridCache
{
    public class HybridCachingOptions
    {
        /// <summary>
        /// Gets or sets the name of the topic.
        /// </summary>
        /// <value>The name of the topic.</value>
        public string TopicName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="T:EasyCaching.HybridCache.HybridCachingOptions"/>
        /// enable logging.
        /// </summary>
        /// <value><c>true</c> if enable logging; otherwise, <c>false</c>.</value>
        public bool EnableLogging { get; set; }

        /// <summary>
        /// local cache provider name
        /// </summary>
        public string LocalCacheProviderName { get; set; }

        /// <summary>
        /// distribute cache provider name
        /// </summary>
        public string DistributedCacheProviderName { get; set; }
    }
}
