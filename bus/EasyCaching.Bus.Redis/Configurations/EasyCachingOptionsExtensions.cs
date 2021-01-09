namespace Microsoft.Extensions.DependencyInjection
{
    using System;
    using EasyCaching.Bus.Redis;
    using EasyCaching.Core;
    using EasyCaching.Core.Configurations;
    using Microsoft.Extensions.Configuration;

    /// <summary>
    /// EasyCaching options extensions.
    /// </summary>
    public static class EasyCachingOptionsExtensions
    {
        /// <summary>
        /// Withs the SERedis bus (specify the config via hard code).
        /// </summary>
        /// <param name="options">Options.</param>
        /// <param name="configure">Configure bus settings.</param>
        /// <param name="name">name.</param>
        public static EasyCachingOptions WithRedisBus(
            this EasyCachingOptions options
            , Action<RedisBusOptions> configure
            , string name = EasyCachingConstValue.DefaultRedisName
            )
        {
            ArgumentCheck.NotNull(configure, nameof(configure));

            options.RegisterExtension(new RedisBusOptionsExtension(name, configure));
            return options;
        }

        /// <summary>
        /// Withs the SERedis bus (read config from configuration file).
        /// </summary>
        /// <param name="options">Options.</param>
        /// <param name="configuration">The configuration.</param>
        /// <param name="name">The name.</param>
        /// <param name="sectionName">The section name in the configuration file.</param>
        public static EasyCachingOptions WithRedisBus(
            this EasyCachingOptions options
            , IConfiguration configuration
            , string name = EasyCachingConstValue.DefaultRedisName
            , string sectionName = EasyCachingConstValue.RedisBusSection
            )
        {
            var dbConfig = configuration.GetSection(sectionName);
            var redisOptions = new RedisBusOptions();
            dbConfig.Bind(redisOptions);

            void configure(RedisBusOptions x)
            {
                x.AbortOnConnectFail = redisOptions.AbortOnConnectFail;
                x.AllowAdmin = redisOptions.AllowAdmin;
                x.Configuration = redisOptions.Configuration;
                x.ConnectionTimeout = redisOptions.ConnectionTimeout;
                x.Database = redisOptions.Database;
                x.IsSsl = redisOptions.IsSsl;
                x.Username = redisOptions.Username;
                x.Password = redisOptions.Password;
                x.SslHost = redisOptions.SslHost;

                foreach (var item in redisOptions.Endpoints) x.Endpoints.Add(item);
            }

            options.RegisterExtension(new RedisBusOptionsExtension(name, configure));
            return options;
        }
    }
}
