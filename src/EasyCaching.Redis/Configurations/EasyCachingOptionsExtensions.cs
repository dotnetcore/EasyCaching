namespace Microsoft.Extensions.DependencyInjection
{
    using EasyCaching.Core;
    using EasyCaching.Core.Configurations;
    using EasyCaching.Core.Decoration;
    using EasyCaching.Redis;
    using Microsoft.Extensions.Configuration;
    using StackExchange.Redis;
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// EasyCaching options extensions.
    /// </summary>
    public static class EasyCachingOptionsExtensions
    {      
        private static readonly Func<Exception, bool> RedisExceptionFilter = exception =>
            exception is RedisException ||
            exception is RedisCommandException || // Derived not from RedisException
            exception is TimeoutException || // Can be thrown on timeout in Redis is some cases, RedisTimeoutException is derived from TimeoutException
            exception is TaskCanceledException; // Can be thrown while waiting for Redis response
        
        /// <summary>
        /// Uses the SERedis provider (specify the config via hard code).
        /// </summary>        
        /// <param name="options">Options.</param>
        /// <param name="configure">Configure provider settings.</param>
        /// <param name="name">The name of this provider instance.</param>
        public static EasyCachingOptions UseRedis(
            this EasyCachingOptions options
            , Action<RedisOptions> configure
            , string name = EasyCachingConstValue.DefaultRedisName
            )
        {
            ArgumentCheck.NotNull(configure, nameof(configure));

            options.RegisterExtension(new RedisOptionsExtension(name, configure));
            return options;
        }

        /// <summary>
        /// Uses the SERedis provider (read config from configuration file).
        /// </summary>        
        /// <param name="options">Options.</param>
        /// <param name="configuration">The configuration.</param>
        /// <param name="name">The name of this provider instance.</param>
        /// <param name="sectionName">The section name in the configuration file.</param>
        public static EasyCachingOptions UseRedis(
            this EasyCachingOptions options
            , IConfiguration configuration
            , string name = EasyCachingConstValue.DefaultRedisName
            , string sectionName = EasyCachingConstValue.RedisSection
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

        public static RedisOptions DecorateWithCircuitBreaker(
            this RedisOptions options,
            ICircuitBreakerParameters initParameters,
            ICircuitBreakerParameters executeParameters)
        {
            return (RedisOptions) options.DecorateWithCircuitBreaker(
                initParameters,
                executeParameters,
                RedisExceptionFilter);
        }

        public static RedisOptions DecorateWithFallback(
            this RedisOptions options,
            Func<string, IServiceProvider, IRedisAndEasyCachingProvider> fallbackCachingProviderFactory)
        {
            return (RedisOptions) options.DecorateWithFallback(
                fallbackCachingProviderFactory,
                RedisExceptionFilter);
        }
    }
}
