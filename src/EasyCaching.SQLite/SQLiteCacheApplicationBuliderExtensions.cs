namespace EasyCaching.SQLite
{
    using Dapper;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.Extensions.DependencyInjection;
    using System;

    /// <summary>
    /// SQLite cache application bulider extensions.
    /// </summary>
    public static class SQLiteCacheApplicationBuliderExtensions
    {
        /// <summary>
        /// Uses the SQLite cache.
        /// </summary>
        /// <returns>The SQLite cache.</returns>
        /// <param name="app">App.</param>
        public static IApplicationBuilder UseSQLiteCache(this IApplicationBuilder app)
        {
            try
            {
                var dbProvider = app.ApplicationServices.GetService<ISQLiteDatabaseProvider>();

                var conn = dbProvider.GetConnection();

                if(conn.State == System.Data.ConnectionState.Closed)
                {
                    conn.Open();
                }

                conn.Execute(ConstSQL.CREATESQL);
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return app;
        }
    }
}
