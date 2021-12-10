﻿namespace EasyCaching.Core
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    /// <summary>
    /// EasyCaching provider.
    /// </summary>
    public interface IEasyCachingProvider
    {
        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>The name.</value>
        string Name { get; }

        /// <summary>
        /// Set the specified cacheKey, cacheValue and expiration.
        /// </summary>
        /// <param name="cacheKey">Cache key.</param>
        /// <param name="cacheValue">Cache value.</param>
        /// <param name="expiration">Expiration.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        void Set<T>(string cacheKey, T cacheValue, TimeSpan expiration);

        /// <summary>
        /// Sets the specified cacheKey, cacheValue and expiration async.
        /// </summary>
        /// <returns>The async.</returns>
        /// <param name="cacheKey">Cache key.</param>
        /// <param name="cacheValue">Cache value.</param>
        /// <param name="expiration">Expiration.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        Task SetAsync<T>(string cacheKey, T cacheValue, TimeSpan expiration);

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
        /// Get the specified cacheKey.
        /// </summary>
        /// <returns>The get.</returns>
        /// <param name="cacheKey">Cache key.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        CacheValue<T> Get<T>(string cacheKey);

        /// <summary>
        /// Get the specified cacheKey async.
        /// </summary>
        /// <returns>The async.</returns>
        /// <param name="cacheKey">Cache key.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        Task<CacheValue<T>> GetAsync<T>(string cacheKey);

        /// <summary>
        /// Remove the specified cacheKey.
        /// </summary>
        /// <param name="cacheKey">Cache key.</param>
        void Remove(string cacheKey);

        /// <summary>
        /// Remove the specified cacheKey async.
        /// </summary>
        /// <returns>The async.</returns>
        /// <param name="cacheKey">Cache key.</param>
        Task RemoveAsync(string cacheKey);

        /// <summary>
        /// Exists the specified cacheKey async.
        /// </summary>
        /// <returns>The async.</returns>
        /// <param name="cacheKey">Cache key.</param>
        Task<bool> ExistsAsync(string cacheKey);

        /// <summary>
        /// Exists the specified cacheKey.
        /// </summary>
        /// <returns>The exists.</returns>
        /// <param name="cacheKey">Cache key.</param>
        bool Exists(string cacheKey);

        /// <summary>
        /// Tries the set.
        /// </summary>
        /// <returns><c>true</c>, if set was tryed, <c>false</c> otherwise.</returns>
        /// <param name="cacheKey">Cache key.</param>
        /// <param name="cacheValue">Cache value.</param>
        /// <param name="expiration">Expiration.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        bool TrySet<T>(string cacheKey, T cacheValue, TimeSpan expiration);

        /// <summary>
        /// Tries the set async.
        /// </summary>
        /// <returns>The set async.</returns>
        /// <param name="cacheKey">Cache key.</param>
        /// <param name="cacheValue">Cache value.</param>
        /// <param name="expiration">Expiration.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        Task<bool> TrySetAsync<T>(string cacheKey, T cacheValue, TimeSpan expiration);

        /// <summary>
        /// Sets all.
        /// </summary>
        /// <param name="value">Value.</param>
        /// <param name="expiration">Expiration.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        void SetAll<T>(IDictionary<string, T> value, TimeSpan expiration);

        /// <summary>
        /// Sets all async.
        /// </summary>
        /// <returns>The all async.</returns>
        /// <param name="value">Value.</param>
        /// <param name="expiration">Expiration.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        Task SetAllAsync<T>(IDictionary<string, T> value, TimeSpan expiration);

        /// <summary>
        /// Removes all.
        /// </summary>
        /// <param name="cacheKeys">Cache keys.</param>
        void RemoveAll(IEnumerable<string> cacheKeys);

        /// <summary>
        /// Removes all async.
        /// </summary>
        /// <returns>The all async.</returns>
        /// <param name="cacheKeys">Cache keys.</param>
        Task RemoveAllAsync(IEnumerable<string> cacheKeys);

        /// <summary>
        /// Get the specified cacheKey, dataRetriever and expiration.
        /// </summary>
        /// <returns>The get.</returns>
        /// <param name="cacheKey">Cache key.</param>
        /// <param name="dataRetriever">Data retriever.</param>
        /// <param name="expiration">Expiration.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        CacheValue<T> Get<T>(string cacheKey, Func<T> dataRetriever, TimeSpan expiration);

        /// <summary>
        /// Gets the specified cacheKey, dataRetriever and expiration async.
        /// </summary>
        /// <returns>The async.</returns>
        /// <param name="cacheKey">Cache key.</param>
        /// <param name="dataRetriever">Data retriever.</param>
        /// <param name="expiration">Expiration.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        Task<CacheValue<T>> GetAsync<T>(string cacheKey, Func<Task<T>> dataRetriever, TimeSpan expiration);

        /// <summary>
        /// Removes cached item by cachekey's prefix.
        /// </summary>
        /// <param name="prefix">Prefix of CacheKey.</param>
        void RemoveByPrefix(string prefix);

        /// <summary>
        /// Removes cached item by cachekey's prefix async.
        /// </summary>
        /// <param name="prefix">Prefix of CacheKey.</param>
        Task RemoveByPrefixAsync(string prefix);

        /// <summary>
        /// Gets the specified cacheKey async.
        /// </summary>
        /// <returns>The async.</returns>
        /// <param name="cacheKey">Cache key.</param>
        /// <param name="type">Object Type.</param>
        Task<object> GetAsync(string cacheKey, Type type);

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
        /// Gets the information of provider.
        /// </summary>
        /// <returns></returns>
        ProviderInfo GetProviderInfo();
    }
}
