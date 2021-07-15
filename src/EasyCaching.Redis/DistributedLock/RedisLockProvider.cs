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
            _database.StringSetAsync(key, value, TimeSpan.FromMilliseconds(ttlMs));

        public bool Add(string key, byte[] value, int ttlMs) =>
            _database.StringSet(key, value, TimeSpan.FromMilliseconds(ttlMs));

        public Task<bool> AddAsync(string key, byte[] value, int ttlMs) =>
            _database.StringSetAsync(key, value, TimeSpan.FromMilliseconds(ttlMs));

        public bool Delete(string key, byte[] value) =>
            (long)_database.ScriptEvaluate(@"if redis.call('GET', KEYS[1]) == ARGV[1] then
    return redis.call('DEL', KEYS[1]);
end
return -1;", new RedisKey[] { key }, new RedisValue[] { value }) >= 0;

        public async Task<bool> DeleteAsync(string key, byte[] value) =>
            (long)await _database.ScriptEvaluateAsync(@"if redis.call('GET', KEYS[1]) == ARGV[1] then
    return redis.call('DEL', KEYS[1]);
end
return -1;", new RedisKey[] { key }, new RedisValue[] { value }) >= 0;

        public bool CanRetry(Exception ex) => ex is RedisConnectionException;
    }
}
