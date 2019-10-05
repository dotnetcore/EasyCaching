namespace EasyCaching.Core
{
    public interface IHybridProviderFactory
    {
        /// <summary>
        /// Gets the hybrid provider.
        /// </summary>
        /// <returns>The hybrid provider.</returns>
        /// <param name="name">Name.</param>        
        IHybridCachingProvider GetHybridCachingProvider(string name);
    }
}
