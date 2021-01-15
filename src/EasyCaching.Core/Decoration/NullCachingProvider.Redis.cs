namespace EasyCaching.Core.Decoration
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public partial class NullCachingProvider : IRedisCachingProvider
    {
        public string RedisName => Name;
        
        public bool KeyDel(string cacheKey) => false;

        public bool KeyExpire(string cacheKey, int second) => false;

        public bool KeyExists(string cacheKey) => false;

        public long TTL(string cacheKey) => 0;

        public List<string> SearchKeys(string cacheKey, int? count = null) => new List<string>();

        public long IncrBy(string cacheKey, long value = 1) => value;

        public double IncrByFloat(string cacheKey, double value = 1) => value;

        public bool StringSet(string cacheKey, string cacheValue, TimeSpan? expiration = null, string when = "") => true;

        public string StringGet(string cacheKey) => null;

        public long StringLen(string cacheKey) => 0;

        public long StringSetRange(string cacheKey, long offest, string value) => 0;

        public string StringGetRange(string cacheKey, long start, long end) => null;

        public bool HMSet(string cacheKey, Dictionary<string, string> vals, TimeSpan? expiration = null) => true;

        public bool HSet(string cacheKey, string field, string cacheValue) => true;

        public bool HExists(string cacheKey, string field) => false;

        public long HDel(string cacheKey, IList<string> fields = null) => 0;

        public string HGet(string cacheKey, string field) => null;

        public Dictionary<string, string> HGetAll(string cacheKey) => new Dictionary<string, string>();

        public long HIncrBy(string cacheKey, string field, long val = 1) => val;

        public List<string> HKeys(string cacheKey) => new List<string>();

        public long HLen(string cacheKey) => 0;

        public List<string> HVals(string cacheKey) => new List<string>();

        public Dictionary<string, string> HMGet(string cacheKey, IList<string> fields) => new Dictionary<string, string>();

        public T LIndex<T>(string cacheKey, long index) => default;

        public long LLen(string cacheKey) => 0;

        public T LPop<T>(string cacheKey) => default;

        public long LPush<T>(string cacheKey, IList<T> cacheValues) => 0;

        public List<T> LRange<T>(string cacheKey, long start, long stop) => new List<T>();

        public long LRem<T>(string cacheKey, long count, T cacheValue) => 0;

        public bool LSet<T>(string cacheKey, long index, T cacheValue) => true;

        public bool LTrim(string cacheKey, long start, long stop) => true;

        public long LPushX<T>(string cacheKey, T cacheValue) => 0;

        public long LInsertBefore<T>(string cacheKey, T pivot, T cacheValue) => 0;

        public long LInsertAfter<T>(string cacheKey, T pivot, T cacheValue) => 0;

        public long RPushX<T>(string cacheKey, T cacheValue) => 0;

        public long RPush<T>(string cacheKey, IList<T> cacheValues) => 0;

        public T RPop<T>(string cacheKey) => default;

        public long SAdd<T>(string cacheKey, IList<T> cacheValues, TimeSpan? expiration = null) => 0;

        public long SCard(string cacheKey) => 0;

        public bool SIsMember<T>(string cacheKey, T cacheValue) => false;

        public List<T> SMembers<T>(string cacheKey) => new List<T>();

        public T SPop<T>(string cacheKey) => default;

        public List<T> SRandMember<T>(string cacheKey, int count = 1) => new List<T>();

        public long SRem<T>(string cacheKey, IList<T> cacheValues = null) => 0;
        
        public long ZAdd<T>(string cacheKey, Dictionary<T, double> cacheValues) => 0;

        public long ZCard(string cacheKey) => 0;

        public long ZCount(string cacheKey, double min, double max) => 0;

        public double ZIncrBy(string cacheKey, string field, double val = 1) => default;

        public long ZLexCount(string cacheKey, string min, string max) => 0;

        public List<T> ZRange<T>(string cacheKey, long start, long stop) => new List<T>();

        public long? ZRank<T>(string cacheKey, T cacheValue) => null;

        public long ZRem<T>(string cacheKey, IList<T> cacheValues) => 0;

        public double? ZScore<T>(string cacheKey, T cacheValue) => null;

        public bool PfAdd<T>(string cacheKey, List<T> values) => false;

        public long PfCount(List<string> cacheKeys) => 0;

        public bool PfMerge(string destKey, List<string> sourceKeys) => true;

        public long GeoAdd(string cacheKey, List<(double longitude, double latitude, string member)> values) => 0;

        public double? GeoDist(string cacheKey, string member1, string member2, string unit = "m") => null;

        public List<string> GeoHash(string cacheKey, List<string> members) => new List<string>();

        public List<(decimal longitude, decimal latitude)?> GeoPos(string cacheKey, List<string> members) => new List<(decimal longitude, decimal latitude)?>();

        public object Eval(string script, string cacheKey, List<object> args) => null;

        public Task<bool> KeyDelAsync(string cacheKey) => Task.FromResult(KeyDel(cacheKey));

        public Task<bool> KeyExpireAsync(string cacheKey, int second) => Task.FromResult(KeyExpire(cacheKey, second));

        public Task<bool> KeyExistsAsync(string cacheKey) => Task.FromResult(KeyExists(cacheKey));

        public Task<long> TTLAsync(string cacheKey) => Task.FromResult(TTL(cacheKey));


        public Task<long> IncrByAsync(string cacheKey, long value = 1) => Task.FromResult(IncrBy(cacheKey, value));

        public Task<double> IncrByFloatAsync(string cacheKey, double value = 1) => Task.FromResult(IncrByFloat(cacheKey, value));

        public Task<bool> StringSetAsync(string cacheKey, string cacheValue, TimeSpan? expiration = null, string when = "") => Task.FromResult(StringSet(cacheKey, cacheValue, expiration, when));

        public Task<string> StringGetAsync(string cacheKey) => Task.FromResult(StringGet(cacheKey));

        public Task<long> StringLenAsync(string cacheKey) => Task.FromResult(StringLen(cacheKey));

        public Task<long> StringSetRangeAsync(string cacheKey, long offest, string value) => Task.FromResult(StringSetRange(cacheKey, offest, value));

        public Task<string> StringGetRangeAsync(string cacheKey, long start, long end) => Task.FromResult(StringGetRange(cacheKey, start, end));

        public Task<bool> HMSetAsync(string cacheKey, Dictionary<string, string> vals, TimeSpan? expiration = null) => Task.FromResult(HMSet(cacheKey, vals, expiration));

        public Task<bool> HSetAsync(string cacheKey, string field, string cacheValue) => Task.FromResult(HSet(cacheKey, field, cacheValue));

        public Task<bool> HExistsAsync(string cacheKey, string field) => Task.FromResult(HExists(cacheKey, field));

        public Task<long> HDelAsync(string cacheKey, IList<string> fields = null) => Task.FromResult(HDel(cacheKey, fields));

        public Task<string> HGetAsync(string cacheKey, string field) => Task.FromResult(HGet(cacheKey, field));

        public Task<Dictionary<string, string>> HGetAllAsync(string cacheKey) => Task.FromResult(HGetAll(cacheKey));

        public Task<long> HIncrByAsync(string cacheKey, string field, long val = 1) => Task.FromResult(HIncrBy(cacheKey, field, val));

        public Task<List<string>> HKeysAsync(string cacheKey) => Task.FromResult(HKeys(cacheKey));

        public Task<long> HLenAsync(string cacheKey) => Task.FromResult(HLen(cacheKey));

        public Task<List<string>> HValsAsync(string cacheKey) => Task.FromResult(HVals(cacheKey));

        public Task<Dictionary<string, string>> HMGetAsync(string cacheKey, IList<string> fields) => Task.FromResult(HMGet(cacheKey, fields));

        public Task<T> LIndexAsync<T>(string cacheKey, long index) => Task.FromResult(LIndex<T>(cacheKey, index));

        public Task<long> LLenAsync(string cacheKey) => Task.FromResult(LLen(cacheKey));

        public Task<T> LPopAsync<T>(string cacheKey) => Task.FromResult(LPop<T>(cacheKey));

        public Task<long> LPushAsync<T>(string cacheKey, IList<T> cacheValues) => Task.FromResult(LPush(cacheKey, cacheValues));

        public Task<List<T>> LRangeAsync<T>(string cacheKey, long start, long stop) => Task.FromResult(LRange<T>(cacheKey, start, stop));

        public Task<long> LRemAsync<T>(string cacheKey, long count, T cacheValue) => Task.FromResult(LRem(cacheKey, count, cacheValue));

        public Task<bool> LSetAsync<T>(string cacheKey, long index, T cacheValue) => Task.FromResult(LSet(cacheKey, index, cacheValue));

        public Task<bool> LTrimAsync(string cacheKey, long start, long stop) => Task.FromResult(LTrim(cacheKey, start, stop));

        public Task<long> LPushXAsync<T>(string cacheKey, T cacheValue) => Task.FromResult(LPushX(cacheKey, cacheValue));

        public Task<long> LInsertBeforeAsync<T>(string cacheKey, T pivot, T cacheValue) => Task.FromResult(LInsertBefore(cacheKey, pivot, cacheValue));

        public Task<long> LInsertAfterAsync<T>(string cacheKey, T pivot, T cacheValue) => Task.FromResult(LInsertAfter(cacheKey, pivot, cacheValue));

        public Task<long> RPushXAsync<T>(string cacheKey, T cacheValue) => Task.FromResult(RPushX(cacheKey, cacheValue));

        public Task<long> RPushAsync<T>(string cacheKey, IList<T> cacheValues) => Task.FromResult(RPush(cacheKey, cacheValues));

        public Task<T> RPopAsync<T>(string cacheKey) => Task.FromResult(RPop<T>(cacheKey));

        
        public Task<long> SAddAsync<T>(string cacheKey, IList<T> cacheValues, TimeSpan? expiration = null) => Task.FromResult(SAdd(cacheKey, cacheValues, expiration));

        public Task<long> SCardAsync(string cacheKey) => Task.FromResult(SCard(cacheKey));

        public Task<bool> SIsMemberAsync<T>(string cacheKey, T cacheValue) => Task.FromResult(SIsMember(cacheKey, cacheValue));

        public Task<List<T>> SMembersAsync<T>(string cacheKey) => Task.FromResult(SMembers<T>(cacheKey));

        public Task<T> SPopAsync<T>(string cacheKey) => Task.FromResult(SPop<T>(cacheKey));

        public Task<List<T>> SRandMemberAsync<T>(string cacheKey, int count = 1) => Task.FromResult(SRandMember<T>(cacheKey, count));

        public Task<long> SRemAsync<T>(string cacheKey, IList<T> cacheValues = null) => Task.FromResult(SRem(cacheKey, cacheValues));

        public Task<long> ZAddAsync<T>(string cacheKey, Dictionary<T, double> cacheValues) => Task.FromResult(ZAdd(cacheKey, cacheValues));

        public Task<long> ZCardAsync(string cacheKey) => Task.FromResult(ZCard(cacheKey));

        
        public Task<long> ZCountAsync(string cacheKey, double min, double max) => Task.FromResult(ZCount(cacheKey, min, max));

        public Task<double> ZIncrByAsync(string cacheKey, string field, double val = 1) => Task.FromResult(ZIncrBy(cacheKey, field, val));

        public Task<long> ZLexCountAsync(string cacheKey, string min, string max) => Task.FromResult(ZLexCount(cacheKey, min, max));

        public Task<List<T>> ZRangeAsync<T>(string cacheKey, long start, long stop) => Task.FromResult(ZRange<T>(cacheKey, start, stop));

        public Task<long?> ZRankAsync<T>(string cacheKey, T cacheValue) => Task.FromResult(ZRank(cacheKey, cacheValue));

        public Task<long> ZRemAsync<T>(string cacheKey, IList<T> cacheValues) => Task.FromResult(ZRem(cacheKey, cacheValues));

        public Task<double?> ZScoreAsync<T>(string cacheKey, T cacheValue) => Task.FromResult(ZScore(cacheKey, cacheValue));

        public Task<bool> PfAddAsync<T>(string cacheKey, List<T> values) => Task.FromResult(PfAdd(cacheKey, values));

        public Task<long> PfCountAsync(List<string> cacheKeys) => Task.FromResult(PfCount(cacheKeys));

        public Task<bool> PfMergeAsync(string destKey, List<string> sourceKeys) => Task.FromResult(PfMerge(destKey, sourceKeys));

        public Task<long> GeoAddAsync(string cacheKey, List<(double longitude, double latitude, string member)> values) => Task.FromResult(GeoAdd(cacheKey, values));

        public Task<double?> GeoDistAsync(string cacheKey, string member1, string member2, string unit = "m") => Task.FromResult(GeoDist(cacheKey, member1, member2, unit));

        public Task<List<string>> GeoHashAsync(string cacheKey, List<string> members) => Task.FromResult(GeoHash(cacheKey, members));

        public Task<List<(decimal longitude, decimal latitude)?>> GeoPosAsync(string cacheKey, List<string> members) => Task.FromResult(GeoPos(cacheKey, members));

        public Task<object> EvalAsync(string script, string cacheKey, List<object> args) => Task.FromResult(Eval(script, cacheKey, args));

    }
}