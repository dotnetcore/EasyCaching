using org.apache.zookeeper;
using org.apache.zookeeper.data;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EasyCaching.Bus.Zookeeper.Internal
{
    /// <summary>
    /// ZooKeeper Client Extensions helper
    /// </summary>
    public static class ZookeeperClientExtensions
    {
        /// <summary>
        /// Create Ephemeral Node
        /// </summary>
        /// <param name="client"></param>
        /// <param name="path"></param>
        /// <param name="data"></param>
        /// <param name="isSequential"></param>
        /// <returns></returns>
        public static Task<string> CreateEphemeralAsync(this IZookeeperClient client, string path, byte[] data,
            bool isSequential = false)
        {
            return client.CreateEphemeralAsync(path, data, ZooDefs.Ids.OPEN_ACL_UNSAFE, isSequential);
        }

        /// <summary>
        /// Create Ephemeral Node
        /// </summary>
        /// <param name="client"></param>
        /// <param name="path"></param>
        /// <param name="data"></param>
        /// <param name="acls"></param>
        /// <param name="isSequential"></param>
        /// <returns></returns>
        public static Task<string> CreateEphemeralAsync(this IZookeeperClient client, string path, byte[] data,
            List<ACL> acls, bool isSequential = false)
        {
            return client.CreateAsync(path, data, acls,
                isSequential ? CreateMode.EPHEMERAL_SEQUENTIAL : CreateMode.EPHEMERAL);
        }

        /// <summary>
        /// Create Node
        /// </summary>
        /// <param name="client"></param>
        /// <param name="path"></param>
        /// <param name="data"></param>
        /// <param name="isSequential"></param>
        /// <returns></returns>
        public static Task<string> CreatePersistentAsync(this IZookeeperClient client, string path, byte[] data,
            bool isSequential = false)
        {
            return client.CreatePersistentAsync(path, data, ZooDefs.Ids.OPEN_ACL_UNSAFE, isSequential);
        }

        /// <summary>
        /// Create Node
        /// </summary>
        /// <param name="client"></param>
        /// <param name="path"></param>
        /// <param name="data"></param>
        /// <param name="acls"></param>
        /// <param name="isSequential"></param>
        /// <returns></returns>
        public static Task<string> CreatePersistentAsync(this IZookeeperClient client, string path, byte[] data,
            List<ACL> acls, bool isSequential = false)
        {
            return client.CreateAsync(path, data, acls,
                isSequential ? CreateMode.PERSISTENT_SEQUENTIAL : CreateMode.PERSISTENT);
        }

        /// <summary>
        /// Recursive delete childnode and self
        /// </summary>
        /// <param name="client"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        public static async Task<bool> DeleteRecursiveAsync(this IZookeeperClient client, string path)
        {
            IEnumerable<string> children;
            try
            {
                children = await client.GetChildrenAsync(path);
            }
            catch (KeeperException.NoNodeException)
            {
                return true;
            }

            foreach (var subPath in children)
            {
                if (!await client.DeleteRecursiveAsync(path + "/" + subPath))
                {
                    return false;
                }
            }

            await client.DeleteAsync(path);
            return true;
        }

        /// <summary>
        /// recurise create childnode and self 
        /// </summary>
        /// <param name="client"></param>
        /// <param name="path"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public static Task CreateRecursiveAsync(this IZookeeperClient client, string path, byte[] data)
        {
            return client.CreateRecursiveAsync(path, data, ZooDefs.Ids.OPEN_ACL_UNSAFE);
        }

        /// <summary>
        /// recurise create childnode and self 
        /// </summary>
        /// <param name="client"></param>
        /// <param name="path"></param>
        /// <param name="data"></param>
        /// <param name="acls"></param>
        /// <returns></returns>
        public static Task CreateRecursiveAsync(this IZookeeperClient client, string path, byte[] data, List<ACL> acls)
        {
            return client.CreateRecursiveAsync(path, p => data, p => acls);
        }

        /// <summary>
        /// recurise create childnode and self 
        /// </summary>
        /// <param name="client"></param>
        /// <param name="path"></param>
        /// <param name="getNodeData">selfnode data hander</param>
        /// <param name="getNodeAcls"></param>
        /// <returns></returns>
        public static async Task CreateRecursiveAsync(this IZookeeperClient client, string path,
            Func<string, byte[]> getNodeData, Func<string, List<ACL>> getNodeAcls)
        {
            var data = getNodeData(path);
            var acls = getNodeAcls(path);
            try
            {
                await client.CreateAsync(path, data, acls, CreateMode.PERSISTENT);
            }
            catch (KeeperException.NodeExistsException)
            {
            }
            catch (KeeperException.NoNodeException)
            {
                var parentDir = path.Substring(0, path.LastIndexOf('/'));
                await CreateRecursiveAsync(client, parentDir, getNodeData, getNodeAcls);
                await client.CreateAsync(path, data, acls, CreateMode.PERSISTENT);
            }
        }

        /// <summary>
        /// wait util zk connect success，timeout is in options
        /// </summary>
        /// <param name="client">zk client</param>
        public static void WaitForRetry(this IZookeeperClient client)
        {
            client.WaitUntilConnected(client.Options.OperatingSpanTimeout);
        }
        /// <summary>
        /// wait util zk connect success
        /// </summary>
        /// <param name="client"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public static bool WaitUntilConnected(this IZookeeperClient client, TimeSpan timeout)
        {
            return client.WaitForKeeperState(Watcher.Event.KeeperState.SyncConnected, timeout);
        }
    }
}
