namespace EasyCaching.Bus.CSRedis
{
    using EasyCaching.Core;
    using EasyCaching.Core.Configurations;
    using EasyCaching.Decoration.Polly;
    using global::CSRedis;
    using Microsoft.Extensions.Configuration;
    using System;

    /// <summary>
    /// EasyCaching options extensions.
    /// </summary>
    public static class EasyCachingOptionsExtensions
    {
        private static readonly Func<Exception, bool> RedisExceptionFilter = exception => exception is RedisClientException;
        
        /// <summary>
        /// Withs the CSRedis bus (specify the config via hard code).
        /// </summary>
        /// <param name="options">Options.</param>
        /// <param name="configure">Configure bus settings.</param>
        /// <param name="name">Unique name of the bus.</param>
        public static EasyCachingOptions WithCSRedisBus(
            this EasyCachingOptions options
            , Action<CSRedisBusOptions> configure
            , string name = "easycachingbus")
        {
            ArgumentCheck.NotNull(configure, nameof(configure));

            options.RegisterExtension(new CSRedisOptionsExtension(name, configure));
            return options;
        }

        /// <summary>
        /// Withs the CSRedis bus (read config from configuration file).
        /// </summary>
        /// <param name="options">Options.</param>
        /// <param name="configuration">The configuration.</param>
        /// <param name="name">Unique name of the bus.</param>
        /// <param name="sectionName">The section name in the configuration file.</param>
        public static EasyCachingOptions WithCSRedisBus(
            this EasyCachingOptions options
            , IConfiguration configuration
            , string name = "easycachingbus"
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

            options.RegisterExtension(new CSRedisOptionsExtension(name, configure));
            return options;
        }
        
        public static CSRedisBusOptions DecorateWithRetry(
            this CSRedisBusOptions options,
            int retryCount)
        {
            return (CSRedisBusOptions) options.DecorateWithRetry(retryCount, RedisExceptionFilter);
        }

        public static CSRedisBusOptions DecorateWithPublishFallback(this CSRedisBusOptions options)
        {
            return (CSRedisBusOptions) options.DecorateWithPublishFallback(RedisExceptionFilter);
        }

        public static CSRedisBusOptions DecorateWithCircuitBreaker(
            this CSRedisBusOptions options,
            ICircuitBreakerParameters initParameters,
            ICircuitBreakerParameters executeParameters,
            TimeSpan subscribeRetryInterval)
        {
            return (CSRedisBusOptions) options.DecorateWithCircuitBreaker(
                initParameters,
                executeParameters,
                subscribeRetryInterval,
                RedisExceptionFilter);
        }
    }
}
