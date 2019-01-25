namespace EasyCaching.CSRedis
{
    using EasyCaching.Core;
    using Microsoft.AspNetCore.Builder;
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
        private readonly Action<RedisOptions> configure;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:EasyCaching.CSRedis.RedisOptionsExtension"/> class.
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

            if (string.IsNullOrWhiteSpace(_name))
            {
                var options = new RedisOptions();
                configure(options);

                services.TryAddSingleton<IRedisDatabaseProvider, RedisDatabaseProvider>();
                services.AddSingleton<IEasyCachingProvider, DefaultCSRedisCachingProvider>();
            }
            else
            {
                services.Configure(_name, configure);

                services.AddSingleton<IEasyCachingProviderFactory, DefaultEasyCachingProviderFactory>();
                services.AddSingleton<IRedisDatabaseProvider, RedisDatabaseProvider>(x =>
                {
                    var optionsMon = x.GetRequiredService<IOptionsMonitor<RedisOptions>>();
                    var options = optionsMon.Get(_name);
                    return new RedisDatabaseProvider(_name, options);
                });

                services.AddSingleton<IEasyCachingProvider, DefaultCSRedisCachingProvider>(x =>
                {
                    var dbProviders = x.GetServices<IRedisDatabaseProvider>();
                    var serializer = x.GetRequiredService<IEasyCachingSerializer>();
                    var options = x.GetRequiredService<IOptionsMonitor<RedisOptions>>();
                    var factory = x.GetService<ILoggerFactory>();
                    return new DefaultCSRedisCachingProvider(_name, dbProviders, serializer, options, factory);
                });
            }
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
