namespace EasyCaching.Bus.Zookeeper
{
    using EasyCaching.Core.Bus;
    using EasyCaching.Core.Configurations;
    using EasyCaching.Core.Serialization;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;
    using System;

    /// <summary>
    /// Zookeeper options extension.
    /// </summary>
    internal sealed class ZookeeperOptionsExtension : IEasyCachingOptionsExtension
    {
        private readonly Action<ZkBusOptions> _zkBusOptions;

        public ZookeeperOptionsExtension(Action<ZkBusOptions> zkBusOptions)
        {
            this._zkBusOptions = zkBusOptions;
        }

        /// <summary>
        /// Adds the services.
        /// </summary>
        /// <param name="services">Services.</param>
        public void AddServices(IServiceCollection services)
        {
            services.AddOptions();

            services.TryAddSingleton<IEasyCachingSerializer, DefaultBinaryFormatterSerializer>();

            services.AddOptions<ZkBusOptions>()
                .Configure(_zkBusOptions);

            //var options = services.BuildServiceProvider()
            //     .GetRequiredService<IOptions<ZkBusOptions>>()
            //     .Value;

            services.AddSingleton<IEasyCachingBus, DefaultZookeeperBus>();
        }
    }
}