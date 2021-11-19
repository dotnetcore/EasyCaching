﻿namespace EasyCaching.Redis
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using EasyCaching.Core;
    using EasyCaching.Core.Serialization;
    using Microsoft.Extensions.Logging;
    using StackExchange.Redis;

    /// <summary>
    /// Default redis caching provider.
    /// </summary>
    public partial class DefaultRedisCachingProvider : EasyCachingAbstractProvider
    {
        /// <summary>
        /// The cache.
        /// </summary>
        private readonly IDatabase _cache;

        /// <summary>
        /// The servers.
        /// </summary>
        private readonly IEnumerable<IServer> _servers;

        /// <summary>
        /// The db provider.
        /// </summary>
        private readonly IRedisDatabaseProvider _dbProvider;

        /// <summary>
        /// The serializer.
        /// </summary>
        private readonly IEasyCachingSerializer _serializer;

        /// <summary>
        /// The options.
        /// </summary>
        private readonly RedisOptions _options;

        /// <summary>
        /// The cache stats.
        /// </summary>
        private readonly CacheStats _cacheStats;

        /// <summary>
        /// The name.
        /// </summary>
        private readonly string _name;

        private readonly ProviderInfo _info;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="T:EasyCaching.Redis.DefaultRedisCachingProvider"/> class.
        /// </summary>
        /// <param name="name">Name.</param>
        /// <param name="dbProviders">Db providers.</param>
        /// <param name="serializers">Serializers.</param>
        /// <param name="options">Options.</param>
        /// <param name="loggerFactory">Logger factory.</param>
        public DefaultRedisCachingProvider(
            string name,
            IEnumerable<IRedisDatabaseProvider> dbProviders,
            IEnumerable<IEasyCachingSerializer> serializers,
            RedisOptions options,
            ILoggerFactory loggerFactory = null)
        {
            ArgumentCheck.NotNullAndCountGTZero(dbProviders, nameof(dbProviders));
            ArgumentCheck.NotNullAndCountGTZero(serializers, nameof(serializers));

            this._name = name;
            this._dbProvider = dbProviders.Single(x => x.DBProviderName.Equals(name));            
            this._options = options;

            if (options.EnableLogging)
            {
                this.Logger = loggerFactory.CreateLogger<DefaultRedisCachingProvider>();
            }
            
            this._cache = _dbProvider.GetDatabase();
            this._servers = _dbProvider.GetServerList();
            this._cacheStats = new CacheStats();

            this._serializer = !string.IsNullOrWhiteSpace(options.SerializerName)
                ? serializers.Single(x => x.Name.Equals(options.SerializerName))
                : serializers.FirstOrDefault(x => x.Name.Equals(_name)) ?? serializers.Single(x => x.Name.Equals(EasyCachingConstValue.DefaultSerializerName));

            this.ProviderName = this._name;
            this.ProviderType = CachingProviderType.Redis;
            this.ProviderStats = this._cacheStats;
            this.ProviderMaxRdSecond = _options.MaxRdSecond;

            _info = new ProviderInfo
            {
                CacheStats = _cacheStats,
                EnableLogging = options.EnableLogging,
                LockMs = options.LockMs,
                MaxRdSecond = options.MaxRdSecond,
                ProviderName = ProviderName,
                ProviderType = ProviderType,
                SerializerName = options.SerializerName,
                SleepMs = options.SleepMs,
                Serializer = _serializer,
                CacheNulls = options.CacheNulls
            };
        }

        /// <summary>
        /// Get the specified cacheKey, dataRetriever and expiration.
        /// </summary>
        /// <returns>The get.</returns>
        /// <param name="cacheKey">Cache key.</param>
        /// <param name="dataRetriever">Data retriever.</param>
        /// <param name="expiration">Expiration.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public override CacheValue<T> BaseGet<T>(string cacheKey, Func<T> dataRetriever, TimeSpan expiration)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));
            ArgumentCheck.NotNegativeOrZero(expiration, nameof(expiration));

            var redisValue = _cache.StringGet(cacheKey);
            var result = Deserialize<T>(cacheKey, redisValue);
            TrackCacheStats(cacheKey, result);

            if (result.HasValue) 
                return result;
            
            if (!_cache.StringSet($"{cacheKey}_Lock", 1, TimeSpan.FromMilliseconds(_options.LockMs), When.NotExists))
            {
                System.Threading.Thread.Sleep(_options.SleepMs);
                return Get(cacheKey, dataRetriever, expiration);
            }

            var item = dataRetriever();
            if (item != null || _options.CacheNulls)
            {
                Set(cacheKey, item, expiration);
                //remove mutex key
                _cache.KeyDelete($"{cacheKey}_Lock");
                result = new CacheValue<T>(item, true);
            }
            else
            {
                //remove mutex key
                _cache.KeyDelete($"{cacheKey}_Lock");
                result = CacheValue<T>.NoValue;
            }

            return result;
        }

        /// <summary>
        /// Get the specified cacheKey.
        /// </summary>
        /// <returns>The get.</returns>
        /// <param name="cacheKey">Cache key.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public override CacheValue<T> BaseGet<T>(string cacheKey)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            var redisValue = _cache.StringGet(cacheKey);
            var result = Deserialize<T>(cacheKey, redisValue);
            TrackCacheStats(cacheKey, result);

            return result;
        }

        /// <summary>
        /// Remove the specified cacheKey.
        /// </summary>
        /// <returns>The remove.</returns>
        /// <param name="cacheKey">Cache key.</param>
        public override void BaseRemove(string cacheKey)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

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
        public override void BaseSet<T>(string cacheKey, T cacheValue, TimeSpan expiration)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));
            ArgumentCheck.NotNull(cacheValue, nameof(cacheValue), _options.CacheNulls);
            ArgumentCheck.NotNegativeOrZero(expiration, nameof(expiration));

            if (MaxRdSecond > 0)
            {
                var addSec = new Random().Next(1, MaxRdSecond);
                expiration = expiration.Add(TimeSpan.FromSeconds(addSec));
            }

            _cache.StringSet(
                cacheKey,
                _serializer.Serialize(cacheValue),
                expiration);
        }

        /// <summary>
        /// Exists the specified cacheKey.
        /// </summary>
        /// <returns>The exists.</returns>
        /// <param name="cacheKey">Cache key.</param>
        public override bool BaseExists(string cacheKey)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            return _cache.KeyExists(cacheKey);
        }

        /// <summary>
        /// Removes cached item by cachekey's prefix.
        /// </summary>
        /// <param name="prefix">Prefix of CacheKey.</param>
        public override void BaseRemoveByPrefix(string prefix)
        {
            ArgumentCheck.NotNullOrWhiteSpace(prefix, nameof(prefix));

            prefix = this.HandlePrefix(prefix);

            Logger?.LogDebug("RemoveByPrefix : prefix = {0}", prefix);

            var redisKeys = this.SearchRedisKeys(prefix);

            _cache.KeyDelete(redisKeys);
        }

        /// <summary>
        /// Searchs the redis keys.
        /// </summary>
        /// <returns>The redis keys.</returns>
        /// <remarks>
        /// If your Redis Servers support command SCAN , 
        /// IServer.Keys will use command SCAN to find out the keys.
        /// Following 
        /// https://github.com/StackExchange/StackExchange.Redis/blob/master/StackExchange.Redis/StackExchange/Redis/RedisServer.cs#L289
        /// </remarks>
        /// <param name="pattern">Pattern.</param>
        private RedisKey[] SearchRedisKeys(string pattern)
        {
            var keys = new List<RedisKey>();

            foreach (var server in _servers)
                // the default pageSize is 10, if there are too many keys here, it will hurt performance
                // see https://github.com/dotnetcore/EasyCaching/pull/199 for more information
                // from this redis dev specification, https://yq.aliyun.com/articles/531067 , maybe the appropriate scope is 100~500, using 200 here.
                keys.AddRange(server.Keys(pattern: pattern, database: _cache.Database, pageSize: 200));

            return keys.Distinct().ToArray();

            //var keys = new HashSet<RedisKey>();

            //int nextCursor = 0;
            //do
            //{
            //    RedisResult redisResult = _cache.Execute("SCAN", nextCursor.ToString(), "MATCH", pattern, "COUNT", "1000");
            //    var innerResult = (RedisResult[])redisResult;

            //    nextCursor = int.Parse((string)innerResult[0]);

            //    List<RedisKey> resultLines = ((RedisKey[])innerResult[1]).ToList();

            //    keys.UnionWith(resultLines);
            //}
            //while (nextCursor != 0);

            //return keys.ToArray();
        }

        /// <summary>
        /// Handles the prefix of CacheKey.
        /// </summary>
        /// <param name="prefix">Prefix of CacheKey.</param>
        /// <exception cref="ArgumentException"></exception>
        private string HandlePrefix(string prefix)
        {
            // Forbid
            if (prefix.Equals("*"))
                throw new ArgumentException("the prefix should not equal to *");

            // Don't start with *
            prefix = new System.Text.RegularExpressions.Regex("^\\*+").Replace(prefix, "");

            // End with *
            if (!prefix.EndsWith("*", StringComparison.OrdinalIgnoreCase))
                prefix = string.Concat(prefix, "*");

            return prefix;
        }

        /// <summary>
        /// Sets all.
        /// </summary>
        /// <param name="values">Values.</param>
        /// <param name="expiration">Expiration.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public override void BaseSetAll<T>(IDictionary<string, T> values, TimeSpan expiration)
        {
            ArgumentCheck.NotNegativeOrZero(expiration, nameof(expiration));
            ArgumentCheck.NotNullAndCountGTZero(values, nameof(values));

            var batch = _cache.CreateBatch();

            foreach (var item in values)
                batch.StringSetAsync(item.Key, _serializer.Serialize(item.Value), expiration);

            batch.Execute();
        }

        /// <summary>
        /// Gets all.
        /// </summary>
        /// <returns>The all.</returns>
        /// <param name="cacheKeys">Cache keys.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public override IDictionary<string, CacheValue<T>> BaseGetAll<T>(IEnumerable<string> cacheKeys)
        {
            ArgumentCheck.NotNullAndCountGTZero(cacheKeys, nameof(cacheKeys));

            var keyArray = cacheKeys.ToArray();
            var values = _cache.StringGet(keyArray.Select(k => (RedisKey)k).ToArray());

            return DeserializeAll<T>(keyArray, values);
        }

        /// <summary>
        /// Gets the by prefix.
        /// </summary>
        /// <returns>The by prefix.</returns>
        /// <param name="prefix">Prefix.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public override IDictionary<string, CacheValue<T>> BaseGetByPrefix<T>(string prefix)
        {
            ArgumentCheck.NotNullOrWhiteSpace(prefix, nameof(prefix));

            prefix = this.HandlePrefix(prefix);

            var redisKeys = this.SearchRedisKeys(prefix);
            
            var values = _cache.StringGet(redisKeys).ToArray();
            
            return DeserializeAll<T>(redisKeys, values);
        }

        /// <summary>
        /// Removes all.
        /// </summary>
        /// <param name="cacheKeys">Cache keys.</param>
        public override void BaseRemoveAll(IEnumerable<string> cacheKeys)
        {
            ArgumentCheck.NotNullAndCountGTZero(cacheKeys, nameof(cacheKeys));

            var redisKeys = cacheKeys.Where(k => !string.IsNullOrEmpty(k)).Select(k => (RedisKey)k).ToArray();
            if (redisKeys.Length > 0)
                _cache.KeyDelete(redisKeys);
        }

        /// <summary>
        /// Gets the count.
        /// </summary>
        /// <returns>The count.</returns>
        /// <param name="prefix">Prefix.</param>
        public override int BaseGetCount(string prefix = "")
        {
            if (string.IsNullOrWhiteSpace(prefix))
            {
                var allCount = 0;

                foreach (var server in _servers)
                    allCount += (int)server.DatabaseSize(_cache.Database);

                return allCount;
            }

            return this.SearchRedisKeys(this.HandlePrefix(prefix)).Length;
        }

        /// <summary>
        /// Flush All Cached Item.
        /// </summary>
        public override void BaseFlush()
        {
            Logger?.LogDebug("Redis -- Flush");

            foreach (var server in _servers)
            {
                server.FlushDatabase(_cache.Database);
            }
        }

        /// <summary>
        /// Tries the set.
        /// </summary>
        /// <returns><c>true</c>, if set was tryed, <c>false</c> otherwise.</returns>
        /// <param name="cacheKey">Cache key.</param>
        /// <param name="cacheValue">Cache value.</param>
        /// <param name="expiration">Expiration.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public override bool BaseTrySet<T>(string cacheKey, T cacheValue, TimeSpan expiration)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));
            ArgumentCheck.NotNull(cacheValue, nameof(cacheValue), _options.CacheNulls);
            ArgumentCheck.NotNegativeOrZero(expiration, nameof(expiration));

            if (MaxRdSecond > 0)
            {
                var addSec = new Random().Next(1, MaxRdSecond);
                expiration = expiration.Add(TimeSpan.FromSeconds(addSec));
            }

            return _cache.StringSet(
                cacheKey,
                _serializer.Serialize(cacheValue),
                expiration,
                When.NotExists
                );
        }

        /// <summary>
        /// Get the expiration of cache key
        /// </summary>
        /// <param name="cacheKey">cache key</param>
        /// <returns>expiration</returns>
        public override TimeSpan BaseGetExpiration(string cacheKey)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            var timeSpan = _cache.KeyTimeToLive(cacheKey);
            return timeSpan.HasValue ? timeSpan.Value : TimeSpan.Zero;
        }

        /// <summary>
        /// Get te information of this provider.
        /// </summary>
        /// <returns></returns>
        public override ProviderInfo BaseGetProviderInfo()
        {
            return _info;
        }

        private CacheValue<T> Deserialize<T>(string cacheKey, RedisValue redisValue)
        {
            if (redisValue.IsNull)
            {
                return CacheValue<T>.NoValue;
            }
            
            try
            {
                var value = _serializer.Deserialize<T>(redisValue);
                if (value == null && !_options.CacheNulls)
                    return CacheValue<T>.NoValue;
                return new CacheValue<T>(value, true);
            }
            catch (Exception ex)
            {
                Logger?.LogWarning(ex, "Error while deserializing cache value with key '{0}'.", cacheKey);
                return CacheValue<T>.NoValue;
            }
        }

        private IDictionary<string, CacheValue<T>> DeserializeAll<T>(RedisKey[] cacheKeys, RedisValue[] redisValues)
        {
            var result = new Dictionary<string, CacheValue<T>>(cacheKeys.Length);
            for (int i = 0; i < cacheKeys.Length; i++)
            {
                var cacheKey = cacheKeys[i];
                var redisValue = redisValues[i];
                result.Add(cacheKey, Deserialize<T>(cacheKey, redisValue));
            }

            return result;
        }

        private IDictionary<string, CacheValue<T>> DeserializeAll<T>(string[] cacheKeys, RedisValue[] redisValues)
        {
            var result = new Dictionary<string, CacheValue<T>>(cacheKeys.Length);
            for (int i = 0; i < cacheKeys.Length; i++)
            {
                var cacheKey = cacheKeys[i];
                var redisValue = redisValues[i];
                result.Add(cacheKey, Deserialize<T>(cacheKey, redisValue));
            }

            return result;
        }
    }
}
