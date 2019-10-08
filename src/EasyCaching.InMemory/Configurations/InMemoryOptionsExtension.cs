namespace EasyCaching.InMemory
{
    using EasyCaching.Core;
    using EasyCaching.Core.Configurations;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;
    using System;
    using System.Linq;

    /// <summary>
    /// InMemory options extension.
    /// </summary>
    internal sealed class InMemoryOptionsExtension : IEasyCachingOptionsExtension
    {
        /// <summary>
        /// The name.
        /// </summary>
        private readonly string _name;

        /// <summary>
        /// The configure.
        /// </summary>
        private readonly Action<InMemoryOptions> configure;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:EasyCaching.InMemory.InMemoryOptionsExtension"/> class.
        /// </summary>
        /// <param name="name">Name.</param>
        /// <param name="configure">Configure.</param>
        public InMemoryOptionsExtension(string name, Action<InMemoryOptions> configure)
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
            services.AddSingleton<IInMemoryCaching, InMemoryCaching>(x =>
            {
                var optionsMon = x.GetRequiredService<Microsoft.Extensions.Options.IOptionsMonitor<InMemoryOptions>>();
                var options = optionsMon.Get(_name);
                return new InMemoryCaching(_name, options.DBConfig);
            });

            services.TryAddSingleton<IEasyCachingProviderFactory, DefaultEasyCachingProviderFactory>();
            services.AddSingleton<IEasyCachingProvider, DefaultInMemoryCachingProvider>(x =>
            {
                var mCache = x.GetServices<IInMemoryCaching>();
                var optionsMon = x.GetRequiredService<Microsoft.Extensions.Options.IOptionsMonitor<InMemoryOptions>>();
                var options = optionsMon.Get(_name);
                // ILoggerFactory can be null
                var factory = x.GetService<Microsoft.Extensions.Logging.ILoggerFactory>();
                return new DefaultInMemoryCachingProvider(_name, mCache, options, factory);
            });
        }
    }
}