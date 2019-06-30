namespace EasyCaching.Disk
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Security.Cryptography;
    using System.Text;
    using System.Threading;
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

        private Timer _saveKeyTimer;

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
            this.ProviderType = CachingProviderType.Disk;
            this.ProviderStats = _cacheStats;
            this.ProviderMaxRdSecond = _options.MaxRdSecond;
            this.IsDistributedProvider = false;

            Init();

            _saveKeyTimer = new Timer(SaveKeyToFile, null, TimeSpan.FromSeconds(10), TimeSpan.FromMinutes(2));
        }

        private void Init()
        {
            var md5FolderName = GetMd5Str(_name);

            var basePath = Path.Combine(_options.DBConfig.BasePath, md5FolderName);

            if (!Directory.Exists(basePath))
            {
                Directory.CreateDirectory(basePath);
            }

            var path = Path.Combine(basePath, $"key.dat");

            if (!File.Exists(path))
            {
                File.Create(path);
            }

            InitCacheKey();
        }

        private void SaveKeyToFile(object state)
        {
            if (!_cacheKeysMap.IsEmpty)
            {
                var md5FolderName = GetMd5Str(_name);
                var path = Path.Combine(_options.DBConfig.BasePath, md5FolderName, $"key.dat");

                var keys = _cacheKeysMap.Keys.ToArray();

                var value = string.Join("\n", keys);

                var bytes = Encoding.UTF8.GetBytes(value);

                for (int i = 0; i < 3; i++)
                {
                    try
                    {
                        using (FileStream stream = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write))
                        {
                            // batch , not all in one
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
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            var path = BuildMd5Path(cacheKey);

            if (!File.Exists(path)) return false;

            var val = GetDiskCacheValue(path);

            return val.Expiration > DateTimeOffset.UtcNow;
        }

        public override async Task<bool> BaseExistsAsync(string cacheKey)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            var path = BuildMd5Path(cacheKey);

            if (!File.Exists(path)) return false;

            var val = await GetDiskCacheValueAsync(path);

            return val.Expiration > DateTimeOffset.UtcNow;
        }

        public override void BaseFlush()
        {
            if (_options.EnableLogging)
                _logger?.LogInformation("Flush");

            var md5FolderName = GetMd5Str(_name);

            var path = Path.Combine(_options.DBConfig.BasePath, md5FolderName);

            DeleteDirectory(path);

            _cacheKeysMap.Clear();
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

        public override CacheValue<T> BaseGet<T>(string cacheKey, Func<T> dataRetriever, TimeSpan expiration)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));
            ArgumentCheck.NotNegativeOrZero(expiration, nameof(expiration));

            var path = GetRawPath(cacheKey);

            if (File.Exists(path))
            {
                var cached = GetDiskCacheValue(path);

                if (cached.Expiration > DateTimeOffset.UtcNow)
                {
                    var t = MessagePackSerializer.Deserialize<T>(cached.Value, MessagePack.Resolvers.ContractlessStandardResolver.Instance);

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
                // remove mutex key
                _cacheKeysMap.TryRemove($"{cacheKey}_Lock", out _);

                return new CacheValue<T>(res, true);
            }
            else
            {
                // remove mutex key
                _cacheKeysMap.TryRemove($"{cacheKey}_Lock", out _);
                return CacheValue<T>.NoValue;
            }
        }

        public override CacheValue<T> BaseGet<T>(string cacheKey)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            var path = GetRawPath(cacheKey);

            if (!File.Exists(path))
            {
                if (_options.EnableLogging)
                    _logger?.LogInformation($"Cache Missed : cachekey = {cacheKey}");

                CacheStats.OnMiss();

                return CacheValue<T>.NoValue;
            }

            var cached = GetDiskCacheValue(path);

            if (cached.Expiration > DateTimeOffset.UtcNow)
            {
                if (_options.EnableLogging)
                    _logger?.LogInformation($"Cache Hit : cachekey = {cacheKey}");

                CacheStats.OnHit();

                var t = MessagePackSerializer.Deserialize<T>(cached.Value, MessagePack.Resolvers.ContractlessStandardResolver.Instance);
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

        public override IDictionary<string, CacheValue<T>> BaseGetAll<T>(IEnumerable<string> cacheKeys)
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
                        var t = MessagePackSerializer.Deserialize<T>(cached.Value, MessagePack.Resolvers.ContractlessStandardResolver.Instance);

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
                var cached = await GetDiskCacheValueAsync(path);

                if (cached.Expiration > DateTimeOffset.UtcNow)
                {
                    var t = MessagePackSerializer.Deserialize<T>(cached.Value, MessagePack.Resolvers.ContractlessStandardResolver.Instance);

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

                var t = MessagePackSerializer.NonGeneric.Deserialize(type, cached.Value);
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

                var t = MessagePackSerializer.Deserialize<T>(cached.Value, MessagePack.Resolvers.ContractlessStandardResolver.Instance);
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

        public override IDictionary<string, CacheValue<T>> BaseGetByPrefix<T>(string prefix)
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
                        var t = MessagePackSerializer.Deserialize<T>(cached.Value, MessagePack.Resolvers.ContractlessStandardResolver.Instance);

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
            if (string.IsNullOrWhiteSpace(prefix))
            {
                return _cacheKeysMap.Count;
            }
            else
            {
                return _cacheKeysMap.Count(x => x.Key.StartsWith(prefix, StringComparison.Ordinal));
            }
        }

        public override TimeSpan BaseGetExpiration(string cacheKey)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

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
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

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
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            var path = GetRawPath(cacheKey);

            if (!File.Exists(path))
            {
                return;
                //return true;
            }

            if (DeleteFileWithRetry(path))
            {
                _cacheKeysMap.TryRemove(cacheKey, out _);
            }
        }

        public override void BaseRemoveAll(IEnumerable<string> cacheKeys)
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

        public override void BaseRemoveByPrefix(string prefix)
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

        public override void BaseSet<T>(string cacheKey, T cacheValue, TimeSpan expiration)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));
            ArgumentCheck.NotNull(cacheValue, nameof(cacheValue));
            ArgumentCheck.NotNegativeOrZero(expiration, nameof(expiration));

            var (path, fileName) = GetFilePath(cacheKey);

            var bytes = BuildDiskCacheValue(cacheValue, expiration);

            using (FileStream stream = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read))
            {
                stream.Write(bytes, 0, bytes.Length);
                //return true;
            }

            AppendKey(cacheKey, fileName);
        }

        public override void BaseSetAll<T>(IDictionary<string, T> values, TimeSpan expiration)
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
            ArgumentCheck.NotNull(cacheValue, nameof(cacheValue));
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

        public override bool BaseTrySet<T>(string cacheKey, T cacheValue, TimeSpan expiration)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));
            ArgumentCheck.NotNull(cacheValue, nameof(cacheValue));
            ArgumentCheck.NotNegativeOrZero(expiration, nameof(expiration));

            var (path, fileName) = GetFilePath(cacheKey);

            if (File.Exists(path))
            {
                var cached = GetDiskCacheValue(path);

                if (cached.Expiration.Subtract(DateTimeOffset.UtcNow) > TimeSpan.Zero)
                {
                    return false;
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
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));
            ArgumentCheck.NotNull(cacheValue, nameof(cacheValue));
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

        private bool DeleteFileWithRetry(string path)
        {
            bool flag = false;

            for (int i = 0; i < 3; i++)
            {
                try
                {
                    File.Delete(path);
                    flag = true;
                    break;
                }
                catch
                {
                    //return false;
                }
            }

            return flag;
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
            _cacheKeysMap.TryAdd(key, md5Key);
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
            var value = MessagePackSerializer.Serialize(t, MessagePack.Resolvers.ContractlessStandardResolver.Instance);

            var cached = new DiskCacheValue(value, DateTimeOffset.UtcNow.AddSeconds((int)ts.TotalSeconds));

            var bytes = MessagePackSerializer.Serialize(cached);

            return bytes;
        }

        private string GetMd5Str(string src)
        {
            using (MD5 md5 = MD5.Create())
            {
                byte[] data = md5.ComputeHash(Encoding.UTF8.GetBytes(src));

                StringBuilder sBuilder = new StringBuilder(64);

                for (int i = 0; i < data.Length; i++)
                {
                    sBuilder.Append(data[i].ToString("x2"));
                }

                return sBuilder.ToString();
            }
        }

        private void DeleteDirectory(string path)
        {
            try
            {
                DirectoryInfo dir = new DirectoryInfo(path);
                FileSystemInfo[] fileinfo = dir.GetFileSystemInfos();
                foreach (FileSystemInfo i in fileinfo)
                {
                    if (i is DirectoryInfo)
                    {
                        DirectoryInfo subdir = new DirectoryInfo(i.FullName);
                        subdir.Delete(true);
                    }
                    else
                    {
                        File.Delete(i.FullName);
                    }
                }
            }
            catch
            {

            }
        }
    }
}
