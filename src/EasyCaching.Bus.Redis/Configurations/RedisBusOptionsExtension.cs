namespace EasyCaching.Bus.Redis
{
    using System;
    using EasyCaching.Core.Bus;
    using EasyCaching.Core.Configurations;
    using EasyCaching.Core.Serialization;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;

    /// <summary>
    /// Redis bus options extension.
    /// </summary>
    internal sealed class RedisBusOptionsExtension : IEasyCachingOptionsExtension
    {
        /// <summary>
        /// The configure.
        /// </summary>
        private readonly Action<RedisBusOptions> configure;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:EasyCaching.Bus.Redis.RedisBusOptionsExtension"/> class.
        /// </summary>
        /// <param name="configure">Configure.</param>
        public RedisBusOptionsExtension(Action<RedisBusOptions> configure)
        {         
            this.configure = configure;
        }

        /// <summary>
        /// Adds the services.
        /// </summary>
        /// <param name="services">Services.</param>
        public void AddServices(IServiceCollection services)
        {
            services.AddOptions();
            services.Configure(configure);

            services.TryAddSingleton<IEasyCachingSerializer, DefaultBinaryFormatterSerializer>();
            services.TryAddSingleton<IRedisSubscriberProvider, RedisSubscriberProvider>();
            services.AddSingleton<IEasyCachingBus, DefaultRedisBus>();
        }

        /// <summary>
        /// Withs the services.
        /// </summary>
        /// <param name="services">Services.</param>
        public void WithServices(IApplicationBuilder services)
        {
            // Method intentionally left empty.
        }
    }
}
