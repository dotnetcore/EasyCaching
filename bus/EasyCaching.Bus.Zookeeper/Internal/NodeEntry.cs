using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using org.apache.zookeeper;
using org.apache.zookeeper.data;

namespace EasyCaching.Bus.Zookeeper
{
    internal class NodeEntry
    {
        #region Field

        private readonly IZookeeperClient _client;

        /// <summary>
        /// 数据变更多播委托。
        /// </summary>
        private NodeDataChangeHandler _dataChangeHandler;

        /// <summary>
        /// 子节点变更多播委托。
        /// </summary>
        private NodeChildrenChangeHandler _childrenChangeHandler;

        /// <summary>
        /// 节点的快照。
        /// </summary>
        private NodeSnapshot _localSnapshot = default(NodeSnapshot);

        #endregion Field

        #region Property

        public string Path { get; }

        #endregion Property

        #region Constructor

        public NodeEntry(string path, IZookeeperClient client)
        {
            Path = path;
            _client = client;
        }

        #endregion Constructor

        #region Public Method

        public async Task<IEnumerable<byte>> GetDataAsync(bool watch = false)
        {
            var zookeeper = _client.ZooKeeper;
            var data = await zookeeper.getDataAsync(Path, watch);

            _localSnapshot.SetData(data?.Data);

            return data?.Data;
        }

        public async Task<IEnumerable<string>> GetChildrenAsync(bool watch = false)
        {
            var zookeeper = _client.ZooKeeper;
            var data = await zookeeper.getChildrenAsync(Path, watch);

            _localSnapshot.SetChildrens(data?.Children);

            return data?.Children;
        }

        public async Task<bool> ExistsAsync(bool watch = false)
        {
            var zookeeper = _client.ZooKeeper;
            var data = await zookeeper.existsAsync(Path, watch);

            var exists = data != null;

            _localSnapshot.SetExists(exists);

            return exists;
        }

        public async Task<string> CreateAsync(byte[] data, List<ACL> acls, CreateMode createMode)
        {
            var zooKeeper = _client.ZooKeeper;
            var path = await zooKeeper.createAsync(Path, data, acls, createMode);

            _localSnapshot.Create(createMode, data, acls);

            return path;
        }

        public Task<Stat> SetDataAsync(byte[] data, int version = -1)
        {
            var zooKeeper = _client.ZooKeeper;
            var stat = zooKeeper.setDataAsync(Path, data, version);

            _localSnapshot.Update(data, version);

            return stat;
        }

        public async Task DeleteAsync(int version = -1)
        {
            var zookeeper = _client.ZooKeeper;
            await zookeeper.deleteAsync(Path, version);

            _localSnapshot.Delete();
        }

        #region Listener

        public async Task SubscribeDataChangeAsync(NodeDataChangeHandler listener)
        {
            _dataChangeHandler += listener;

            //watch data change
            await WatchDataChangeAsync();
        }

        public void UnSubscribeDataChange(NodeDataChangeHandler listener)
        {
            _dataChangeHandler -= listener;
        }

        public async Task<IEnumerable<string>> SubscribeChildrenChange(NodeChildrenChangeHandler listener)
        {
            _childrenChangeHandler += listener;

            //watch childnode change
            return await WatchChildrenChange();
        }

        public void UnSubscribeChildrenChange(NodeChildrenChangeHandler listener)
        {
            _childrenChangeHandler -= listener;
        }

        #endregion Listener

        #endregion Public Method

        #region Private Method

        /// <summary>
        /// Notify node change
        /// </summary>
        /// <param name="watchedEvent">zookeeper sdk watch hander parmas</param>
        /// <param name="isFirstConnection">wheather is zk first connect</param>
        internal async Task OnChange(WatchedEvent watchedEvent, bool isFirstConnection)
        {
            var path = watchedEvent.getPath();
            var stateChanged = path == null;

            if (stateChanged)
            {
                await OnStatusChangeHandle(watchedEvent, isFirstConnection);
            }
            else if (path == Path) 
            {
                var eventType = watchedEvent.get_Type();

                var dataChanged = new[]
                {
                    Watcher.Event.EventType.NodeCreated,
                    Watcher.Event.EventType.NodeDataChanged,
                    Watcher.Event.EventType.NodeDeleted
                }.Contains(eventType);

                if (dataChanged)
                {
                    if (eventType == Watcher.Event.EventType.NodeCreated && HasChildrenChangeHandler)
                        await _client.RetryUntilConnected(() => GetChildrenAsync(true));

                    await OnDataChangeHandle(watchedEvent);
                }
                else
                {
                    await OnChildrenChangeHandle(watchedEvent);
                }
            }
        }

        /// <summary>
        /// wheather has data  change handler。
        /// </summary>
        private bool HasDataChangeHandler => HasHandler(_dataChangeHandler);

        /// <summary>
        /// wheather has childnode change handler。
        /// </summary>
        private bool HasChildrenChangeHandler => HasHandler(_childrenChangeHandler);

        /// <summary>
        /// stat change handle
        /// </summary>
        /// <param name="watchedEvent"></param>
        /// <param name="isFirstConnection"></param>
        private async Task OnStatusChangeHandle(WatchedEvent watchedEvent, bool isFirstConnection)
        {
            if (isFirstConnection)
                return;

            await RestoreEphemeral();

            if (HasDataChangeHandler)
                await OnDataChangeHandle(watchedEvent);
            if (HasChildrenChangeHandler)
                await OnChildrenChangeHandle(watchedEvent);
        }

