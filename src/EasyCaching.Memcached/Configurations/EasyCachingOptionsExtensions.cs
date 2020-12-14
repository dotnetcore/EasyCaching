namespace Microsoft.Extensions.DependencyInjection
{
    using EasyCaching.Core;
    using EasyCaching.Core.Configurations;
    using EasyCaching.Memcached;
    using Microsoft.Extensions.Configuration;
    using System;

    /// <summary>
    /// EasyCaching options extensions.
    /// </summary>
    public static class EasyCachingOptionsExtensions
    {
        /// <summary>
        /// Uses the memcached provider (specify the config via hard code).
        /// </summary>        
        /// <param name="options">Options.</param>
        /// <param name="configure">Configure provider settings.</param>
        /// <param name="name">The name of this provider instance.</param>
        public static EasyCachingOptions UseMemcached(
            this EasyCachingOptions options
            , Action<MemcachedOptions> configure
            , string name = EasyCachingConstValue.DefaultMemcachedName
            )
        {
            ArgumentCheck.NotNull(configure, nameof(configure));

            options.RegisterExtension(new MemcachedOptionsExtension(name, configure));
            return options;
        }

        /// <summary>
        /// Uses the memcached provider (read config from configuration file).
        /// </summary>
        /// <param name="options">Options.</param>
        /// <param name="configuration">The configuration.</param>
        /// <param name="name">The name of this provider instance.</param>
        /// <param name="sectionName">The section name in the configuration file.</param>
        public static EasyCachingOptions UseMemcached(
            this EasyCachingOptions options
            , IConfiguration configuration
            , string name = EasyCachingConstValue.DefaultMemcachedName
            , string sectionName = EasyCachingConstValue.MemcachedSection
            )
        {
            var dbConfig = configuration.GetSection(sectionName);
            var mOptions = new MemcachedOptions();
            dbConfig.Bind(mOptions);

            void configure(MemcachedOptions x)
            {
                x.EnableLogging = mOptions.EnableLogging;
                x.MaxRdSecond = mOptions.MaxRdSecond;
                x.LockMs = mOptions.LockMs;
                x.SleepMs = mOptions.SleepMs;
                x.SerializerName = mOptions.SerializerName;
                x.CacheNulls = mOptions.CacheNulls;
                x.DBConfig = mOptions.DBConfig;
            }

            options.RegisterExtension(new MemcachedOptionsExtension(name, configure));
            return options;
        }
    }
}
