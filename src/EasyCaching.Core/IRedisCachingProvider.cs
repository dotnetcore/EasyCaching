namespace EasyCaching.Core
{
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
        long RPushX<T>(string cacheKey, T cacheValue);
        long RPush<T>(string cacheKey, IList<T> cacheValues);
        T RPop<T>(string cacheKey);
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
        Task<long> RPushXAsync<T>(string cacheKey, T cacheValue);
        Task<long> RPushAsync<T>(string cacheKey, IList<T> cacheValues);
        Task<T> RPopAsync<T>(string cacheKey);
        #endregion

        #region Set
        long SAdd<T>(string cacheKey, IList<T> cacheValues, TimeSpan? expiration = null);
        long SCard(string cacheKey);
        bool SIsMember<T>(string cacheKey, T cacheValue);
        List<T> SMembers<T>(string cacheKey);
        T SPop<T>(string cacheKey);
        List<T> SRandMember<T>(string cacheKey, int count = 1);
        long SRem<T>(string cacheKey, IList<T> cacheValues = null);
        Task<long> SAddAsync<T>(string cacheKey, IList<T> cacheValues, TimeSpan? expiration = null);
        Task<long> SCardAsync(string cacheKey);
        Task<bool> SIsMemberAsync<T>(string cacheKey, T cacheValue);
        Task<List<T>> SMembersAsync<T>(string cacheKey);
        Task<T> SPopAsync<T>(string cacheKey);
        Task<List<T>> SRandMemberAsync<T>(string cacheKey, int count = 1);
        Task<long> SRemAsync<T>(string cacheKey, IList<T> cacheValues = null);
        #endregion

        #region Sorted Set
        long ZAdd<T>(string cacheKey, Dictionary<T, double> cacheValues);
        long ZCard(string cacheKey);
        long ZCount(string cacheKey, double min, double max);
        long ZLexCount(string cacheKey, string min, string max);
        List<T> ZRange<T>(string cacheKey, long start, long stop);
        long? ZRank<T>(string cacheKey, T cacheValue);
        long ZRem<T>(string cacheKey, IList<T> cacheValues);
        Task<long> ZAddAsync<T>(string cacheKey, Dictionary<T, double> cacheValues);
        Task<long> ZCardAsync(string cacheKey);
        Task<long> ZCountAsync(string cacheKey, double min, double max);
        Task<long> ZLexCountAsync(string cacheKey, string min, string max);
        Task<List<T>> ZRangeAsync<T>(string cacheKey, long start, long stop);
        Task<long?> ZRankAsync<T>(string cacheKey, T cacheValue);
        Task<long> ZRemAsync<T>(string cacheKey, IList<T> cacheValues);
        #endregion
    }
}
