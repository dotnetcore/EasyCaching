namespace EasyCaching.Memory
{
    using System;
    using EasyCaching.Core;
    using Microsoft.Extensions.Caching.Memory;

    /// <summary>
    /// MemoryCaching provider.
    /// </summary>
    public class MemoryCachingProvider : IEasyCachingProvider
    {
        /// <summary>
        /// The MemoryCache.
        /// </summary>
        private readonly IMemoryCache _cache;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:EasyCaching.Memory.MemoryCachingProvider"/> class.
        /// </summary>
        /// <param name="cache">Microsoft MemoryCache.</param>
        public MemoryCachingProvider(IMemoryCache cache)
        {
            this._cache = cache;
        }

        /// <summary>
        /// Get cacheValue by specified cacheKey.
        /// </summary>
        /// <returns>The cacheValue.</returns>
        /// <param name="cacheKey">Cache key.</param>
        public object Get(string cacheKey)
        {
            if (string.IsNullOrWhiteSpace(cacheKey))
                throw new ArgumentNullException(nameof(cacheKey));

            return _cache.Get(cacheKey);
        }

        /// <summary>
        /// Set the specified cacheEntry.
        /// </summary>
        /// <returns>The set.</returns>
        /// <param name="cacheEntry">Cache entry.</param>
        public void Set(CacheEntry cacheEntry)
        {
            _cache.Set(cacheEntry.CacheKey, cacheEntry.CacheValue, cacheEntry.AbsoluteExpirationRelativeToNow);
        }
    }
}
