
namespace EasyCaching.CSRedis
{
    using EasyCaching.Core;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IRedisCachingProvider : IEasyCachingProvider
    {
        #region Hashes
        bool HMSet(string cacheKey, Dictionary<string, string> vals, TimeSpan? expiration = null);
        bool HSet(string cacheKey, string field, string cacheValue);
        bool HExists(string cacheKey, string field);
        long HDel(string cacheKey, IList<string> fields = null);
        string HGet(string cacheKey, string field);
        Dictionary<string, string> HGetAll(string cacheKey);
        long HIncrBy(string cacheKey, string field, long val = 1);
        List<string> HKeys(string cacheKey);
        long HLen(string cacheKey);
        List<string> HVals(string cacheKey);
        Dictionary<string, string> HMGet(string cacheKey, IList<string> fields);
        Task<bool> HMSetAsync(string cacheKey, Dictionary<string, string> vals, TimeSpan? expiration = null);
        Task<bool> HSetAsync(string cacheKey, string field, string cacheValue);
        Task<bool> HExistsAsync(string cacheKey, string field);
        Task<long> HDelAsync(string cacheKey, IList<string> fields = null);
        Task<string> HGetAsync(string cacheKey, string field);
        Task<Dictionary<string, string>> HGetAllAsync(string cacheKey);
        Task<long> HIncrByAsync(string cacheKey, string field, long val = 1);
        Task<List<string>> HKeysAsync(string cacheKey);
        Task<long> HLenAsync(string cacheKey);
        Task<List<string>> HValsAsync(string cacheKey);
        Task<Dictionary<string, string>> HMGetAsync(string cacheKey, IList<string> fields);
        #endregion

        #region List

        T LIndex<T>(string cacheKey, long index);
        long LLen(string cacheKey);
        T LPop<T>(string cacheKey);
        long LPush<T>(string cacheKey, IList<T> cacheValues);
        List<T> LRange<T>(string cacheKey, long start, long stop);
        long LRem<T>(string cacheKey, long count, T cacheValue);
        bool LSet<T>(string cacheKey, long index, T cacheValue);
        bool LTrim(string cacheKey, long start, long stop);
        long LPushX<T>(string cacheKey, T cacheValue);
        long LInsertBefore<T>(string cacheKey, T pivot, T cacheValue);
        long LInsertAfter<T>(string cacheKey, T pivot, T cacheValue);
        Task<T> LIndexAsync<T>(string cacheKey, long index);
        Task<long> LLenAsync(string cacheKey);
        Task<T> LPopAsync<T>(string cacheKey);
        Task<long> LPushAsync<T>(string cacheKey, IList<T> cacheValues);
        Task<List<T>> LRangeAsync<T>(string cacheKey, long start, long stop);
        Task<long> LRemAsync<T>(string cacheKey, long count, T cacheValue);
        Task<bool> LSetAsync<T>(string cacheKey, long index, T cacheValue);
        Task<bool> LTrimAsync(string cacheKey, long start, long stop);
        Task<long> LPushXAsync<T>(string cacheKey, T cacheValue);
        Task<long> LInsertBeforeAsync<T>(string cacheKey, T pivot, T cacheValue);
        Task<long> LInsertAfterAsync<T>(string cacheKey, T pivot, T cacheValue);
        #endregion
    }
}
