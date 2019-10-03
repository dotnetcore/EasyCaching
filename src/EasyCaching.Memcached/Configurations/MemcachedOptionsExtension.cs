namespace EasyCaching.Memcached
{
    using EasyCaching.Core;
    using EasyCaching.Core.Configurations;
    using EasyCaching.Core.Serialization;
    using Enyim.Caching.Memcached;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using System;
    using System.Linq;

    /// <summary>
    /// Memcached options extension.
    /// </summary>
    internal sealed class MemcachedOptionsExtension : IEasyCachingOptionsExtension
    {
        /// <summary>
        /// The name.
        /// </summary>
        private readonly string _name;
        /// <summary>
        /// The configure.
        /// </summary>
        private readonly Action<MemcachedOptions> configure;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:EasyCaching.Memcached.MemcachedOptionsExtension"/> class.
        /// </summary>
        /// <param name="name">Name.</param>
        /// <param name="configure">Configure.</param>
        public MemcachedOptionsExtension(string name, Action<MemcachedOptions> configure)
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
            services.TryAddSingleton<IMemcachedKeyTransformer, DefaultKeyTransformer>();
            services.TryAddSingleton<IEasyCachingSerializer, DefaultBinaryFormatterSerializer>();
            services.AddSingleton<EasyCachingTranscoder>(x =>
            {
                var optionsMon = x.GetRequiredService<IOptionsMonitor<MemcachedOptions>>();
                var options = optionsMon.Get(_name);
                var serializers = x.GetServices<IEasyCachingSerializer>();
                return new EasyCachingTranscoder(_name, options, serializers);
            });
            services.AddSingleton<EasyCachingMemcachedClientConfiguration>(x =>
            {
                var optionsMon = x.GetRequiredService<IOptionsMonitor<MemcachedOptions>>();
                var options = optionsMon.Get(_name);
                var loggerFactory = x.GetRequiredService<ILoggerFactory>();
                var transcoders = x.GetServices<EasyCachingTranscoder>();
                var transformer = x.GetRequiredService<IMemcachedKeyTransformer>();
                return new EasyCachingMemcachedClientConfiguration(_name, loggerFactory, options, transcoders, transformer);
            });

            services.AddSingleton<EasyCachingMemcachedClient>(x =>
            {
                var loggerFactory = x.GetRequiredService<ILoggerFactory>();
                var configs = x.GetServices<EasyCachingMemcachedClientConfiguration>();
                var config = configs.FirstOrDefault(y => y.Name.Equals(_name));
                return new EasyCachingMemcachedClient(_name, loggerFactory, config);
            });

            services.AddSingleton<IEasyCachingProvider, DefaultMemcachedCachingProvider>(x =>
            {
                var clients = x.GetServices<EasyCachingMemcachedClient>();
                var optionsMon = x.GetRequiredService<IOptionsMonitor<MemcachedOptions>>();
                var options = optionsMon.Get(_name);
                var factory = x.GetService<ILoggerFactory>();
                return new DefaultMemcachedCachingProvider(_name, clients, options, factory);
            });
        }
    }
}
