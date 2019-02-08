using EasyCaching.Core;
using EasyCaching.SQLServer.Configurations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;

namespace EasyCaching.SQLServer
{
    public static class SQLServerCacheServiceCollectionExtensions
    {
        /// <summary>
        /// Adds the SQL ite cache.
        /// </summary>
        /// <returns>The SQL ite cache.</returns>
        /// <param name="services">Services.</param>
        /// <param name="providerAction">Provider action.</param>
        public static IServiceCollection AddSQLServerCache(
            this IServiceCollection services,
            Action<SQLServerOptions> providerAction)
        {
            ArgumentCheck.NotNull(services, nameof(services));

            services.AddOptions()
                .Configure(providerAction);

            services.TryAddSingleton<ISQLDatabaseProvider, SQLDatabaseProvider>();
            services.AddSingleton<IEasyCachingProvider, DefaultSQLServerCachingProvider>();

            return services;
        }

        /// <summary>
        /// Adds the SQLite cache.
        /// </summary>
        /// <returns>The SQL ite cache.</returns>
        /// <example>
        /// <![CDATA[
        /// "easycaching": {
        ///     "sqlserver": {
        ///         "CachingProviderType": 4,
        ///         "MaxRdSecond": 120,
        ///         "Order": 2,
        ///         "dbconfig": {
        ///             "connectionString": "",
        ///             "schemaName": "",
        ///             "tableName": ""
        ///         }
        ///     }
        /// }
        /// ]]>
        /// </example>
        /// <param name="services">Services.</param>
        /// <param name="configuration">Configuration.</param>
        public static IServiceCollection AddSQLServerCache(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            var dbConfig = configuration.GetSection(EasyCachingConstValue.SQLServerSection);
            services.Configure<SQLServerOptions>(dbConfig);

            services.TryAddSingleton<ISQLDatabaseProvider, SQLDatabaseProvider>();
            services.AddSingleton<IEasyCachingProvider, DefaultSQLServerCachingProvider>();

            return services;
        }

        /// <summary>
        /// Adds the SQL server cache with factory.
        /// </summary>
        /// <returns>The SQL ite cache with factory.</returns>
        /// <param name="services">Services.</param>
        /// <param name="providerName">Provider name.</param>
        /// <param name="providerAction">Provider action.</param>
        public static IServiceCollection AddSQLServerCacheWithFactory(
            this IServiceCollection services,
            string providerName,
            Action<SQLServerOptions> providerAction)
        {
            ArgumentCheck.NotNull(services, nameof(services));
            ArgumentCheck.NotNullOrWhiteSpace(providerName, nameof(providerName));

            services.AddOptions()
                .Configure(providerName, providerAction)
                .AddSingleton<IEasyCachingProviderFactory, DefaultEasyCachingProviderFactory>()
                .AddSingleton<ISQLDatabaseProvider, SQLDatabaseProvider>(x =>
            {
                var optionsMon = x.GetRequiredService<IOptionsMonitor<SQLServerOptions>>();
                var options = optionsMon.Get(providerName);
                return new SQLDatabaseProvider(providerName, options);
            })
                .AddSingleton<IEasyCachingProvider, DefaultSQLServerCachingProvider>(x =>
            {
                var dbProviders = x.GetServices<ISQLDatabaseProvider>();
                var optionsMon = x.GetRequiredService<IOptionsMonitor<SQLServerOptions>>();
                var options = optionsMon.Get(providerName);
                var factory = x.GetService<ILoggerFactory>();
                return new DefaultSQLServerCachingProvider(providerName, dbProviders, options, factory);
            });

            return services;
        }

        /// <summary>
        /// Adds the SQL server cache with factory.
        /// </summary>
        /// <returns>The SQL ite cache with factory.</returns>
        /// <param name="services">Services.</param>
        /// <param name="providerName">Provider name.</param>
        /// <param name="sectionName">Section name.</param>
        /// <param name="configuration">Configuration.</param>
        public static IServiceCollection AddSQLServerCacheWithFactory(
            this IServiceCollection services,
            string providerName,
            string sectionName,
            IConfiguration configuration)
        {
            ArgumentCheck.NotNullOrWhiteSpace(providerName, nameof(providerName));
            ArgumentCheck.NotNullOrWhiteSpace(sectionName, nameof(sectionName));

            var cacheConfig = configuration.GetSection(sectionName);
            services.Configure<SQLServerOptions>(providerName, cacheConfig)
                .AddSingleton<IEasyCachingProviderFactory, DefaultEasyCachingProviderFactory>()
                .AddSingleton<ISQLDatabaseProvider, SQLDatabaseProvider>(x =>
            {
                var optionsMon = x.GetRequiredService<IOptionsMonitor<SQLServerOptions>>();
                var options = optionsMon.Get(providerName);
                return new SQLDatabaseProvider(providerName, options);
            });

            services.AddSingleton<IEasyCachingProvider, DefaultSQLServerCachingProvider>(x =>
            {
                var dbProviders = x.GetServices<ISQLDatabaseProvider>();
                var optionsMon = x.GetRequiredService<IOptionsMonitor<SQLServerOptions>>();
                var options = optionsMon.Get(providerName);
                var factory = x.GetService<ILoggerFactory>();
                return new DefaultSQLServerCachingProvider(providerName, dbProviders, options, factory);
            });

            return services;
        }
    }
}