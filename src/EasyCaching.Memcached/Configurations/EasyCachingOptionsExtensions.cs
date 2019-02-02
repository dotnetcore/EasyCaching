namespace EasyCaching.Memcached
{
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
        /// Uses the memcached.
        /// </summary>
        /// <returns>The memcached.</returns>
        /// <param name="options">Options.</param>
        /// <param name="configure">Configure.</param>
        /// <param name="name">Name.</param>
        public static EasyCachingOptions UseMemcached(this EasyCachingOptions options, Action<MemcachedOptions> configure, string name = "")
        {
            if (configure == null)
            {
                throw new ArgumentNullException(nameof(configure));
            }

            options.RegisterExtension(new MemcachedOptionsExtension(name, configure));
            return options;
        }

        /// <summary>
        /// Uses the memcached.
        /// </summary>
        /// <returns>The memcached.</returns>
        /// <param name="options">Options.</param>
        /// <param name="configuration">Configuration.</param>
        /// <param name="name">Name.</param>
        /// <param name="sectionName">Section name.</param>
        public static EasyCachingOptions UseMemcached(this EasyCachingOptions options, IConfiguration configuration, string name = "", string sectionName = EasyCachingConstValue.MemcachedSection)
        {
            var dbConfig = configuration.GetSection(sectionName);
            var mOptions = new MemcachedOptions();
            dbConfig.Bind(mOptions);

            void configure(MemcachedOptions x)
            {
                x.CachingProviderType = mOptions.CachingProviderType;
                x.EnableLogging = mOptions.EnableLogging;
                x.MaxRdSecond = mOptions.MaxRdSecond;
                x.Order = mOptions.Order;
                x.DBConfig = mOptions.DBConfig;
            }

            options.RegisterExtension(new MemcachedOptionsExtension(name, configure));
            return options;
        }
    }
}
