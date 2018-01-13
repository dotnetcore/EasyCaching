namespace EasyCaching.SQLite
{
    using EasyCaching.Core;
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// SQLiteCaching provider.
    /// </summary>
    public class SQLiteCachingProvider : IEasyCachingProvider
    {
        /// <summary>
        /// <see cref="T:EasyCaching.SQLite.SQLiteCachingProvider"/> is not distributed cache.
        /// </summary>
        /// <value><c>true</c> if is distributed cache; otherwise, <c>false</c>.</value>
        public bool IsDistributedCache => false;

        public bool Exists(string cacheKey)
        {
            throw new NotImplementedException();
        }

        public Task<bool> ExistsAsync(string cacheKey)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Get the specified cacheKey, dataRetriever and expiration.
        /// </summary>
        /// <returns>The get.</returns>
        /// <param name="cacheKey">Cache key.</param>
        /// <param name="dataRetriever">Data retriever.</param>
        /// <param name="expiration">Expiration.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public CacheValue<T> Get<T>(string cacheKey, Func<T> dataRetriever, TimeSpan expiration) where T : class
        {
            var result = SQLHelper.Instance.Get(cacheKey) as T;

            if (result != null)
                return new CacheValue<T>(result, true);

            if (dataRetriever != null)
            {
                result = dataRetriever.Invoke();
                Set(cacheKey, result, expiration);
                return new CacheValue<T>(result, true);
            }
            else
            {
                return CacheValue<T>.NoValue;
            }
        }

        public Task<CacheValue<T>> GetAsync<T>(string cacheKey, Func<Task<T>> dataRetriever, TimeSpan expiration) where T : class
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Remove the specified cacheKey.
        /// </summary>
        /// <returns>The remove.</returns>
        /// <param name="cacheKey">Cache key.</param>
        public void Remove(string cacheKey)
        {
            SQLHelper.Instance.Delete(cacheKey);
        }

        public Task RemoveAsync(string cacheKey)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Set the specified cacheKey, cacheValue and absoluteExpirationRelativeToNow.
        /// </summary>
        /// <returns>The set.</returns>
        /// <param name="cacheKey">Cache key.</param>
        /// <param name="cacheValue">Cache value.</param>
        /// <param name="expiration">Expiration.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public void Set<T>(string cacheKey, T cacheValue, TimeSpan expiration) where T : class
        {
            SQLHelper.Instance.Set(cacheKey, Newtonsoft.Json.JsonConvert.SerializeObject(cacheValue), expiration.Ticks / 10000000);
        }

        public Task SetAsync<T>(string cacheKey, T cacheValue, TimeSpan expiration) where T : class
        {
            throw new NotImplementedException();
        }
    }
}
