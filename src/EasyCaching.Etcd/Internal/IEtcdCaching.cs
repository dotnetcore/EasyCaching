using EasyCaching.Core;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EasyCaching.Etcd
{
    public interface IEtcdCaching
    {
        string ProviderName { get; }

        /// <summary>
        /// get data
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <returns></returns>
        CacheValue<T> Get<T>(string cacheKey);

        /// <summary>
        /// get data
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <returns></returns>
        Task<CacheValue<T>> GetAsync<T>(string cacheKey);

        /// <summary>
        /// get rangevalues
        /// </summary>
        /// <param name="prefixKey"></param>
        /// <returns></returns>
        IDictionary<string, string> GetAll(string prefixKey);

        /// <summary>
        /// get rangevalues
        /// </summary>
        /// <param name="prefixKey"></param>
        /// <returns></returns>
        Task<IDictionary<string, string>> GetAllAsync(string prefixKey);

        /// <summary>
        ///  data exists
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <returns></returns>
        bool Exists(string cacheKey);

        /// <summary>
        ///  data exists
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <returns></returns>
        Task<bool> ExistsAsync(string cacheKey);

        /// <summary>
        /// put ke-val with leaseId
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="ts"></param>
        /// <returns></returns>
        bool Set<T>(string key, T value, TimeSpan? ts);

        /// <summary>
        /// put ke-val with leaseId
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="ts"></param>
        /// <returns></returns>
        Task<bool> SetAsync<T>(string key, T value, TimeSpan? ts);

        /// <summary>
        /// set lock with leaseId
        /// </summary>
        /// <param name="key"></param>
        /// <param name="ts"></param>
        /// <returns></returns>
        bool Lock(string key, TimeSpan? ts);

        /// <summary>
        /// set lock with leaseId
        /// </summary>
        /// <param name="key"></param>
        /// <param name="ts"></param>
        /// <returns></returns>
        Task<bool> LockAsync(string key, TimeSpan? ts);

        /// <summary>
        /// release lock 
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        bool UnLock(string key);

        /// <summary>
        /// release lock 
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        /// <returns></returns>
        Task<bool> UnLockAsnyc(string key);

        /// <summary>
        /// get key expireTTL
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        long GetExpireTTL(string key);

        /// <summary>
        /// get key expireTTL
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        Task<long> GetExpireTTLAsync(string key);

        /// <summary>
        /// delete key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        long Delete(string key);

        /// <summary>
        /// delete key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        Task<long> DeleteAsync(string key);

        /// <summary>
        /// delete range key
        /// </summary>
        /// <param name="prefixKey"></param>
        /// <returns></returns>
        long DeleteRangeData(string prefixKey);

        /// <summary>
        /// delete range key
        /// </summary>
        /// <param name="prefixKey"></param>
        /// <returns></returns>
        Task<long> DeleteRangeDataAsync(string prefixKey);
    }
}