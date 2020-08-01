namespace Microsoft.Extensions.DependencyInjection
{
    using EasyCaching.Bus.CSRedis;
    using EasyCaching.Core;
    using EasyCaching.Core.Configurations;
    using Microsoft.Extensions.Configuration;
    using System;

    /// <summary>
    /// EasyCaching options extensions.
    /// </summary>
    public static class EasyCachingOptionsExtensions
    {
        /// <summary>
        /// Withs the CSRedis bus (specify the config via hard code).
        /// </summary>
        /// <param name="options">Options.</param>
        /// <param name="configure">Configure bus settings.</param>
        public static EasyCachingOptions WithCSRedisBus(
            this EasyCachingOptions options
            , Action<CSRedisBusOptions> configure
            )
        {
            ArgumentCheck.NotNull(configure, nameof(configure));

            options.RegisterExtension(new CSRedisOptionsExtension(configure));
            return options;
        }

        /// <summary>
        /// Withs the CSRedis bus (read config from configuration file).
        /// </summary>
        /// <param name="options">Options.</param>
        /// <param name="configuration">The configuration.</param>
        /// <param name="sectionName">The section name in the configuration file.</param>
        public static EasyCachingOptions WithCSRedisBus(
            this EasyCachingOptions options
            , IConfiguration configuration
            , string sectionName = EasyCachingConstValue.RedisBusSection
            )
        {
            var dbConfig = configuration.GetSection(sectionName);
            var redisOptions = new CSRedisBusOptions();
            dbConfig.Bind(redisOptions);

            void configure(CSRedisBusOptions x)
            {
                x.ConnectionStrings = redisOptions.ConnectionStrings;
            }

            options.RegisterExtension(new CSRedisOptionsExtension(configure));
            return options;
        }
    }
}
