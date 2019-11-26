namespace EasyCaching.CSRedis
{
    using EasyCaching.Core;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public partial class DefaultCSRedisCachingProvider : IRedisCachingProvider
    {
        public bool PfAdd<T>(string cacheKey, List<T> values)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));
            ArgumentCheck.NotNullAndCountGTZero(values, nameof(values));

            var list = new List<byte[]>();

            foreach (var item in values)
            {
                list.Add(_serializer.Serialize(item));
            }

            var res = _cache.PfAdd(cacheKey, list.ToArray());
            return res;
        }

        public async Task<bool> PfAddAsync<T>(string cacheKey, List<T> values)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));
            ArgumentCheck.NotNullAndCountGTZero(values, nameof(values));

            var list = new List<byte[]>();

            foreach (var item in values)
            {
                list.Add(_serializer.Serialize(item));
            }

            var res = await _cache.PfAddAsync(cacheKey, list.ToArray());
            return res;
        }

        [System.Obsolete()]
        public long PfCount(List<string> cacheKeys)
        {
            ArgumentCheck.NotNullAndCountGTZero(cacheKeys, nameof(cacheKeys));
          
            var res = _cache.PfCount(cacheKeys.ToArray());
            return res;
        }

        [System.Obsolete()]
        public async Task<long> PfCountAsync(List<string> cacheKeys)
        {
            ArgumentCheck.NotNullAndCountGTZero(cacheKeys, nameof(cacheKeys));

            var res = await _cache.PfCountAsync(cacheKeys.ToArray());
            return res;
        }

        [System.Obsolete()]
        public bool PfMerge(string destKey, List<string> sourceKeys)
        {
            ArgumentCheck.NotNullOrWhiteSpace(destKey, nameof(destKey));
            ArgumentCheck.NotNullAndCountGTZero(sourceKeys, nameof(sourceKeys));

            var res = _cache.PfMerge(destKey, sourceKeys.ToArray());
            return res;
        }

        [System.Obsolete()]
        public async Task<bool> PfMergeAsync(string destKey, List<string> sourceKeys)
        {
            ArgumentCheck.NotNullOrWhiteSpace(destKey, nameof(destKey));
            ArgumentCheck.NotNullAndCountGTZero(sourceKeys, nameof(sourceKeys));

            var res = await _cache.PfMergeAsync(destKey, sourceKeys.ToArray());
            return res;
        }
    }
}
