namespace EasyCaching.Core
{
    /// <summary>
    /// EasyCaching provider factory.
    /// </summary>
    public interface IEasyCachingProviderFactory
    {
        /// <summary>
        /// Gets the caching provider.
        /// </summary>
        /// <returns>The caching provider.</returns>
        /// <param name="name">Name.</param>
        IEasyCachingProvider GetCachingProvider(string name);
    }
}
