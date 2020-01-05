namespace EasyCaching.InMemory
{
    using EasyCaching.Core;
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    public class InMemoryCaching : IInMemoryCaching
    {
        private readonly ConcurrentDictionary<string, CacheEntry> _memory;
        private DateTimeOffset _lastExpirationScan;
        private readonly InMemoryCachingOptions _options;
        private readonly string _name;
        private long _cacheSize = 0L;
        private const string _UPTOLIMIT_KEY = "inter_up_to_limit_key";

        public InMemoryCaching(string name, InMemoryCachingOptions optionsAccessor)
        {
            ArgumentCheck.NotNull(optionsAccessor, nameof(optionsAccessor));

            _name = name;
            _options = optionsAccessor;
            _memory = new ConcurrentDictionary<string, CacheEntry>();
            _lastExpirationScan = SystemClock.UtcNow;
        }

        public string ProviderName => this._name;

        public void Clear(string prefix = "")
        {
            if (string.IsNullOrWhiteSpace(prefix))
            {                
                _memory.Clear();

                if (_options.SizeLimit.HasValue)
                    Interlocked.Exchange(ref _cacheSize, 0);
            }
            else
            {
                RemoveByPrefix(prefix);
            }
        }

        public int GetCount(string prefix = "")
        {
            return string.IsNullOrWhiteSpace(prefix)
                ? _memory.Count
                : _memory.Count(x => x.Key.StartsWith(prefix, StringComparison.OrdinalIgnoreCase));
        }

        internal void RemoveExpiredKey(string key)
        {
           bool flag = _memory.TryRemove(key, out _);
            if (_options.SizeLimit.HasValue && flag)
                Interlocked.Decrement(ref _cacheSize);
        }

        public CacheValue<T> Get<T>(string key)
        {
            ArgumentCheck.NotNullOrWhiteSpace(key, nameof(key));

            if (!_memory.TryGetValue(key, out var cacheEntry))
            {
                return CacheValue<T>.NoValue;
            }

            if (cacheEntry.ExpiresAt < SystemClock.UtcNow)
            {
                RemoveExpiredKey(key);
                return CacheValue<T>.NoValue;
            }

            try
            {
                var value = cacheEntry.GetValue<T>(_options.EnableReadDeepClone);
                return new CacheValue<T>(value, true);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"some error herer, message = {ex.Message}");
                return CacheValue<T>.NoValue;
            }
        }

        public object Get(string key)
        {
            ArgumentCheck.NotNullOrWhiteSpace(key, nameof(key));

            if (!_memory.TryGetValue(key, out var cacheEntry))
            {
                return null;
            }

            if (cacheEntry.ExpiresAt < SystemClock.UtcNow)
            {
                RemoveExpiredKey(key);
                return null;
            }

            try
            {
                return cacheEntry.Value;
            }
            catch
            {
                return null;
            }
        }

        public bool Add<T>(string key, T value, TimeSpan? expiresIn = null)
        {
            ArgumentCheck.NotNullOrWhiteSpace(key, nameof(key));

            var expiresAt = expiresIn.HasValue ? SystemClock.UtcNow.SafeAdd(expiresIn.Value) : DateTimeOffset.MaxValue;
            return SetInternal(new CacheEntry(key, value, expiresAt), true);
        }

        public bool Set<T>(string key, T value, TimeSpan? expiresIn = null)
        {
            ArgumentCheck.NotNullOrWhiteSpace(key, nameof(key));

            var expiresAt = expiresIn.HasValue ? SystemClock.UtcNow.SafeAdd(expiresIn.Value) : DateTimeOffset.MaxValue;
            return SetInternal(new CacheEntry(key, value, expiresAt));
        }

        private bool SetInternal(CacheEntry entry, bool addOnly = false)
        {
            if (entry.ExpiresAt < SystemClock.UtcNow)
            {
                RemoveExpiredKey(entry.Key);
                return false;
            }

            if (_options.SizeLimit.HasValue && Interlocked.Read(ref _cacheSize) >= _options.SizeLimit)
            {
                // prevent alaways access the following logic after up to limit
                if (_memory.TryAdd(_UPTOLIMIT_KEY, new CacheEntry(_UPTOLIMIT_KEY, 1, DateTimeOffset.UtcNow.AddSeconds(5))))
                {
                    var shouldRemoveCount = 5;

                    if (_options.SizeLimit.Value >= 10000)
                    {
                        shouldRemoveCount = (int)(_options.SizeLimit * 0.005d);
                    }
                    else if (_options.SizeLimit.Value >= 1000 && _options.SizeLimit.Value < 10000)
                    {
                        shouldRemoveCount = (int)(_options.SizeLimit * 0.01d);
                    }

                    var oldestList = _memory.ToArray()
                                    .OrderBy(kvp => kvp.Value.LastAccessTicks)
                                    .ThenBy(kvp => kvp.Value.InstanceNumber)
                                    .Take(shouldRemoveCount)
                                    .Select(kvp => kvp.Key);

                    RemoveAll(oldestList);

                    //// this key will be remove by ScanForExpiredItems.
                    //_memory.TryRemove(_UPTOLIMIT_KEY, out _);
                }
            }

            CacheEntry deep = null;
            if (_options.EnableWriteDeepClone)
            {
                try
                {
                    deep = DeepClonerGenerator.CloneObject(entry);
                }
                catch (Exception)
                {
                    deep = entry;
                }
            }
            else
            {
                deep = entry;
            }

            if (addOnly)
            {
                if (!_memory.TryAdd(deep.Key, deep))
                {
                    if (!_memory.TryGetValue(deep.Key, out var existingEntry) || existingEntry.ExpiresAt >= SystemClock.UtcNow)
                        return false;

                    _memory.AddOrUpdate(deep.Key, deep, (k, cacheEntry) => deep);

                    if(_options.SizeLimit.HasValue)
                        Interlocked.Increment(ref _cacheSize);
                }
            }
            else
            {
                _memory.AddOrUpdate(deep.Key, deep, (k, cacheEntry) => deep);

                if (_options.SizeLimit.HasValue)
                    Interlocked.Increment(ref _cacheSize);
            }

            StartScanForExpiredItems();

            return true;
        }

        public bool Exists(string key)
        {
            ArgumentCheck.NotNullOrWhiteSpace(key, nameof(key));

            return _memory.TryGetValue(key, out var entry) && entry.ExpiresAt > SystemClock.UtcNow;
        }

        public int RemoveAll(IEnumerable<string> keys = null)
        {
            if (keys == null)
            {
                if (_options.SizeLimit.HasValue)
                {
                    int count = (int)Interlocked.Read(ref _cacheSize);
                    Interlocked.Exchange(ref _cacheSize, 0);
                    _memory.Clear();
                    return count;
                }
                else
                {
                    int count = _memory.Count;
                    _memory.Clear();
                    return count;
                }
            }

            int removed = 0;
            foreach (string key in keys)
            {
                if (string.IsNullOrEmpty(key))
                    continue;

                if (_memory.TryRemove(key, out _))
                { 
                    removed++;
                    if (_options.SizeLimit.HasValue)
                        Interlocked.Decrement(ref _cacheSize);
                }
            }

            return removed;
        }

        public bool Remove(string key)
        {
            bool flag = _memory.TryRemove(key, out _);

            if (_options.SizeLimit.HasValue && !key.Equals(_UPTOLIMIT_KEY) && flag)
            { 
                Interlocked.Decrement(ref _cacheSize);
            }

            return flag;
        }

        public int RemoveByPrefix(string prefix)
        {
            var keysToRemove = _memory.Keys.Where(x => x.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)).ToList();
            return RemoveAll(keysToRemove);
        }

        public IDictionary<string, CacheValue<T>> GetAll<T>(IEnumerable<string> keys)
        {
            var map = new Dictionary<string, CacheValue<T>>();
            foreach (string key in keys)
                map[key] = Get<T>(key);

            return map;
        }

        public int SetAll<T>(IDictionary<string, T> values, TimeSpan? expiresIn = null)
        {
            if (values == null || values.Count == 0) return 0;

            var list = new List<bool>();

            foreach (var entry in values) list.Add(Set(entry.Key, entry.Value, expiresIn));

            return list.Count(r => r);
        }

        public bool Replace<T>(string key, T value, TimeSpan? expiresIn = null)
        {
            ArgumentCheck.NotNullOrWhiteSpace(key, nameof(key));

            if (!_memory.ContainsKey(key)) return false;

            return Set(key, value, expiresIn);
        }

        private void StartScanForExpiredItems()
        {
            var utcNow = SystemClock.UtcNow;
            if (TimeSpan.FromSeconds(_options.ExpirationScanFrequency) < utcNow - _lastExpirationScan)
            {
                _lastExpirationScan = utcNow;
                Task.Factory.StartNew(state => ScanForExpiredItems((InMemoryCaching)state), this,
                    CancellationToken.None, TaskCreationOptions.DenyChildAttach, TaskScheduler.Default);
            }
        }

        private void ScanForExpiredItems(InMemoryCaching cache)
        {
            var now = SystemClock.UtcNow;
            foreach (var entry in cache._memory.Values.Where(x => x.ExpiresAt < now))
            {
                cache.Remove(entry.Key);
            }
        }

        public IDictionary<string, CacheValue<T>> GetByPrefix<T>(string key)
        {
            var values = _memory.Values.Where(x => x.Key.StartsWith(key, StringComparison.OrdinalIgnoreCase) && x.ExpiresAt > SystemClock.UtcNow);
            return values.ToDictionary(k => k.Key, v => new CacheValue<T>(v.GetValue<T>(_options.EnableReadDeepClone), true));
        }

        public TimeSpan GetExpiration(string key)
        {
            if (!_memory.TryGetValue(key, out var value))
                return TimeSpan.Zero;

            if (value.ExpiresAt >= SystemClock.UtcNow)
                return value.ExpiresAt.Subtract(SystemClock.UtcNow);

            return TimeSpan.Zero;
        }

        private class CacheEntry
        {
            private object _cacheValue;
            private static long _instanceCount;

            public CacheEntry(string key, object value, DateTimeOffset expiresAt)
            {
                Key = key;
                Value = value;
                ExpiresAt = expiresAt;
                LastModifiedTicks = SystemClock.UtcNow.Ticks;
                InstanceNumber = Interlocked.Increment(ref _instanceCount);
            }

            internal string Key { get; private set; }
            internal long InstanceNumber { get; private set; }
            internal DateTimeOffset ExpiresAt { get; set; }
            internal long LastAccessTicks { get; private set; }
            internal long LastModifiedTicks { get; private set; }

            /// <summary>
            /// the cache value
            /// </summary>
            internal object Value
            {
                get
                {
                    LastAccessTicks = SystemClock.UtcNow.Ticks;
                    return _cacheValue;
                }
                set
                {
                    _cacheValue = value;
                    LastAccessTicks = SystemClock.UtcNow.Ticks;
                    LastModifiedTicks = SystemClock.UtcNow.Ticks;
                }
            }

            /// <summary>
            /// conver to T
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <returns></returns>
            public T GetValue<T>(bool isDeepClone = true)
            {
                object val = Value;
              
                var t = typeof(T);

                if (t == TypeHelper.BoolType || t == TypeHelper.StringType || t == TypeHelper.CharType || t == TypeHelper.DateTimeType || t.IsNumeric())
                    return (T)Convert.ChangeType(val, t);

                if (t == TypeHelper.NullableBoolType || t == TypeHelper.NullableCharType || t == TypeHelper.NullableDateTimeType || t.IsNullableNumeric())
                    return val == null ? default(T) : (T)Convert.ChangeType(val, Nullable.GetUnderlyingType(t));

                return isDeepClone 
                    ? DeepClonerGenerator.CloneObject<T>((T)val)
                    : (T)val;
            }
        }
    }
}
