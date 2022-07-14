using System.Collections.Generic;
using System.Threading.Tasks;
using org.apache.zookeeper;

namespace EasyCaching.Bus.Zookeeper
{
    /// <summary>
    /// connect state changehander params
    /// </summary>
    public class ConnectionStateChangeArgs
    {
        /// <summary>
        /// connect state
        /// </summary>
        public Watcher.Event.KeeperState State { get; set; }
    }

    /// <summary>
    /// NodeChange params
    /// </summary>
    public abstract class NodeChangeArgs
    {
        /// <summary>
        /// create a new node with params
        /// </summary>
        /// <param name="path"></param>
        /// <param name="type"></param>
        protected NodeChangeArgs(string path, Watcher.Event.EventType type)
        {
            Path = path;
            Type = type;
        }

        /// <summary>
        /// changeType
        /// </summary>
        public Watcher.Event.EventType Type { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public string Path { get; private set; }
    }

    /// <summary>
    /// node data change params
    /// </summary>
    public sealed class NodeDataChangeArgs : NodeChangeArgs
    {
        /// <summary>
        /// create  new nodedata with params
        /// </summary>
        /// <param name="path"></param>
        /// <param name="type"></param>
        /// <param name="currentData"></param>
        public NodeDataChangeArgs(string path, Watcher.Event.EventType type, IEnumerable<byte> currentData) : base(path,
            type)
        {
            CurrentData = currentData;
        }

        /// <summary>
        /// current nodedata last data
        /// </summary>
        public IEnumerable<byte> CurrentData { get; private set; }
    }

    /// <summary>
    ///  childnode change args
    /// </summary>
    public sealed class NodeChildrenChangeArgs : NodeChangeArgs
    {
        /// <summary>
        /// create childnodes change agrs
        /// </summary>
        /// <param name="path"></param>
        /// <param name="type">event tyoe </param>
        /// <param name="currentChildrens"></param>
        public NodeChildrenChangeArgs(string path, Watcher.Event.EventType type, IEnumerable<string> currentChildrens) :
            base(path, type)
        {
            CurrentChildrens = currentChildrens;
        }

        /// <summary>
        /// currentnode all childnodes
        /// </summary>
        public IEnumerable<string> CurrentChildrens { get; private set; }
    }

    /// <summary>
    /// node datachange delegate
    /// </summary>
    /// <param name="client"></param>
    /// <param name="args"></param>
    public delegate Task NodeDataChangeHandler(IZookeeperClient client, NodeDataChangeArgs args);

    /// <summary>
    /// childnode datachange delegate
    /// </summary>
    /// <param name="client"></param>
    /// <param name="args"></param>
    public delegate Task NodeChildrenChangeHandler(IZookeeperClient client, NodeChildrenChangeArgs args);

    /// <summary>
    /// connectstat change delegate
    /// </summary>
    /// <param name="client"></param>
    /// <param name="args"></param>
    public delegate Task ConnectionStateChangeHandler(IZookeeperClient client, ConnectionStateChangeArgs args);
}