namespace EasyCaching.Core
{
    using System.Reflection;

    /// <summary>
    /// Easycaching key generator.
    /// </summary>
    public interface IEasyCachingKeyGenerator
    {

        /// <summary>
        /// Gets the cache key.
        /// </summary>
        /// <returns>The cache key.</returns>
        /// <param name="methodInfo">Method info.</param>
        /// <param name="prefix">Prefix.</param>
        string GetCacheKey(MethodInfo methodInfo, string prefix);
    }
}
