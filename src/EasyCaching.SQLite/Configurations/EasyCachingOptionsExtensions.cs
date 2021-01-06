namespace Microsoft.Extensions.DependencyInjection
{
    using System;
    using EasyCaching.Core;
    using EasyCaching.Core.Configurations;
    using EasyCaching.SQLite;
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
        public static EasyCachingOptions UseSQLite(
            this EasyCachingOptions options
            , Action<SQLiteOptions> configure
            , string name = EasyCachingConstValue.DefaultSQLiteName
            )
        {
            ArgumentCheck.NotNull(configure, nameof(configure));

            options.RegisterExtension(new SQLiteOptionsExtension(name, configure));
            return options;
        }

        /// <summary>
        /// Uses the SQLite provider (read config from configuration file).
        /// </summary>
        /// <param name="options">Options.</param>
        /// <param name="configuration">The configuration.</param>
        /// <param name="name">The name of this provider instance.</param>
        /// <param name="sectionName">The section name in the configuration file.</param>
        public static EasyCachingOptions UseSQLite(
            this EasyCachingOptions options
            , IConfiguration configuration
            , string name = EasyCachingConstValue.DefaultSQLiteName
            , string sectionName = EasyCachingConstValue.SQLiteSection
            )
        {
            var dbConfig = configuration.GetSection(sectionName);
            var sqliteOptions = new SQLiteOptions();
            dbConfig.Bind(sqliteOptions);

            void configure(SQLiteOptions x)
            {
                x.EnableLogging = sqliteOptions.EnableLogging;
                x.MaxRdSecond = sqliteOptions.MaxRdSecond;             
                x.LockMs = sqliteOptions.LockMs;
                x.SleepMs = sqliteOptions.SleepMs;
                x.SerializerName = sqliteOptions.SerializerName;
                x.CacheNulls = sqliteOptions.CacheNulls;
                x.DBConfig = sqliteOptions.DBConfig;
            }

            options.RegisterExtension(new SQLiteOptionsExtension(name, configure));
            return options;
        }
    }
}
