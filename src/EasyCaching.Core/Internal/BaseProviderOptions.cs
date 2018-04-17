namespace EasyCaching.Core.Internal
{
    using System;

    /// <summary>
    /// Base provider options.
    /// </summary>
    public class BaseProviderOptions
    {
        /// <summary>
        /// Gets or sets the type of the caching provider.
        /// </summary>
        /// <remarks>
        /// Reserved, do not used.
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
    }


}
