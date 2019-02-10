namespace EasyCaching.CSRedis
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
        /// Uses the CSRedis.
        /// </summary>
        /// <returns>The CSR edis.</returns>
        /// <param name="options">Options.</param>
        /// <param name="configure">Configure.</param>
        /// <param name="name">Name.</param>
        public static EasyCachingOptions UseCSRedis(this EasyCachingOptions options, Action<RedisOptions> configure, string name = EasyCachingConstValue.DefaultCSRedisName)
        {
            ArgumentCheck.NotNull(configure, nameof(configure));

            options.RegisterExtension(new RedisOptionsExtension(name, configure));
            return options;
        }

        /// <summary>
        /// Uses the CSRedis.
        /// </summary>
        /// <returns>The CSR edis.</returns>
        /// <param name="options">Options.</param>
        /// <param name="configuration">Configuration.</param>
        /// <param name="name">Name.</param>
        /// <param name="sectionName">Section name.</param>
        public static EasyCachingOptions UseCSRedis(this EasyCachingOptions options, IConfiguration configuration, string name = "", string sectionName = EasyCachingConstValue.CSRedisSection)
        {
            var dbConfig = configuration.GetSection(sectionName);
            var redisOptions = new RedisOptions();
            dbConfig.Bind(redisOptions);

            void configure(RedisOptions x)
            {
                x.CachingProviderType = redisOptions.CachingProviderType;
                x.EnableLogging = redisOptions.EnableLogging;
                x.MaxRdSecond = redisOptions.MaxRdSecond;
                x.Order = redisOptions.Order;
                x.DBConfig = redisOptions.DBConfig;
            }

            options.RegisterExtension(new RedisOptionsExtension(name, configure));
            return options;
        }
    }
}