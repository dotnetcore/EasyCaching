using System;
using Dapper;
using EasyCaching.Core;
using EasyCaching.Core.Configurations;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EasyCaching.SQLServer.Configurations
{
    /// <summary>
    /// SQLite options extension.
    /// </summary>
    internal sealed class SQLServerOptionsExtension : IEasyCachingOptionsExtension
    {
        /// <summary>
        /// The name.
        /// </summary>
        private readonly string _name;
        /// <summary>
        /// The configure.
        /// </summary>
        private readonly Action<SQLServerOptions> configure;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:EasyCaching.SQLite.SQLiteOptionsExtension"/> class.
        /// </summary>
        /// <param name="name">Name.</param>
        /// <param name="configure">Configure.</param>
        public SQLServerOptionsExtension(string name, Action<SQLServerOptions> configure)
        {
            this._name = name;
            this.configure = configure;
        }

        /// <summary>
        /// Adds the services.
        /// </summary>
        /// <param name="services">Services.</param>
        public void AddServices(IServiceCollection services)
        {
            services.AddOptions();

            if (string.IsNullOrWhiteSpace(_name))
            {
                services.Configure(configure);

                services.TryAddSingleton<ISQLDatabaseProvider, SQLDatabaseProvider>();
                services.AddSingleton<IEasyCachingProvider, DefaultSQLServerCachingProvider>();
            }
            else
            {
                services.Configure(_name, configure);

                services.AddSingleton<IEasyCachingProviderFactory, DefaultEasyCachingProviderFactory>();
                services.AddSingleton<ISQLDatabaseProvider, SQLDatabaseProvider>(x =>
                {
                    var optionsMon = x.GetRequiredService<IOptionsMonitor<SQLServerOptions>>();
                    var options = optionsMon.Get(_name);
                    return new SQLDatabaseProvider(_name, options);
                });

                services.AddSingleton<IEasyCachingProvider, DefaultSQLServerCachingProvider>(x =>
                {
                    var dbProviders = x.GetServices<ISQLDatabaseProvider>();
                    var optionsMon = x.GetRequiredService<IOptionsMonitor<SQLServerOptions>>();
                    var options = optionsMon.Get(_name);
                    var factory = x.GetService<ILoggerFactory>();
                    return new DefaultSQLServerCachingProvider(_name, dbProviders, options, factory);
                });
            }
        }

        /// <summary>
        /// Withs the services.
        /// </summary>
        /// <param name="app">App.</param>
        public void WithServices(IApplicationBuilder app)
        {
            try
            {
                var dbProviders = app.ApplicationServices.GetServices<ISQLDatabaseProvider>();
                var optionsMon = app.ApplicationServices.GetRequiredService<IOptionsMonitor<SQLServerOptions>>();
                foreach (var dbProvider in dbProviders)
                {
                    var conn = dbProvider.GetConnection();

                    if (conn.State == System.Data.ConnectionState.Closed)
                    {
                        conn.Open();
                    }
                    
                    var options = optionsMon.Get(dbProvider.DBProviderName);
                    conn.Execute(string.Format(ConstSQL.CREATESQL, options.DBConfig.SchemaName,
                        options.DBConfig.TableName));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
