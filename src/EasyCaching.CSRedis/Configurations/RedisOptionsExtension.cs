namespace EasyCaching.CSRedis
{
    using EasyCaching.Core;
    using EasyCaching.Core.Configurations;
    using EasyCaching.Core.Serialization;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using System;

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
        private readonly Action<RedisOptions> _configure;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:EasyCaching.CSRedis.RedisOptionsExtension"/> class.
        /// </summary>
        /// <param name="name">Name.</param>
        /// <param name="configure">Configure.</param>
        public RedisOptionsExtension(string name, Action<RedisOptions> configure)
        {
            ArgumentCheck.NotNullOrWhiteSpace(name, nameof(name));

            this._name = name;
            this._configure = configure;
        }

        /// <summary>
        /// Adds the services.
        /// </summary>
        /// <param name="services">Services.</param>
        public void AddServices(IServiceCollection services)
        {
            services.AddOptions();

            services.TryAddSingleton<IEasyCachingSerializer, DefaultBinaryFormatterSerializer>();

            services.Configure(_name, _configure);

            services.TryAddSingleton<IEasyCachingProviderFactory, DefaultEasyCachingProviderFactory>();

            services.AddSingleton<EasyCachingCSRedisClient>(x =>
            {
                var optionsMon = x.GetRequiredService<IOptionsMonitor<RedisOptions>>();
                var options = optionsMon.Get(_name);

                var conns = options.DBConfig.ConnectionStrings;
                var rule = options.DBConfig.NodeRule;

                if (conns.Count == 1)
                {
                    var redisClient = new EasyCachingCSRedisClient(_name, conns[0]);
                    return redisClient;
                }
                else
                {
                    var redisClient = new EasyCachingCSRedisClient(_name, rule, conns.ToArray());
                    return redisClient;
                }
            });

            Func<IServiceProvider, DefaultCSRedisCachingProvider> createFactory = x =>
            {
                var clients = x.GetServices<EasyCachingCSRedisClient>();
                var serializers = x.GetServices<IEasyCachingSerializer>();
                var optionsMon = x.GetRequiredService<IOptionsMonitor<RedisOptions>>();
                var options = optionsMon.Get(_name);
                var factory = x.GetService<ILoggerFactory>();
                return new DefaultCSRedisCachingProvider(_name, clients, serializers, options, factory);
            };

            services.AddSingleton<IEasyCachingProvider, DefaultCSRedisCachingProvider>(createFactory);
            services.AddSingleton<IRedisCachingProvider, DefaultCSRedisCachingProvider>(createFactory);
        }
    }
}
