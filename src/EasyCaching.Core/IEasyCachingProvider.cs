namespace EasyCaching.Core
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    /// <summary>
    /// EasyCaching provider.
    /// </summary>
    public interface IEasyCachingProvider : IEasyCachingProviderBase
    {
        /// <summary>
        /// Gets a value indicating whether this <see cref="T:EasyCaching.Core.IEasyCachingProvider"/> is distributed cache.
        /// </summary>
        /// <value><c>true</c> if is distributed cache; otherwise, <c>false</c>.</value>
        bool IsDistributedCache { get; }

       /// <summary>
        /// Gets all.
        /// </summary>
        /// <returns>The all.</returns>
        /// <param name="cacheKeys">Cache keys.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        IDictionary<string, CacheValue<T>> GetAll<T>(IEnumerable<string> cacheKeys);

        /// <summary>
        /// Gets all async.
        /// </summary>
        /// <returns>The all async.</returns>
        /// <param name="cacheKeys">Cache keys.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        Task<IDictionary<string, CacheValue<T>>> GetAllAsync<T>(IEnumerable<string> cacheKeys);

        /// <summary>
        /// Gets the by prefix.
        /// </summary>
        /// <returns>The by prefix.</returns>
        /// <param name="prefix">Prefix.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        IDictionary<string, CacheValue<T>> GetByPrefix<T>(string prefix);

        /// <summary>
        /// Gets the by prefix async.
        /// </summary>
        /// <returns>The by prefix async.</returns>
        /// <param name="prefix">Prefix.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        Task<IDictionary<string, CacheValue<T>>> GetByPrefixAsync<T>(string prefix);
       
        /// <summary>
        /// Gets the count.
        /// </summary>
        /// <returns>The count.</returns>
        /// <param name="prefix">Prefix.</param>
        int GetCount(string prefix = "");

        /// <summary>
        /// Gets the count async.
        /// </summary>
        /// <returns>The count.</returns>
        /// <param name="prefix">Prefix.</param>
        Task<int> GetCountAsync(string prefix = "");

        /// <summary>
        /// Flush All Cached Item.
        /// </summary>
        void Flush();

        /// <summary>
        /// Flush All Cached Item async.
        /// </summary>
        /// <returns>The async.</returns>
        Task FlushAsync();

        /// <summary>
        /// Gets the max rd second.
        /// </summary>
        /// <value>The max random second.</value>
        int MaxRdSecond { get; }

        /// <summary>
        /// Gets the type of the caching provider.
        /// </summary>
        /// <value>The type of the caching provider.</value>
        CachingProviderType CachingProviderType { get; }

        /// <summary>
        /// Gets or sets the cache stats.
        /// </summary>
        /// <value>The get stats.</value>
        CacheStats CacheStats { get; }

        /// <summary>
        /// Gets the exporation of specify cachekey.
        /// </summary>
        /// <param name="cacheKey">Cache key.</param>
        /// <returns></returns>
        TimeSpan GetExpiration(string cacheKey);

        /// <summary>
        /// Gets the exporation of specify cachekey async.
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <returns></returns>
        Task<TimeSpan> GetExpirationAsync(string cacheKey);

        /// <summary>
        /// Gets the information of provider.
        /// </summary>
        /// <returns></returns>
        ProviderInfo GetProviderInfo();
    }
}
