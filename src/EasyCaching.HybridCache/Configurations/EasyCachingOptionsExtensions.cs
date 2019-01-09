namespace EasyCaching.HybridCache
{
    using EasyCaching.Core;

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
        public static EasyCachingOptions UseHybrid(this EasyCachingOptions options)
        {
            options.RegisterExtension(new HybridCacheOptionsExtension());

            return options;
        }
    }
}
