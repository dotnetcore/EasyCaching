namespace Microsoft.Extensions.DependencyInjection
{
    using EasyCaching.Core;
    using EasyCaching.Core.Configurations;
    using EasyCaching.InMemory;
    using Microsoft.Extensions.Configuration;
    using System;

    /// <summary>
    /// EasyCaching options extensions of InMemory.
    /// </summary>
    public static class EasyCachingOptionsExtensions
    {
        /// <summary>
        /// Uses the in-memory provider.
        /// </summary>        
        /// <param name="options">Options.</param>
        /// <param name="name">The name of this provider instance.</param>
        public static EasyCachingOptions UseInMemory(
            this EasyCachingOptions options
            , string name = EasyCachingConstValue.DefaultInMemoryName
            )
        {
            var option = new InMemoryOptions();

            void configure(InMemoryOptions x)
            {
                x.EnableLogging = option.EnableLogging;
                x.MaxRdSecond = option.MaxRdSecond;
                x.DBConfig = option.DBConfig;
            }

            return options.UseInMemory(configure, name);
        }

        /// <summary>
        /// Uses the in-memory provider (specify the config via hard code).
        /// </summary>        
        /// <param name="options">Options.</param>
        /// <param name="configure">Configure provider settings.</param>
        /// <param name="name">The name of this provider instance.</param>
        public static EasyCachingOptions UseInMemory(
            this EasyCachingOptions options
            , Action<InMemoryOptions> configure
            , string name = EasyCachingConstValue.DefaultInMemoryName
            )
        {
            if (configure == null)
            {
                throw new ArgumentNullException(nameof(configure));
            }

            options.RegisterExtension(new InMemoryOptionsExtension(name, configure));

            return options;
        }

        /// <summary>
        /// Uses the in-memory provider (read config from configuration file).
        /// </summary>        
        /// <param name="options">Options.</param>
        /// <param name="configuration">The configuration.</param>
        /// <param name="name">The name of this provider instance.</param>
        /// <param name="sectionName">The section name in the configuration file.</param>
        public static EasyCachingOptions UseInMemory(
            this EasyCachingOptions options
            , IConfiguration configuration
            , string name = EasyCachingConstValue.DefaultInMemoryName
            , string sectionName = EasyCachingConstValue.InMemorySection
            )
        {
            var dbConfig = configuration.GetSection(sectionName);
            var memoryOptions = new InMemoryOptions();
            dbConfig.Bind(memoryOptions);

            void configure(InMemoryOptions x)
            {
                x.EnableLogging = memoryOptions.EnableLogging;
                x.MaxRdSecond = memoryOptions.MaxRdSecond;
                x.LockMs = memoryOptions.LockMs;
                x.SleepMs = memoryOptions.SleepMs;
                x.SerializerName = memoryOptions.SerializerName;
                x.CacheNulls = memoryOptions.CacheNulls;
                x.DBConfig = memoryOptions.DBConfig;
            }
            return options.UseInMemory(configure,name);
        }
    }
}
