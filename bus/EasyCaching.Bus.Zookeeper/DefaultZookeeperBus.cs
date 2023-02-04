namespace EasyCaching.Bus.Zookeeper
{
    using EasyCaching.Core;
    using EasyCaching.Core.Bus;
    using EasyCaching.Core.Serialization;
    using Microsoft.Extensions.Options;
    using org.apache.zookeeper;
    using org.apache.zookeeper.data;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    public class DefaultZookeeperBus : EasyCachingAbstractBus
    {
        /// <summary>
        /// The zookeeper Bus options.
        /// </summary>
        private readonly ZkBusOptions _zkBusOptions;

        /// <summary>
        /// The zookeeper Client
        /// </summary>
        private ZooKeeper _zkClient;

        /// <summary>
        /// zookeeper data chane delegate event
        /// </summary>
        /// <param name="event"></param>
        /// <returns></returns>

        public delegate Task NodeDataChangeHandler(WatchedEvent @event);

        /// <summary>
        /// event
        /// </summary>
        private NodeDataChangeHandler _dataChangeHandler;

        /// <summary>
        /// lock
        /// </summary>
        private readonly object _zkEventLock = new object();

        /// <summary>
        /// The serializer.
        /// </summary>
        private readonly IEasyCachingSerializer _serializer;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:EasyCaching.Bus.Zookeeper.DefaultZookeeperBus"/> class.
        /// </summary>
        /// <param name="zkBusOptions"></param>
        /// <param name="serializer"></param>
        public DefaultZookeeperBus(
             IOptionsMonitor<ZkBusOptions> zkBusOptions
            , IEasyCachingSerializer serializer)
        {
            this.BusName = "easycachingbus";
            this._zkBusOptions = zkBusOptions.CurrentValue;
            this._zkClient = CreateClient(zkBusOptions.CurrentValue, new ZkNodeDataWatch(this));

            this._serializer = serializer;
        }

        /// <summary>
        /// Publish the specified topic and message.
        /// </summary>
        /// <param name="topic">Topic.</param>
        /// <param name="message">Message.</param>
        public override void BasePublish(string topic, EasyCachingMessage message)
        {
            var msg = _serializer.Serialize(message);
            var path = $"{topic}";
            Task.Run(async () =>
            {
                if (!await PathExistsAsync(path, true))
                {
                    await CreateRecursiveAsync(path, null, ZooDefs.Ids.OPEN_ACL_UNSAFE, CreateMode.PERSISTENT);
                }
                await SetDataAsync(path, msg);
            }).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Publishs the async.
        /// </summary>
        /// <returns>The async.</returns>
        /// <param name="topic">Topic.</param>
        /// <param name="message">Message.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        public override async Task BasePublishAsync(string topic, EasyCachingMessage message, CancellationToken cancellationToken = default(CancellationToken))
        {
            var msg = _serializer.Serialize(message);
            var path = $"{topic}";

            if (!await PathExistsAsync(path, true))
            {
                await CreateRecursiveAsync(path, null, ZooDefs.Ids.OPEN_ACL_UNSAFE, CreateMode.PERSISTENT);
            }
            await SetDataAsync(path, msg);
        }

        /// <summary>
        /// Subscribe the specified topic and action.
        /// </summary>
        /// <param name="topic">Topic.</param>
        /// <param name="action">Action.</param>
        public override void BaseSubscribe(string topic, Action<EasyCachingMessage> action)
        {
            var path = $"{topic}";
            Task.Factory.StartNew(async () =>
            {
                await SubscribeDataChangeAsync(path, SubscribeDataChange);
            }, TaskCreationOptions.LongRunning);
        }

        /// <summary>
        /// Subscribe the specified topic and action async.
        /// </summary>
        /// <param name="topic">Topic.</param>
        /// <param name="action">Action.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        public override async Task BaseSubscribeAsync(string topic, Action<EasyCachingMessage> action, CancellationToken cancellationToken = default(CancellationToken))
        {
            var path = $"{topic}";
            await SubscribeDataChangeAsync(path, SubscribeDataChange);
        }

        /// <summary>
        /// Ons the message.
        /// </summary>
        /// <param name="body">Body.</param>
        private void OnMessage(byte[] body)
        {
            var message = _serializer.Deserialize<EasyCachingMessage>(body);
            BaseOnMessage(message);
        }

        /// <summary>
        /// create zk client
        /// </summary>
        /// <param name="options"></param>
        /// <param name="watcher"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        private ZooKeeper CreateClient(ZkBusOptions options, Watcher watcher)
        {
            ZooKeeper.LogToFile = options.LogToFile;
            var zk = new ZooKeeper(options.ConnectionString, options.SessionTimeout, watcher);
            if (!string.IsNullOrEmpty(options.Digest))
            {
                zk.addAuthInfo("digest", Encoding.UTF8.GetBytes(options.Digest));
            }
            var operationStartTime = DateTime.Now;
            while (true)
            {
                if (zk.getState() == ZooKeeper.States.CONNECTING)
                {
                    Thread.Sleep(100);
                }
                else if (zk.getState() == ZooKeeper.States.CONNECTED
                    || zk.getState() == ZooKeeper.States.CONNECTEDREADONLY)
                {
                    return zk;
                }
                if (DateTime.Now - operationStartTime > TimeSpan.FromMilliseconds(options.OperatingTimeout))
                {
                    throw new TimeoutException(
                        $"connect cannot be retried because of retry timeout ({options.OperatingTimeout}Milliseconds)");
                }
            }
        }

        /// <summary>
        /// subscribe data change
        /// </summary>
        /// <param name="event"></param>
        /// <returns></returns>
        private async Task SubscribeDataChange(WatchedEvent @event)
        {
            var state = @event.getState();
            if (state == Watcher.Event.KeeperState.Expired)
            {
                await ReZkConnect();
            }

            var eventType = @event.get_Type();
            byte[] nodeData = await GetDataAsync(@event.getPath());

            switch (eventType)
            {
                case Watcher.Event.EventType.NodeCreated:
                    break;

                case Watcher.Event.EventType.NodeDeleted:
                case Watcher.Event.EventType.NodeDataChanged:
                    if (!nodeData.Any())
                    {
                        return;
                    }

                    //hander business logical
                    OnMessage(nodeData);
                    break;
            }
            await Task.CompletedTask;
        }

        /// <summary>
        ///  reconnnect zk
        /// </summary>
        /// <returns></returns>
        private async Task ReZkConnect()
        {
            if (!Monitor.TryEnter(_zkEventLock, _zkBusOptions.ConnectionTimeout))
                return;
            try
            {
                if (_zkClient != null)
                {
                    try
                    {
                        await _zkClient.closeAsync();
                    }
                    catch
                    {
                        // ignored
                    }
                }

                _zkClient = CreateClient(_zkBusOptions, new ZkNodeDataWatch(this));
            }
            finally
            {
                Monitor.Exit(_zkEventLock);
            }
        }

        /// <summary>
        /// subscribe data change
        /// </summary>
        /// <param name="path"></param>
        /// <param name="listener"></param>
        /// <returns></returns>
        private async Task SubscribeDataChangeAsync(string path, NodeDataChangeHandler listener)
        {
            _dataChangeHandler += listener;
            await PathExistsAsync(path, true);
        }

        /// <summary>
        /// pathExists
        /// </summary>
        /// <param name="path"></param>
        /// <param name="watch"></param>
        /// <returns></returns>
        private async Task<bool> PathExistsAsync(string path, bool watch = false)
        {
            path = GetZooKeeperPath(path);
            var state = await _zkClient.existsAsync(path, watch);
            return state != null;
        }

        /// <summary>
        /// set node data
        /// </summary>
        /// <param name="path"></param>
        /// <param name="data"></param>
        /// <param name="version"></param>
        /// <returns>node stat</returns>
        public async Task<Stat> SetDataAsync(string path, byte[] data, int version = -1)
        {
            path = GetZooKeeperPath(path);
            var stat = await _zkClient.setDataAsync(path, data, version);
            return stat;
        }

        /// <summary>
        /// get data
        /// </summary>
        /// <param name="path"></param>
        /// <param name="pathCv"></param>
        /// <returns></returns>
        public async Task<byte[]> GetDataAsync(string path, bool pathCv = false)
        {
            if (pathCv)
            {
                path = GetZooKeeperPath(path);
            }
            var data = await _zkClient.getDataAsync(path);
            return data?.Data;
        }

        /// <summary>
        /// recurive create
        /// </summary>
        /// <param name="path"></param>
        /// <param name="data"></param>
        /// <param name="acls"></param>
        /// <param name="createMode"></param>
        /// <returns></returns>
        private async Task<bool> CreateRecursiveAsync(string path, byte[] data, List<ACL> acls, CreateMode createMode)
        {
            path = GetZooKeeperPath(path);
            var paths = path.Trim('/').Split('/');
            var cur = "";
            foreach (var item in paths)
            {
                if (string.IsNullOrEmpty(item))
                {
                    continue;
                }
                cur += $"/{item}";
                var existStat = await _zkClient.existsAsync(cur, null);
                if (existStat != null)
                {
                    continue;
                }

                if (cur.Equals(path))
                {
                    await _zkClient.createAsync(cur, data, acls, createMode);
                }
                else
                {
                    await _zkClient.createAsync(cur, null, acls, createMode);
                }
            }
            return await Task.FromResult(true);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private string GetZooKeeperPath(string path)
        {
            var basePath = _zkBusOptions.BaseRoutePath ?? "/";

            if (!basePath.StartsWith("/"))
                basePath = basePath.Insert(0, "/");

            basePath = basePath.TrimEnd('/');

            if (!path.StartsWith("/"))
                path = path.Insert(0, "/");

            path = $"{basePath}{path.TrimEnd('/')}";
            return string.IsNullOrEmpty(path) ? "/" : path;
        }

        /// <summary>
        /// watch zkNode data Change
        /// </summary>
        private class ZkNodeDataWatch : Watcher
        {
            private readonly DefaultZookeeperBus _defaultZookeeperBus;

            public ZkNodeDataWatch(DefaultZookeeperBus defaultZookeeperBus)
            {
                _defaultZookeeperBus = defaultZookeeperBus;
            }

            public override async Task process(WatchedEvent watchedEvent)
            {
                var path = watchedEvent.getPath();
                if (path != null)
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
                        await _defaultZookeeperBus._dataChangeHandler(watchedEvent);
                    }
                }
            }
        }
    }
}