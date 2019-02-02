namespace EasyCaching.HybridCache
{
    using System;
    using EasyCaching.Core.Configurations;

    /// <summary>
    /// EasyCaching options extensions.
    /// </summary>
    public static class EasyCachingOptionsExtensions
    {
        /// <summary>
        /// Uses the hybrid.
        /// </summary>
        /// <returns>The hybrid.</returns>
        /// <param name="options">Options.</param>
        public static EasyCachingOptions UseHybrid(this EasyCachingOptions options, Action<HybridCachingOptions> configure)
        {
            options.RegisterExtension(new HybridCacheOptionsExtension(configure));

            return options;
        }
    }
}
