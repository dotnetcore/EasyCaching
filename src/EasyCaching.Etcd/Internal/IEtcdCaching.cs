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
        CacheValue<T> GetVal<T>(string cacheKey);

        /// <summary>
        /// get data
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <returns></returns>
        Task<CacheValue<T>> GetValAsync<T>(string cacheKey);

        /// <summary>
        /// get rangevalues
        /// </summary>
        /// <param name="prefixKey"></param>
        /// <returns></returns>
        IDictionary<string, string> GetRangeVals(string prefixKey);

        /// <summary>
        /// get rangevalues
        /// </summary>
        /// <param name="prefixKey"></param>
        /// <returns></returns>
        Task<IDictionary<string, string>> GetRangeValsAsync(string prefixKey);

        /// <summary>
        ///  data exists
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <returns></returns>
        bool GetDataExists(string cacheKey);

        /// <summary>
        ///  data exists
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <returns></returns>
        Task<bool> GetDataExistsAsync(string cacheKey);

        /// <summary>
        /// put ke-val with leaseId
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="ts"></param>
        /// <returns></returns>
        bool AddEphemeralData<T>(string key, T value, TimeSpan? ts);

        /// <summary>
        /// put ke-val with leaseId
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="ts"></param>
        /// <returns></returns>
        Task<bool> AddEphemeralDataAsync<T>(string key, T value, TimeSpan? ts);

        /// <summary>
        /// delete key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        long DeleteData(string key);

        /// <summary>
        /// delete key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        Task<long> DeleteDataAsync(string key);

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