namespace EasyCaching.CSRedis
{
    using EasyCaching.Core;
    using EasyCaching.Core.Internal;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public partial class DefaultCSRedisCachingProvider : IRedisCachingProvider
    {
        public bool HMSet(string cacheKey, Dictionary<string, string> vals, TimeSpan? expiration = null)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));
            ArgumentCheck.NotNull(vals, nameof(vals));

            var list = new List<object>(vals.Count * 2);
            foreach (var item in vals)
            {
                list.Add(item.Key);
                list.Add(item.Value);
            }
            var flag = _cache.HMSet(cacheKey, list.ToArray());
            if (expiration.HasValue)
            {
                flag = flag && _cache.Expire(cacheKey, expiration.Value);
            }
            return flag;
        }

        public bool HSet(string cacheKey, string field, string cacheValue)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));
            ArgumentCheck.NotNullOrWhiteSpace(field, nameof(field));

            return _cache.HSet(cacheKey, field, cacheValue);
        }

        public bool HExists(string cacheKey, string field)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));
            ArgumentCheck.NotNullOrWhiteSpace(field, nameof(field));

            return _cache.HExists(cacheKey, field);
        }

        public long HDel(string cacheKey, IList<string> fields = null)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            if (fields != null && fields.Any())
            {
                return _cache.HDel(cacheKey, fields.ToArray());
            }
            else
            {
                return _cache.Del(cacheKey);
            }
        }

        public string HGet(string cacheKey, string field)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));
            ArgumentCheck.NotNullOrWhiteSpace(field, nameof(field));

            var res = _cache.HGet(cacheKey, field);
            return res;
        }

        public Dictionary<string, string> HGetAll(string cacheKey)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            var res = _cache.HGetAll(cacheKey);
            return res;
        }

        public long HIncrBy(string cacheKey, string field, long val = 1)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));
            ArgumentCheck.NotNullOrWhiteSpace(field, nameof(field));

            return _cache.HIncrBy(cacheKey, field, val);
        }

        public List<string> HKeys(string cacheKey)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            var keys = _cache.HKeys(cacheKey);
            return keys.ToList();
        }

        public long HLen(string cacheKey)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            return _cache.HLen(cacheKey);
        }

        public List<string> HVals(string cacheKey)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            return _cache.HVals(cacheKey).ToList();
        }

        public Dictionary<string, string> HMGet(string cacheKey, IList<string> fields)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));
            ArgumentCheck.NotNullAndCountGTZero(fields, nameof(fields));

            var dict = new Dictionary<string, string>();

            var res = _cache.HMGet(cacheKey, fields.ToArray());

            for (int i = 0; i < fields.Count(); i++)
            {
                if (!dict.ContainsKey(fields[i]))
                {
                    dict.Add(fields[i], res[i]);
                }
            }

            return dict;
        }

        public async Task<bool> HMSetAsync(string cacheKey, Dictionary<string, string> vals, TimeSpan? expiration = null)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));
            ArgumentCheck.NotNull(vals, nameof(vals));

            var list = new List<object>(vals.Count * 2);
            foreach (var item in vals)
            {
                list.Add(item.Key);
                list.Add(item.Value);
            }
            var flag = await _cache.HMSetAsync(cacheKey, list.ToArray());
            if (expiration.HasValue)
            {
                flag = flag && await _cache.ExpireAsync(cacheKey, expiration.Value);
            }
            return flag;
        }

        public async Task<bool> HSetAsync(string cacheKey, string field, string cacheValue)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));
            ArgumentCheck.NotNullOrWhiteSpace(field, nameof(field));

            return await _cache.HSetAsync(cacheKey, field, cacheValue);
        }

        public async Task<bool> HExistsAsync(string cacheKey, string field)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));
            ArgumentCheck.NotNullOrWhiteSpace(field, nameof(field));

            return await _cache.HExistsAsync(cacheKey, field);
        }

        public async Task<long> HDelAsync(string cacheKey, IList<string> fields)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            if (fields != null && fields.Any())
            {
                return await _cache.HDelAsync(cacheKey, fields.ToArray());
            }
            else
            {
                return await _cache.DelAsync(cacheKey);
            }
        }

        public async Task<string> HGetAsync(string cacheKey, string field)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));
            ArgumentCheck.NotNullOrWhiteSpace(field, nameof(field));

            var res = await _cache.HGetAsync(cacheKey, field);
            return res;
        }

        public async Task<Dictionary<string, string>> HGetAllAsync(string cacheKey)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            var res = await _cache.HGetAllAsync(cacheKey);
            return res;
        }

        public async Task<long> HIncrByAsync(string cacheKey, string field, long val = 1)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));
            ArgumentCheck.NotNullOrWhiteSpace(field, nameof(field));

            return await _cache.HIncrByAsync(cacheKey, field, val);
        }

        public async Task<List<string>> HKeysAsync(string cacheKey)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            var keys = await _cache.HKeysAsync(cacheKey);
            return keys.ToList();
        }

        public async Task<long> HLenAsync(string cacheKey)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            return await _cache.HLenAsync(cacheKey);
        }

        public async Task<List<string>> HValsAsync(string cacheKey)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            return (await _cache.HValsAsync(cacheKey)).ToList();
        }

        public async Task<Dictionary<string, string>> HMGetAsync(string cacheKey, IList<string> fields)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));
            ArgumentCheck.NotNullAndCountGTZero(fields, nameof(fields));

            var dict = new Dictionary<string, string>();

            var res = await _cache.HMGetAsync(cacheKey, fields.ToArray());

            for (int i = 0; i < fields.Count(); i++)
            {
                if (!dict.ContainsKey(fields[i]))
                {
                    dict.Add(fields[i], res[i]);
                }
            }

            return dict;
        }
    }
}
