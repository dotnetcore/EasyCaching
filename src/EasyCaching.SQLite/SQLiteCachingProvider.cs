namespace EasyCaching.SQLite
{    
    using EasyCaching.Core;

    /// <summary>
    /// SQLiteCaching provider.
    /// </summary>
    public class SQLiteCachingProvider : IEasyCachingProvider
    {        
        /// <summary>
        /// Get the specified cacheKey.
        /// </summary>
        /// <returns>The get.</returns>
        /// <param name="cacheKey">Cache key.</param>
        public object Get(string cacheKey)
        {
            return SQLHelper.Instance.Get(cacheKey);
        }

        /// <summary>
        /// Set the specified cacheEntry.
        /// </summary>
        /// <returns>The set.</returns>
        /// <param name="cacheEntry">Cache entry.</param>
        public void Set(CacheEntry cacheEntry)
        {
            SQLHelper.Instance.Set(cacheEntry.CacheKey, Newtonsoft.Json.JsonConvert.SerializeObject( cacheEntry.CacheValue), cacheEntry.AbsoluteExpirationRelativeToNow.Ticks/10000000);
        }
    }
}
