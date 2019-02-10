namespace EasyCaching.SQLite
{
    using Dapper;
    using EasyCaching.Core;
    using EasyCaching.Core.Configurations;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using System;

    /// <summary>
    /// SQLite options extension.
    /// </summary>
    internal sealed class SQLiteOptionsExtension : IEasyCachingOptionsExtension
    {
        /// <summary>
        /// The name.
        /// </summary>
        private readonly string _name;
        /// <summary>
        /// The configure.
        /// </summary>
        private readonly Action<SQLiteOptions> configure;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:EasyCaching.SQLite.SQLiteOptionsExtension"/> class.
        /// </summary>
        /// <param name="name">Name.</param>
        /// <param name="configure">Configure.</param>
        public SQLiteOptionsExtension(string name, Action<SQLiteOptions> configure)
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

                services.TryAddSingleton<ISQLiteDatabaseProvider, SQLiteDatabaseProvider>();
                services.AddSingleton<IEasyCachingProvider, DefaultSQLiteCachingProvider>();
            }
            else
            {
                services.Configure(_name, configure);

                services.AddSingleton<IEasyCachingProviderFactory, DefaultEasyCachingProviderFactory>();
                services.AddSingleton<ISQLiteDatabaseProvider, SQLiteDatabaseProvider>(x =>
                {
                    var optionsMon = x.GetRequiredService<IOptionsMonitor<SQLiteOptions>>();
                    var options = optionsMon.Get(_name);
                    return new SQLiteDatabaseProvider(_name, options);
                });

                services.AddSingleton<IEasyCachingProvider, DefaultSQLiteCachingProvider>(x =>
                {
                    var dbProviders = x.GetServices<ISQLiteDatabaseProvider>();
                    var optionsMon = x.GetRequiredService<IOptionsMonitor<SQLiteOptions>>();
                    var options = optionsMon.Get(_name);
                    var factory = x.GetService<ILoggerFactory>();
                    return new DefaultSQLiteCachingProvider(_name, dbProviders, options, factory);
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
                var dbProviders = app.ApplicationServices.GetServices<ISQLiteDatabaseProvider>();

                foreach (var dbProvider in dbProviders)
                {
                    var conn = dbProvider.GetConnection();

                    if (conn.State == System.Data.ConnectionState.Closed)
                    {
                        conn.Open();
                    }

                    conn.Execute(ConstSQL.CREATESQL);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
