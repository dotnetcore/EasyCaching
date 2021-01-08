namespace EasyCaching.Core.Decoration
{
    using EasyCaching.Core;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public class DecoratedRedisAndEasyCachingProvider 
        : DecoratedEasyCachingProvider<IRedisAndEasyCachingProvider>, IRedisAndEasyCachingProvider
    {
        public DecoratedRedisAndEasyCachingProvider(
            string name,
            IEasyCachingProviderDecorator<IRedisAndEasyCachingProvider> decorator) 
            : base(name, decorator)
        {
        }

        public string RedisName => Name;

        public bool KeyDel(string cacheKey)
        {
            return Decorator.Execute(provider => provider.KeyDel(cacheKey));
        }

        public Task<bool> KeyDelAsync(string cacheKey)
        {
            return Decorator.ExecuteAsync(provider => provider.KeyDelAsync(cacheKey));
        }

        public bool KeyExpire(string cacheKey, int second)
        {
            return Decorator.Execute(provider => provider.KeyExpire(cacheKey, second));
        }

        public Task<bool> KeyExpireAsync(string cacheKey, int second)
        {
            return Decorator.ExecuteAsync(provider => provider.KeyExpireAsync(cacheKey, second));
        }

        public Task<bool> KeyExistsAsync(string cacheKey)
        {
            return Decorator.ExecuteAsync(provider => provider.KeyExistsAsync(cacheKey));
        }

        public bool KeyExists(string cacheKey)
        {
            return Decorator.Execute(provider => provider.KeyExists(cacheKey));
        }

        public long TTL(string cacheKey)
        {
            return Decorator.Execute(provider => provider.TTL(cacheKey));
        }

        public Task<long> TTLAsync(string cacheKey)
        {
            return Decorator.ExecuteAsync(provider => provider.TTLAsync(cacheKey));
        }

        public List<string> SearchKeys(string cacheKey, int? count = null)
        {
            return Decorator.Execute(provider => provider.SearchKeys(cacheKey, count));
        }

        public long IncrBy(string cacheKey, long value = 1)
        {
            return Decorator.Execute(provider => provider.IncrBy(cacheKey, value));
        }

        public Task<long> IncrByAsync(string cacheKey, long value = 1)
        {
            return Decorator.ExecuteAsync(provider => provider.IncrByAsync(cacheKey, value));
        }

        public double IncrByFloat(string cacheKey, double value = 1)
        {
            return Decorator.Execute(provider => provider.IncrByFloat(cacheKey, value));
        }

        public Task<double> IncrByFloatAsync(string cacheKey, double value = 1)
        {
            return Decorator.ExecuteAsync(provider => provider.IncrByFloatAsync(cacheKey, value));
        }

        public bool StringSet(string cacheKey, string cacheValue, TimeSpan? expiration = null, string when = "")
        {
            return Decorator.Execute(provider => provider.StringSet(cacheKey, cacheValue, expiration, when));
        }

        public Task<bool> StringSetAsync(string cacheKey, string cacheValue, TimeSpan? expiration = null, string when = "")
        {
            return Decorator.ExecuteAsync(provider => provider.StringSetAsync(cacheKey, cacheValue, expiration, when));
        }

        public string StringGet(string cacheKey)
        {
            return Decorator.Execute(provider => provider.StringGet(cacheKey));
        }

        public Task<string> StringGetAsync(string cacheKey)
        {
            return Decorator.ExecuteAsync(provider => provider.StringGetAsync(cacheKey));
        }

        public long StringLen(string cacheKey)
        {
            return Decorator.Execute(provider => provider.StringLen(cacheKey));
        }

        public Task<long> StringLenAsync(string cacheKey)
        {
            return Decorator.ExecuteAsync(provider => provider.StringLenAsync(cacheKey));
        }

        public long StringSetRange(string cacheKey, long offest, string value)
        {
            return Decorator.Execute(provider => provider.StringSetRange(cacheKey, offest, value));
        }

        public Task<long> StringSetRangeAsync(string cacheKey, long offest, string value)
        {
            return Decorator.ExecuteAsync(provider => provider.StringSetRangeAsync(cacheKey, offest, value));
        }

        public string StringGetRange(string cacheKey, long start, long end)
        {
            return Decorator.Execute(provider => provider.StringGetRange(cacheKey, start, end));
        }

        public Task<string> StringGetRangeAsync(string cacheKey, long start, long end)
        {
            return Decorator.ExecuteAsync(provider => provider.StringGetRangeAsync(cacheKey, start, end));
        }

        public bool HMSet(string cacheKey, Dictionary<string, string> vals, TimeSpan? expiration = null)
        {
            return Decorator.Execute(provider => provider.HMSet(cacheKey, vals, expiration));
        }

        public bool HSet(string cacheKey, string field, string cacheValue)
        {
            return Decorator.Execute(provider => provider.HSet(cacheKey, field, cacheValue));
        }

        public bool HExists(string cacheKey, string field)
        {
            return Decorator.Execute(provider => provider.HExists(cacheKey, field));
        }

        public long HDel(string cacheKey, IList<string> fields = null)
        {
            return Decorator.Execute(provider => provider.HDel(cacheKey, fields));
        }

        public string HGet(string cacheKey, string field)
        {
            return Decorator.Execute(provider => provider.HGet(cacheKey, field));
        }

        public Dictionary<string, string> HGetAll(string cacheKey)
        {
            return Decorator.Execute(provider => provider.HGetAll(cacheKey));
        }

        public long HIncrBy(string cacheKey, string field, long val = 1)
        {
            return Decorator.Execute(provider => provider.HIncrBy(cacheKey, field, val));
        }

        public List<string> HKeys(string cacheKey)
        {
            return Decorator.Execute(provider => provider.HKeys(cacheKey));
        }

        public long HLen(string cacheKey)
        {
            return Decorator.Execute(provider => provider.HLen(cacheKey));
        }

        public List<string> HVals(string cacheKey)
        {
            return Decorator.Execute(provider => provider.HVals(cacheKey));
        }

        public Dictionary<string, string> HMGet(string cacheKey, IList<string> fields)
        {
            return Decorator.Execute(provider => provider.HMGet(cacheKey, fields));
        }

        public Task<bool> HMSetAsync(string cacheKey, Dictionary<string, string> vals, TimeSpan? expiration = null)
        {
            return Decorator.ExecuteAsync(provider => provider.HMSetAsync(cacheKey, vals, expiration));
        }

        public Task<bool> HSetAsync(string cacheKey, string field, string cacheValue)
        {
            return Decorator.ExecuteAsync(provider => provider.HSetAsync(cacheKey, field, cacheValue));
        }

        public Task<bool> HExistsAsync(string cacheKey, string field)
        {
            return Decorator.ExecuteAsync(provider => provider.HExistsAsync(cacheKey, field));
        }

        public Task<long> HDelAsync(string cacheKey, IList<string> fields = null)
        {
            return Decorator.ExecuteAsync(provider => provider.HDelAsync(cacheKey, fields));
        }

        public Task<string> HGetAsync(string cacheKey, string field)
        {
            return Decorator.ExecuteAsync(provider => provider.HGetAsync(cacheKey, field));
        }

        public Task<Dictionary<string, string>> HGetAllAsync(string cacheKey)
        {
            return Decorator.ExecuteAsync(provider => provider.HGetAllAsync(cacheKey));
        }

        public Task<long> HIncrByAsync(string cacheKey, string field, long val = 1)
        {
            return Decorator.ExecuteAsync(provider => provider.HIncrByAsync(cacheKey, field, val));
        }

        public Task<List<string>> HKeysAsync(string cacheKey)
        {
            return Decorator.ExecuteAsync(provider => provider.HKeysAsync(cacheKey));
        }

        public Task<long> HLenAsync(string cacheKey)
        {
            return Decorator.ExecuteAsync(provider => provider.HLenAsync(cacheKey));
        }

        public Task<List<string>> HValsAsync(string cacheKey)
        {
            return Decorator.ExecuteAsync(provider => provider.HValsAsync(cacheKey));
        }

        public Task<Dictionary<string, string>> HMGetAsync(string cacheKey, IList<string> fields)
        {
            return Decorator.ExecuteAsync(provider => provider.HMGetAsync(cacheKey, fields));
        }

        public T LIndex<T>(string cacheKey, long index)
        {
            return Decorator.Execute(provider => provider.LIndex<T>(cacheKey, index));
        }

        public long LLen(string cacheKey)
        {
            return Decorator.Execute(provider => provider.LLen(cacheKey));
        }

        public T LPop<T>(string cacheKey)
        {
            return Decorator.Execute(provider => provider.LPop<T>(cacheKey));
        }

        public long LPush<T>(string cacheKey, IList<T> cacheValues)
        {
            return Decorator.Execute(provider => provider.LPush(cacheKey, cacheValues));
        }

        public List<T> LRange<T>(string cacheKey, long start, long stop)
        {
            return Decorator.Execute(provider => provider.LRange<T>(cacheKey, start, stop));
        }

        public long LRem<T>(string cacheKey, long count, T cacheValue)
        {
            return Decorator.Execute(provider => provider.LRem(cacheKey, count, cacheValue));
        }

        public bool LSet<T>(string cacheKey, long index, T cacheValue)
        {
            return Decorator.Execute(provider => provider.LSet(cacheKey, index, cacheValue));
        }

        public bool LTrim(string cacheKey, long start, long stop)
        {
            return Decorator.Execute(provider => provider.LTrim(cacheKey, start, stop));
        }

        public long LPushX<T>(string cacheKey, T cacheValue)
        {
            return Decorator.Execute(provider => provider.LPushX(cacheKey, cacheValue));
        }

        public long LInsertBefore<T>(string cacheKey, T pivot, T cacheValue)
        {
            return Decorator.Execute(provider => provider.LInsertBefore(cacheKey, pivot, cacheValue));
        }

        public long LInsertAfter<T>(string cacheKey, T pivot, T cacheValue)
        {
            return Decorator.Execute(provider => provider.LInsertAfter(cacheKey, pivot, cacheValue));
        }

        public long RPushX<T>(string cacheKey, T cacheValue)
        {
            return Decorator.Execute(provider => provider.RPushX(cacheKey, cacheValue));
        }

        public long RPush<T>(string cacheKey, IList<T> cacheValues)
        {
            return Decorator.Execute(provider => provider.RPush(cacheKey, cacheValues));
        }

        public T RPop<T>(string cacheKey)
        {
            return Decorator.Execute(provider => provider.RPop<T>(cacheKey));
        }

        public Task<T> LIndexAsync<T>(string cacheKey, long index)
        {
            return Decorator.ExecuteAsync(provider => provider.LIndexAsync<T>(cacheKey, index));
        }

        public Task<long> LLenAsync(string cacheKey)
        {
            return Decorator.ExecuteAsync(provider => provider.LLenAsync(cacheKey));
        }

        public Task<T> LPopAsync<T>(string cacheKey)
        {
            return Decorator.ExecuteAsync(provider => provider.LPopAsync<T>(cacheKey));
        }

        public Task<long> LPushAsync<T>(string cacheKey, IList<T> cacheValues)
        {
            return Decorator.ExecuteAsync(provider => provider.LPushAsync(cacheKey, cacheValues));
        }

        public Task<List<T>> LRangeAsync<T>(string cacheKey, long start, long stop)
        {
            return Decorator.ExecuteAsync(provider => provider.LRangeAsync<T>(cacheKey, start, stop));
        }

        public Task<long> LRemAsync<T>(string cacheKey, long count, T cacheValue)
        {
            return Decorator.ExecuteAsync(provider => provider.LRemAsync(cacheKey, count, cacheValue));
        }

        public Task<bool> LSetAsync<T>(string cacheKey, long index, T cacheValue)
        {
            return Decorator.ExecuteAsync(provider => provider.LSetAsync(cacheKey, index, cacheValue));
        }

        public Task<bool> LTrimAsync(string cacheKey, long start, long stop)
        {
            return Decorator.ExecuteAsync(provider => provider.LTrimAsync(cacheKey, start, stop));
        }

        public Task<long> LPushXAsync<T>(string cacheKey, T cacheValue)
        {
            return Decorator.ExecuteAsync(provider => provider.LPushXAsync(cacheKey, cacheValue));
        }

        public Task<long> LInsertBeforeAsync<T>(string cacheKey, T pivot, T cacheValue)
        {
            return Decorator.ExecuteAsync(provider => provider.LInsertBeforeAsync(cacheKey, pivot, cacheValue));
        }

        public Task<long> LInsertAfterAsync<T>(string cacheKey, T pivot, T cacheValue)
        {
            return Decorator.ExecuteAsync(provider => provider.LInsertAfterAsync(cacheKey, pivot, cacheValue));
        }

        public Task<long> RPushXAsync<T>(string cacheKey, T cacheValue)
        {
            return Decorator.ExecuteAsync(provider => provider.RPushXAsync(cacheKey, cacheValue));
        }

        public Task<long> RPushAsync<T>(string cacheKey, IList<T> cacheValues)
        {
            return Decorator.ExecuteAsync(provider => provider.RPushAsync(cacheKey, cacheValues));
        }

        public Task<T> RPopAsync<T>(string cacheKey)
        {
            return Decorator.ExecuteAsync(provider => provider.RPopAsync<T>(cacheKey));
        }

        public long SAdd<T>(string cacheKey, IList<T> cacheValues, TimeSpan? expiration = null)
        {
            return Decorator.Execute(provider => provider.SAdd(cacheKey, cacheValues, expiration));
        }

        public long SCard(string cacheKey)
        {
            return Decorator.Execute(provider => provider.SCard(cacheKey));
        }

        public bool SIsMember<T>(string cacheKey, T cacheValue)
        {
            return Decorator.Execute(provider => provider.SIsMember(cacheKey, cacheValue));
        }

        public List<T> SMembers<T>(string cacheKey)
        {
            return Decorator.Execute(provider => provider.SMembers<T>(cacheKey));
        }

        public T SPop<T>(string cacheKey)
        {
            return Decorator.Execute(provider => provider.SPop<T>(cacheKey));
        }

        public List<T> SRandMember<T>(string cacheKey, int count = 1)
        {
            return Decorator.Execute(provider => provider.SRandMember<T>(cacheKey, count));
        }

        public long SRem<T>(string cacheKey, IList<T> cacheValues = null)
        {
            return Decorator.Execute(provider => provider.SRem(cacheKey, cacheValues));
        }

        public Task<long> SAddAsync<T>(string cacheKey, IList<T> cacheValues, TimeSpan? expiration = null)
        {
            return Decorator.ExecuteAsync(provider => provider.SAddAsync(cacheKey, cacheValues, expiration));
        }

        public Task<long> SCardAsync(string cacheKey)
        {
            return Decorator.ExecuteAsync(provider => provider.SCardAsync(cacheKey));
        }

        public Task<bool> SIsMemberAsync<T>(string cacheKey, T cacheValue)
        {
            return Decorator.ExecuteAsync(provider => provider.SIsMemberAsync(cacheKey, cacheValue));
        }

        public Task<List<T>> SMembersAsync<T>(string cacheKey)
        {
            return Decorator.ExecuteAsync(provider => provider.SMembersAsync<T>(cacheKey));
        }

        public Task<T> SPopAsync<T>(string cacheKey)
        {
            return Decorator.ExecuteAsync(provider => provider.SPopAsync<T>(cacheKey));
        }

        public Task<List<T>> SRandMemberAsync<T>(string cacheKey, int count = 1)
        {
            return Decorator.ExecuteAsync(provider => provider.SRandMemberAsync<T>(cacheKey, count));
        }

        public Task<long> SRemAsync<T>(string cacheKey, IList<T> cacheValues = null)
        {
            return Decorator.ExecuteAsync(provider => provider.SRemAsync(cacheKey, cacheValues));
        }

        public long ZAdd<T>(string cacheKey, Dictionary<T, double> cacheValues)
        {
            return Decorator.Execute(provider => provider.ZAdd(cacheKey, cacheValues));
        }

        public long ZCard(string cacheKey)
        {
            return Decorator.Execute(provider => provider.ZCard(cacheKey));
        }

        public long ZCount(string cacheKey, double min, double max)
        {
            return Decorator.Execute(provider => provider.ZCount(cacheKey, min, max));
        }

        public double ZIncrBy(string cacheKey, string field, double val = 1)
        {
            return Decorator.Execute(provider => provider.ZIncrBy(cacheKey, field, val));
        }

        public long ZLexCount(string cacheKey, string min, string max)
        {
            return Decorator.Execute(provider => provider.ZLexCount(cacheKey, min, max));
        }

        public List<T> ZRange<T>(string cacheKey, long start, long stop)
        {
            return Decorator.Execute(provider => provider.ZRange<T>(cacheKey, start, stop));
        }

        public long? ZRank<T>(string cacheKey, T cacheValue)
        {
            return Decorator.Execute(provider => provider.ZRank(cacheKey, cacheValue));
        }

        public long ZRem<T>(string cacheKey, IList<T> cacheValues)
        {
            return Decorator.Execute(provider => provider.ZRem(cacheKey, cacheValues));
        }

        public double? ZScore<T>(string cacheKey, T cacheValue)
        {
            return Decorator.Execute(provider => provider.ZScore(cacheKey, cacheValue));
        }

        public Task<long> ZAddAsync<T>(string cacheKey, Dictionary<T, double> cacheValues)
        {
            return Decorator.ExecuteAsync(provider => provider.ZAddAsync(cacheKey, cacheValues));
        }

        public Task<long> ZCardAsync(string cacheKey)
        {
            return Decorator.ExecuteAsync(provider => provider.ZCardAsync(cacheKey));
        }

        public Task<long> ZCountAsync(string cacheKey, double min, double max)
        {
            return Decorator.ExecuteAsync(provider => provider.ZCountAsync(cacheKey, min, max));
        }

        public Task<double> ZIncrByAsync(string cacheKey, string field, double val = 1)
        {
            return Decorator.ExecuteAsync(provider => provider.ZIncrByAsync(cacheKey, field, val));
        }

        public Task<long> ZLexCountAsync(string cacheKey, string min, string max)
        {
            return Decorator.ExecuteAsync(provider => provider.ZLexCountAsync(cacheKey, min, max));
        }

        public Task<List<T>> ZRangeAsync<T>(string cacheKey, long start, long stop)
        {
            return Decorator.ExecuteAsync(provider => provider.ZRangeAsync<T>(cacheKey, start, stop));
        }

        public Task<long?> ZRankAsync<T>(string cacheKey, T cacheValue)
        {
            return Decorator.ExecuteAsync(provider => provider.ZRankAsync(cacheKey, cacheValue));
        }

        public Task<long> ZRemAsync<T>(string cacheKey, IList<T> cacheValues)
        {
            return Decorator.ExecuteAsync(provider => provider.ZRemAsync(cacheKey, cacheValues));
        }

        public Task<double?> ZScoreAsync<T>(string cacheKey, T cacheValue)
        {
            return Decorator.ExecuteAsync(provider => provider.ZScoreAsync(cacheKey, cacheValue));
        }

        public bool PfAdd<T>(string cacheKey, List<T> values)
        {
            return Decorator.Execute(provider => provider.PfAdd(cacheKey, values));
        }

        public Task<bool> PfAddAsync<T>(string cacheKey, List<T> values)
        {
            return Decorator.ExecuteAsync(provider => provider.PfAddAsync(cacheKey, values));
        }

        public long PfCount(List<string> cacheKeys)
        {
            return Decorator.Execute(provider => provider.PfCount(cacheKeys));
        }

        public Task<long> PfCountAsync(List<string> cacheKeys)
        {
            return Decorator.ExecuteAsync(provider => provider.PfCountAsync(cacheKeys));
        }

        public bool PfMerge(string destKey, List<string> sourceKeys)
        {
            return Decorator.Execute(provider => provider.PfMerge(destKey, sourceKeys));
        }

        public Task<bool> PfMergeAsync(string destKey, List<string> sourceKeys)
        {
            return Decorator.ExecuteAsync(provider => provider.PfMergeAsync(destKey, sourceKeys));
        }

        public long GeoAdd(string cacheKey, List<(double longitude, double latitude, string member)> values)
        {
            return Decorator.Execute(provider => provider.GeoAdd(cacheKey, values));
        }

        public Task<long> GeoAddAsync(string cacheKey, List<(double longitude, double latitude, string member)> values)
        {
            return Decorator.ExecuteAsync(provider => provider.GeoAddAsync(cacheKey, values));
        }

        public double? GeoDist(string cacheKey, string member1, string member2, string unit = "m")
        {
            return Decorator.Execute(provider => provider.GeoDist(cacheKey, member1, member2, unit));
        }

        public Task<double?> GeoDistAsync(string cacheKey, string member1, string member2, string unit = "m")
        {
            return Decorator.ExecuteAsync(provider => provider.GeoDistAsync(cacheKey, member1, member2, unit));
        }

        public List<string> GeoHash(string cacheKey, List<string> members)
        {
            return Decorator.Execute(provider => provider.GeoHash(cacheKey, members));
        }

        public Task<List<string>> GeoHashAsync(string cacheKey, List<string> members)
        {
            return Decorator.ExecuteAsync(provider => provider.GeoHashAsync(cacheKey, members));
        }

        public List<(decimal longitude, decimal latitude)?> GeoPos(string cacheKey, List<string> members)
        {
            return Decorator.Execute(provider => provider.GeoPos(cacheKey, members));
        }

        public Task<List<(decimal longitude, decimal latitude)?>> GeoPosAsync(string cacheKey, List<string> members)
        {
            return Decorator.ExecuteAsync(provider => provider.GeoPosAsync(cacheKey, members));
        }

        public object Eval(string script, string cacheKey, List<object> args)
        {
            return Decorator.Execute(provider => provider.Eval(script, cacheKey, args));
        }

        public Task<object> EvalAsync(string script, string cacheKey, List<object> args)
        {
            return Decorator.ExecuteAsync(provider => provider.EvalAsync(script, cacheKey, args));
        }
    }
}