namespace EasyCaching.SQLite
{
    using EasyCaching.Core;
    using EasyCaching.Core.Configurations;
    using EasyCaching.Core.Decoration;
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

            services.Configure(_name, configure);

            services.TryAddSingleton<IEasyCachingProviderFactory, DefaultEasyCachingProviderFactory>();
            services.AddSingleton<ISQLiteDatabaseProvider, SQLiteDatabaseProvider>(x =>
            {
                var optionsMon = x.GetRequiredService<IOptionsMonitor<SQLiteOptions>>();
                var options = optionsMon.Get(_name);
                return new SQLiteDatabaseProvider(_name, options);
            });

            services.AddSingleton<IEasyCachingProvider>(serviceProvider =>
            {
                var dbProviders = serviceProvider.GetServices<ISQLiteDatabaseProvider>();
                var optionsMon = serviceProvider.GetRequiredService<IOptionsMonitor<SQLiteOptions>>();
                var options = optionsMon.Get(_name);
                var factory = serviceProvider.GetService<ILoggerFactory>();
                return options.CreateDecoratedProvider(
                    _name,
                    serviceProvider,
                    () => new DefaultSQLiteCachingProvider(_name, dbProviders, options, factory));
            });
        }
    }
}