        private async Task OnDataChangeHandle(WatchedEvent watchedEvent)
        {
            if (!HasDataChangeHandler)
                return;

            //get currentnode news data fuchander
            var getCurrentData = new Func<Task<IEnumerable<byte>>>(() => _client.RetryUntilConnected(async () =>
            {
                try
                {
                    return await GetDataAsync();
                }
                catch (KeeperException.NoNodeException) 
                {
                    return null;
                }
            }));

            //base event type build nodechange hander params
            NodeDataChangeArgs args;
            switch (watchedEvent.get_Type())
            {
                case Watcher.Event.EventType.NodeCreated:
                    args = new NodeDataChangeArgs(Path, Watcher.Event.EventType.NodeCreated, await getCurrentData());
                    break;

                case Watcher.Event.EventType.NodeDeleted:
                    args = new NodeDataChangeArgs(Path, Watcher.Event.EventType.NodeDeleted, null);
                    break;

                case Watcher.Event.EventType.NodeDataChanged:
                case Watcher.Event.EventType.None: 
                    args = new NodeDataChangeArgs(Path, Watcher.Event.EventType.NodeDataChanged,
                        await getCurrentData());
                    break;

                default:
                    throw new NotSupportedException($"UnSupport Event Type：{watchedEvent.get_Type()}");
            }

            await _dataChangeHandler(_client, args);

            //watch data change
            await WatchDataChangeAsync();
        }

        private async Task OnChildrenChangeHandle(WatchedEvent watchedEvent)
        {
            if (!HasChildrenChangeHandler)
                return;

            //get currentnode newsChildNodeInfo
            var getCurrentChildrens = new Func<Task<IEnumerable<string>>>(() => _client.RetryUntilConnected(
                async () =>
                {
                    try
                    {
                        return await GetChildrenAsync();
                    }
                    catch (KeeperException.NoNodeException)
                    {
                        return null;
                    }
                }));

            //根据事件类型构建节点子节点变更事件参数
            NodeChildrenChangeArgs args;
            switch (watchedEvent.get_Type())
            {
                case Watcher.Event.EventType.NodeCreated:
                    args = new NodeChildrenChangeArgs(Path, Watcher.Event.EventType.NodeCreated,
                        await getCurrentChildrens());
                    break;

                case Watcher.Event.EventType.NodeDeleted:
                    args = new NodeChildrenChangeArgs(Path, Watcher.Event.EventType.NodeDeleted, null);
                    break;

                case Watcher.Event.EventType.NodeChildrenChanged:
                case Watcher.Event.EventType.None: 
                    args = new NodeChildrenChangeArgs(Path, Watcher.Event.EventType.NodeChildrenChanged,
                        await getCurrentChildrens());
                    break;

                default:
                    throw new NotSupportedException($"UnSupport even type：{watchedEvent.get_Type()}");
            }

            await _childrenChangeHandler(_client, args);

            //watch childNode change
            await WatchChildrenChange();
        }

        private async Task WatchDataChangeAsync()
        {
            await _client.RetryUntilConnected(() => ExistsAsync(true));
        }

        private async Task<IEnumerable<string>> WatchChildrenChange()
        {
            return await _client.RetryUntilConnected(async () =>
            {
                await ExistsAsync(true);
                try
                {
                    return await GetChildrenAsync(true);
                }
                catch (KeeperException.NoNodeException)
                {
                }

                return null;
            });
        }

        private static bool HasHandler(MulticastDelegate multicast)
        {
            return multicast != null && multicast.GetInvocationList().Any();
        }

        private async Task RestoreEphemeral()
        {
            if (!_client.Options.EnableEphemeralNodeRestore)
                return;


            if (!_localSnapshot.IsExist)
                return;

            if (_localSnapshot.Mode != CreateMode.EPHEMERAL && _localSnapshot.Mode != CreateMode.EPHEMERAL_SEQUENTIAL)
                return;

            try
            {
                await _client.RetryUntilConnected(async () =>
                {
                    try
                    {
                        return await CreateAsync(_localSnapshot.Data?.ToArray(), _localSnapshot.Acls,
                            _localSnapshot.Mode);
                    }
                    catch (KeeperException.NodeExistsException) 
                    {
                        return Path;
                    }
                });
            }
            catch (Exception exception)
            {
                Console.WriteLine($"recover failed node,Exeption：{exception.Message}");
            }
        }

        #endregion Private Method

        #region Help Type

        public struct NodeSnapshot
        {
            public bool IsExist { get; set; }
            public CreateMode Mode { get; set; }
            public IEnumerable<byte> Data { get; set; }
            public int? Version { get; set; }
            public List<ACL> Acls { get; set; }
            public IEnumerable<string> Childrens { get; set; }

            public void Create(CreateMode mode, byte[] data, List<ACL> acls)
            {
                IsExist = true;
                Mode = mode;
                Data = data;
                Version = -1;
                Acls = acls;
                Childrens = null;
            }

            public void Update(IEnumerable<byte> data, int version)
            {
                IsExist = true;
                Data = data;
                Version = version;
            }

            public void Delete()
            {
                IsExist = false;
                Mode = null;
                Data = null;
                Version = null;
                Acls = null;
                Childrens = null;
            }

            public void SetData(IEnumerable<byte> data)
            {
                IsExist = true;
                Data = data;
            }

            public void SetChildrens(IEnumerable<string> childrens)
            {
                IsExist = true;
                Childrens = childrens;
            }

            public void SetExists(bool exists)
            {
                if (!exists)
                {
                    Delete();
                    return;
                }

                IsExist = true;
            }
        }

        #endregion Help Type
    }
}