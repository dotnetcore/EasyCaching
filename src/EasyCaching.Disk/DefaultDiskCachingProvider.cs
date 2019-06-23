namespace EasyCaching.Disk
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Security.Cryptography;
    using System.Text;
    using System.Threading.Tasks;
    using EasyCaching.Core;
    using MessagePack;
    using Microsoft.Extensions.Logging;

    public class DefaultDiskCachingProvider : EasyCachingAbstractProvider
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

        private readonly ConcurrentDictionary<string, string> _cacheKeysMap;


        public DefaultDiskCachingProvider(string name,
            DiskOptions options,
            ILoggerFactory loggerFactory = null)
        {
            this._name = name;
            this._options = options;
            this._logger = loggerFactory?.CreateLogger<DefaultDiskCachingProvider>();

            this._cacheKeysMap = new ConcurrentDictionary<string, string>();

            this._cacheStats = new CacheStats();

            this.ProviderName = _name;
            this.ProviderType = CachingProviderType.Ext1;
            this.ProviderStats = _cacheStats;
            this.ProviderMaxRdSecond = _options.MaxRdSecond;
            this.IsDistributedProvider = false;

            InitCacheKey();
        }

        private void InitCacheKey()
        {
            var path = BuildRawPath("key");

            if (!File.Exists(path)) return;

            for (int i = 0; i < 3; i++)
            {
                try
                {
                    using (FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
                        {
                            string line;
                            while ((line = reader.ReadLine()) != null)
                            {
                                _cacheKeysMap.TryAdd(line, GetMd5Str(line));
                            }
                        }
                    }                                         

                    break;
                }
                catch 
                {

                }
            }
        }

        public override bool BaseExists(string cacheKey)
        {
            var path = BuildMd5Path(cacheKey);

            if (!File.Exists(path)) return false;

            var val = GetDiskCacheValue(path);

            return val.Expiration > DateTimeOffset.UtcNow;
        }

        public override async Task<bool> BaseExistsAsync(string cacheKey)
        {
            var path = BuildMd5Path(cacheKey);

            if (!File.Exists(path)) return false;

            var val = await GetDiskCacheValueAsync(path);

            return val.Expiration > DateTimeOffset.UtcNow;
        }

        public override void BaseFlush()
        {
            var path = Path.Combine(_options.DBConfig.BasePath, _name);

            Directory.Delete(path, true);
        }

        public override Task BaseFlushAsync()
        {
            var path = Path.Combine(_options.DBConfig.BasePath, _name);

            Directory.Delete(path, true);

            return Task.CompletedTask;
        }

        public override CacheValue<T> BaseGet<T>(string cacheKey, Func<T> dataRetriever, TimeSpan expiration)
        {
            var path = GetRawPath(cacheKey);

            if (File.Exists(path))
            {
                var cached = GetDiskCacheValue(path);

                if (cached.Expiration > DateTimeOffset.UtcNow)
                {
                    var t = MessagePackSerializer.Deserialize<T>(cached.Value);

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
                return Get(cacheKey, dataRetriever, expiration);
            }

            var res = dataRetriever();

            if (res != null)
            {
                Set(cacheKey, res, expiration);
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

        public override CacheValue<T> BaseGet<T>(string cacheKey)
        {
            var path = GetRawPath(cacheKey);

            if (!File.Exists(path)) return CacheValue<T>.Null;

            var cached = GetDiskCacheValue(path);

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
            var path = GetRawPath(cacheKey);

            if (File.Exists(path))
            {
                var cached = await GetDiskCacheValueAsync(path);

                if (cached.Expiration > DateTimeOffset.UtcNow)
                {
                    var t = MessagePackSerializer.Deserialize<T>(cached.Value);

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

            if (res != null)
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

        public override async Task<object> BaseGetAsync(string cacheKey, Type type)
        {
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
                var t = MessagePackSerializer.NonGeneric.Deserialize(type, cached.Value);
                return t;
            }
            else
            {
                return null;
            }
        }

        public override async Task<CacheValue<T>> BaseGetAsync<T>(string cacheKey)
        {
            var path = GetRawPath(cacheKey);

            if (!File.Exists(path)) return CacheValue<T>.Null;

            var cached = await GetDiskCacheValueAsync(path);

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

        public override IDictionary<string, CacheValue<T>> BaseGetByPrefix<T>(string prefix)
        {
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
                        dict.Add(item, CacheValue<T>.Null);
                    }
                }
                else
                {
                    var cached = GetDiskCacheValue(path);

                    if (cached.Expiration > DateTimeOffset.UtcNow)
                    {
                        var t = MessagePackSerializer.Deserialize<T>(cached.Value);

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

        public override async Task<IDictionary<string, CacheValue<T>>> BaseGetByPrefixAsync<T>(string prefix)
        {
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
                        dict.Add(item, CacheValue<T>.Null);
                    }
                }
                else
                {
                    var cached = await GetDiskCacheValueAsync(path);

                    if (cached.Expiration > DateTimeOffset.UtcNow)
                    {
                        var t = MessagePackSerializer.Deserialize<T>(cached.Value);

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

        public override int BaseGetCount(string prefix = "")
        {
            throw new NotImplementedException();
        }

        public override TimeSpan BaseGetExpiration(string cacheKey)
        {
            var path = GetRawPath(cacheKey);

            if (!File.Exists(path))
            {
                return TimeSpan.Zero;
            }

            var cached = GetDiskCacheValue(path);

            return cached.Expiration.Subtract(DateTimeOffset.UtcNow);
        }

        public override async Task<TimeSpan> BaseGetExpirationAsync(string cacheKey)
        {
            var path = GetRawPath(cacheKey);

            if (!File.Exists(path))
            {
                return TimeSpan.Zero;
            }

            var cached = await GetDiskCacheValueAsync(path);

            return cached.Expiration.Subtract(DateTimeOffset.UtcNow);
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
            var path = GetRawPath(cacheKey);

            if (!File.Exists(path))
            {
                return;
                //return true;
            }

            DeleteFileWithRetry(path);
        }

        public override void BaseRemoveAll(IEnumerable<string> cacheKeys)
        {
            foreach (string key in cacheKeys)
            {
                if (string.IsNullOrWhiteSpace(key))
                    continue;

                var path = GetRawPath(key);

                if (!File.Exists(path))
                {
                    continue;
                }

                DeleteFileWithRetry(path);
            }
        }

        public override Task BaseRemoveAllAsync(IEnumerable<string> cacheKeys)
        {
            foreach (string key in cacheKeys)
            {
                if (string.IsNullOrWhiteSpace(key))
                    continue;

                var path = GetRawPath(key);

                if (!File.Exists(path))
                {
                    continue;
                }

                DeleteFileWithRetry(path);
            }

            return Task.CompletedTask;
        }

        public override Task BaseRemoveAsync(string cacheKey)
        {
            var path = GetRawPath(cacheKey);

            if (!File.Exists(path))
            {
                return Task.CompletedTask;
                //return true;
            }

            DeleteFileWithRetry(path);

            return Task.CompletedTask;
        }

        public override void BaseRemoveByPrefix(string prefix)
        {
            var list = _cacheKeysMap.Where(x => x.Key.StartsWith(prefix, StringComparison.Ordinal)).Select(x => x.Value).ToList();

            foreach (var item in list)
            {
                var path = BuildRawPath(item);

                DeleteFileWithRetry(path);
            }
        }

        public override Task BaseRemoveByPrefixAsync(string prefix)
        {
            var list = _cacheKeysMap.Where(x => x.Key.StartsWith(prefix, StringComparison.Ordinal)).Select(x => x.Value).ToList();

            foreach (var item in list)
            {
                var path = BuildRawPath(item);

                DeleteFileWithRetry(path);
            }

            return Task.CompletedTask;
        }

        public override void BaseSet<T>(string cacheKey, T cacheValue, TimeSpan expiration)
        {
            var (path, fileName) = GetFilePath(cacheKey);

            var bytes = BuildDiskCacheValue(cacheValue, expiration);

            using (FileStream stream = new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.Read))
            {
                stream.Write(bytes, 0, bytes.Length);
                //return true;
            }

            AppendKey(cacheKey, fileName);
        }

        public override void BaseSetAll<T>(IDictionary<string, T> values, TimeSpan expiration)
        {
            foreach (var item in values)
            {
                try
                {
                    var (path, fileName) = GetFilePath(item.Key);

                    var bytes = BuildDiskCacheValue(item.Value, expiration);

                    using (FileStream stream = new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.Read))
                    {
                        stream.Write(bytes, 0, bytes.Length);
                    }

                    AppendKey(item.Key, fileName);
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
                    var (path, fileName) = GetFilePath(item.Key);

                    var bytes = BuildDiskCacheValue(item.Value, expiration);

                    using (FileStream stream = new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.Read))
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
            var (path, fileName) = GetFilePath(cacheKey);

            var bytes = BuildDiskCacheValue(cacheValue, expiration);

            using (FileStream stream = new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.Read))
            {
                await stream.WriteAsync(bytes, 0, bytes.Length);
                //return true;
            }

            AppendKey(cacheKey, fileName);
        }

        public override bool BaseTrySet<T>(string cacheKey, T cacheValue, TimeSpan expiration)
        {
            var (path, fileName) = GetFilePath(cacheKey);

            if (File.Exists(path))
            {
                var cached = GetDiskCacheValue(path);

                if (cached.Expiration.Subtract(DateTimeOffset.UtcNow) > TimeSpan.Zero)
                {
                    return true;
                }
            }

            var bytes = BuildDiskCacheValue(cacheValue, expiration);

            using (FileStream stream = new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.None))
            {
                stream.Write(bytes, 0, bytes.Length);
                AppendKey(cacheKey, fileName);
                return true;
            }
        }

        public override async Task<bool> BaseTrySetAsync<T>(string cacheKey, T cacheValue, TimeSpan expiration)
        {
            var (path, fileName) = GetFilePath(cacheKey);

            if (File.Exists(path))
            {
                var cached = await GetDiskCacheValueAsync(path);

                if (cached.Expiration.Subtract(DateTimeOffset.UtcNow) > TimeSpan.Zero)
                {
                    return true;
                }
            }

            var bytes = BuildDiskCacheValue(cacheValue, expiration);

            using (FileStream stream = new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.None))
            {
                await stream.WriteAsync(bytes, 0, bytes.Length);
                AppendKey(cacheKey, fileName);
                return true;
            }
        }

        private (string path, string md5Name) GetFilePath(string key)
        {
            var md5FolderName = GetMd5Str(_name);
            var md5FileName = GetMd5Str(key);

            var path = Path.Combine(_options.DBConfig.BasePath, md5FolderName, $"{md5FileName}.dat");
            return (path, md5FileName);
        }

        private string BuildRawPath(string key)
        {
            var md5FolderName = GetMd5Str(_name);

            var path = Path.Combine(_options.DBConfig.BasePath, md5FolderName, $"{key}.dat");
            return path;
        }

        private string GetRawPath(string cacheKey)
        {
            var path = string.Empty;
            if (_cacheKeysMap.TryGetValue(cacheKey, out var fileName))
            {
                path = BuildRawPath(fileName);
            }
            else
            {
                path = BuildMd5Path(cacheKey);
            }
            return path;
        }


        private string BuildMd5Path(string key)
        {
            var md5FolderName = GetMd5Str(_name);
            var md5FileName = GetMd5Str(key);

            var path = Path.Combine(_options.DBConfig.BasePath, md5FolderName, $"{md5FileName}.dat");
            return path;
        }

        private void DeleteFileWithRetry(string path)
        {
            for (int i = 0; i < 3; i++)
            {
                try
                {
                    File.Delete(path);
                    break;
                }
                catch
                {
                    //return false;
                }
            }
        }

        private DiskCacheValue GetDiskCacheValue(string path)
        {
            using (FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                var cached = MessagePackSerializer.Deserialize<DiskCacheValue>(stream);

                return cached;
            }
        }

        private void AppendKey(string key, string md5Key)
        {
            if (_cacheKeysMap.TryAdd(key, md5Key))
            {
                var md5FolderName = GetMd5Str(_name);
                var path = Path.Combine(_options.DBConfig.BasePath, md5FolderName, $"key.dat");

                var bytes = Encoding.UTF8.GetBytes(key);

                for (int i = 0; i < 3; i++)
                {
                    try
                    {
                        using (FileStream stream = new FileStream(path, FileMode.Append, FileAccess.Write, FileShare.Read))
                        {
                            stream.Write(bytes, 0, bytes.Length);
                        }

                        break;
                    }
                    catch
                    {

                    }
                }
            }
        }

        private async Task<DiskCacheValue> GetDiskCacheValueAsync(string path)
        {
            using (FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                var cached = await MessagePackSerializer.DeserializeAsync<DiskCacheValue>(stream);

                return cached;
            }
        }

        private byte[] BuildDiskCacheValue<T>(T t, TimeSpan ts)
        {
            var value = MessagePackSerializer.Serialize(t);

            var cached = new DiskCacheValue(value, (int)ts.TotalSeconds);

            var bytes = MessagePackSerializer.Serialize(cached);

            return bytes;
        }

        private string GetMd5Str(string src)
        {
            using (MD5 md5 = MD5.Create())
            {
                byte[] bytes_md5_in = Encoding.UTF8.GetBytes(src);
                byte[] bytes_md5_out = md5.ComputeHash(bytes_md5_in);

                var md5_out = BitConverter.ToString(bytes_md5_out);

                return md5_out;
            }
        }
    }
}
