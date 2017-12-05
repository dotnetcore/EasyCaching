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
        /// Get the specified cacheKey and dataRetriever.
        /// </summary>
        /// <returns>The get.</returns>
        /// <param name="cacheKey">Cache key.</param>
        /// <param name="dataRetriever">Data retriever.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public T Get<T>(string cacheKey, Func<T> dataRetriever = null) where T : class
        {
            var result = SQLHelper.Instance.Get(cacheKey) as T;

            if (result != null)
                return result;

            if (dataRetriever != null)
            {
                result = dataRetriever.Invoke();
                Set(cacheKey, result, TimeSpan.FromMinutes(10));
            }

            return result;
        }

        /// <summary>
        /// Get the specified cacheKey and dataRetriever.
        /// </summary>
        /// <returns>The get.</returns>
        /// <param name="cacheKey">Cache key.</param>
        /// <param name="dataRetriever">Data retriever.</param>
        public object Get(string cacheKey, Func<object> dataRetriever = null)
        {
            var result = SQLHelper.Instance.Get(cacheKey);

            if (result != null)
                return result;

            if (dataRetriever != null)
            {
                result = dataRetriever.Invoke();
                Set(cacheKey, result, TimeSpan.FromMinutes(10));
            }

            return result;
        }

        /// <summary>
        /// Set the specified cacheKey, cacheValue and absoluteExpirationRelativeToNow.
        /// </summary>
        /// <returns>The set.</returns>
        /// <param name="cacheKey">Cache key.</param>
        /// <param name="cacheValue">Cache value.</param>
        /// <param name="absoluteExpirationRelativeToNow">Absolute expiration relative to now.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public void Set<T>(string cacheKey, T cacheValue, TimeSpan absoluteExpirationRelativeToNow) where T : class
        {
            SQLHelper.Instance.Set(cacheKey, Newtonsoft.Json.JsonConvert.SerializeObject(cacheValue), absoluteExpirationRelativeToNow.Ticks / 10000000);
        }

        /// <summary>
        /// Set the specified cacheKey, cacheValue and absoluteExpirationRelativeToNow.
        /// </summary>
        /// <returns>The set.</returns>
        /// <param name="cacheKey">Cache key.</param>
        /// <param name="cacheValue">Cache value.</param>
        /// <param name="absoluteExpirationRelativeToNow">Absolute expiration relative to now.</param>
        public void Set(string cacheKey, object cacheValue, TimeSpan absoluteExpirationRelativeToNow)
        {
            SQLHelper.Instance.Set(cacheKey, Newtonsoft.Json.JsonConvert.SerializeObject(cacheValue), absoluteExpirationRelativeToNow.Ticks / 10000000);
        }
    }
}
