using Dapper;
using EasyCaching.SQLServer.Configurations;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System;
using Microsoft.Extensions.Options;

namespace EasyCaching.SQLServer
{
    /// <summary>
    /// SQLite cache application bulider extensions.
    /// </summary>
    public static class SQLServerApplicationBuliderExtensions
    {
        /// <summary>
        /// Uses the SQLite cache.
        /// </summary>
        /// <returns>The SQLite cache.</returns>
        /// <param name="app">App.</param>
        public static IApplicationBuilder UseSQLServerCache(this IApplicationBuilder app)
        {
            try
            {
                var dbProvider = app.ApplicationServices.GetService<ISQLDatabaseProvider>();

                var conn = dbProvider.GetConnection();

                if (conn.State == System.Data.ConnectionState.Closed)
                {
                    conn.Open();
                }

                var optionsMon = app.ApplicationServices.GetRequiredService<IOptionsMonitor<SQLServerOptions>>();
                var options = optionsMon.CurrentValue.DBConfig;
                conn.Execute(string.Format(ConstSQL.CREATESQL, options.SchemaName,
                    options.TableName));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return app;
        }
    }
}