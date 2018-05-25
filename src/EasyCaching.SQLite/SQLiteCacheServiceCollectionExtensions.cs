namespace EasyCaching.SQLite
{
    using System;
    using EasyCaching.Core;
    using EasyCaching.Core.Internal;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;

    /// <summary>
    /// SQLite cache service collection extensions.
    /// </summary>
    public static class SQLiteCacheServiceCollectionExtensions
    {
        /// <summary>
        /// Adds the SQLite cache.
        /// </summary>
        /// <returns>The SQLite cache.</returns>
        /// <param name="services">Services.</param>
        /// <param name="optionsAction">Options action.</param>
        public static IServiceCollection AddSQLiteCache(
            this IServiceCollection services, 
            Action<SQLiteDBOption> optionsAction)
        {            
            var providerOptions = new SQLiteOptions();

            return services.AddSQLiteCache(optionsAction,x=>
            {
                x.CachingProviderType = providerOptions.CachingProviderType;
                x.MaxRdSecond = providerOptions.MaxRdSecond;
                x.Order = providerOptions.Order;
            });
        }

        /// <summary>
        /// Adds the SQLite cache.
        /// </summary>
        /// <returns>The SQLite cache.</returns>
        /// <param name="services">Services.</param>
        /// <param name="dbAction">Db action.</param>
        /// <param name="providerAction">Provider action.</param>
        public static IServiceCollection AddSQLiteCache(
            this IServiceCollection services, 
            Action<SQLiteDBOption> dbAction,
            Action<SQLiteOptions> providerAction)
        {
            ArgumentCheck.NotNull(services, nameof(services));
            ArgumentCheck.NotNull(dbAction, nameof(dbAction));

            services.AddOptions();
            services.Configure(dbAction);

            var providerOptions = new SQLiteOptions();
            providerAction(providerOptions);
            services.AddSingleton(providerOptions);

            services.TryAddSingleton<ISQLiteDatabaseProvider, SQLiteDatabaseProvider>();
            services.TryAddSingleton<IEasyCachingProvider, DefaultSQLiteCachingProvider>();

            return services;
        }             

        /// <summary>
        /// Adds the SQLite cache.
        /// </summary>
        /// <returns>The SQL ite cache.</returns>
        /// <![CDATA[
        /// "easycaching": {
        ///     "CachingProviderType": 3,
        ///     "MaxRdSecond": 120,
        ///     "Order": 2,
        ///     "dbconfig": {            
        ///         "FileName": "my.db"
        ///     }
        /// }
        /// ]]>
        /// <param name="services">Services.</param>
        /// <param name="configuration">Configuration.</param>
        public static IServiceCollection AddSQLiteCache(
           this IServiceCollection services,
            IConfiguration configuration)
        {
            var dbConfig = configuration.GetSection("easycaching");
            services.Configure<SQLiteOptions>(dbConfig);

            var sqliteConfig = configuration.GetSection("easycaching:dbconfig");
            services.Configure<SQLiteDBOption>(sqliteConfig);

            services.TryAddSingleton<ISQLiteDatabaseProvider, SQLiteDatabaseProvider>();
            services.TryAddSingleton<IEasyCachingProvider, DefaultSQLiteCachingProvider>();

            return services;
        }
    }
}
