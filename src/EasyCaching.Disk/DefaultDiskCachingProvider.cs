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

        public DefaultDiskCachingProvider(string name,
            DiskOptions options,
            ILoggerFactory loggerFactory = null)
        {
            this._name = name;
            //this._cache = cache.Single(x => x.ProviderName == _name);
            this._options = options;
            this._logger = loggerFactory?.CreateLogger<DefaultDiskCachingProvider>();

            this._cacheStats = new CacheStats();

            this.ProviderName = _name;
            this.ProviderType = CachingProviderType.Ext1;
            this.ProviderStats = _cacheStats;
            this.ProviderMaxRdSecond = _options.MaxRdSecond;
            this.IsDistributedProvider = false;
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

        public override async Task<bool> BaseExistsAsync(string cacheKey)
        {
            var path = GetFilePath(cacheKey);

            if (!File.Exists(path))
            {
                return false;
            }

            using (FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                var val = await MessagePackSerializer.DeserializeAsync<DiskCacheValue>(stream);

                return val.Expiration > DateTimeOffset.UtcNow;
            }
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
            var path = GetFilePath(cacheKey);

            if (File.Exists(path))
            {
                using (FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    var cached = MessagePackSerializer.Deserialize<DiskCacheValue>(stream);

                    if (cached.Expiration > DateTimeOffset.UtcNow)
                    {
                        var t = MessagePackSerializer.Deserialize<T>(cached.Value);

                        if (_options.EnableLogging)
                            _logger?.LogInformation($"Cache Hit : cachekey = {cacheKey}");

                        CacheStats.OnHit();

                        return new CacheValue<T>(t, true);
                    }                   
                }
            }

            CacheStats.OnMiss();

            if (_options.EnableLogging)
                _logger?.LogInformation($"Cache Missed : cachekey = {cacheKey}");

            // TODO: how to add mutex key here
            //if (!_cache.Add($"{cacheKey}_Lock", 1, TimeSpan.FromMilliseconds(_options.LockMs)))
            //{
            //    System.Threading.Thread.Sleep(_options.SleepMs);
            //    return Get(cacheKey, dataRetriever, expiration);
            //}

            var res = dataRetriever();

            if (res != null)
            {
                Set(cacheKey, res, expiration);
                //remove mutex key
                //_cache.Remove($"{cacheKey}_Lock");

                return new CacheValue<T>(res, true);
            }
            else
            {
                //remove mutex key
                //_cache.Remove($"{cacheKey}_Lock");
                return CacheValue<T>.NoValue;
            }
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

        public override async Task<CacheValue<T>> BaseGetAsync<T>(string cacheKey, Func<Task<T>> dataRetriever, TimeSpan expiration)
        {
            var path = GetFilePath(cacheKey);

            if (File.Exists(path))
            {
                using (FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    var cached = await MessagePackSerializer.DeserializeAsync<DiskCacheValue>(stream);

                    if (cached.Expiration > DateTimeOffset.UtcNow)
                    {
                        var t = MessagePackSerializer.Deserialize<T>(cached.Value);

                        if (_options.EnableLogging)
                            _logger?.LogInformation($"Cache Hit : cachekey = {cacheKey}");

                        CacheStats.OnHit();

                        return new CacheValue<T>(t, true);
                    }
                }
            }

            CacheStats.OnMiss();

            if (_options.EnableLogging)
                _logger?.LogInformation($"Cache Missed : cachekey = {cacheKey}");

            // TODO: how to add mutex key here
            //if (!_cache.Add($"{cacheKey}_Lock", 1, TimeSpan.FromMilliseconds(_options.LockMs)))
            //{
            //    System.Threading.Thread.Sleep(_options.SleepMs);
            //    return Get(cacheKey, dataRetriever, expiration);
            //}

            var res = await dataRetriever();

            if (res != null)
            {
                Set(cacheKey, res, expiration);
                //remove mutex key
                //_cache.Remove($"{cacheKey}_Lock");

                return new CacheValue<T>(res, true);
            }
            else
            {
                //remove mutex key
                //_cache.Remove($"{cacheKey}_Lock");
                return CacheValue<T>.NoValue;
            }
        }

        public override async Task<object> BaseGetAsync(string cacheKey, Type type)
        {
            var path = GetFilePath(cacheKey);

            if (!File.Exists(path))
            {
                CacheStats.OnMiss();

                if (_options.EnableLogging)
                    _logger?.LogInformation($"Cache Missed : cachekey = {cacheKey}");

                return null;
            }

            using (FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                var cached = await MessagePackSerializer.DeserializeAsync<DiskCacheValue>(stream);

                if (cached.Expiration > DateTimeOffset.UtcNow)
                {
                    var t = MessagePackSerializer.NonGeneric.Deserialize(type, cached.Value);
                    return t;
                }
                else
                {
                    return null;
                }
            }
        }

        public override async Task<CacheValue<T>> BaseGetAsync<T>(string cacheKey)
        {
            var path = GetFilePath(cacheKey);

            if (!File.Exists(path)) return CacheValue<T>.Null;

            using (FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                var cached = await MessagePackSerializer.DeserializeAsync<DiskCacheValue>(stream);

                if (cached.Expiration > DateTimeOffset.UtcNow)
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
            var path = GetFilePath(cacheKey);

            if (!File.Exists(path))
            {
                return TimeSpan.Zero;
            }

            using (FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                var cached = MessagePackSerializer.Deserialize<DiskCacheValue>(stream);

                return cached.Expiration.Subtract(DateTimeOffset.UtcNow);
            }            
        }

        public override async Task<TimeSpan> BaseGetExpirationAsync(string cacheKey)
        {
            var path = GetFilePath(cacheKey);

            if (!File.Exists(path))
            {
                return TimeSpan.Zero;
            }

            using (FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                var cached = await MessagePackSerializer.DeserializeAsync<DiskCacheValue>(stream);

                return cached.Expiration.Subtract(DateTimeOffset.UtcNow);
            }
        }

        public override void BaseRefresh<T>(string cacheKey, T cacheValue, TimeSpan expiration)
        {
            // Obsolete
        }

        public override Task BaseRefreshAsync<T>(string cacheKey, T cacheValue, TimeSpan expiration)
        {
            // Obsolete
            return Task.CompletedTask;
        }

        public override void BaseRemove(string cacheKey)
        {
            var path = GetFilePath(cacheKey);

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
            foreach (string key in cacheKeys)
            {
                if (string.IsNullOrWhiteSpace(key))
                    continue;

                var path = GetFilePath(key);

                if (!File.Exists(path))
                {
                    continue;
                }

                try
                {
                    File.Delete(path);
                }
                catch
                {
                }
            }
        }

        public override Task BaseRemoveAllAsync(IEnumerable<string> cacheKeys)
        {
            foreach (string key in cacheKeys)
            {
                if (string.IsNullOrWhiteSpace(key))
                    continue;

                var path = GetFilePath(key);

                if (!File.Exists(path))
                {
                    continue;
                }

                try
                {
                    File.Delete(path);
                }
                catch
                {
                }
            }

            return Task.CompletedTask;
        }

        public override Task BaseRemoveAsync(string cacheKey)
        {
            var path = GetFilePath(cacheKey);

            if (!File.Exists(path))
            {
                return Task.CompletedTask;
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

            return Task.CompletedTask;
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
            foreach (var item in values)
            {
                try
                {
                    var path = GetFilePath(item.Key);

                    var val = MessagePackSerializer.Serialize(item.Value);

                    var cached = new DiskCacheValue(val, (int)expiration.TotalSeconds);

                    var bytes = MessagePackSerializer.Serialize(cached);

                    using (FileStream stream = new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.Read))
                    {
                        stream.Write(bytes, 0, bytes.Length);
                    }
                }
                catch
                {

                }
            }
        }

        public override async Task BaseSetAllAsync<T>(IDictionary<string, T> values, TimeSpan expiration)
        {
            foreach (var item in values)
            {
                try
                {
                    var path = GetFilePath(item.Key);

                    var val = MessagePackSerializer.Serialize(item.Value);

                    var cached = new DiskCacheValue(val, (int)expiration.TotalSeconds);

                    var bytes = MessagePackSerializer.Serialize(cached);

                    using (FileStream stream = new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.Read))
                    {
                        await stream.WriteAsync(bytes, 0, bytes.Length);
                    }
                }
                catch
                {

                }
            }
        }

        public override async Task BaseSetAsync<T>(string cacheKey, T cacheValue, TimeSpan expiration)
        {
            var path = GetFilePath(cacheKey);

            var value = MessagePackSerializer.Serialize(cacheValue);

            var cached = new DiskCacheValue(value, (int)expiration.TotalSeconds);

            var bytes = MessagePackSerializer.Serialize(cached);

            using (FileStream stream = new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.Read))
            {
                await stream.WriteAsync(bytes, 0, bytes.Length);
                //return true;
            }
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
            // TODO: Special characters for file name
            var path = Path.Combine(_options.DBConfig.BasePath, _name, $"{key}.dat");
            return path;
        }
    }
}
