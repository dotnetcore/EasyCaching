using System;
using EasyCaching.Core;
using EasyCaching.Core.Configurations;
using Microsoft.Extensions.Configuration;

namespace EasyCaching.SQLServer.Configurations
{
    /// <summary>
    /// Easy caching options extensions.
    /// </summary>
    public static class EasyCachingOptionsExtensions
    {
        /// <summary>
        /// Uses the SQLite.
        /// </summary>
        /// <returns>The SQL ite.</returns>
        /// <param name="options">Options.</param>
        /// <param name="configure">Configure.</param>
        /// <param name="name">Name.</param>
        public static EasyCachingOptions UseSQLServer(this EasyCachingOptions options, Action<SQLServerOptions> configure, string name = "")
        {
            if (configure == null)
            {
                throw new ArgumentNullException(nameof(configure));
            }

            options.RegisterExtension(new SQLServerOptionsExtension(name, configure));
            return options;
        }

        /// <summary>
        /// Uses the SQLite.
        /// </summary>
        /// <returns>The SQL ite.</returns>
        /// <param name="options">Options.</param>
        /// <param name="configuration">Configuration.</param>
        /// <param name="name">Name.</param>
        /// <param name="sectionName">Section name.</param>
        public static EasyCachingOptions UseSQLServer(this EasyCachingOptions options, IConfiguration configuration, string name = "", string sectionName = EasyCachingConstValue.SQLServerSection)
        {
            var dbConfig = configuration.GetSection(sectionName);
            var serverOptions = new SQLServerOptions();
            dbConfig.Bind(serverOptions);

            void configure(SQLServerOptions x)
            {
                x.CachingProviderType = serverOptions.CachingProviderType;
                x.EnableLogging = serverOptions.EnableLogging;
                x.MaxRdSecond = serverOptions.MaxRdSecond;
                x.Order = serverOptions.Order;
                x.DBConfig = serverOptions.DBConfig;
            }

            options.RegisterExtension(new SQLServerOptionsExtension(name, configure));
            return options;
        }
    }
}
