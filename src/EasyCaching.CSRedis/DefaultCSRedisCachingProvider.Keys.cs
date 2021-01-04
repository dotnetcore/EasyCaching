namespace EasyCaching.CSRedis
{
    using EasyCaching.Core;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public partial class DefaultCSRedisCachingProvider : IRedisCachingProvider
    {
        public string RedisName => this._name;

        public bool KeyDel(string cacheKey)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            var count = _cache.Del(cacheKey);
            return count == 1;
        }

        public async Task<bool> KeyDelAsync(string cacheKey)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            var count = await _cache.DelAsync(cacheKey);
            return count == 1;
        }

        public bool KeyExpire(string cacheKey, int second)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            var flag = _cache.Expire(cacheKey, second);
            return flag;
        }

        public async Task<bool> KeyExpireAsync(string cacheKey, int second)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            var flag = await _cache.ExpireAsync(cacheKey, second);
            return flag;
        }

        public bool KeyExists(string cacheKey)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            var flag = _cache.Exists(cacheKey);
            return flag;
        }

        public async Task<bool> KeyExistsAsync(string cacheKey)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            var flag = await _cache.ExistsAsync(cacheKey);
            return flag;
        }

        public long TTL(string cacheKey)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            var second = _cache.Ttl(cacheKey);
            return second;
        }

        public async Task<long> TTLAsync(string cacheKey)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            var second = await _cache.TtlAsync(cacheKey);
            return second;
        }

        public object Eval(string script, string cacheKey, List<object> args)
        {
            ArgumentCheck.NotNullOrWhiteSpace(script, nameof(script));
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            var res = _cache.Eval(script, cacheKey, args.ToArray());
            return res;
        }

        public async Task<object> EvalAsync(string script, string cacheKey, List<object> args)
        {
            ArgumentCheck.NotNullOrWhiteSpace(script, nameof(script));
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            var res = await _cache.EvalAsync(script, cacheKey, args.ToArray());
            return res;
        }

        public List<string> SearchKeys(string cacheKey, int? count)
        {
            var keys = new List<string>();

            long nextCursor = 0;
            do
            {
                var scanResult = _cache.Scan(nextCursor, cacheKey, count);
                nextCursor = scanResult.Cursor;
                var items = scanResult.Items;
                keys.AddRange(items);
            }
            while (nextCursor != 0);

            return keys;
        }
    }
}
