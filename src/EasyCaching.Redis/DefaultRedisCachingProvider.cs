namespace EasyCaching.Redis
{
    using System;
    using EasyCaching.Core;
    using StackExchange.Redis;

    /// <summary>
    /// Default redis caching provider.
    /// </summary>
    public class DefaultRedisCachingProvider : IEasyCachingProvider
    {
        /// <summary>
        /// The database.
        /// </summary>
        private readonly IDatabase _database;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:EasyCaching.Redis.DefaultRedisCachingProvider"/> class.
        /// </summary>
        /// <param name="database">Database.</param>
        public DefaultRedisCachingProvider(IDatabase database)
        {
            this._database = database;
        }

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
            var result = _database.StringGet(cacheKey);
            if (!result.IsNull)
                return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(result);

            var item = dataRetriever.Invoke();
            Set(cacheKey, item, expiration);

            return item;
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
            var result = _database.StringGet(cacheKey);
            if (!result.IsNull)
                return Newtonsoft.Json.JsonConvert.DeserializeObject(result);

            var item = dataRetriever.Invoke();
            Set(cacheKey, item, expiration);

            return item;
        }

        /// <summary>
        /// Set the specified cacheKey, cacheValue and expiration.
        /// </summary>
        /// <returns>The set.</returns>
        /// <param name="cacheKey">Cache key.</param>
        /// <param name="cacheValue">Cache value.</param>
        /// <param name="expiration">Expiration.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public void Set<T>(string cacheKey, T cacheValue, TimeSpan expiration) where T : class
        {
            _database.StringSet(cacheKey, Newtonsoft.Json.JsonConvert.SerializeObject(cacheValue), expiration);
        }

        /// <summary>
        /// Set the specified cacheKey, cacheValue and expiration.
        /// </summary>
        /// <returns>The set.</returns>
        /// <param name="cacheKey">Cache key.</param>
        /// <param name="cacheValue">Cache value.</param>
        /// <param name="expiration">Expiration.</param>
        public void Set(string cacheKey, object cacheValue, TimeSpan expiration)
        {
            _database.StringSet(cacheKey,Newtonsoft.Json.JsonConvert.SerializeObject(cacheValue),expiration);
        }
    }
}
