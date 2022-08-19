﻿namespace EasyCaching.Core
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    /// <summary>
    /// Redis caching provider.
    /// </summary>
    /// <remarks>
    /// Contains some features of redis
    /// </remarks>
    public interface IRedisCachingProvider
    {
        string RedisName { get; }

        #region Keys
        /// <summary>
        /// https://redis.io/commands/del
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <returns></returns>
        bool KeyDel(string cacheKey);
        /// <summary>
        /// https://redis.io/commands/del
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <returns></returns>
        Task<bool> KeyDelAsync(string cacheKey);
        /// <summary>
        /// https://redis.io/commands/expire
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <param name="second"></param>
        /// <returns></returns>
        bool KeyExpire(string cacheKey, int second);
        /// <summary>
        /// https://redis.io/commands/expire
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <param name="second"></param>
        /// <returns></returns>
        Task<bool> KeyExpireAsync(string cacheKey, int second);
        /// <summary>
        /// https://redis.io/commands/persist
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <returns></returns>
        bool KeyPersist(string cacheKey);
        /// <summary>
        /// https://redis.io/commands/persist
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <returns></returns>
        Task<bool> KeyPersistAsync(string cacheKey);
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// <param name="cacheKey"></param>
        Task<bool> KeyExistsAsync(string cacheKey);
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// <param name="cacheKey"></param>
        bool KeyExists(string cacheKey);
        /// <summary>
        /// https://redis.io/commands/ttl
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <returns></returns>
        long TTL(string cacheKey);
        /// <summary>
        /// https://redis.io/commands/ttl
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <returns></returns>
        Task<long> TTLAsync(string cacheKey);

        /// <summary>
        /// Searchs the keys.
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        List<string> SearchKeys(string cacheKey, int? count = null);

        /// <summary>
        /// Searchs the keys.
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        Task<List<string>> SearchKeysAsync(string cacheKey, int? count = null);
        #endregion

        #region String
        /// <summary>
        /// https://redis.io/commands/incrby
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        long IncrBy(string cacheKey, long value = 1);
        /// <summary>
        /// https://redis.io/commands/incrby
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        Task<long> IncrByAsync(string cacheKey, long value = 1);
        /// <summary>
        /// https://redis.io/commands/incrbyfloat
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        double IncrByFloat(string cacheKey, double value = 1);
        /// <summary>
        /// https://redis.io/commands/incrbyfloat
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        Task<double> IncrByFloatAsync(string cacheKey, double value = 1);
        /// <summary>
        /// https://redis.io/commands/set
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <param name="cacheValue"></param>
        /// <param name="expiration"></param>
        /// <param name="when">nx,xx,</param>
        /// <returns></returns>
        bool StringSet(string cacheKey, string cacheValue, System.TimeSpan? expiration = null, string when = "");
        /// <summary>
        /// https://redis.io/commands/set
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <param name="cacheValue"></param>
        /// <param name="expiration"></param>
        /// <param name="when">nx,xx,</param>
        /// <returns></returns>
        Task<bool> StringSetAsync(string cacheKey, string cacheValue, System.TimeSpan? expiration = null, string when = "");
        /// <summary>
        /// https://redis.io/commands/get
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <returns></returns>
        string StringGet(string cacheKey);
        /// <summary>
        /// https://redis.io/commands/get
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <returns></returns>
        Task<string> StringGetAsync(string cacheKey);
        /// <summary>
        /// https://redis.io/commands/strlen
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <returns></returns>
        long StringLen(string cacheKey);
        /// <summary>
        /// https://redis.io/commands/strlen
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <returns></returns>
        Task<long> StringLenAsync(string cacheKey);
        /// <summary>
        /// https://redis.io/commands/setrange
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <param name="offest"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        long StringSetRange(string cacheKey, long offest, string value);
        /// <summary>
        /// https://redis.io/commands/setrange
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <param name="offest"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        Task<long> StringSetRangeAsync(string cacheKey, long offest, string value);
        /// <summary>
        /// https://redis.io/commands/getrange
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        string StringGetRange(string cacheKey, long start, long end);
        /// <summary>
        /// https://redis.io/commands/getrange
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        Task<string> StringGetRangeAsync(string cacheKey, long start, long end);
        #endregion

        #region Hashes
        /// <summary>
        /// https://redis.io/commands/hmset
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <param name="vals"></param>
        /// <param name="expiration"></param>
        /// <returns></returns>        
        bool HMSet(string cacheKey, Dictionary<string, string> vals, TimeSpan? expiration = null);
        /// <summary>
        /// https://redis.io/commands/hset
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <param name="field"></param>
        /// <param name="cacheValue"></param>
        /// <returns></returns>
        bool HSet(string cacheKey, string field, string cacheValue);
        /// <summary>
        /// https://redis.io/commands/hexists
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <param name="field"></param>
        /// <returns></returns>
        bool HExists(string cacheKey, string field);
        /// <summary>
        /// https://redis.io/commands/hdel
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <param name="fields"></param>
        /// <returns></returns>
        long HDel(string cacheKey, IList<string> fields = null);
        /// <summary>
        /// https://redis.io/commands/hget
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <param name="field"></param>
        /// <returns></returns>
        string HGet(string cacheKey, string field);
        /// <summary>
        /// https://redis.io/commands/hgetall
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <returns></returns>
        Dictionary<string, string> HGetAll(string cacheKey);
        /// <summary>
        /// https://redis.io/commands/hincrby
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <param name="field"></param>
        /// <param name="val"></param>
        /// <returns></returns>
        long HIncrBy(string cacheKey, string field, long val = 1);
        /// <summary>
        /// https://redis.io/commands/hkeys
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <returns></returns>
        List<string> HKeys(string cacheKey);
        /// <summary>
        /// https://redis.io/commands/hlen
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <returns></returns>
        long HLen(string cacheKey);
        /// <summary>
        /// https://redis.io/commands/hvals
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <returns></returns>
        List<string> HVals(string cacheKey);
        /// <summary>
        /// https://redis.io/commands/hmget
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <param name="fields"></param>
        /// <returns></returns>
        Dictionary<string, string> HMGet(string cacheKey, IList<string> fields);
        /// <summary>
        /// https://redis.io/commands/hset
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <param name="vals"></param>
        /// <param name="expiration"></param>
        /// <returns></returns>
        Task<bool> HMSetAsync(string cacheKey, Dictionary<string, string> vals, TimeSpan? expiration = null);
        /// <summary>
        /// https://redis.io/commands/hset
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <param name="field"></param>
        /// <param name="cacheValue"></param>
        /// <returns></returns>
        Task<bool> HSetAsync(string cacheKey, string field, string cacheValue);
        /// <summary>
        /// https://redis.io/commands/hexists
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <param name="field"></param>
        /// <returns></returns>
        Task<bool> HExistsAsync(string cacheKey, string field);
        /// <summary>
        /// https://redis.io/commands/hdel
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <param name="fields"></param>
        /// <returns></returns>
        Task<long> HDelAsync(string cacheKey, IList<string> fields = null);
        /// <summary>
        /// https://redis.io/commands/hget
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <param name="field"></param>
        /// <returns></returns>
        Task<string> HGetAsync(string cacheKey, string field);
        /// <summary>
        /// https://redis.io/commands/hgetall
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <returns></returns>
        Task<Dictionary<string, string>> HGetAllAsync(string cacheKey);
        /// <summary>
        /// https://redis.io/commands/hincrby
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <param name="field"></param>
        /// <param name="val"></param>
        /// <returns></returns>
        Task<long> HIncrByAsync(string cacheKey, string field, long val = 1);
        /// <summary>
        /// https://redis.io/commands/hkeys
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <returns></returns>
        Task<List<string>> HKeysAsync(string cacheKey);
        /// <summary>
        /// https://redis.io/commands/hlen
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <returns></returns>
        Task<long> HLenAsync(string cacheKey);
        /// <summary>
        /// https://redis.io/commands/hvals
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <returns></returns>
        Task<List<string>> HValsAsync(string cacheKey);
        /// <summary>
        /// https://redis.io/commands/hmget
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <param name="fields"></param>
        /// <returns></returns>
        Task<Dictionary<string, string>> HMGetAsync(string cacheKey, IList<string> fields);
        #endregion

        #region List
        /// <summary>
        /// https://redis.io/commands/lindex
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cacheKey"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        T LIndex<T>(string cacheKey, long index);
        /// <summary>
        /// https://redis.io/commands/llen
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <returns></returns>
        long LLen(string cacheKey);
        /// <summary>
        /// https://redis.io/commands/lpop
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cacheKey"></param>
        /// <returns></returns>
        T LPop<T>(string cacheKey);
        /// <summary>
        /// https://redis.io/commands/lpush
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cacheKey"></param>
        /// <param name="cacheValues"></param>
        /// <returns></returns>
        long LPush<T>(string cacheKey, IList<T> cacheValues);
        /// <summary>
        /// https://redis.io/commands/lrange
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cacheKey"></param>
        /// <param name="start"></param>
        /// <param name="stop"></param>
        /// <returns></returns>
        List<T> LRange<T>(string cacheKey, long start, long stop);
        /// <summary>
        /// https://redis.io/commands/lrem
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cacheKey"></param>
        /// <param name="count"></param>
        /// <param name="cacheValue"></param>
        /// <returns></returns>
        long LRem<T>(string cacheKey, long count, T cacheValue);
        /// <summary>
        /// https://redis.io/commands/lset
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cacheKey"></param>
        /// <param name="index"></param>
        /// <param name="cacheValue"></param>
        /// <returns></returns>
        bool LSet<T>(string cacheKey, long index, T cacheValue);
        /// <summary>
        /// https://redis.io/commands/ltrim
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <param name="start"></param>
        /// <param name="stop"></param>
        /// <returns></returns>
        bool LTrim(string cacheKey, long start, long stop);
        /// <summary>
        /// https://redis.io/commands/lpushx
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cacheKey"></param>
        /// <param name="cacheValue"></param>
        /// <returns></returns>
        long LPushX<T>(string cacheKey, T cacheValue);
        /// <summary>
        /// https://redis.io/commands/linsert
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cacheKey"></param>
        /// <param name="pivot"></param>
        /// <param name="cacheValue"></param>
        /// <returns></returns>
        long LInsertBefore<T>(string cacheKey, T pivot, T cacheValue);
        /// <summary>
        /// https://redis.io/commands/linsert
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cacheKey"></param>
        /// <param name="pivot"></param>
        /// <param name="cacheValue"></param>
        /// <returns></returns>
        long LInsertAfter<T>(string cacheKey, T pivot, T cacheValue);
        /// <summary>
        /// https://redis.io/commands/rpushx
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cacheKey"></param>
        /// <param name="cacheValue"></param>
        /// <returns></returns>
        long RPushX<T>(string cacheKey, T cacheValue);
        /// <summary>
        /// https://redis.io/commands/rpush
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cacheKey"></param>
        /// <param name="cacheValues"></param>
        /// <returns></returns>
        long RPush<T>(string cacheKey, IList<T> cacheValues);
        /// <summary>
        /// https://redis.io/commands/rpop
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cacheKey"></param>
        /// <returns></returns>
        T RPop<T>(string cacheKey);
        /// <summary>
        /// https://redis.io/commands/lindex
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cacheKey"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        Task<T> LIndexAsync<T>(string cacheKey, long index);
        /// <summary>
        /// https://redis.io/commands/llen
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <returns></returns>
        Task<long> LLenAsync(string cacheKey);
        /// <summary>
        /// https://redis.io/commands/lpop
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cacheKey"></param>
        /// <returns></returns>
        Task<T> LPopAsync<T>(string cacheKey);
        /// <summary>
        /// https://redis.io/commands/lpush
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cacheKey"></param>
        /// <param name="cacheValues"></param>
        /// <returns></returns>
        Task<long> LPushAsync<T>(string cacheKey, IList<T> cacheValues);
        /// <summary>
        /// https://redis.io/commands/lrange
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cacheKey"></param>
        /// <param name="start"></param>
        /// <param name="stop"></param>
        /// <returns></returns>
        Task<List<T>> LRangeAsync<T>(string cacheKey, long start, long stop);
        /// <summary>
        /// https://redis.io/commands/lrem
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cacheKey"></param>
        /// <param name="count"></param>
        /// <param name="cacheValue"></param>
        /// <returns></returns>
        Task<long> LRemAsync<T>(string cacheKey, long count, T cacheValue);
        /// <summary>
        /// https://redis.io/commands/lset
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cacheKey"></param>
        /// <param name="index"></param>
        /// <param name="cacheValue"></param>
        /// <returns></returns>
        Task<bool> LSetAsync<T>(string cacheKey, long index, T cacheValue);
        /// <summary>
        /// https://redis.io/commands/ltrim
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <param name="start"></param>
        /// <param name="stop"></param>
        /// <returns></returns>
        Task<bool> LTrimAsync(string cacheKey, long start, long stop);
        /// <summary>
        /// https://redis.io/commands/lpushx
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cacheKey"></param>
        /// <param name="cacheValue"></param>
        /// <returns></returns>
        Task<long> LPushXAsync<T>(string cacheKey, T cacheValue);
        /// <summary>
        /// https://redis.io/commands/linsert
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cacheKey"></param>
        /// <param name="pivot"></param>
        /// <param name="cacheValue"></param>
        /// <returns></returns>
        Task<long> LInsertBeforeAsync<T>(string cacheKey, T pivot, T cacheValue);
        /// <summary>
        /// https://redis.io/commands/linsert
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cacheKey"></param>
        /// <param name="pivot"></param>
        /// <param name="cacheValue"></param>
        /// <returns></returns>
        Task<long> LInsertAfterAsync<T>(string cacheKey, T pivot, T cacheValue);
        /// <summary>
        /// https://redis.io/commands/rpushx
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cacheKey"></param>
        /// <param name="cacheValue"></param>
        /// <returns></returns>
        Task<long> RPushXAsync<T>(string cacheKey, T cacheValue);
        /// <summary>
        /// https://redis.io/commands/rpush
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cacheKey"></param>
        /// <param name="cacheValues"></param>
        /// <returns></returns>
        Task<long> RPushAsync<T>(string cacheKey, IList<T> cacheValues);
        /// <summary>
        /// https://redis.io/commands/rpop
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cacheKey"></param>
        /// <returns></returns>
        Task<T> RPopAsync<T>(string cacheKey);
        #endregion

        #region Set
        /// <summary>
        /// https://redis.io/commands/sadd
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cacheKey"></param>
        /// <param name="cacheValues"></param>
        /// <param name="expiration"></param>
        /// <returns></returns>
        long SAdd<T>(string cacheKey, IList<T> cacheValues, TimeSpan? expiration = null);
        /// <summary>
        /// https://redis.io/commands/scard
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <returns></returns>
        long SCard(string cacheKey);
        /// <summary>
        /// https://redis.io/commands/sismember
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cacheKey"></param>
        /// <param name="cacheValue"></param>
        /// <returns></returns>
        bool SIsMember<T>(string cacheKey, T cacheValue);
        /// <summary>
        /// https://redis.io/commands/smembers
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cacheKey"></param>
        /// <returns></returns>
        List<T> SMembers<T>(string cacheKey);
        /// <summary>
        /// https://redis.io/commands/spop
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cacheKey"></param>
        /// <returns></returns>
        T SPop<T>(string cacheKey);
        /// <summary>
        /// https://redis.io/commands/srandmember
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cacheKey"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        List<T> SRandMember<T>(string cacheKey, int count = 1);
        /// <summary>
        /// https://redis.io/commands/srem
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cacheKey"></param>
        /// <param name="cacheValues"></param>
        /// <returns></returns>
        long SRem<T>(string cacheKey, IList<T> cacheValues = null);
        /// <summary>
        /// https://redis.io/commands/sadd
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cacheKey"></param>
        /// <param name="cacheValues"></param>
        /// <param name="expiration"></param>
        /// <returns></returns>
        Task<long> SAddAsync<T>(string cacheKey, IList<T> cacheValues, TimeSpan? expiration = null);
        /// <summary>
        /// https://redis.io/commands/scard
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <returns></returns>
        Task<long> SCardAsync(string cacheKey);
        /// <summary>
        /// https://redis.io/commands/sismember
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cacheKey"></param>
        /// <param name="cacheValue"></param>
        /// <returns></returns>
        Task<bool> SIsMemberAsync<T>(string cacheKey, T cacheValue);
        /// <summary>
        /// https://redis.io/commands/smembers
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cacheKey"></param>
        /// <returns></returns>
        Task<List<T>> SMembersAsync<T>(string cacheKey);
        /// <summary>
        /// https://redis.io/commands/spop
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cacheKey"></param>
        /// <returns></returns>
        Task<T> SPopAsync<T>(string cacheKey);
        /// <summary>
        /// https://redis.io/commands/srandmember
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cacheKey"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        Task<List<T>> SRandMemberAsync<T>(string cacheKey, int count = 1);
        /// <summary>
        /// https://redis.io/commands/srem
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cacheKey"></param>
        /// <param name="cacheValues"></param>
        /// <returns></returns>
        Task<long> SRemAsync<T>(string cacheKey, IList<T> cacheValues = null);
        #endregion

        #region Sorted Set
        /// <summary>
        /// https://redis.io/commands/zadd
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cacheKey"></param>
        /// <param name="cacheValues"></param>
        /// <returns></returns>
        long ZAdd<T>(string cacheKey, Dictionary<T, double> cacheValues);
        /// <summary>
        /// https://redis.io/commands/zcard
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <returns></returns>
        long ZCard(string cacheKey);
        /// <summary>
        /// https://redis.io/commands/zcount
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        long ZCount(string cacheKey, double min, double max);
        /// <summary>
        /// https://redis.io/commands/zincrby
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <param name="field"></param>
        /// <param name="val"></param>
        /// <returns></returns>
        double ZIncrBy(string cacheKey, string field, double val = 1);
        /// <summary>
        /// https://redis.io/commands/zlexcount
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        long ZLexCount(string cacheKey, string min, string max);
        /// <summary>
        /// https://redis.io/commands/zrange
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cacheKey"></param>
        /// <param name="start"></param>
        /// <param name="stop"></param>
        /// <returns></returns>
        List<T> ZRange<T>(string cacheKey, long start, long stop);
        /// <summary>
        /// https://redis.io/commands/zrangebyscore/        
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cacheKey"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <param name="count"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        List<T> ZRangeByScore<T>(string cacheKey, double min, double max, long? count = null, long offset = 0);
        /// <summary>
        /// https://redis.io/commands/zrank
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cacheKey"></param>
        /// <param name="cacheValue"></param>
        /// <returns></returns>
        long? ZRank<T>(string cacheKey, T cacheValue);
        /// <summary>
        /// https://redis.io/commands/zrem
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cacheKey"></param>
        /// <param name="cacheValues"></param>
        /// <returns></returns>
        long ZRem<T>(string cacheKey, IList<T> cacheValues);
        /// <summary>
        /// https://redis.io/commands/zscore
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cacheKey"></param>
        /// <param name="cacheValue"></param>
        /// <returns></returns>
        double? ZScore<T>(string cacheKey, T cacheValue);
        /// <summary>
        /// https://redis.io/commands/zadd
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cacheKey"></param>
        /// <param name="cacheValues"></param>
        /// <returns></returns>
        Task<long> ZAddAsync<T>(string cacheKey, Dictionary<T, double> cacheValues);
        /// <summary>
        /// https://redis.io/commands/zcard
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <returns></returns>
        Task<long> ZCardAsync(string cacheKey);
        /// <summary>
        /// https://redis.io/commands/zcount
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        Task<long> ZCountAsync(string cacheKey, double min, double max);
        /// <summary>
        /// https://redis.io/commands/zincrby
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <param name="field"></param>
        /// <param name="val"></param>
        /// <returns></returns>
        Task<double> ZIncrByAsync(string cacheKey, string field, double val = 1);
        /// <summary>
        /// https://redis.io/commands/zlexcount
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        Task<long> ZLexCountAsync(string cacheKey, string min, string max);
        /// <summary>
        /// https://redis.io/commands/zrange
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cacheKey"></param>
        /// <param name="start"></param>
        /// <param name="stop"></param>
        /// <returns></returns>
        Task<List<T>> ZRangeAsync<T>(string cacheKey, long start, long stop);
        /// <summary>
        /// https://redis.io/commands/zrangebyscore/        
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cacheKey"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <param name="count"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        Task<List<T>> ZRangeByScoreAsync<T>(string cacheKey, double min, double max, long? count = null, long offset = 0);
        /// <summary>
        /// https://redis.io/commands/zrank
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cacheKey"></param>
        /// <param name="cacheValue"></param>
        /// <returns></returns>
        Task<long?> ZRankAsync<T>(string cacheKey, T cacheValue);
        /// <summary>
        /// https://redis.io/commands/zrem
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cacheKey"></param>
        /// <param name="cacheValues"></param>
        /// <returns></returns>
        Task<long> ZRemAsync<T>(string cacheKey, IList<T> cacheValues);
        /// <summary>
        /// https://redis.io/commands/zscore
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cacheKey"></param>
        /// <param name="cacheValue"></param>
        /// <returns></returns>
        Task<double?> ZScoreAsync<T>(string cacheKey, T cacheValue);
        #endregion

        #region Hyperloglog
        /// <summary>
        /// https://redis.io/commands/pfadd
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cacheKey"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        bool PfAdd<T>(string cacheKey, List<T> values);
        /// <summary>
        /// https://redis.io/commands/pfadd
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cacheKey"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        Task<bool> PfAddAsync<T>(string cacheKey, List<T> values);
        /// <summary>
        /// https://redis.io/commands/pfcount
        /// </summary>
        /// <param name="cacheKeys"></param>
        /// <returns></returns>
        long PfCount(List<string> cacheKeys);
        /// <summary>
        /// https://redis.io/commands/pfcount
        /// </summary>
        /// <param name="cacheKeys"></param>
        /// <returns></returns>
        Task<long> PfCountAsync(List<string> cacheKeys);
        /// <summary>
        /// https://redis.io/commands/pfmerge
        /// </summary>
        /// <param name="destKey"></param>
        /// <param name="sourceKeys"></param>
        /// <returns></returns>
        bool PfMerge(string destKey, List<string> sourceKeys);
        /// <summary>
        /// https://redis.io/commands/pfmerge
        /// </summary>
        /// <param name="destKey"></param>
        /// <param name="sourceKeys"></param>
        /// <returns></returns>
        Task<bool> PfMergeAsync(string destKey, List<string> sourceKeys);
        #endregion

        #region Geo
        /// <summary>
        /// https://redis.io/commands/geoadd
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        long GeoAdd(string cacheKey, List<(double longitude, double latitude, string member)> values);
        /// <summary>
        /// https://redis.io/commands/geoadd
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        Task<long> GeoAddAsync(string cacheKey, List<(double longitude, double latitude, string member)> values);
        /// <summary>
        /// https://redis.io/commands/geodist
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <param name="member1"></param>
        /// <param name="member2"></param>
        /// <param name="unit"></param>
        /// <returns></returns>
        double? GeoDist(string cacheKey, string member1, string member2, string unit = "m");
        /// <summary>
        /// https://redis.io/commands/geodist
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <param name="member1"></param>
        /// <param name="member2"></param>
        /// <param name="unit"></param>
        /// <returns></returns>
        Task<double?> GeoDistAsync(string cacheKey, string member1, string member2, string unit = "m");
        /// <summary>
        /// https://redis.io/commands/geohash
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <param name="members"></param>
        /// <returns></returns>
        List<string> GeoHash(string cacheKey, List<string> members);
        /// <summary>
        /// https://redis.io/commands/geohash
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <param name="members"></param>
        /// <returns></returns>
        Task<List<string>> GeoHashAsync(string cacheKey, List<string> members);
        /// <summary>
        /// https://redis.io/commands/geopos
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <param name="members"></param>
        /// <returns></returns>
        List<(decimal longitude, decimal latitude)?> GeoPos(string cacheKey, List<string> members);
        /// <summary>
        /// https://redis.io/commands/geopos
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <param name="members"></param>
        /// <returns></returns>
        Task<List<(decimal longitude, decimal latitude)?>> GeoPosAsync(string cacheKey, List<string> members);
        #endregion

        object Eval(string script, string cacheKey, List<object> args);

        Task<object> EvalAsync(string script, string cacheKey, List<object> args);
    }
}
