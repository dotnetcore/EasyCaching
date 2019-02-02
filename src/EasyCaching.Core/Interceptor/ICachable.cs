namespace EasyCaching.Core.Internal
{
    /// <summary>
    /// Cachable.
    /// </summary>
    public interface ICachable
    {
        /// <summary>
        /// Gets the cache key.
        /// </summary>
        /// <value>The cache key.</value>
        string CacheKey { get; }
    }
}
