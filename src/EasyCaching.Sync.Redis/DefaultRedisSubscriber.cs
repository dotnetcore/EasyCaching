namespace EasyCaching.Sync.Redis
{
    using EasyCaching.Core;
    using EasyCaching.Core.Internal;
    using StackExchange.Redis;
    using System;
    using System.Threading;
    using Microsoft.Extensions.Options;

    /// <summary>
    /// Default redis subscriber.
    /// </summary>
    public class DefaultRedisSubscriber : IEasyCachingSubscriber
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
        /// The local cache.
        /// </summary>
        private readonly IEasyCachingProvider _localCache;

        /// <summary>
        /// The serialize.
        /// </summary>
        private readonly IEasyCachingSerializer _serialize;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:EasyCaching.Sync.Redis.DefaultRedisSubscriber"/> class.
        /// </summary>
        /// <param name="options">Options.</param>
        /// <param name="localCache">Local cache.</param>
        /// <param name="serialize">Serialize.</param>
        public DefaultRedisSubscriber(
            IOptions<RedisCacheOptions> options,
            IEasyCachingProvider localCache, 
            IEasyCachingSerializer serialize)
        {
            this._options = options.Value;
            this._localCache = localCache;
            this._serialize = serialize;
        }

        /// <summary>
        /// Subscribe the specified channel with notifyType.
        /// </summary>
        /// <returns>The subscribe.</returns>
        /// <param name="channel">Channel.</param>
        /// <param name="notifyType">Notify type.</param>
        public void Subscribe(string channel, NotifyType notifyType)
        {
            switch (notifyType)
            {
                case NotifyType.Add:
                    _connection.GetSubscriber().Subscribe(channel, CacheAddAction);
                    break;
                case NotifyType.Update:
                    _connection.GetSubscriber().Subscribe(channel, CacheUpdateAction);
                    break;
                case NotifyType.Delete:
                    _connection.GetSubscriber().Subscribe(channel, CacheDeleteAction);
                    break;
            }
        }

        private void CacheDeleteAction(RedisChannel arg1, RedisValue arg2)
        {
            throw new NotImplementedException();
        }

        private void CacheAddAction(RedisChannel arg1, RedisValue arg2)
        {
            throw new NotImplementedException();
        }

        private void CacheUpdateAction(RedisChannel arg1, RedisValue arg2)
        {
            throw new NotImplementedException();
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
