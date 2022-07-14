namespace EasyCaching.Bus.Zookeeper
{
    using System;
    using System.Collections.Concurrent;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using EasyCaching.Bus.Zookeeper.Internal;
    using EasyCaching.Core;
    using EasyCaching.Core.Bus;
    using EasyCaching.Core.Serialization;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Logging.Abstractions;
    using Microsoft.Extensions.Options;
    using org.apache.zookeeper;

    public class DefaultZookeeperBus : EasyCachingAbstractBus
    {     
        /// <summary>
        /// The zookeeper Bus options.
        /// </summary>
        private readonly ZkBusOptions _zkBusOptions;

        /// <summary>
        /// The serializer.
        /// </summary>
        private readonly IEasyCachingSerializer _serializer;

        /// <summary>
        /// zookeeper clientFactory
        /// </summary>

        private readonly IZookeeperClientFactory _zookeeperClientFactory;

        /// <summary>
        /// 
        /// </summary>
        private ConcurrentDictionary<string, string> zkSubscribeWatchers = new ConcurrentDictionary<string, string>();


        /// <summary>
        /// log
        /// </summary>

        private readonly ILogger _logger = NullLogger<DefaultZookeeperBus>.Instance;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:EasyCaching.Bus.Zookeeper.DefaultZookeeperBus"/> class.
        /// </summary>
        /// <param name="zkBusOptions"></param>
        /// <param name="zookeeperClientFactory"></param>
        /// <param name="serializer"></param>
        public DefaultZookeeperBus(
             IOptionsMonitor<ZkBusOptions> zkBusOptions
            , IZookeeperClientFactory zookeeperClientFactory
            , IEasyCachingSerializer serializer)
        {
            this.BusName = "easycachingbus";
            this._zkBusOptions = zkBusOptions.CurrentValue;

            this._zookeeperClientFactory = zookeeperClientFactory;

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
            IZookeeperClient zookeeperClient = _zookeeperClientFactory.GetZooKeeperClient();
            Task.Run(async () =>
            {
                if (!await zookeeperClient.ExistsAsync(path))
                {
                    await zookeeperClient.CreateRecursiveAsync(path, null,ZooDefs.Ids.OPEN_ACL_UNSAFE);
                }
                await zookeeperClient.SetDataAsync(path, msg);
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
            var msg =  _serializer.Serialize(message);
            var path = $"/{topic}";
            IZookeeperClient zookeeperClient = _zookeeperClientFactory.GetZooKeeperClient();
            if (!await zookeeperClient.ExistsAsync(path))
            {
                await zookeeperClient.CreateRecursiveAsync(path, null, ZooDefs.Ids.OPEN_ACL_UNSAFE);
            }
            await zookeeperClient.SetDataAsync(path, msg);
        }

        /// <summary>
        /// Subscribe the specified topic and action.
        /// </summary>
        /// <param name="topic">Topic.</param>
        /// <param name="action">Action.</param>
        public override void BaseSubscribe(string topic, Action<EasyCachingMessage> action)
        {
            Task.Factory.StartNew(() =>
            {
                var path = $"/{topic}";
                IZookeeperClient zookeeperClient = _zookeeperClientFactory.GetZooKeeperClient();
                zookeeperClient.SubscribeDataChangeAsync(path, SubscribeDataChange);

            }, TaskCreationOptions.LongRunning);
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
        /// subscribe data change
        /// </summary>
        /// <param name="client"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        private async Task SubscribeDataChange(IZookeeperClient client, NodeDataChangeArgs args)
        {
            var eventType = args.Type;
            byte[] nodeData = null;
            if (args.CurrentData != null && args.CurrentData.Any())
            {
                nodeData = args.CurrentData.ToArray();
            }

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
#if DEBUG
                    var jonString = Encoding.UTF8.GetString(nodeData);
                    Console.WriteLine("Node change");
#endif
                    break;
            }
           await Task.CompletedTask;
        }
    }
}
