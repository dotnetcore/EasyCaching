namespace EasyCaching.Core.Configurations
{
    /// <summary>
    /// Base provider options.
    /// </summary>
    public class BaseProviderOptions
    {
        /// <summary>
        /// Gets or sets the type of the caching provider.
        /// </summary>
        /// <remarks>
        /// Reserved, not used yet.
        /// </remarks>
        /// <value>The type of the caching provider.</value>
        public CachingProviderType CachingProviderType { get; set; }

        /// <summary>
        /// Gets or sets the max random second.
        /// </summary>
        /// <remarks>
        /// Prevent Cache Crash
        /// </remarks>
        /// <value>The max random second.</value>
        public int MaxRdSecond { get; set; } = 120;

        /// <summary>
        /// Gets or sets the order.
        /// </summary>
        /// <remarks>
        /// Mainly for hybird
        /// </remarks>
        /// <value>The order.</value>
        public int Order { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether enable logging.
        /// </summary>
        /// <value><c>true</c> if enable logging; otherwise, <c>false</c>.</value>
        public bool EnableLogging { get; set; }

        /// <summary>
        /// Gets or sets the sleep ms.
        /// </summary>
        /// <value>The sleep ms.</value>
        public int SleepMs { get; set; } = 300;

        /// <summary>
        /// Gets or sets the lock ms.
        /// </summary>
        /// <value>The lock ms.</value>
        public int LockMs { get; set; } = 5000;
    }
}
