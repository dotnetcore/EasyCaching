using EasyCaching.Core.DistributedLock;
using Grpc.Core;
using System;
using System.Threading.Tasks;

namespace EasyCaching.Etcd.DistributedLock
{
    public class EtcdLockProvider : IDistributedLockProvider
    {
        private readonly IEtcdCaching _etcdClient;

        public EtcdLockProvider(IEtcdCaching etcdClient)
        {
            _etcdClient = etcdClient;
        }

        public Task<bool> SetAsync(string key, byte[] value, int ttlMs) =>
            _etcdClient.SetAsync(key,value,TimeSpan.FromMilliseconds(ttlMs));

        public bool Add(string key, byte[] value, int ttlMs) =>
            _etcdClient.Lock(key, TimeSpan.FromMilliseconds(ttlMs));

        public Task<bool> AddAsync(string key, byte[] value, int ttlMs) =>
            _etcdClient.LockAsync(key,TimeSpan.FromMilliseconds(ttlMs));

        public bool Delete(string key, byte[] value) =>
            _etcdClient.UnLock(key);
       

        public async Task<bool> DeleteAsync(string key, byte[] value) =>
           await _etcdClient.UnLockAsnyc(key);

        public bool CanRetry(Exception ex) => ex is RpcException;
    }
}
