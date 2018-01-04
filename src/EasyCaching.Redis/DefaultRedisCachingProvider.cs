namespace EasyCaching.Redis
{
    using EasyCaching.Core;
    using Microsoft.Extensions.Options;
    using StackExchange.Redis;
    using System;
    using System.Threading;

    /// <summary>
    /// Default redis caching provider.
    /// </summary>
    public class DefaultRedisCachingProvider : IEasyCachingProvider
    {
        /// <summary>
        /// The connection.
        /// </summary>
        private volatile ConnectionMultiplexer _connection;

        /// <summary>
        /// The cache.
        /// </summary>
        private IDatabase _cache;

        /// <summary>
        /// The options.
        /// </summary>
        private readonly RedisCacheOptions _options;

        /// <summary>
        /// The connection lock.
        /// </summary>
        private readonly SemaphoreSlim _connectionLock = new SemaphoreSlim(initialCount: 1, maxCount: 1);

        /// <summary>
        /// Initializes a new instance of the <see cref="T:EasyCaching.Redis.DefaultRedisCachingProvider"/> class.
        /// </summary>
        /// <param name="options">Options.</param>
        public DefaultRedisCachingProvider(IOptions<RedisCacheOptions> options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            _options = options.Value;
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
            if (cacheKey == null)
            {
                throw new ArgumentNullException(nameof(cacheKey));
            }

            Connect();

            var result = _cache.StringGet(cacheKey);
            if (!result.IsNull)
                return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(result);

            var item = dataRetriever?.Invoke();
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
            if (cacheKey == null)
            {
                throw new ArgumentNullException(nameof(cacheKey));
            }

            Connect();

            var result = _cache.StringGet(cacheKey);
            if (!result.IsNull)
                return Newtonsoft.Json.JsonConvert.DeserializeObject(result);

            var item = dataRetriever?.Invoke();
            Set(cacheKey, item, expiration);

            return item;
        }

        /// <summary>
        /// Remove the specified cacheKey.
        /// </summary>
        /// <returns>The remove.</returns>
        /// <param name="cacheKey">Cache key.</param>
        public void Remove(string cacheKey)
        {
            if (cacheKey == null)
            {
                throw new ArgumentNullException(nameof(cacheKey));
            }

            Connect();

            _cache.KeyDelete(cacheKey);
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
            if (cacheKey == null)
            {
                throw new ArgumentNullException(nameof(cacheKey));
            }

            if (cacheValue == null)
            {
                throw new ArgumentNullException(nameof(cacheValue));
            }

            Connect();

            _cache.StringSet(cacheKey, Newtonsoft.Json.JsonConvert.SerializeObject(cacheValue), expiration);
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

            if (cacheKey == null)
            {
                throw new ArgumentNullException(nameof(cacheKey));
            }

            if (cacheValue == null)
            {
                throw new ArgumentNullException(nameof(cacheValue));
            }

            Connect();

            _cache.StringSet(cacheKey, Newtonsoft.Json.JsonConvert.SerializeObject(cacheValue), expiration);
        }

        /// <summary>
        /// Connect this instance.
        /// </summary>
        private void Connect()
        {
            if (_connection != null)
            {
                return;
            }

            _connectionLock.Wait();
            try
            {
                if (_connection == null)
                {
                    var configurationOptions = new ConfigurationOptions
                    {
                        ConnectTimeout = _options.ConnectionTimeout,
                        Password = _options.Password,
                        Ssl = _options.IsSsl,
                        SslHost = _options.SslHost,
                    };

                    foreach (var endpoint in _options.Endpoints)
                    {
                        configurationOptions.EndPoints.Add(endpoint.Host, endpoint.Port);
                    }

                    _connection = ConnectionMultiplexer.Connect(configurationOptions.ToString());

                    _cache = _connection.GetDatabase(_options.Database);
                }
            }
            finally
            {
                _connectionLock.Release();
            }
        }
    }
}
