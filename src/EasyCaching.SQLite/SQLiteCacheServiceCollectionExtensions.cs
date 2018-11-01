namespace EasyCaching.SQLite
{
    using System;
    using EasyCaching.Core;
    using EasyCaching.Core.Internal;
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

           
        public static IServiceCollection AddSQLiteCacheWithFactory(
            this IServiceCollection services,
            string name,
            Action<SQLiteOptions> providerAction)
        {
            ArgumentCheck.NotNull(services, nameof(services));

            services.AddOptions();
            services.Configure(providerAction);
            services.AddSingleton<IEasyCachingProviderFactory, DefaultEasyCachingProviderFactory>();
            services.AddSingleton<ISQLiteDatabaseProvider, SQLiteDatabaseProvider>(x =>
            {
                var optionsMon = x.GetRequiredService<IOptionsMonitor<SQLiteOptions>>();
                var options = optionsMon.Get(name);
                return new SQLiteDatabaseProvider(name, options);
            });

            services.AddSingleton<IEasyCachingProvider, DefaultSQLiteCachingProvider>(x =>
            {
                var dbProviders = x.GetServices<ISQLiteDatabaseProvider>();
                var options = x.GetRequiredService<IOptionsMonitor<SQLiteOptions>>();
                var factory = x.GetService<ILoggerFactory>();
                return new DefaultSQLiteCachingProvider(name, dbProviders, options, factory);
            });

            return services;
        }

        /// <summary>
        /// Adds the default redis cache.
        /// </summary>
        /// <example>
        /// <![CDATA[
        /// "easycaching": {
        ///     "redis":{
        ///         "CachingProviderType": 2,
        ///         "MaxRdSecond": 120,
        ///         "Order": 2,
        ///         "dbconfig": {            
        ///             "Password": null,
        ///             "IsSsl": false,
        ///             "SslHost": null,
        ///             "ConnectionTimeout": 5000,
        ///             "AllowAdmin": true,
        ///             "Endpoints": [
        ///                 {
        ///                     "Host": "localhost",
        ///                     "Port": 6739
        ///                 }
        ///             ],
        ///             "Database": 0,
        ///             "Configuration":"localhost:6379,allowAdmin=false"
        ///         }
        ///     }
        /// }      
        /// ]]>
        /// </example>
        /// <returns>The default redis cache.</returns>
        /// <param name="services">Services.</param>
        /// <param name="configuration">Configuration.</param>
        public static IServiceCollection AddSQLiteCacheWithFactory(
           this IServiceCollection services,
            string name,
            IConfiguration configuration)
        {
            var cacheConfig = configuration.GetSection(EasyCachingConstValue.SQLiteSection);
            services.Configure<SQLiteOptions>(name, cacheConfig);

            //var redisConfig = configuration.GetSection(EasyCachingConstValue.ConfigChildSection);
            //services.Configure<RedisDBOptions>(redisConfig);
            services.AddSingleton<IEasyCachingProviderFactory, DefaultEasyCachingProviderFactory>();
            services.AddSingleton<ISQLiteDatabaseProvider, SQLiteDatabaseProvider>(x =>
            {
                var optionsMon = x.GetRequiredService<IOptionsMonitor<SQLiteOptions>>();
                var options = optionsMon.Get(name);
                return new SQLiteDatabaseProvider(name, options);
            });

            services.AddSingleton<IEasyCachingProvider, DefaultSQLiteCachingProvider>(x =>
            {
                var dbProviders = x.GetServices<ISQLiteDatabaseProvider>();
                var options = x.GetRequiredService<IOptionsMonitor<SQLiteOptions>>();
                var factory = x.GetService<ILoggerFactory>();
                return new DefaultSQLiteCachingProvider(name, dbProviders, options, factory);
            });

            return services;
        }
    }
}
