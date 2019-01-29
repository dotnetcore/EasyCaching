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
        public long SAdd<T>(string cacheKey, IList<T> cacheValues, TimeSpan? expiration = null)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));
            ArgumentCheck.NotNullAndCountGTZero(cacheValues, nameof(cacheValues));

            var list = new List<byte[]>();

            foreach (var item in cacheValues)
            {
                list.Add(_serializer.Serialize(item));
            }

            var len = _cache.SAdd<byte[]>(cacheKey, list.ToArray());

            if (expiration.HasValue)
            {
                _cache.Expire(cacheKey, expiration.Value.Seconds);
            }

            return len;
        }

        public long SCard(string cacheKey)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            var len = _cache.SCard(cacheKey);
            return len;
        }

        public bool SIsMember<T>(string cacheKey,T cacheValue)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            var bytes = _serializer.Serialize(cacheValue);

            var flag = _cache.SIsMember(cacheKey, bytes);
            return flag;
        }

        public List<T> SMembers<T>(string cacheKey)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            var list = new List<T>();

            var bytes = _cache.SMembers<byte[]>(cacheKey);

            foreach (var item in bytes)
            {
                list.Add(_serializer.Deserialize<T>(item));
            }
            
            return list;
        }

        public T SPop<T>(string cacheKey)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));
            
            var bytes = _cache.SPop<byte[]>(cacheKey);

            return _serializer.Deserialize<T>(bytes);
        }

        public List<T> SRandMember<T>(string cacheKey, int count = 1)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            var list = new List<T>();

            var bytes = _cache.SRandMembers<byte[]>(cacheKey, count);

            foreach (var item in bytes)
            {
                list.Add(_serializer.Deserialize<T>(item));
            }

            return list;
        }

        public long SRem<T>(string cacheKey, IList<T> cacheValues = null)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            var len = 0L;

            if (cacheValues != null && cacheValues.Any())
            {
                var bytes = new List<byte[]>();

                foreach (var item in cacheValues)
                {
                    bytes.Add(_serializer.Serialize<T>(item));
                }

                len = _cache.SRem<byte[]>(cacheKey, bytes.ToArray());
            }
            else
            {
                len = _cache.Del(cacheKey);
            }

            return len;
        }

        public async Task<long> SAddAsync<T>(string cacheKey, IList<T> cacheValues, TimeSpan? expiration = null)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));
            ArgumentCheck.NotNullAndCountGTZero(cacheValues, nameof(cacheValues));

            var list = new List<byte[]>();

            foreach (var item in cacheValues)
            {
                list.Add(_serializer.Serialize(item));
            }

            var len = await _cache.SAddAsync<byte[]>(cacheKey, list.ToArray());

            if (expiration.HasValue)
            {
                await _cache.ExpireAsync(cacheKey, expiration.Value.Seconds);
            }

            return len;
        }

        public async Task<long> SCardAsync(string cacheKey)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            var len = await _cache.SCardAsync(cacheKey);
            return len;
        }

        public async Task<bool> SIsMemberAsync<T>(string cacheKey, T cacheValue)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            var bytes = _serializer.Serialize(cacheValue);

            var flag = await _cache.SIsMemberAsync(cacheKey, bytes);
            return flag;
        }

        public async Task<List<T>> SMembersAsync<T>(string cacheKey)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            var list = new List<T>();

            var vals = await _cache.SMembersAsync<byte[]>(cacheKey);

            foreach (var item in vals)
            {
                list.Add(_serializer.Deserialize<T>(item));
            }

            return list;
        }

        public async Task<T> SPopAsync<T>(string cacheKey)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            var bytes = await _cache.SPopAsync<byte[]>(cacheKey);

            return _serializer.Deserialize<T>(bytes);
        }

        public async Task< List<T>> SRandMemberAsync<T>(string cacheKey, int count = 1)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            var list = new List<T>();

            var bytes = await _cache.SRandMembersAsync<byte[]>(cacheKey, count);

            foreach (var item in bytes)
            {
                list.Add(_serializer.Deserialize<T>(item));
            }

            return list;
        }

        public async Task<long> SRemAsync<T>(string cacheKey, IList<T> cacheValues = null)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            var len = 0L;

            if (cacheValues != null && cacheValues.Any())
            {
                var bytes = new List<byte[]>();

                foreach (var item in cacheValues)
                {
                    bytes.Add(_serializer.Serialize<T>(item));
                }

                len = await _cache.SRemAsync<byte[]>(cacheKey, bytes.ToArray());
            }
            else
            {
                len = await _cache.DelAsync(cacheKey);
            }

            return len;
        }
    }
}
