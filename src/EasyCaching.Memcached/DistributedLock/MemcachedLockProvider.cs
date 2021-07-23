using EasyCaching.Core.DistributedLock;
using Enyim.Caching.Memcached;
using System;
using System.Threading.Tasks;

namespace EasyCaching.Memcached.DistributedLock
{
    public class MemcachedLockProvider : IDistributedLockProvider
    {
        private readonly string _name;
        private readonly EasyCachingMemcachedClient _memcachedClient;

        public MemcachedLockProvider(string name, EasyCachingMemcachedClient memcachedClient)
        {
            _name = name;
            _memcachedClient = memcachedClient;
        }

        public Task<bool> SetAsync(string key, byte[] value, int ttlMs) =>
            _memcachedClient.StoreAsync(StoreMode.Set, $"{_name}/{key}", value, TimeSpan.FromMilliseconds(ttlMs));

        public bool Add(string key, byte[] value, int ttlMs) =>
            _memcachedClient.Store(StoreMode.Set, $"{_name}/{key}", value, TimeSpan.FromMilliseconds(ttlMs));

        public Task<bool> AddAsync(string key, byte[] value, int ttlMs) =>
            _memcachedClient.StoreAsync(StoreMode.Add, $"{_name}/{key}", value, TimeSpan.FromMilliseconds(ttlMs));

        public bool Delete(string key, byte[] value)
        {
            key = $"{_name}/{key}";

            var val = _memcachedClient.Get<byte[]>(key);

            return val != null && value.AsSpan().SequenceEqual(val) && _memcachedClient.Remove(key);
        }

        public async Task<bool> DeleteAsync(string key, byte[] value)
        {
            key = $"{_name}/{key}";

            var val = await _memcachedClient.GetValueAsync<byte[]>(key);

            return val != null && value.AsSpan().SequenceEqual(val) && await _memcachedClient.RemoveAsync(key);
        }

        public bool CanRetry(Exception ex) => ex is MemcachedException;
    }
}
