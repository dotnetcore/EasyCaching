using EasyCaching.Core.DistributedLock;
using StackExchange.Redis;
using System;
using System.Threading.Tasks;

namespace EasyCaching.Redis.DistributedLock
{
    public class RedisLockProvider : IDistributedLockProvider
    {
        private readonly IDatabase _database;

        public RedisLockProvider(IDatabase database) => _database = database;

        public Task<bool> SetAsync(string key, byte[] value, int ttlMs) =>
            _database.StringSetAsync((RedisKey)key, (RedisValue)value, TimeSpan.FromMilliseconds(ttlMs));

        public bool Add(string key, byte[] value, int ttlMs) =>
            _database.LockTake((RedisKey)key, (RedisValue)value, TimeSpan.FromMilliseconds(ttlMs));

        public Task<bool> AddAsync(string key, byte[] value, int ttlMs) =>
            _database.LockTakeAsync((RedisKey)key, (RedisValue)value, TimeSpan.FromMilliseconds(ttlMs));

        public bool Delete(string key, byte[] value) => _database.LockRelease((RedisKey)key, (RedisValue)value);

        public Task<bool> DeleteAsync(string key, byte[] value) =>
            _database.LockReleaseAsync((RedisKey)key, (RedisValue)value);

        public bool CanRetry(Exception ex) => ex is RedisConnectionException;
    }
}