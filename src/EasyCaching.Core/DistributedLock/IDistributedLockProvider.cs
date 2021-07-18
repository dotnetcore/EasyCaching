using System;
using System.Threading.Tasks;

namespace EasyCaching.Core.DistributedLock
{
    public interface IDistributedLockProvider
    {
        Task<bool> SetAsync(string key, byte[] value, int ttlMs);

        bool Add(string key, byte[] value, int ttlMs);
        Task<bool> AddAsync(string key, byte[] value, int ttlMs);

        bool Delete(string key, byte[] value);
        Task<bool> DeleteAsync(string key, byte[] value);

        bool CanRetry(Exception ex);
    }
}
