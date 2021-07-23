using CSRedis;
using EasyCaching.Core.DistributedLock;
using System;
using System.Threading.Tasks;

namespace EasyCaching.CSRedis.DistributedLock
{
    public class CSRedisLockProvider : IDistributedLockProvider
    {
        private readonly string _name;
        private readonly EasyCachingCSRedisClient _database;

        public CSRedisLockProvider(string name, EasyCachingCSRedisClient database)
        {
            _name = name;
            _database = database;
        }

        public Task<bool> SetAsync(string key, byte[] value, int ttlMs) =>
            _database.SetAsync($"{_name}/{key}", value, TimeSpan.FromMilliseconds(ttlMs));

        public bool Add(string key, byte[] value, int ttlMs) =>
            _database.Set($"{_name}/{key}", value, TimeSpan.FromMilliseconds(ttlMs), RedisExistence.Nx);

        public Task<bool> AddAsync(string key, byte[] value, int ttlMs) =>
            _database.SetAsync($"{_name}/{key}", value, TimeSpan.FromMilliseconds(ttlMs), RedisExistence.Nx);

        public bool Delete(string key, byte[] value) =>
            (long)_database.Eval(@"if redis.call('GET', KEYS[1]) == ARGV[1] then
    return redis.call('DEL', KEYS[1]);
end
return -1;", $"{_name}/{key}", value) >= 0;

        public async Task<bool> DeleteAsync(string key, byte[] value) =>
            (long)await _database.EvalAsync(@"if redis.call('GET', KEYS[1]) == ARGV[1] then
    return redis.call('DEL', KEYS[1]);
end
return -1;", $"{_name}/{key}", value) >= 0;

        public bool CanRetry(Exception ex) => ex is RedisClientException;
    }
}
