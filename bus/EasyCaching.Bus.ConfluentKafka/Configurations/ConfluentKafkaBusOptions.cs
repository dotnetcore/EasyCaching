using Confluent.Kafka;

namespace EasyCaching.Bus.ConfluentKafka
{
    /// <summary>
    /// kafka bus options
    /// </summary>
    public class ConfluentKafkaBusOptions
    {
        /// <summary>
        ///  kafka address(BootstrapServers must)
        /// </summary>
        public string BootstrapServers { get; set; }


        /// <summary>
        /// kafka bus producer options.
        /// </summary>
        public ProducerConfig ProducerConfig { get; set; }

        /// <summary>
        /// kafka bus consumer options.(if GroupId value below is empty,then ConsumerConfig.GroupId must )
        /// </summary>
        public ConsumerConfig ConsumerConfig { get; set; }

        /// <summary>
        /// kafka bus consumer options with consumer groupId
        /// (if ConsumerConfig below has give GroupId value , this options can ignore)
        /// </summary>
        public string GroupId { get; set; }

        /// <summary>
        /// kafka bus consumer  consume count
        /// </summary>
        public int ConsumerCount { get; set; } = 1;
    }
  
}
