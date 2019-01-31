namespace EasyCaching.CSRedis
{
    using EasyCaching.Core;
    using EasyCaching.Core.Internal;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public partial class DefaultCSRedisCachingProvider : IRedisCachingProvider
    {
        public T LIndex<T>(string cacheKey, long index)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            var bytes = _cache.LIndex<byte[]>(cacheKey, index);
            return _serializer.Deserialize<T>(bytes);
        }

        public long LLen(string cacheKey)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            return _cache.LLen(cacheKey);
        }

        public T LPop<T>(string cacheKey)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            var bytes = _cache.LPop<byte[]>(cacheKey);
            return _serializer.Deserialize<T>(bytes);
        }

        public long LPush<T>(string cacheKey, IList<T> cacheValues)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));
            ArgumentCheck.NotNullAndCountGTZero(cacheValues, nameof(cacheValues));

            var list = new List<byte[]>();

            foreach (var item in cacheValues)
            {
                list.Add(_serializer.Serialize(item));
            }

            var len = _cache.LPush<byte[]>(cacheKey, list.ToArray());
            return len;
        }

        public List<T> LRange<T>(string cacheKey, long start, long stop)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            var list = new List<T>();

            var bytes = _cache.LRange<byte[]>(cacheKey, start, stop);

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
            return _cache.LRem(cacheKey, count, bytes);
        }

        public bool LSet<T>(string cacheKey, long index, T cacheValue)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            var bytes = _serializer.Serialize(cacheValue);
            return _cache.LSet(cacheKey, index, bytes);
        }

        public bool LTrim(string cacheKey, long start, long stop)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            return _cache.LTrim(cacheKey, start, stop);
        }

        public long LPushX<T>(string cacheKey, T cacheValue)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            var bytes = _serializer.Serialize(cacheValue);
            return _cache.LPushX(cacheKey, bytes);
        }

        public long LInsertBefore<T>(string cacheKey, T pivot, T cacheValue)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            var pivotBytes = _serializer.Serialize(pivot);
            var cacheValueBytes = _serializer.Serialize(cacheValue);
            return _cache.LInsertBefore(cacheKey, pivotBytes, cacheValueBytes);
        }

        public long LInsertAfter<T>(string cacheKey, T pivot, T cacheValue)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            var pivotBytes = _serializer.Serialize(pivot);
            var cacheValueBytes = _serializer.Serialize(cacheValue);
            return _cache.LInsertAfter(cacheKey, pivotBytes, cacheValueBytes);
        }

        public long RPushX<T>(string cacheKey, T cacheValue)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            var bytes = _serializer.Serialize(cacheValue);
            return _cache.RPushX(cacheKey, bytes);
        }

        public long RPush<T>(string cacheKey, IList<T> cacheValues)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));
            ArgumentCheck.NotNullAndCountGTZero(cacheValues, nameof(cacheValues));

            var list = new List<byte[]>();

            foreach (var item in cacheValues)
            {
                list.Add(_serializer.Serialize(item));
            }

            var len = _cache.RPush<byte[]>(cacheKey, list.ToArray());
            return len;
        }

        public T RPop<T>(string cacheKey)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            var bytes = _cache.RPop<byte[]>(cacheKey);
            return _serializer.Deserialize<T>(bytes);
        }

        public async Task<T> LIndexAsync<T>(string cacheKey, long index)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            var bytes = await _cache.LIndexAsync<byte[]>(cacheKey, index);
            return _serializer.Deserialize<T>(bytes);
        }

        public async Task<long> LLenAsync(string cacheKey)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            return await _cache.LLenAsync(cacheKey);
        }

        public async Task<T> LPopAsync<T>(string cacheKey)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            var bytes = await _cache.LPopAsync<byte[]>(cacheKey);
            return _serializer.Deserialize<T>(bytes);
        }

        public async Task<long> LPushAsync<T>(string cacheKey, IList<T> cacheValues)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));
            ArgumentCheck.NotNullAndCountGTZero(cacheValues, nameof(cacheValues));

            var list = new List<byte[]>();

            foreach (var item in cacheValues)
            {
                list.Add(_serializer.Serialize(item));
            }

            var len = await _cache.LPushAsync<byte[]>(cacheKey, list.ToArray());
            return len;
        }

        public async Task<List<T>> LRangeAsync<T>(string cacheKey, long start, long stop)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            var list = new List<T>();

            var bytes = await _cache.LRangeAsync<byte[]>(cacheKey, start, stop);

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
            return await _cache.LRemAsync(cacheKey, count, bytes);
        }

        public async Task<bool> LSetAsync<T>(string cacheKey, long index, T cacheValue)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            var bytes = _serializer.Serialize(cacheValue);
            return await _cache.LSetAsync(cacheKey, index, bytes);
        }

        public async Task<bool> LTrimAsync(string cacheKey, long start, long stop)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            return await _cache.LTrimAsync(cacheKey, start, stop);
        }

        public async Task<long> LPushXAsync<T>(string cacheKey, T cacheValue)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            var bytes = _serializer.Serialize(cacheValue);
            return await _cache.LPushXAsync(cacheKey, bytes);
        }

        public async Task<long> LInsertBeforeAsync<T>(string cacheKey, T pivot, T cacheValue)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            var pivotBytes = _serializer.Serialize(pivot);
            var cacheValueBytes = _serializer.Serialize(cacheValue);
            return await _cache.LInsertBeforeAsync(cacheKey, pivotBytes, cacheValueBytes);
        }

        public async Task<long> LInsertAfterAsync<T>(string cacheKey, T pivot, T cacheValue)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            var pivotBytes = _serializer.Serialize(pivot);
            var cacheValueBytes = _serializer.Serialize(cacheValue);
            return await _cache.LInsertAfterAsync(cacheKey, pivotBytes, cacheValueBytes);
        }

        public async Task<long> RPushXAsync<T>(string cacheKey, T cacheValue)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            var bytes = _serializer.Serialize(cacheValue);
            return await _cache.RPushXAsync(cacheKey, bytes);
        }

        public async Task<long> RPushAsync<T>(string cacheKey, IList<T> cacheValues)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));
            ArgumentCheck.NotNullAndCountGTZero(cacheValues, nameof(cacheValues));

            var list = new List<byte[]>();

            foreach (var item in cacheValues)
            {
                list.Add(_serializer.Serialize(item));
            }

            var len = await _cache.RPushAsync<byte[]>(cacheKey, list.ToArray());
            return len;
        }

        public async Task<T> RPopAsync<T>(string cacheKey)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            var bytes = await _cache.RPopAsync<byte[]>(cacheKey);
            return _serializer.Deserialize<T>(bytes);
        }
    }
}
