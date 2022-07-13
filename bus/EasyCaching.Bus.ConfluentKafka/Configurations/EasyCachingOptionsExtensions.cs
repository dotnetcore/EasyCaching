namespace Microsoft.Extensions.DependencyInjection
{
    using EasyCaching.Bus.ConfluentKafka;
    using EasyCaching.Core;
    using EasyCaching.Core.Configurations;
    using Microsoft.Extensions.Configuration;
    using System;

    /// <summary>
    /// EasyCaching options extensions.
    /// </summary>
    public static class EasyCachingOptionsExtensions 
    {
        /// <summary>
        /// Withs the ConfluentKafka bus (specify the config via hard code).
        /// </summary>
        /// <param name="options"></param>
        /// <param name="configure"></param>
        /// <returns></returns>
        public static EasyCachingOptions WithConfluentKafkaBus(
            this EasyCachingOptions options
            , Action<ConfluentKafkaBusOptions> configure
            )
        {
            ArgumentCheck.NotNull(configure, nameof(configure));
            //option convert
            ConfluentKafkaBusOptions kafkaOptions = new ConfluentKafkaBusOptions();
            configure.Invoke(kafkaOptions);
            void kafkaBusConfigure(ConfluentKafkaBusOptions x)
            {
                x.BootstrapServers = kafkaOptions.BootstrapServers;
                x.ProducerConfig = kafkaOptions.ProducerConfig ?? new Confluent.Kafka.ProducerConfig();
                x.ConsumerConfig = kafkaOptions.ConsumerConfig ?? new Confluent.Kafka.ConsumerConfig();
                //address
                x.ProducerConfig.BootstrapServers = x.ProducerConfig.BootstrapServers ?? kafkaOptions.BootstrapServers;
                x.ConsumerConfig.BootstrapServers = x.ConsumerConfig.BootstrapServers ?? kafkaOptions.BootstrapServers;
                //consumer groupId
                x.ConsumerConfig.GroupId = x.ConsumerConfig.GroupId ?? kafkaOptions.GroupId;
            }

            options.RegisterExtension(new ConfluentKafkaOptionsExtension(kafkaBusConfigure));
            return options;
        }

        /// <summary>
        /// Withs the ConfluentKafka bus (read config from configuration file).
        /// </summary>
        /// <param name="options"></param>
        /// <param name="configuration"></param>
        /// <param name="sectionName">The section name in the configuration file.</param>
        /// <returns></returns>
        public static EasyCachingOptions WithConfluentKafkaBus(
            this EasyCachingOptions options
            , IConfiguration configuration
            , string sectionName = EasyCachingConstValue.KafkaBusSection
            )
        {
            var dbConfig = configuration.GetSection(sectionName);
            var kafkaOptions = new ConfluentKafkaBusOptions();
            dbConfig.Bind(kafkaOptions);

            void configure(ConfluentKafkaBusOptions x)
            {
                x.BootstrapServers = kafkaOptions.BootstrapServers;
                x.ProducerConfig = kafkaOptions.ProducerConfig ?? new Confluent.Kafka.ProducerConfig();
                x.ConsumerConfig = kafkaOptions.ConsumerConfig ?? new Confluent.Kafka.ConsumerConfig();
                //address
                x.ProducerConfig.BootstrapServers = x.ProducerConfig.BootstrapServers ?? kafkaOptions.BootstrapServers;
                x.ConsumerConfig.BootstrapServers = x.ConsumerConfig.BootstrapServers ?? kafkaOptions.BootstrapServers;
                //consumer groupId
                x.ConsumerConfig.GroupId = x.ConsumerConfig.GroupId ?? kafkaOptions.GroupId;
            }

            options.RegisterExtension(new ConfluentKafkaOptionsExtension(configure));
            return options;
        }
    }
}
