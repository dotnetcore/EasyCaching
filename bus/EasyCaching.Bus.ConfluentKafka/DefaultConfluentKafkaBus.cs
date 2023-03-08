namespace EasyCaching.Bus.ConfluentKafka
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Confluent.Kafka;
    using EasyCaching.Core;
    using EasyCaching.Core.Bus;
    using EasyCaching.Core.Serialization;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Logging.Abstractions;
    using Microsoft.Extensions.Options;

    public class DefaultConfluentKafkaBus : EasyCachingAbstractBus
    {


        /// <summary>
        /// The kafka Bus options.
        /// </summary>
        private readonly ConfluentKafkaBusOptions _kafkaBusOptions;

        /// <summary>
        /// The serializer.
        /// </summary>
        private readonly IEasyCachingSerializer _serializer;

        /// <summary>
        /// kafka producer object
        /// </summary>

        private readonly IProducer<Null, byte[]> _producer;


        /// <summary>
        /// log
        /// </summary>

        private readonly ILogger _logger = NullLogger<DefaultConfluentKafkaBus>.Instance;


        /// <summary>
        /// Initializes a new instance of the <see cref="T:EasyCaching.Bus.ConfluentKafka.DefaultConfluentKafkaBus"/> class.
        /// </summary>
        /// <param name="kafkaBusOptions"></param>
        /// <param name="serializer"></param>
        public DefaultConfluentKafkaBus(
             IOptionsMonitor<ConfluentKafkaBusOptions> kafkaBusOptions
            , IEasyCachingSerializer serializer)
        {
            this.BusName = "easycachingbus";
            this._kafkaBusOptions = kafkaBusOptions.CurrentValue;

            this._producer = new ProducerBuilder<Null, byte[]>(this._kafkaBusOptions.ProducerConfig).Build();

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

            _producer.Produce(topic, new Message<Null, byte[]> { Value = msg });
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

            await _producer.ProduceAsync(topic, new Message<Null, byte[]> { Value = msg });
        }

        /// <summary>
        /// Subscribe the specified topic and action.
        /// </summary>
        /// <param name="topic">Topic.</param>
        /// <param name="action">Action.</param>
        public override void BaseSubscribe(string topic, Action<EasyCachingMessage> action)
        {
            _ = StartConsumer(topic);
        }

        /// <summary>
        /// Subscribe the specified topic and action async.
        /// </summary>
        /// <param name="topic">Topic.</param>
        /// <param name="action">Action.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        public override async Task BaseSubscribeAsync(string topic, Action<EasyCachingMessage> action, CancellationToken cancellationToken = default(CancellationToken))
        {
            await StartConsumer(topic);
        }

        /// <summary>
        /// Ons the consumer.
        /// </summary>
        /// <param name="topic">Topic</param>
        private Task StartConsumer(string topic)
        {
            return Task.Factory.StartNew(() =>
            {
                for (int i = 0; i < this._kafkaBusOptions.ConsumerCount; i++)
                {
                    using (var consumer = new ConsumerBuilder<Null, byte[]>(this._kafkaBusOptions.ConsumerConfig).Build())
                    {
                        consumer.Subscribe(topic);
                        try
                        {
                            while (true)
                            {
                                try
                                {
                                    var cr = consumer.Consume();
                                    if (cr.IsPartitionEOF
                                       || cr.Message == null
                                       || cr.Message.Value.Length == 0)
                                    {
                                        continue;
                                    }
                                    OnMessage(cr.Message.Value);
                                }
                                catch (ConsumeException ex)
                                {
                                    _logger.LogError(ex, "Consumer {0} error of reason {1}.", topic, ex.Error.Reason);
                                }
                                catch (OperationCanceledException)
                                {
                                    throw;
                                }
                                catch (Exception ex)
                                {
                                    _logger.LogError(ex, "Consumer {0} error.", topic);
                                }
                            }
                        }
                        catch (OperationCanceledException ex)
                        {
                            _logger.LogWarning(ex, "Consumer {0} cancel.", topic);
                            consumer.Close();
                        }
                    }
                }
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
    }
}
