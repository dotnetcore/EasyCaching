using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using org.apache.zookeeper;
using org.apache.zookeeper.data;

namespace EasyCaching.Bus.Zookeeper
{
    /// <summary>
    /// Abstract ZooKeeperClient
    /// </summary>
    public interface IZookeeperClient : IDisposable
    {
        /// <summary>
        /// ZooKeeperConnect object
        /// </summary>
        ZooKeeper ZooKeeper { get; }

        /// <summary>
        /// Options
        /// </summary>
        ZkBusOptions Options { get; }

        /// <summary>
        /// wait zk connect to give states
        /// </summary>
        /// <param name="states"></param>
        /// <param name="timeout"></param>
        /// <returns>success:true,fail:false</returns>
        bool WaitForKeeperState(Watcher.Event.KeeperState states, TimeSpan timeout);

        /// <summary>
        /// retry util zk connected
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="callable">execute zk action</param>
        /// <returns></returns>
        Task<T> RetryUntilConnected<T>(Func<Task<T>> callable);

        /// <summary>
        /// get give node data
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        Task<IEnumerable<byte>> GetDataAsync(string path);

        /// <summary>
        /// get childnodes
        /// </summary>
        /// <param name="path"></param>
        /// <returns>childnodesList</returns>
        Task<IEnumerable<string>> GetChildrenAsync(string path);

        /// <summary>
        /// node exists
        /// </summary>
        /// <param name="path"></param>
        /// <returns>if have return true，then return false。</returns>
        Task<bool> ExistsAsync(string path);

        /// <summary>
        /// create node
        /// </summary>
        /// <param name="path"></param>
        /// <param name="data"></param>
        /// <param name="acls"></param>
        /// <param name="createMode"></param>
        /// <returns></returns>
        /// <remarks>
        /// 
        /// </remarks>
        Task<string> CreateAsync(string path, byte[] data, List<ACL> acls, CreateMode createMode);

        /// <summary>
        /// set node data
        /// </summary>
        /// <param name="path"></param>
        /// <param name="data"></param>
        /// <param name="version"></param>
        /// <returns>node stat</returns>
        Task<Stat> SetDataAsync(string path, byte[] data, int version = -1);

        /// <summary>
        /// delete node
        /// </summary>
        /// <param name="path"></param>
        /// <param name="version"></param>
        Task DeleteAsync(string path, int version = -1);

        /// <summary>
        /// subscribe node data change
        /// </summary>
        /// <param name="path"></param>
        /// <param name="listener"></param>
        Task SubscribeDataChangeAsync(string path, NodeDataChangeHandler listener);

        /// <summary>
        /// unsubscribe node data change
        /// </summary>
        /// <param name="path"></param>
        /// <param name="listener"></param>
        void UnSubscribeDataChange(string path, NodeDataChangeHandler listener);

        /// <summary>
        /// subscribe connect stat change
        /// </summary>
        /// <param name="listener"></param>
        void SubscribeStatusChange(ConnectionStateChangeHandler listener);

        /// <summary>
        /// unsubscribe connect stat change
        /// </summary>
        /// <param name="listener"></param>
        void UnSubscribeStatusChange(ConnectionStateChangeHandler listener);

        /// <summary>
        /// subscribe childnode change
        /// </summary>
        /// <param name="path"></param>
        /// <param name="listener"></param>
        Task<IEnumerable<string>> SubscribeChildrenChange(string path, NodeChildrenChangeHandler listener);

        /// <summary>
        /// Unsubscribe childnode change
        /// </summary>
        /// <param name="path"></param>
        /// <param name="listener"></param>
        void UnSubscribeChildrenChange(string path, NodeChildrenChangeHandler listener);
    }
}