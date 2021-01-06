namespace EasyCaching.CSRedis
{
    using EasyCaching.Core;
    using EasyCaching.Core.Internal;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public partial class DefaultCSRedisCachingProvider : IRedisCachingProvider
    {
        public long ZAdd<T>(string cacheKey, Dictionary<T, double> cacheValues)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            var param = new List<(decimal, object)>();

            foreach (var item in cacheValues)
            {
                param.Add(((decimal, object))(item.Value, _serializer.Serialize(item.Key)));
            }

            var len = _cache.ZAdd(cacheKey, param.ToArray());

            return len;
        }

        public long ZCard(string cacheKey)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            var len = _cache.ZCard(cacheKey);
            return len;
        }

        public long ZCount(string cacheKey, double min, double max)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            var len = _cache.ZCount(cacheKey, (decimal)min, (decimal)max);
            return len;
        }
        public double ZIncrBy(string cacheKey, string field, double val = 1)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));
            ArgumentCheck.NotNullOrWhiteSpace(field, nameof(field));

            var value = _cache.ZIncrBy(cacheKey, field, (decimal)val);
            return (double)value;
        }
        public long ZLexCount(string cacheKey, string min, string max)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            var len = _cache.ZLexCount(cacheKey, min, max);
            return len;
        }

        public List<T> ZRange<T>(string cacheKey, long start, long stop)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            var list = new List<T>();

            var bytes = _cache.ZRange<byte[]>(cacheKey, start, stop);

            foreach (var item in bytes)
            {
                list.Add(_serializer.Deserialize<T>(item));
            }

            return list;
        }

        public long? ZRank<T>(string cacheKey, T cacheValue)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            var bytes = _serializer.Serialize(cacheValue);

            var index = _cache.ZRank(cacheKey, bytes);

            return index;
        }


        public long ZRem<T>(string cacheKey, IList<T> cacheValues)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            var bytes = new List<byte[]>();

            foreach (var item in cacheValues)
            {
                bytes.Add(_serializer.Serialize(item));
            }

            var len = _cache.ZRem<byte[]>(cacheKey, bytes.ToArray());

            return len;
        }

        public double? ZScore<T>(string cacheKey, T cacheValue)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            var bytes = _serializer.Serialize(cacheValue);

            var score = _cache.ZScore(cacheKey, bytes);

            return (double?)score;
        }

        public async Task<long> ZAddAsync<T>(string cacheKey, Dictionary<T, double> cacheValues)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            var param = new List<(decimal, object)>();

            foreach (var item in cacheValues)
            {
                param.Add(((decimal, object))(item.Value, _serializer.Serialize(item.Key)));
            }

            var len = await _cache.ZAddAsync(cacheKey, param.ToArray());

            return len;
        }


        public async Task<long> ZCardAsync(string cacheKey)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            var len = await _cache.ZCardAsync(cacheKey);
            return len;
        }

        public async Task<long> ZCountAsync(string cacheKey, double min, double max)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            var len = await _cache.ZCountAsync(cacheKey, (decimal)min, (decimal)max);
            return len;
        }

        public async Task<double> ZIncrByAsync(string cacheKey, string field, double val = 1)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));
            ArgumentCheck.NotNullOrWhiteSpace(field, nameof(field));

            var value= await _cache.ZIncrByAsync(cacheKey, field, (decimal)val);
            return (double)value;
        }

        public async Task<long> ZLexCountAsync(string cacheKey, string min, string max)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            var len = await _cache.ZLexCountAsync(cacheKey, min, max);
            return len;
        }

        public async Task<List<T>> ZRangeAsync<T>(string cacheKey, long start, long stop)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            var list = new List<T>();

            var bytes = await _cache.ZRangeAsync<byte[]>(cacheKey, start, stop);

            foreach (var item in bytes)
            {
                list.Add(_serializer.Deserialize<T>(item));
            }

            return list;
        }

        public async Task<long?> ZRankAsync<T>(string cacheKey, T cacheValue)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            var bytes = _serializer.Serialize(cacheValue);

            var index = await _cache.ZRankAsync(cacheKey, bytes);

            return index;
        }

        public async Task<long> ZRemAsync<T>(string cacheKey, IList<T> cacheValues)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            var bytes = new List<byte[]>();

            foreach (var item in cacheValues)
            {
                bytes.Add(_serializer.Serialize(item));
            }

            var len = await _cache.ZRemAsync<byte[]>(cacheKey, bytes.ToArray());

            return len;
        }

        public async Task<double?> ZScoreAsync<T>(string cacheKey, T cacheValue)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            var bytes = _serializer.Serialize(cacheValue);

            var score = await _cache.ZScoreAsync(cacheKey, bytes);

            return (double?)score;
        }

    }
}
