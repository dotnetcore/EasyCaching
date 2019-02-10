namespace EasyCaching.InMemory
{
    using EasyCaching.Core;
    using EasyCaching.Core.Configurations;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.Extensions.DependencyInjection;
    using System;

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
            services.Configure(configure);
            services.AddSingleton<IInMemoryCaching, InMemoryCaching>();

            if (string.IsNullOrWhiteSpace(_name))
            {
                services.AddSingleton<IEasyCachingProvider, DefaultInMemoryCachingProvider>();
            }
            else
            {
                services.AddSingleton<IEasyCachingProviderFactory, DefaultEasyCachingProviderFactory>();
                services.AddSingleton<IEasyCachingProvider, DefaultInMemoryCachingProvider>(x =>
                {
                    var mCache = x.GetRequiredService<IInMemoryCaching>();
                    var options = x.GetRequiredService<Microsoft.Extensions.Options.IOptionsMonitor<InMemoryOptions>>();
                    //ILoggerFactory can be null
                    var factory = x.GetService<Microsoft.Extensions.Logging.ILoggerFactory>();
                    return new DefaultInMemoryCachingProvider(_name, mCache, options, factory);
                });
            }
        }

        /// <summary>
        /// Withs the services.
        /// </summary>
        /// <param name="services">Services.</param>
        public void WithServices(IApplicationBuilder services)
        {
            // Method intentionally left empty.
        }
    }
}