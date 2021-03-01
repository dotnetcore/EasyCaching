namespace EasyCaching.Disk
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using EasyCaching.Core;
    using MessagePack;
    using MessagePack.Resolvers;
    using Microsoft.Extensions.Logging;

    public partial class DefaultDiskCachingProvider : EasyCachingAbstractProvider
    {   
        public override async Task<bool> BaseExistsAsync(string cacheKey)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            var path = BuildMd5Path(cacheKey);

            if (!File.Exists(path)) return false;

            var val = await GetDiskCacheValueAsync(path);

            return val.Expiration > DateTimeOffset.UtcNow;
        }

        public override Task BaseFlushAsync()
        {
            if (_options.EnableLogging)
                _logger?.LogInformation("FlushAsync");

            var md5FolderName = GetMd5Str(_name);

            var path = Path.Combine(_options.DBConfig.BasePath, md5FolderName);

            DeleteDirectory(path);

            _cacheKeysMap.Clear();

            return Task.CompletedTask;
        }            

        public override async Task<IDictionary<string, CacheValue<T>>> BaseGetAllAsync<T>(IEnumerable<string> cacheKeys)
        {
            ArgumentCheck.NotNullAndCountGTZero(cacheKeys, nameof(cacheKeys));

            IDictionary<string, CacheValue<T>> dict = new Dictionary<string, CacheValue<T>>();

            foreach (var item in cacheKeys)
            {
                var path = GetRawPath(item);

                if (!File.Exists(path))
                {
                    if (!dict.ContainsKey(item))
                    {
                        dict.Add(item, CacheValue<T>.NoValue);
                    }
                }
                else
                {
                    var cached = await GetDiskCacheValueAsync(path);

                    if (cached.Expiration > DateTimeOffset.UtcNow)
                    {
                        var t = MessagePackSerializer.Deserialize<T>(cached.Value, MessagePackSerializerOptions.Standard.WithResolver(ContractlessStandardResolver.Instance));

                        if (!dict.ContainsKey(item))
                        {
                            dict.Add(item, new CacheValue<T>(t, true));
                        }
                    }
                    else
                    {
                        if (!dict.ContainsKey(item))
                        {
                            dict.Add(item, CacheValue<T>.NoValue);
                        }
                    }
                }
            }

            return dict;
        }

        public override async Task<CacheValue<T>> BaseGetAsync<T>(string cacheKey, Func<Task<T>> dataRetriever, TimeSpan expiration)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));
            ArgumentCheck.NotNegativeOrZero(expiration, nameof(expiration));

            var path = GetRawPath(cacheKey);

            if (File.Exists(path))
            {
                /*
                 GetAsync_Parallel_Should_Succeed always failed in CI due to this reason, but succeed in local PC

                 MessagePack.MessagePackSerializationException : Failed to deserialize EasyCaching.Disk.DiskCacheValue value.
                    ---- System.IO.EndOfStreamException : Attempted to read past the end of the stream.
                 */
                var cached = await GetDiskCacheValueAsync(path);
                //var cached = GetDiskCacheValueAsync(path).ConfigureAwait(false).GetAwaiter().GetResult();
                //var cached = GetDiskCacheValue(path);

                if (cached.Expiration > DateTimeOffset.UtcNow)
                {
                    var t = MessagePackSerializer.Deserialize<T>(cached.Value, MessagePackSerializerOptions.Standard.WithResolver(ContractlessStandardResolver.Instance));

                    if (_options.EnableLogging)
                        _logger?.LogInformation($"Cache Hit : cachekey = {cacheKey}");

                    CacheStats.OnHit();

                    return new CacheValue<T>(t, true);
                }
            }

            CacheStats.OnMiss();

            if (_options.EnableLogging)
                _logger?.LogInformation($"Cache Missed : cachekey = {cacheKey}");

            // TODO: how to add mutex key here
            if (!_cacheKeysMap.TryAdd($"{cacheKey}_Lock", "1"))
            {
                System.Threading.Thread.Sleep(_options.SleepMs);
                return await GetAsync(cacheKey, dataRetriever, expiration);
            }

            var res = await dataRetriever();

            if (res != null || _options.CacheNulls)
            {
                await SetAsync(cacheKey, res, expiration);
                //remove mutex key
                _cacheKeysMap.TryRemove($"{cacheKey}_Lock", out _);
                return new CacheValue<T>(res, true);
            }
            else
            {
                //remove mutex key
                _cacheKeysMap.TryRemove($"{cacheKey}_Lock", out _);
                return CacheValue<T>.NoValue;
            }
        }

        public override Task<int> BaseGetCountAsync(string prefix = "")
        {
            if (string.IsNullOrWhiteSpace(prefix))
            {
                return Task.FromResult(_cacheKeysMap.Count);
            }
            else
            {
                return Task.FromResult(_cacheKeysMap.Count(x => x.Key.StartsWith(prefix, StringComparison.Ordinal)));
            }
        }

        public override async Task<object> BaseGetAsync(string cacheKey, Type type)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            var path = GetRawPath(cacheKey);

            if (!File.Exists(path))
            {
                CacheStats.OnMiss();

                if (_options.EnableLogging)
                    _logger?.LogInformation($"Cache Missed : cachekey = {cacheKey}");

                return null;
            }

            var cached = await GetDiskCacheValueAsync(path);

            if (cached.Expiration > DateTimeOffset.UtcNow)
            {
                if (_options.EnableLogging)
                    _logger?.LogInformation($"Cache Hit : cachekey = {cacheKey}");

                CacheStats.OnHit();

                var t = MessagePackSerializer.Deserialize(type, cached.Value, MessagePackSerializerOptions.Standard.WithResolver(ContractlessStandardResolver.Instance));
                return t;
            }
            else
            {
                if (_options.EnableLogging)
                    _logger?.LogInformation($"Cache Missed : cachekey = {cacheKey}");

                CacheStats.OnMiss();

                return null;
            }
        }

        public override async Task<CacheValue<T>> BaseGetAsync<T>(string cacheKey)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            var path = GetRawPath(cacheKey);

            if (!File.Exists(path))
            {
                CacheStats.OnMiss();

                if (_options.EnableLogging)
                    _logger?.LogInformation($"Cache Missed : cachekey = {cacheKey}");

                return CacheValue<T>.NoValue;
            }

            var cached = await GetDiskCacheValueAsync(path);

            if (cached.Expiration > DateTimeOffset.UtcNow)
            {
                if (_options.EnableLogging)
                    _logger?.LogInformation($"Cache Hit : cachekey = {cacheKey}");

                CacheStats.OnHit();

                var t = MessagePackSerializer.Deserialize<T>(cached.Value, MessagePackSerializerOptions.Standard.WithResolver(ContractlessStandardResolver.Instance));
                return new CacheValue<T>(t, true);
            }
            else
            {
                if (_options.EnableLogging)
                    _logger?.LogInformation($"Cache Missed : cachekey = {cacheKey}");

                CacheStats.OnMiss();

                return CacheValue<T>.NoValue;
            }
        }
            
        public override async Task<IDictionary<string, CacheValue<T>>> BaseGetByPrefixAsync<T>(string prefix)
        {
            ArgumentCheck.NotNullOrWhiteSpace(prefix, nameof(prefix));

            IDictionary<string, CacheValue<T>> dict = new Dictionary<string, CacheValue<T>>();

            var list = _cacheKeysMap.Where(x => x.Key.StartsWith(prefix, StringComparison.Ordinal)).Select(x => x.Key).ToList();

            if (list == null || !list.Any()) return dict;

            foreach (var item in list)
            {
                var path = GetRawPath(item);

                if (!File.Exists(path))
                {
                    if (!dict.ContainsKey(item))
                    {
                        dict.Add(item, CacheValue<T>.NoValue);
                    }
                }
                else
                {
                    var cached = await GetDiskCacheValueAsync(path);

                    if (cached.Expiration > DateTimeOffset.UtcNow)
                    {
                        var t = MessagePackSerializer.Deserialize<T>(cached.Value, MessagePackSerializerOptions.Standard.WithResolver(ContractlessStandardResolver.Instance));

                        if (!dict.ContainsKey(item))
                        {
                            dict.Add(item, new CacheValue<T>(t, true));
                        }
                    }
                    else
                    {
                        if (!dict.ContainsKey(item))
                        {
                            dict.Add(item, CacheValue<T>.NoValue);
                        }
                    }
                }
            }

            return dict;
        }
             
        public override async Task<TimeSpan> BaseGetExpirationAsync(string cacheKey)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            var path = GetRawPath(cacheKey);

            if (!File.Exists(path))
            {
                return TimeSpan.Zero;
            }

            var cached = await GetDiskCacheValueAsync(path);

            return cached.Expiration.Subtract(DateTimeOffset.UtcNow);
        }             
              
        public override Task BaseRemoveAllAsync(IEnumerable<string> cacheKeys)
        {
            ArgumentCheck.NotNullAndCountGTZero(cacheKeys, nameof(cacheKeys));

            foreach (string key in cacheKeys)
            {
                if (string.IsNullOrWhiteSpace(key))
                    continue;

                var path = GetRawPath(key);

                if (!File.Exists(path))
                {
                    continue;
                }

                if (DeleteFileWithRetry(path))
                {
                    _cacheKeysMap.TryRemove(key, out _);
                }
            }

            return Task.CompletedTask;
        }

        public override Task BaseRemoveAsync(string cacheKey)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            var path = GetRawPath(cacheKey);

            if (!File.Exists(path))
            {
                return Task.CompletedTask;
                //return true;
            }

            if (DeleteFileWithRetry(path))
            {
                _cacheKeysMap.TryRemove(cacheKey, out _);
            }

            return Task.CompletedTask;
        }

        public override Task BaseRemoveByPrefixAsync(string prefix)
        {
            ArgumentCheck.NotNullOrWhiteSpace(prefix, nameof(prefix));

            var list = _cacheKeysMap.Where(x => x.Key.StartsWith(prefix, StringComparison.Ordinal)).Select(x => x.Key).ToList();

            foreach (var item in list)
            {
                var path = BuildMd5Path(item);

                if (DeleteFileWithRetry(path))
                {
                    _cacheKeysMap.TryRemove(item, out _);
                }
            }

            return Task.CompletedTask;
        }
          
        public override async Task BaseSetAllAsync<T>(IDictionary<string, T> values, TimeSpan expiration)
        {
            ArgumentCheck.NotNegativeOrZero(expiration, nameof(expiration));
            ArgumentCheck.NotNullAndCountGTZero(values, nameof(values));

            foreach (var item in values)
            {
                try
                {
                    var (path, fileName) = GetFilePath(item.Key);

                    var bytes = BuildDiskCacheValue(item.Value, expiration);

                    using (FileStream stream = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read))
                    {
                        await stream.WriteAsync(bytes, 0, bytes.Length);
                    }

                    AppendKey(item.Key, fileName);
                }
                catch
                {

                }
            }
        }

        public override async Task BaseSetAsync<T>(string cacheKey, T cacheValue, TimeSpan expiration)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));
            ArgumentCheck.NotNull(cacheValue, nameof(cacheValue), _options.CacheNulls);
            ArgumentCheck.NotNegativeOrZero(expiration, nameof(expiration));

            var (path, fileName) = GetFilePath(cacheKey);

            var bytes = BuildDiskCacheValue(cacheValue, expiration);

            using (FileStream stream = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read))
            {
                await stream.WriteAsync(bytes, 0, bytes.Length);
                //return true;
            }

            AppendKey(cacheKey, fileName);
        }

        public override async Task<bool> BaseTrySetAsync<T>(string cacheKey, T cacheValue, TimeSpan expiration)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));
            ArgumentCheck.NotNull(cacheValue, nameof(cacheValue), _options.CacheNulls);
            ArgumentCheck.NotNegativeOrZero(expiration, nameof(expiration));

            var (path, fileName) = GetFilePath(cacheKey);

            if (File.Exists(path))
            {
                var cached = await GetDiskCacheValueAsync(path);

                if (cached.Expiration.Subtract(DateTimeOffset.UtcNow) > TimeSpan.Zero)
                {
                    return false;
                }
            }

            var bytes = BuildDiskCacheValue(cacheValue, expiration);

            using (FileStream stream = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None))
            {
                await stream.WriteAsync(bytes, 0, bytes.Length);
                AppendKey(cacheKey, fileName);
                return true;
            }
        }
               
        private async Task<DiskCacheValue> GetDiskCacheValueAsync(string path)
        {
            using (FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                var cached = await MessagePackSerializer.DeserializeAsync<DiskCacheValue>(stream, MessagePackSerializerOptions.Standard.WithResolver(ContractlessStandardResolver.Instance));

                return cached;
            }
        }
    }
}
