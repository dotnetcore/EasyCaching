namespace EasyCaching.Redis
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using EasyCaching.Core;
    using EasyCaching.Core.Internal;
    using StackExchange.Redis;

    /// <summary>
    /// Default redis caching provider.
    /// </summary>
    public partial class DefaultRedisCachingProvider : IEasyCachingProvider
    {
        public T LIndex<T>(string cacheKey, long index)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            var bytes = _cache.ListGetByIndex(cacheKey, index);
            return _serializer.Deserialize<T>(bytes);
        }

        public long LLen(string cacheKey)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            return _cache.ListLength(cacheKey);
        }

        public T LPop<T>(string cacheKey)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            var bytes = _cache.ListLeftPop(cacheKey);
            return _serializer.Deserialize<T>(bytes);
        }

        public long LPush<T>(string cacheKey, IList<T> cacheValues)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));
            ArgumentCheck.NotNullAndCountGTZero(cacheValues, nameof(cacheValues));

            var list = new List<RedisValue>();

            foreach (var item in cacheValues)
            {
                list.Add(_serializer.Serialize(item));
            }

            var len = _cache.ListLeftPush(cacheKey, list.ToArray());
            return len;
        }

        public List<T> LRange<T>(string cacheKey, long start, long stop)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            var list = new List<T>();

            var bytes = _cache.ListRange(cacheKey, start, stop);

            foreach (var item in bytes)
            {
                list.Add(_serializer.Deserialize<T>(item));
            }

            return list;
        }

        public long LRem<T>(string cacheKey, long count, T cacheValue)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            var bytes = _serializer.Serialize(cacheValue);
            return _cache.ListRemove(cacheKey, bytes, count);
        }

        public bool LSet<T>(string cacheKey, long index, T cacheValue)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            var bytes = _serializer.Serialize(cacheValue);
            _cache.ListSetByIndex(cacheKey, index, bytes);
            return true;
        }

        public bool LTrim(string cacheKey, long start, long stop)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            _cache.ListTrim(cacheKey, start, stop);
            return true;
        }

        public long LPushX<T>(string cacheKey, T cacheValue)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            var bytes = _serializer.Serialize(cacheValue);
            return _cache.ListLeftPush(cacheKey, bytes);
        }

        public long LInsertBefore<T>(string cacheKey, T pivot, T cacheValue)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            var pivotBytes = _serializer.Serialize(pivot);
            var cacheValueBytes = _serializer.Serialize(cacheValue);
            return _cache.ListInsertBefore(cacheKey, pivotBytes, cacheValueBytes);
        }

        public long LInsertAfter<T>(string cacheKey, T pivot, T cacheValue)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            var pivotBytes = _serializer.Serialize(pivot);
            var cacheValueBytes = _serializer.Serialize(cacheValue);
            return _cache.ListInsertAfter(cacheKey, pivotBytes, cacheValueBytes);
        }

        public long RPushX<T>(string cacheKey, T cacheValue)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            var bytes = _serializer.Serialize(cacheValue);
            return _cache.ListRightPush(cacheKey, bytes);            
        }

        public long RPush<T>(string cacheKey, IList<T> cacheValues)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));
            ArgumentCheck.NotNullAndCountGTZero(cacheValues, nameof(cacheValues));

            var list = new List<RedisValue>();

            foreach (var item in cacheValues)
            {
                list.Add(_serializer.Serialize(item));
            }

            var len = _cache.ListRightPush(cacheKey, list.ToArray());
            return len;
        }

        public T RPop<T>(string cacheKey)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            var bytes = _cache.ListRightPop(cacheKey);
            return _serializer.Deserialize<T>(bytes);
        }

        public async Task<T> LIndexAsync<T>(string cacheKey, long index)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            var bytes = await _cache.ListGetByIndexAsync(cacheKey, index);
            return _serializer.Deserialize<T>(bytes);
        }

        public async Task<long> LLenAsync(string cacheKey)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            return await _cache.ListLengthAsync(cacheKey);
        }

        public async Task<T> LPopAsync<T>(string cacheKey)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            var bytes = await _cache.ListLeftPopAsync(cacheKey);
            return _serializer.Deserialize<T>(bytes);
        }

        public async Task<long> LPushAsync<T>(string cacheKey, IList<T> cacheValues)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));
            ArgumentCheck.NotNullAndCountGTZero(cacheValues, nameof(cacheValues));

            var list = new List<RedisValue>();

            foreach (var item in cacheValues)
            {
                list.Add(_serializer.Serialize(item));
            }

            var len = await _cache.ListLeftPushAsync(cacheKey, list.ToArray());
            return len;
        }

        public async Task<List<T>> LRangeAsync<T>(string cacheKey, long start, long stop)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            var list = new List<T>();

            var bytes = await _cache.ListRangeAsync(cacheKey, start, stop);

            foreach (var item in bytes)
            {
                list.Add(_serializer.Deserialize<T>(item));
            }

            return list;
        }

        public async Task<long> LRemAsync<T>(string cacheKey, long count, T cacheValue)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            var bytes = _serializer.Serialize(cacheValue);
            return await _cache.ListRemoveAsync(cacheKey, bytes, count);
        }

        public async Task<bool> LSetAsync<T>(string cacheKey, long index, T cacheValue)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            var bytes = _serializer.Serialize(cacheValue);
            await _cache.ListSetByIndexAsync(cacheKey, index, bytes);
            return true;
        }

        public async Task<bool> LTrimAsync(string cacheKey, long start, long stop)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            await _cache.ListTrimAsync(cacheKey, start, stop);
            return true;
        }

        public Task<long> LPushXAsync<T>(string cacheKey, T cacheValue)
        {
            //ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            //var bytes = _serializer.Serialize(cacheValue);
            //return await _cache.ListLeftPushAsync(cacheKey, bytes);
            throw new NotImplementedException();
        }

        public async Task<long> LInsertBeforeAsync<T>(string cacheKey, T pivot, T cacheValue)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            var pivotBytes = _serializer.Serialize(pivot);
            var cacheValueBytes = _serializer.Serialize(cacheValue);
            return await _cache.ListInsertBeforeAsync(cacheKey, pivotBytes, cacheValueBytes);
        }

        public async Task<long> LInsertAfterAsync<T>(string cacheKey, T pivot, T cacheValue)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            var pivotBytes = _serializer.Serialize(pivot);
            var cacheValueBytes = _serializer.Serialize(cacheValue);
            return await _cache.ListInsertAfterAsync(cacheKey, pivotBytes, cacheValueBytes);
        }

        public Task<long> RPushXAsync<T>(string cacheKey, T cacheValue)
        {
            //ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            //var bytes = _serializer.Serialize(cacheValue);
            //return await _cache.ListRightPushAsync(cacheKey, bytes);
            throw new NotImplementedException();
        }

        public async Task<long> RPushAsync<T>(string cacheKey, IList<T> cacheValues)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));
            ArgumentCheck.NotNullAndCountGTZero(cacheValues, nameof(cacheValues));

            var list = new List<RedisValue>();

            foreach (var item in cacheValues)
            {
                list.Add(_serializer.Serialize(item));
            }

            var len = await _cache.ListRightPushAsync(cacheKey, list.ToArray());
            return len;
        }

        public async Task<T> RPopAsync<T>(string cacheKey)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            var bytes = await _cache.ListRightPopAsync(cacheKey);
            return _serializer.Deserialize<T>(bytes);
        }
    }
}
