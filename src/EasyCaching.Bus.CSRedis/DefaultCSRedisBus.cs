namespace EasyCaching.Bus.CSRedis
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using EasyCaching.Core.Bus;
    using Newtonsoft.Json;

    public class DefaultCSRedisBus : IEasyCachingBus
    {
        /// <summary>
        /// The cache.
        /// </summary>
        private readonly EasyCachingCSRedisClient _client;

        /// <summary>
        /// The handler.
        /// </summary>
        private Action<EasyCachingMessage> _handler;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:EasyCaching.Bus.CSRedis.DefaultCSRedisBus"/> class.
        /// </summary>
        /// <param name="clients">Clients.</param>
        public DefaultCSRedisBus(IEnumerable<EasyCachingCSRedisClient> clients)
        {
            this._client = clients.FirstOrDefault(x => x.Name.Equals("easycachingbus"));
        }

        /// <summary>
        /// Publish the specified topic and message.
        /// </summary>
        /// <param name="topic">Topic.</param>
        /// <param name="message">Message.</param>
        public void Publish(string topic, EasyCachingMessage message)
        {
            var msg = JsonConvert.SerializeObject(message);

            _client.Publish(topic, msg);
        }

        /// <summary>
        /// Publishs the async.
        /// </summary>
        /// <returns>The async.</returns>
        /// <param name="topic">Topic.</param>
        /// <param name="message">Message.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        public async Task PublishAsync(string topic, EasyCachingMessage message, CancellationToken cancellationToken = default(CancellationToken))
        {
            var msg = JsonConvert.SerializeObject(message);

            await _client.PublishAsync(topic, msg);
        }

        /// <summary>
        /// Subscribe the specified topic and action.
        /// </summary>
        /// <param name="topic">Topic.</param>
        /// <param name="action">Action.</param>
        public void Subscribe(string topic, Action<EasyCachingMessage> action)
        {
            _handler = action;

            _client.Subscribe(
                (topic, msg => OnMessage(msg.Body))
            );
        }

        /// <summary>
        /// Ons the message.
        /// </summary>
        /// <param name="body">Body.</param>
        private void OnMessage(string body)
        {
            var message = JsonConvert.DeserializeObject<EasyCachingMessage>(body);

            _handler?.Invoke(message);
        }
    }
}
