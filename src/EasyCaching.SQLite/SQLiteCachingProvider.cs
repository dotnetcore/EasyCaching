namespace EasyCaching.SQLite
{
    using EasyCaching.Core;
    using System;

    /// <summary>
    /// SQLiteCaching provider.
    /// </summary>
    public class SQLiteCachingProvider : IEasyCachingProvider
    {
        /// <summary>
        /// Get the specified cacheKey, dataRetriever and expiration.
        /// </summary>
        /// <returns>The get.</returns>
        /// <param name="cacheKey">Cache key.</param>
        /// <param name="dataRetriever">Data retriever.</param>
        /// <param name="expiration">Expiration.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public T Get<T>(string cacheKey, Func<T> dataRetriever, TimeSpan expiration) where T : class
        {
            var result = SQLHelper.Instance.Get(cacheKey) as T;

            if (result != null)
                return result;

            if (dataRetriever != null)
            {
                result = dataRetriever.Invoke();
                Set(cacheKey, result, expiration);
            }

            return result;
        }

        /// <summary>
        /// Get the specified cacheKey, dataRetriever and expiration.
        /// </summary>
        /// <returns>The get.</returns>
        /// <param name="cacheKey">Cache key.</param>
        /// <param name="dataRetriever">Data retriever.</param>
        /// <param name="expiration">Expiration.</param>
        public object Get(string cacheKey, Func<object> dataRetriever, TimeSpan expiration)
        {
            var result = SQLHelper.Instance.Get(cacheKey);

            if (result != null)
                return result;

            if (dataRetriever != null)
            {
                result = dataRetriever.Invoke();
                Set(cacheKey, result, expiration);
            }

            return result;
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

        /// <summary>
        /// Set the specified cacheKey, cacheValue and absoluteExpirationRelativeToNow.
        /// </summary>
        /// <returns>The set.</returns>
        /// <param name="cacheKey">Cache key.</param>
        /// <param name="cacheValue">Cache value.</param>
        /// <param name="expiration">Expiration.</param>
        public void Set(string cacheKey, object cacheValue, TimeSpan expiration)
        {
            SQLHelper.Instance.Set(cacheKey, Newtonsoft.Json.JsonConvert.SerializeObject(cacheValue), expiration.Ticks / 10000000);
        }
    }
}
