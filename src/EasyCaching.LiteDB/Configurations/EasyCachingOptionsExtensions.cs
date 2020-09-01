namespace Microsoft.Extensions.DependencyInjection
{
    using System;
    using EasyCaching.Core;
    using EasyCaching.Core.Configurations;
    using EasyCaching.LiteDB;
    using Microsoft.Extensions.Configuration;

    /// <summary>
    /// EasyCaching options extensions.
    /// </summary>
    public static class EasyCachingOptionsExtensions
    {
        /// <summary>
        /// Uses the SQLite provider (specify the config via hard code).
        /// </summary>
        /// <param name="options">Options.</param>
        /// <param name="configure">Configure provider settings.</param>
        /// <param name="name">The name of this provider instance.</param>
        public static EasyCachingOptions UseLiteDB(
            this EasyCachingOptions options
            , Action<LiteDBOptions> configure
            , string name = EasyCachingConstValue.DefaultLiteDBName
            )
        {
            ArgumentCheck.NotNull(configure, nameof(configure));

            options.RegisterExtension(new LiteDBOptionsExtension(name, configure));
            return options;
        }

        /// <summary>
        /// Uses the SQLite provider (read config from configuration file).
        /// </summary>
        /// <param name="options">Options.</param>
        /// <param name="configuration">The configuration.</param>
        /// <param name="name">The name of this provider instance.</param>
        /// <param name="sectionName">The section name in the configuration file.</param>
        public static EasyCachingOptions UseLiteDB(
            this EasyCachingOptions options
            , IConfiguration configuration
            , string name = EasyCachingConstValue.DefaultLiteDBName
            , string sectionName = EasyCachingConstValue.LiteDBSection
            )
        {
            var dbConfig = configuration.GetSection(sectionName);
            var sqliteOptions = new LiteDBOptions();
            dbConfig.Bind(sqliteOptions);

            void configure(LiteDBOptions x)
            {
                x.EnableLogging = sqliteOptions.EnableLogging;
                x.MaxRdSecond = sqliteOptions.MaxRdSecond;             
                x.LockMs = sqliteOptions.LockMs;
                x.SleepMs = sqliteOptions.SleepMs;
                x.SerializerName = sqliteOptions.SerializerName;
                x.DBConfig = sqliteOptions.DBConfig;
            }

            options.RegisterExtension(new LiteDBOptionsExtension(name, configure));
            return options;
        }
    }
}
