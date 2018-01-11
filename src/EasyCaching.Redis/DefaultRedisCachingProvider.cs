namespace EasyCaching.Redis
{
    using EasyCaching.Core;
    using EasyCaching.Core.Internal;
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
        /// The serializer.
        /// </summary>
        private readonly IEasyCachingSerializer _serializer;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:EasyCaching.Redis.DefaultRedisCachingProvider"/> class.
        /// </summary>
        /// <param name="options">Options.</param>
        /// <param name="serializer">Serializer.</param>
        public DefaultRedisCachingProvider(
            IOptions<RedisCacheOptions> options,
            IEasyCachingSerializer serializer)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            if (serializer == null)
            {
                throw new ArgumentNullException(nameof(serializer));
            }

            _options = options.Value;
            _serializer = serializer;
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
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            Connect();

            var result = _cache.StringGet(cacheKey);
            if (!result.IsNull)
                return _serializer.Deserialize<T>(result);

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
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            Connect();

            var result = _cache.StringGet(cacheKey);
            if (!result.IsNull)
                return _serializer.Deserialize<object>(result);

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
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

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
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            ArgumentCheck.NotNull(cacheValue, nameof(cacheValue));

            Connect();

            _cache.StringSet(cacheKey, _serializer.Serialize<T>(cacheValue), expiration);
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
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            ArgumentCheck.NotNull(cacheValue, nameof(cacheValue));

            Connect();

            _cache.StringSet(cacheKey, _serializer.Serialize(cacheValue), expiration);
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
