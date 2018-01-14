namespace EasyCaching.SQLite
{
    using System;
    using EasyCaching.Core;
    using EasyCaching.Core.Internal;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;

    /// <summary>
    /// SQLite cache service collection extensions.
    /// </summary>
    public static class SQLiteCacheServiceCollectionExtensions
    {
        /// <summary>
        /// Adds the default redis cache.
        /// </summary>
        /// <returns>The default redis cache.</returns>
        /// <param name="services">Services.</param>
        /// <param name="optionsAction">Options action.</param>
        public static IServiceCollection AddSQLiteCache(this IServiceCollection services, Action<SQLiteCacheOption> optionsAction)
        {
            ArgumentCheck.NotNull(services, nameof(services));
            ArgumentCheck.NotNull(optionsAction, nameof(optionsAction));

            services.AddOptions();
            services.Configure(optionsAction);
            services.TryAddSingleton<ISQLiteDatabaseProvider, SQLiteDatabaseProvider>();
            services.TryAddSingleton<IEasyCachingProvider, SQLiteCachingProvider>();

            return services;
        }
    }
}
