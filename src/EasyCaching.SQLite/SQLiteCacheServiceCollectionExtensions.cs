namespace EasyCaching.SQLite
{
    using System;
    using EasyCaching.Core;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;

    /// <summary>
    /// SQLite cache service collection extensions.
    /// </summary>
    public static class SQLiteCacheServiceCollectionExtensions
    {     
        /// <summary>
        /// Adds the SQL ite cache.
        /// </summary>
        /// <returns>The SQL ite cache.</returns>
        /// <param name="services">Services.</param>
        /// <param name="providerAction">Provider action.</param>
        public static IServiceCollection AddSQLiteCache(
            this IServiceCollection services,
            Action<SQLiteOptions> providerAction)
        {
            ArgumentCheck.NotNull(services, nameof(services));

            services.AddOptions();
            services.Configure(providerAction);

            services.TryAddSingleton<ISQLiteDatabaseProvider, SQLiteDatabaseProvider>();
            services.AddSingleton<IEasyCachingProvider, DefaultSQLiteCachingProvider>();

            return services;
        } 

        /// <summary>
        /// Adds the SQLite cache.
        /// </summary>
        /// <returns>The SQL ite cache.</returns>
        /// <example>
        /// <![CDATA[
        /// "easycaching": {
        ///     "sqlite": {
        ///         "CachingProviderType": 3,
        ///         "MaxRdSecond": 120,
        ///         "Order": 2,
        ///         "dbconfig": {            
        ///             "FileName": "my.db"
        ///         }
        ///     }
        /// }
        /// ]]>
        /// </example>
        /// <param name="services">Services.</param>
        /// <param name="configuration">Configuration.</param>
        public static IServiceCollection AddSQLiteCache(
           this IServiceCollection services,
            IConfiguration configuration)
        {
            var dbConfig = configuration.GetSection(EasyCachingConstValue.SQLiteSection);
            services.Configure<SQLiteOptions>(dbConfig);

            services.TryAddSingleton<ISQLiteDatabaseProvider, SQLiteDatabaseProvider>();
            services.AddSingleton<IEasyCachingProvider, DefaultSQLiteCachingProvider>();

            return services;
        }
           
        /// <summary>
        /// Adds the SQL ite cache with factory.
        /// </summary>
        /// <returns>The SQL ite cache with factory.</returns>
        /// <param name="services">Services.</param>
        /// <param name="providerName">Provider name.</param>
        /// <param name="providerAction">Provider action.</param>
        public static IServiceCollection AddSQLiteCacheWithFactory(
            this IServiceCollection services,
            string providerName,
            Action<SQLiteOptions> providerAction)
        {
            ArgumentCheck.NotNull(services, nameof(services));
            ArgumentCheck.NotNullOrWhiteSpace(providerName, nameof(providerName));

            services.AddOptions();
            services.Configure(providerName, providerAction);
            services.AddSingleton<IEasyCachingProviderFactory, DefaultEasyCachingProviderFactory>();
            services.AddSingleton<ISQLiteDatabaseProvider, SQLiteDatabaseProvider>(x =>
            {
                var optionsMon = x.GetRequiredService<IOptionsMonitor<SQLiteOptions>>();
                var options = optionsMon.Get(providerName);
                return new SQLiteDatabaseProvider(providerName, options);
            });

            services.AddSingleton<IEasyCachingProvider, DefaultSQLiteCachingProvider>(x =>
            {
                var dbProviders = x.GetServices<ISQLiteDatabaseProvider>();
                var optionsMon = x.GetRequiredService<IOptionsMonitor<SQLiteOptions>>();
                var options = optionsMon.Get(providerName);
                var factory = x.GetService<ILoggerFactory>();
                return new DefaultSQLiteCachingProvider(providerName, dbProviders, options, factory);
            });

            return services;
        }

        /// <summary>
        /// Adds the SQL ite cache with factory.
        /// </summary>
        /// <returns>The SQL ite cache with factory.</returns>
        /// <param name="services">Services.</param>
        /// <param name="providerName">Provider name.</param>
        /// <param name="sectionName">Section name.</param>
        /// <param name="configuration">Configuration.</param>
        public static IServiceCollection AddSQLiteCacheWithFactory(
           this IServiceCollection services,
            string providerName,
            string sectionName,
            IConfiguration configuration)
        {
            ArgumentCheck.NotNullOrWhiteSpace(providerName, nameof(providerName));
            ArgumentCheck.NotNullOrWhiteSpace(sectionName, nameof(sectionName));

            var cacheConfig = configuration.GetSection(sectionName);
            services.Configure<SQLiteOptions>(providerName, cacheConfig);

            services.AddSingleton<IEasyCachingProviderFactory, DefaultEasyCachingProviderFactory>();
            services.AddSingleton<ISQLiteDatabaseProvider, SQLiteDatabaseProvider>(x =>
            {
                var optionsMon = x.GetRequiredService<IOptionsMonitor<SQLiteOptions>>();
                var options = optionsMon.Get(providerName);
                return new SQLiteDatabaseProvider(providerName, options);
            });

            services.AddSingleton<IEasyCachingProvider, DefaultSQLiteCachingProvider>(x =>
            {
                var dbProviders = x.GetServices<ISQLiteDatabaseProvider>();
                var optionsMon = x.GetRequiredService<IOptionsMonitor<SQLiteOptions>>();
                var options = optionsMon.Get(providerName);
                var factory = x.GetService<ILoggerFactory>();
                return new DefaultSQLiteCachingProvider(providerName, dbProviders, options, factory);
            });

            return services;
        }
    }
}
