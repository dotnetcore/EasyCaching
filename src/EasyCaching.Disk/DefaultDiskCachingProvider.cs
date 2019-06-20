namespace EasyCaching.Disk
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading.Tasks;
    using EasyCaching.Core;
    using MessagePack;
    using Microsoft.Extensions.Logging;

    public class DefaultDiskCachingProvider :  EasyCachingAbstractProvider
    {
        /// <summary>
        /// The options.
        /// </summary>
        private readonly DiskOptions _options;

        /// <summary>
        /// The logger.
        /// </summary>
        private readonly ILogger _logger;

        /// <summary>
        /// The cache stats.
        /// </summary>
        private readonly CacheStats _cacheStats;

        /// <summary>
        /// The name.
        /// </summary>
        private readonly string _name;

        public DefaultDiskCachingProvider()
        {
        }

        public override bool BaseExists(string cacheKey)
        {
            var path = GetFilePath(cacheKey);

            if (!File.Exists(path))
            {
                return false;
            }

            using (FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                var val = MessagePackSerializer.Deserialize<DiskCacheValue>(stream);

                return val.Expiration > DateTimeOffset.UtcNow;
            }
        }

        public override Task<bool> BaseExistsAsync(string cacheKey)
        {
            throw new NotImplementedException();
        }

        public override void BaseFlush()
        {
            throw new NotImplementedException();
        }

        public override Task BaseFlushAsync()
        {
            throw new NotImplementedException();
        }

        public override CacheValue<T> BaseGet<T>(string cacheKey, Func<T> dataRetriever, TimeSpan expiration)
        {
            throw new NotImplementedException();
        }

        public override CacheValue<T> BaseGet<T>(string cacheKey)
        {
            var path = GetFilePath(cacheKey);

            if (!File.Exists(path)) return CacheValue<T>.Null;

            using (FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                var cached = MessagePackSerializer.Deserialize<DiskCacheValue>(stream);

                if(cached.Expiration > DateTimeOffset.UtcNow)
                {
                    var t = MessagePackSerializer.Deserialize<T>(cached.Value);
                    return new CacheValue<T>(t, true);
                }
                else
                {
                    return CacheValue<T>.NoValue;
                }
            }
        }

        public override IDictionary<string, CacheValue<T>> BaseGetAll<T>(IEnumerable<string> cacheKeys)
        {
            throw new NotImplementedException();
        }

        public override Task<IDictionary<string, CacheValue<T>>> BaseGetAllAsync<T>(IEnumerable<string> cacheKeys)
        {
            throw new NotImplementedException();
        }

        public override Task<CacheValue<T>> BaseGetAsync<T>(string cacheKey, Func<Task<T>> dataRetriever, TimeSpan expiration)
        {
            throw new NotImplementedException();
        }

        public override Task<object> BaseGetAsync(string cacheKey, Type type)
        {
            throw new NotImplementedException();
        }

        public override Task<CacheValue<T>> BaseGetAsync<T>(string cacheKey)
        {
            throw new NotImplementedException();
        }

        public override IDictionary<string, CacheValue<T>> BaseGetByPrefix<T>(string prefix)
        {
            throw new NotImplementedException();
        }

        public override Task<IDictionary<string, CacheValue<T>>> BaseGetByPrefixAsync<T>(string prefix)
        {
            throw new NotImplementedException();
        }

        public override int BaseGetCount(string prefix = "")
        {
            throw new NotImplementedException();
        }

        public override TimeSpan BaseGetExpiration(string cacheKey)
        {
            throw new NotImplementedException();
        }

        public override Task<TimeSpan> BaseGetExpirationAsync(string cacheKey)
        {
            throw new NotImplementedException();
        }

        public override void BaseRefresh<T>(string cacheKey, T cacheValue, TimeSpan expiration)
        {
            throw new NotImplementedException();
        }

        public override Task BaseRefreshAsync<T>(string cacheKey, T cacheValue, TimeSpan expiration)
        {
            throw new NotImplementedException();
        }

        public override void BaseRemove(string cacheKey)
        {
            var path = GetFilePath(key);

            if (!File.Exists(path))
            {
                return;
                //return true;
            }

            try
            {
                File.Delete(path);
                //return true;
            }
            catch
            {
                //return false;
            }
        }

        public override void BaseRemoveAll(IEnumerable<string> cacheKeys)
        {
            throw new NotImplementedException();
        }

        public override Task BaseRemoveAllAsync(IEnumerable<string> cacheKeys)
        {
            throw new NotImplementedException();
        }

        public override Task BaseRemoveAsync(string cacheKey)
        {
            throw new NotImplementedException();
        }

        public override void BaseRemoveByPrefix(string prefix)
        {
            throw new NotImplementedException();
        }

        public override Task BaseRemoveByPrefixAsync(string prefix)
        {
            throw new NotImplementedException();
        }

        public override void BaseSet<T>(string cacheKey, T cacheValue, TimeSpan expiration)
        {
            var path = GetFilePath(cacheKey);

            var value = MessagePackSerializer.Serialize(cacheValue);

            var cached = new DiskCacheValue(value, (int)expiration.TotalSeconds);

            var bytes = MessagePackSerializer.Serialize(cached);

            using (FileStream stream = new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.Read))
            {
                stream.Write(bytes, 0, bytes.Length);
                //return true;
            }
        }

        public override void BaseSetAll<T>(IDictionary<string, T> values, TimeSpan expiration)
        {
            throw new NotImplementedException();
        }

        public override Task BaseSetAllAsync<T>(IDictionary<string, T> values, TimeSpan expiration)
        {
            throw new NotImplementedException();
        }

        public override Task BaseSetAsync<T>(string cacheKey, T cacheValue, TimeSpan expiration)
        {
            throw new NotImplementedException();
        }

        public override bool BaseTrySet<T>(string cacheKey, T cacheValue, TimeSpan expiration)
        {
            throw new NotImplementedException();
        }

        public override Task<bool> BaseTrySetAsync<T>(string cacheKey, T cacheValue, TimeSpan expiration)
        {
            throw new NotImplementedException();
        }

        private string GetFilePath(string key)
        {
            var path = Path.Combine(_options.DBConfig.BasePath, $"{key}.dat");
            return path;
        }
    }
}
