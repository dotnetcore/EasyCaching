namespace EasyCaching.Bus.Redis
{
    using System;
    using EasyCaching.Core;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;

    internal sealed class RedisBusOptionsExtension : IEasyCachingOptionsExtension
    {
        private readonly Action<RedisBusOptions> configure;

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
