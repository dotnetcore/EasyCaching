namespace EasyCaching.Redis
{
    using EasyCaching.Core;
    using Microsoft.Extensions.Configuration;
    using System;

    /// <summary>
    /// EasyCaching options extensions.
    /// </summary>
    public static class EasyCachingOptionsExtensions
    {
        /// <summary>
        /// Uses the redis.
        /// </summary>
        /// <returns>The redis.</returns>
        /// <param name="options">Options.</param>
        /// <param name="configure">Configure.</param>
        /// <param name="name">Name.</param>
        public static EasyCachingOptions UseRedis(this EasyCachingOptions options, Action<RedisOptions> configure, string name = "")
        {
            if (configure == null)
            {
                throw new ArgumentNullException(nameof(configure));
            }

            options.RegisterExtension(new RedisOptionsExtension(name, configure));
            return options;
        }

        /// <summary>
        /// Uses the redis.
        /// </summary>
        /// <returns>The redis.</returns>
        /// <param name="options">Options.</param>
        /// <param name="configuration">Configuration.</param>
        /// <param name="name">Name.</param>
        public static EasyCachingOptions UseRedis(this EasyCachingOptions options, IConfiguration configuration, string name = "")
        {
            var dbConfig = configuration.GetSection(EasyCachingConstValue.InMemorySection);
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
