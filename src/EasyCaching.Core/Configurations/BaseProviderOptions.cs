﻿namespace EasyCaching.Core.Configurations
{
    /// <summary>
    /// Base provider options.
    /// </summary>
    public class BaseProviderOptions
    {   
        /// <summary>
        /// Gets or sets the max random second.
        /// </summary>
        /// <remarks>
        /// If this value greater then zero, the seted cache items' expiration will add a random second
        /// This is mainly for preventing Cache Crash
        /// </remarks>
        /// <value>The max random second.</value>
        public int MaxRdSecond { get; set; } = 120;

        /// <summary>
        /// Gets or sets a value indicating whether enable logging.
        /// </summary>
        /// <value><c>true</c> if enable logging; otherwise, <c>false</c>.</value>
        public bool EnableLogging { get; set; }

        /// <summary>
        /// Gets or sets the sleep ms.
        /// when mutex key alive, it will sleep some time, default is 300
        /// </summary>
        /// <value>The sleep ms.</value>
        public int SleepMs { get; set; } = 300;

        /// <summary>
        /// Gets or sets the lock ms.
        /// mutex key's alive time(ms), default is 5000
        /// </summary>
        /// <value>The lock ms.</value>
        public int LockMs { get; set; } = 5000;

        /// <summary>
        /// Gets or sets the serializer name that should be use in this provider.
        /// Mainly for distributed cache
        /// </summary>
        public string SerializerName { get; set; }

        /// <summary>
        /// Get or sets whether null values should be cached, default is true.
        /// </summary>
        public bool CacheNulls { get; set; } = true;
    }
}
