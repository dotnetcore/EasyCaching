﻿namespace EasyCaching.Redis
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using EasyCaching.Core;
    using StackExchange.Redis;

    /// <summary>
    /// Default redis caching provider.
    /// </summary>
    public partial class DefaultRedisCachingProvider : IEasyCachingProvider
    {
        public long ZAdd<T>(string cacheKey, Dictionary<T, double> cacheValues)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            var param = new List<SortedSetEntry>();

            foreach (var item in cacheValues)
            {
                param.Add(new SortedSetEntry(_serializer.Serialize(item.Key), item.Value));
            }

            var len = _cache.SortedSetAdd(cacheKey, param.ToArray());

            return len;
        }

        public long ZCard(string cacheKey)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            var len = _cache.SortedSetLength(cacheKey);
            return len;
        }

        public long ZCount(string cacheKey, double min, double max)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            var len = _cache.SortedSetLengthByValue(cacheKey, min, max);
            return len;
        }

        public double ZIncrBy(string cacheKey, string field, double val = 1)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));
            ArgumentCheck.NotNullOrWhiteSpace(field, nameof(field));

            var value = _cache.SortedSetIncrement(cacheKey, field, val);
            return value;
        }

        public long ZLexCount(string cacheKey, string min, string max)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            var len = _cache.SortedSetLengthByValue(cacheKey, min, max);
            return len;
        }

        public List<T> ZRange<T>(string cacheKey, long start, long stop)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            var list = new List<T>();

            var bytes = _cache.SortedSetRangeByRank(cacheKey, start, stop);

            foreach (var item in bytes)
            {
                list.Add(_serializer.Deserialize<T>(item));
            }

            return list;
        }

        public List<T> ZRangeByScore<T>(string cacheKey, double min, double max, long? count = null, long offset = 0)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            var list = new List<T>();

            var bytes = _cache.SortedSetRangeByScore(cacheKey, min, max, skip: offset, take: count ?? -1);

            foreach (var item in bytes)
            {
                list.Add(_serializer.Deserialize<T>(item));
            }

            return list;
        }

        public long ZRangeRemByScore(string cacheKey, double min, double max)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            return _cache.SortedSetRemoveRangeByScore(cacheKey, min, max);
        }

        public long? ZRank<T>(string cacheKey, T cacheValue)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            var bytes = _serializer.Serialize(cacheValue);

            var index = _cache.SortedSetRank(cacheKey, bytes);

            return index;
        }


        public long ZRem<T>(string cacheKey, IList<T> cacheValues)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            var bytes = new List<RedisValue>();

            foreach (var item in cacheValues)
            {
                bytes.Add(_serializer.Serialize(item));
            }

            var len = _cache.SortedSetRemove(cacheKey, bytes.ToArray());

            return len;
        }

        public double? ZScore<T>(string cacheKey, T cacheValue)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            var bytes = _serializer.Serialize(cacheValue);

            var score = _cache.SortedSetScore(cacheKey, bytes);

            return score;
        }

        public async Task<long> ZAddAsync<T>(string cacheKey, Dictionary<T, double> cacheValues)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            var param = new List<SortedSetEntry>();

            foreach (var item in cacheValues)
            {
                param.Add(new SortedSetEntry(_serializer.Serialize(item.Key), item.Value));
            }

            var len = await _cache.SortedSetAddAsync(cacheKey, param.ToArray());

            return len;
        }


        public async Task<long> ZCardAsync(string cacheKey)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            var len = await _cache.SortedSetLengthAsync(cacheKey);
            return len;
        }

        public async Task<long> ZCountAsync(string cacheKey, double min, double max)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            var len = await _cache.SortedSetLengthByValueAsync(cacheKey, min, max);
            return len;
        }

        public async Task<double> ZIncrByAsync(string cacheKey, string field, double val = 1)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));
            ArgumentCheck.NotNullOrWhiteSpace(field, nameof(field));

            var value = await _cache.SortedSetIncrementAsync(cacheKey, field, val);
            return value;
        }

        public async Task<long> ZLexCountAsync(string cacheKey, string min, string max)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            var len = await _cache.SortedSetLengthByValueAsync(cacheKey, min, max);
            return len;
        }

        public async Task<List<T>> ZRangeAsync<T>(string cacheKey, long start, long stop)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            var list = new List<T>();

            var bytes = await _cache.SortedSetRangeByRankAsync(cacheKey, start, stop);

            foreach (var item in bytes)
            {
                list.Add(_serializer.Deserialize<T>(item));
            }

            return list;
        }

        public async Task<List<T>> ZRangeByScoreAsync<T>(string cacheKey, double min, double max, long? count = null, long offset = 0)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            var list = new List<T>();

            var bytes = await _cache.SortedSetRangeByScoreAsync(cacheKey, min, max, skip: offset, take: count ?? -1);

            foreach (var item in bytes)
            {
                list.Add(_serializer.Deserialize<T>(item));
            }

            return list;
        }

        public async Task<long> ZRangeRemByScoreAsync(string cacheKey, double min, double max)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));
            
            return await _cache.SortedSetRemoveRangeByScoreAsync(cacheKey, min, max);
        }

        public async Task<long?> ZRankAsync<T>(string cacheKey, T cacheValue)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            var bytes = _serializer.Serialize(cacheValue);

            var index = await _cache.SortedSetRankAsync(cacheKey, bytes);

            return index;
        }

        public async Task<long> ZRemAsync<T>(string cacheKey, IList<T> cacheValues)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            var bytes = new List<RedisValue>();

            foreach (var item in cacheValues)
            {
                bytes.Add(_serializer.Serialize(item));
            }

            var len = await _cache.SortedSetRemoveAsync(cacheKey, bytes.ToArray());

            return len;
        }

        public async Task<double?> ZScoreAsync<T>(string cacheKey, T cacheValue)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            var bytes = _serializer.Serialize(cacheValue);

            var score = await _cache.SortedSetScoreAsync(cacheKey, bytes);

            return score;
        }
    }
}
