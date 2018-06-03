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
            services.TryAddSingleton<IEasyCachingProvider, DefaultSQLiteCachingProvider>();

            return services;
        } 


        /// <summary>
        /// Adds the SQLite cache.
        /// </summary>
        /// <returns>The SQL ite cache.</returns>
        /// <example>
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
        /// </example>
        /// <param name="services">Services.</param>
        /// <param name="configuration">Configuration.</param>
        public static IServiceCollection AddSQLiteCache(
           this IServiceCollection services,
            IConfiguration configuration)
        {
            var dbConfig = configuration.GetSection(EasyCachingConstValue.ConfigSection);
            services.Configure<SQLiteOptions>(dbConfig);

            services.TryAddSingleton<ISQLiteDatabaseProvider, SQLiteDatabaseProvider>();
            services.TryAddSingleton<IEasyCachingProvider, DefaultSQLiteCachingProvider>();

            return services;
        }
    }
}
