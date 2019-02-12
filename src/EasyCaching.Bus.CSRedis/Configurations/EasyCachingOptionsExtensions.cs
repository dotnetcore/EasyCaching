namespace EasyCaching.Bus.CSRedis
{
    using System;
    using EasyCaching.Core;
    using EasyCaching.Core.Configurations;
    using Microsoft.Extensions.Configuration;

    /// <summary>
    /// EasyCaching options extensions.
    /// </summary>
    public static class EasyCachingOptionsExtensions
    {
        /// <summary>
        /// Withs the redis bus.
        /// </summary>
        /// <returns>The redis bus.</returns>
        /// <param name="options">Options.</param>
        /// <param name="configure">Configure.</param>
        public static EasyCachingOptions WithCSRedisBus(this EasyCachingOptions options, Action<CSRedisBusOptions> configure)
        {
            if (configure == null)
            {
                throw new ArgumentNullException(nameof(configure));
            }

            options.RegisterExtension(new CSRedisOptionsExtension(configure));
            return options;
        }

        /// <summary>
        /// Withs the redis bus.
        /// </summary>
        /// <returns>The redis bus.</returns>
        /// <param name="options">Options.</param>
        /// <param name="configuration">Configuration.</param>
        /// <param name="sectionName">Section name.</param>
        public static EasyCachingOptions WithCSRedisBus(this EasyCachingOptions options, IConfiguration configuration, string sectionName = EasyCachingConstValue.RedisBusSection)
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
