namespace EasyCaching.Redis
{
    using EasyCaching.Core;
    using EasyCaching.Core.Configurations;
    using EasyCaching.Core.Decoration;
    using EasyCaching.Core.Serialization;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using System;
    using System.Linq;

    /// <summary>
    /// Redis options extension.
    /// </summary>
    internal sealed class RedisOptionsExtension : IEasyCachingOptionsExtension
    {
        /// <summary>
        /// The name.
        /// </summary>
        private readonly string _name;

        /// <summary>
        /// The configure.
        /// </summary>
        private readonly Action<RedisOptions> configure;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:EasyCaching.Redis.RedisOptionsExtension"/> class.
        /// </summary>
        /// <param name="name">Name.</param>
        /// <param name="configure">Configure.</param>
        public RedisOptionsExtension(string name, Action<RedisOptions> configure)
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

            services.TryAddSingleton<IEasyCachingSerializer, DefaultBinaryFormatterSerializer>();

            services.Configure(_name, configure);

            services.TryAddSingleton<IEasyCachingProviderFactory, DefaultEasyCachingProviderFactory>();
            services.AddSingleton<IRedisDatabaseProvider, RedisDatabaseProvider>(x =>
            {
                var optionsMon = x.GetRequiredService<IOptionsMonitor<RedisOptions>>();
                var options = optionsMon.Get(_name);
                return new RedisDatabaseProvider(_name, options);
            });

            services.AddSingleton<IRedisAndEasyCachingProvider>(serviceProvider =>
            {
                var dbProviders = serviceProvider.GetServices<IRedisDatabaseProvider>();
                var serializers = serviceProvider.GetServices<IEasyCachingSerializer>();
                var optionsMon = serviceProvider.GetRequiredService<IOptionsMonitor<RedisOptions>>();
                var options = optionsMon.Get(_name);
                var factory = serviceProvider.GetService<ILoggerFactory>();

                return options.CreateDecoratedProvider(
                    _name,
                    serviceProvider,
                    () => new DefaultRedisCachingProvider(_name, dbProviders, serializers, options, factory));
            });
            services.AddSingleton<IEasyCachingProvider>(GetProviderByName);
            services.AddSingleton<IRedisCachingProvider>(GetProviderByName);
        }

        private IRedisAndEasyCachingProvider GetProviderByName(IServiceProvider serviceProvider) =>
            serviceProvider.GetServices<IRedisAndEasyCachingProvider>().Single(provider => provider.Name == _name);
    }
}
