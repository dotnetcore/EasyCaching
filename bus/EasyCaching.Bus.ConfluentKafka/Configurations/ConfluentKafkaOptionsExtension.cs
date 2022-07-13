namespace EasyCaching.Bus.ConfluentKafka
{
    using System;
    using EasyCaching.Core.Bus;
    using EasyCaching.Core.Configurations;
    using EasyCaching.Core.Serialization;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;
    using Microsoft.Extensions.Options;

    /// <summary>
    /// Kafka options extension.
    /// </summary>
    internal sealed class ConfluentKafkaOptionsExtension : IEasyCachingOptionsExtension
    {

        private readonly Action<ConfluentKafkaBusOptions> _confluentKafkaBusOptions;

        public ConfluentKafkaOptionsExtension(Action<ConfluentKafkaBusOptions> confluentKafkaBusOptions)
        {
            this._confluentKafkaBusOptions = confluentKafkaBusOptions;
        }

        /// <summary>
        /// Adds the services.
        /// </summary>
        /// <param name="services">Services.</param>
        public void AddServices(IServiceCollection services)
        {
            services.AddOptions();

            services.TryAddSingleton<IEasyCachingSerializer, DefaultBinaryFormatterSerializer>();

            services.AddOptions<ConfluentKafkaBusOptions>()
                .Configure(_confluentKafkaBusOptions);


            var options = services.BuildServiceProvider()
                 .GetRequiredService<IOptions<ConfluentKafkaBusOptions>>()
                 .Value;

            services.AddSingleton<IEasyCachingBus, DefaultConfluentKafkaBus>();

        }
    }
}
