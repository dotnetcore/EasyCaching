namespace EasyCaching.Bus.CSRedis
{
    using System;
    using System.Linq;
    using EasyCaching.Core.Bus;
    using EasyCaching.Core.Decoration;
    using EasyCaching.Core.Configurations;
    using EasyCaching.Core.Serialization;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;
    using Microsoft.Extensions.Options;

    /// <summary>
    /// Redis options extension.
    /// </summary>
    internal sealed class CSRedisOptionsExtension : IEasyCachingOptionsExtension
    {
        /// <summary>
        /// The name.
        /// </summary>
        private readonly string _name;

        /// <summary>
        /// The configure.
        /// </summary>
        private readonly Action<CSRedisBusOptions> _configure;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:EasyCaching.CSRedis.RedisOptionsExtension"/> class.
        /// </summary>        
        /// <param name="name">Unique name of the bus.</param>
        /// <param name="configure">Configure.</param>
        public CSRedisOptionsExtension(string name, Action<CSRedisBusOptions> configure)
        {
            _name = name;
            _configure = configure;
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

            services.AddSingleton<EasyCachingCSRedisClient>(x =>
            {
                var optionsMon = x.GetRequiredService<IOptionsMonitor<CSRedisBusOptions>>();
                var options = optionsMon.Get(_name);

                var conns = options.ConnectionStrings;
                var rule = options.NodeRule;
                var sentinels = options.Sentinels;
                var readOnly = options.ReadOnly;

                if (sentinels != null && sentinels.Any())
                {
                    var redisClient = new EasyCachingCSRedisClient(_name, conns[0], sentinels.ToArray(), readOnly);
                    return redisClient;
                }

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

            services.AddSingleton<IEasyCachingBus>(serviceProvider =>
            {
                var optionsMon = serviceProvider.GetRequiredService<IOptionsMonitor<CSRedisBusOptions>>();
                var options = optionsMon.Get(_name);
                var clients = serviceProvider.GetServices<EasyCachingCSRedisClient>();
                
                return options.CreateDecoratedBus(
                    _name,
                    serviceProvider,
                    () => new DefaultCSRedisBus(_name, clients));
            });
        }
    }
}
