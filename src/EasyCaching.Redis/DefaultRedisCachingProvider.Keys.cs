namespace EasyCaching.Redis
{
    using EasyCaching.Core;
    using StackExchange.Redis;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    /// <summary>
    /// Default redis caching provider.
    /// </summary>
    public partial class DefaultRedisCachingProvider : IRedisCachingProvider
    {
        public string RedisName => this._name;

        public bool KeyDel(string cacheKey)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            var flag = _cache.KeyDelete(cacheKey);
            return flag;
        }

        public async Task<bool> KeyDelAsync(string cacheKey)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            var flag = await _cache.KeyDeleteAsync(cacheKey);
            return flag;
        }

        public bool KeyExpire(string cacheKey, int second)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            var flag = _cache.KeyExpire(cacheKey, TimeSpan.FromSeconds(second));
            return flag;
        }

        public async Task<bool> KeyExpireAsync(string cacheKey, int second)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            var flag = await _cache.KeyExpireAsync(cacheKey, TimeSpan.FromSeconds(second));
            return flag;
        }

        public bool KeyExists(string cacheKey)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            var flag = _cache.KeyExists(cacheKey);
            return flag;
        }

        public async Task<bool> KeyExistsAsync(string cacheKey)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            var flag = await _cache.KeyExistsAsync(cacheKey);
            return flag;
        }

        public long TTL(string cacheKey)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            var ts = _cache.KeyTimeToLive(cacheKey);
            return ts.HasValue ? (long)ts.Value.TotalSeconds : -1;
        }

        public async Task<long> TTLAsync(string cacheKey)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            var ts = await _cache.KeyTimeToLiveAsync(cacheKey);
            return ts.HasValue ? (long)ts.Value.TotalSeconds : -1;
        }

        public object Eval(string script, string cacheKey, List<object> args)
        {
            ArgumentCheck.NotNullOrWhiteSpace(script, nameof(script));
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            var redisKey = new RedisKey[] { cacheKey };

            var redisValues = new List<RedisValue>();

            foreach (var item in args)
            {
                if (item.GetType().Equals(typeof(byte[])))
                {
                    redisValues.Add((byte[])item);
                }
                else
                {
                    redisValues.Add(item.ToString());
                }
            }

            var res = _cache.ScriptEvaluate(script, redisKey, redisValues.ToArray());

            return res;
        }

        public async Task<object> EvalAsync(string script, string cacheKey, List<object> args)
        {
            ArgumentCheck.NotNullOrWhiteSpace(script, nameof(script));
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            var redisKey = new RedisKey[] { cacheKey };

            var redisValues = new List<RedisValue>();

            foreach (var item in args)
            {
                if (item.GetType().Equals(typeof(byte[])))
                {
                    redisValues.Add((byte[])item);
                }
                else
                {
                    redisValues.Add(item.ToString());
                }
            }

            var res = await _cache.ScriptEvaluateAsync(script, redisKey, redisValues.ToArray());

            return res;
        }
    }
}
