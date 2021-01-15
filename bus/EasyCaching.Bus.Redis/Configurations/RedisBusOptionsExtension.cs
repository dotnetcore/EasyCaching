namespace EasyCaching.Bus.Redis
{
    using System;
    using EasyCaching.Core.Bus;
    using EasyCaching.Core.Configurations;
    using EasyCaching.Core.Serialization;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;

    /// <summary>
    /// Redis bus options extension.
    /// </summary>
    internal sealed class RedisBusOptionsExtension : IEasyCachingOptionsExtension
    {
        /// <summary>
        /// The name.
        /// </summary>
        private readonly string _name;

        /// <summary>
        /// The configure.
        /// </summary>
        private readonly Action<RedisBusOptions> configure;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:EasyCaching.Bus.Redis.RedisBusOptionsExtension"/> class.
        /// </summary>
        /// <param name="name">Name.</param>
        /// <param name="configure">Configure.</param>
        public RedisBusOptionsExtension(string name, Action<RedisBusOptions> configure)
        {
            this._name = name;
            this.configure = configure;
        }

        /// <summary>
        /// Adds the services.
        /// </summary>
        /// <param name="services">Services.</param>
        public void AddServices(IServiceCollection services)
        {
            services.AddOptions();
            services.Configure(_name, configure);

            services.TryAddSingleton<IEasyCachingSerializer, DefaultBinaryFormatterSerializer>();
            services.AddSingleton<IRedisSubscriberProvider, RedisSubscriberProvider>(x =>
            {
                var optionsMon = x.GetRequiredService<IOptionsMonitor<RedisBusOptions>>();
                var options = optionsMon.Get(_name);
                return new RedisSubscriberProvider(_name, options);
            });

            services.AddSingleton<IEasyCachingBus, DefaultRedisBus>(x =>
            {
                var subProviders = x.GetServices<IRedisSubscriberProvider>();
                var serializers = x.GetServices<IEasyCachingSerializer>();
                var optionsMon = x.GetRequiredService<IOptionsMonitor<RedisBusOptions>>();
                var options = optionsMon.Get(_name);
                var factory = x.GetService<ILoggerFactory>();
                return new DefaultRedisBus(_name, subProviders, options, serializers);
            });
        }
    }
}
