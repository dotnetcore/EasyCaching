namespace EasyCaching.Core.Interceptor
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
