﻿namespace EasyCaching.HybridCache
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
        /// Gets or sets a value indicating whether an exception should be thrown if an error on the distributed cache has occurred
        /// </summary>
        /// <value><c>true</c> if distributed cache exceptions should not be ignored; otherwise, <c>false</c>.</value>
        public bool ThrowIfDistributedCacheError { get; set; }

        /// <summary>
        /// local cache provider name
        /// </summary>
        public string LocalCacheProviderName { get; set; }

        /// <summary>
        /// distribute cache provider name
        /// </summary>
        public string DistributedCacheProviderName { get; set; }

        /// <summary>
        /// Gets the default expiration when get ttl from distributed cache was failed
        /// </summary>        
        public int DefaultExpirationForTtlFailed { get; set; } = 60;

        /// <summary>
        /// The bus retry count.
        /// </summary>
        /// <remarks>
        /// When sending message failed, we will retry some times, default is 3 times.
        /// </remarks>
        public int BusRetryCount { get; set; } = 3;

        /// <summary>
        /// Flush the local cache on bus disconnection/reconnection
        /// </summary>
        /// <remarks>
        /// Flushing the local cache will avoid using stale data but may cause app jitters until the local cache get's re-populated.
        /// </remarks>
        public bool FlushLocalCacheOnBusReconnection { get; set; } = false;
    }
}
