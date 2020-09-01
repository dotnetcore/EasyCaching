namespace EasyCaching.LiteDB
{
    using EasyCaching.Core;
    using EasyCaching.Core.Configurations;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using System;

    /// <summary>
    /// LiteDB options extension.
    /// </summary>
    internal sealed class LiteDBOptionsExtension : IEasyCachingOptionsExtension
    {
        /// <summary>
        /// The name.
        /// </summary>
        private readonly string _name;

        /// <summary>
        /// The configure.
        /// </summary>
        private readonly Action<LiteDBOptions> configure;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:EasyCaching.LiteDB.LiteDBOptionsExtension"/> class.
        /// </summary>
        /// <param name="name">Name.</param>
        /// <param name="configure">Configure.</param>
        public LiteDBOptionsExtension(string name, Action<LiteDBOptions> configure)
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
            services.AddSingleton<ILiteDBDatabaseProvider, LiteDBDatabaseProvider>(x =>
            {
                var optionsMon = x.GetRequiredService<IOptionsMonitor<LiteDBOptions>>();
                var options = optionsMon.Get(_name);
                return new LiteDBDatabaseProvider(_name, options);
            });

            services.AddSingleton<IEasyCachingProvider, DefaultLiteDBCachingProvider>(x =>
            {
                var dbProviders = x.GetServices<ILiteDBDatabaseProvider>();
                var optionsMon = x.GetRequiredService<IOptionsMonitor<LiteDBOptions>>();
                var options = optionsMon.Get(_name);
                var factory = x.GetService<ILoggerFactory>();
                return new DefaultLiteDBCachingProvider(_name, dbProviders, options, factory);
            });
        }
    }
}
