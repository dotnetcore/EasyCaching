namespace Microsoft.Extensions.DependencyInjection
{
    using EasyCaching.Core;
    using EasyCaching.Core.Configurations;
    using EasyCaching.CSRedis;
    using EasyCaching.Redis;
    using Microsoft.Extensions.Configuration;
    using System;

    /// <summary>
    /// EasyCaching options extensions.
    /// </summary>
    public static class EasyCachingOptionsExtensions
    {
        /// <summary>
        /// Uses the CSRedis provider (specify the config via hard code).
        /// </summary>
        /// <param name="options">Options.</param>
        /// <param name="configure">Configure provider settings.</param>
        /// <param name="name">The name of this provider instance.</param>
        public static EasyCachingOptions UseCSRedis(
            this EasyCachingOptions options
            , Action<RedisOptions> configure
            , string name = EasyCachingConstValue.DefaultCSRedisName
            )
        {
            ArgumentCheck.NotNull(configure, nameof(configure));

            options.RegisterExtension(new RedisOptionsExtension(name, configure));
            return options;
        }

        /// <summary>
        /// Uses the CSRedis provider (read config from configuration file).
        /// </summary>
        /// <param name="options">Options.</param>
        /// <param name="configuration">The configuration.</param>
        /// <param name="name">The name of this provider instance.</param>
        /// <param name="sectionName">The section name in the configuration file.</param>
        public static EasyCachingOptions UseCSRedis(
            this EasyCachingOptions options
            , IConfiguration configuration
            , string name = EasyCachingConstValue.DefaultCSRedisName
            , string sectionName = EasyCachingConstValue.CSRedisSection
            )
        {
            var dbConfig = configuration.GetSection(sectionName);
            var redisOptions = new RedisOptions();
            dbConfig.Bind(redisOptions);

            void configure(RedisOptions x)
            {
                x.EnableLogging = redisOptions.EnableLogging;
                x.MaxRdSecond = redisOptions.MaxRdSecond;
                x.LockMs = redisOptions.LockMs;
                x.SleepMs = redisOptions.SleepMs;
                x.SerializerName = redisOptions.SerializerName;
                x.CacheNulls = redisOptions.CacheNulls;
                x.DBConfig = redisOptions.DBConfig;
            }

            options.RegisterExtension(new RedisOptionsExtension(name, configure));
            return options;
        }

        /// <summary>
        /// Uses the CSRedis lock.
        /// </summary>
        /// <param name="options">Options.</param>
        /// <param name="name">The name of this provider instance.</param>
        public static EasyCachingOptions UseCSRedisLock(this EasyCachingOptions options
            , string name = EasyCachingConstValue.DefaultCSRedisName)
        {
            options.RegisterExtension(new CSRedisLockExtension(name));

            return options;
        }
    }
}