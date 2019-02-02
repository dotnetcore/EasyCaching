namespace EasyCaching.Memcached
{
    using System;
    using System.Linq;
    using EasyCaching.Core;
    using EasyCaching.Core.Configurations;
    using EasyCaching.Core.Serialization;
    using Enyim.Caching;
    using Enyim.Caching.Configuration;
    using Enyim.Caching.Memcached;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;

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

            if (string.IsNullOrWhiteSpace(_name))
            {
                services.Configure(configure);

                services.TryAddSingleton<ITranscoder, EasyCachingTranscoder>();
                services.TryAddSingleton<IMemcachedKeyTransformer, DefaultKeyTransformer>();
                services.TryAddSingleton<IEasyCachingSerializer, DefaultBinaryFormatterSerializer>();
                services.AddSingleton<IMemcachedClientConfiguration, EasyCachingMemcachedClientConfiguration>(x =>
                {
                    var options = x.GetRequiredService<IOptionsMonitor<MemcachedOptions>>();
                    var loggerFactory = x.GetRequiredService<ILoggerFactory>();
                    var transcoder = x.GetRequiredService<ITranscoder>();
                    var transformer = x.GetRequiredService<IMemcachedKeyTransformer>();
                    return new EasyCachingMemcachedClientConfiguration(loggerFactory, options, transcoder, transformer);
                });

                services.AddSingleton<EasyCachingMemcachedClient>(x =>
                {
                    var loggerFactory = x.GetRequiredService<ILoggerFactory>();
                    var config = x.GetRequiredService<IMemcachedClientConfiguration>();
                    return new EasyCachingMemcachedClient(EasyCachingConstValue.DefaultMemcachedName, loggerFactory, config);
                });

                services.AddSingleton<IEasyCachingProvider, DefaultMemcachedCachingProvider>();
            }
            else
            {
                services.Configure(_name, configure);

                services.AddSingleton<IEasyCachingProviderFactory, DefaultEasyCachingProviderFactory>();
                services.TryAddSingleton<ITranscoder, EasyCachingTranscoder>();
                services.TryAddSingleton<IMemcachedKeyTransformer, DefaultKeyTransformer>();
                services.TryAddSingleton<IEasyCachingSerializer, DefaultBinaryFormatterSerializer>();
                services.AddSingleton<EasyCachingMemcachedClientConfiguration>(x =>
                {
                    var optionsMon = x.GetRequiredService<IOptionsMonitor<MemcachedOptions>>();
                    var options = optionsMon.Get(_name);
                    var loggerFactory = x.GetRequiredService<ILoggerFactory>();
                    var transcoder = x.GetRequiredService<ITranscoder>();
                    var transformer = x.GetRequiredService<IMemcachedKeyTransformer>();
                    return new EasyCachingMemcachedClientConfiguration(_name, loggerFactory, options, transcoder, transformer);
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

        /// <summary>
        /// Withs the services.
        /// </summary>
        /// <param name="app">App.</param>
        public void WithServices(IApplicationBuilder app)
        {
            try
            {
                var clients = app.ApplicationServices.GetServices<IMemcachedClient>();

                foreach (var client in clients)
                {
                    client.GetAsync<string>("EnyimMemcached").Wait();
                }

                Console.WriteLine("EnyimMemcached Started.");
            }
            catch (Exception ex)
            {
                app.ApplicationServices.GetService<ILogger<IMemcachedClient>>()
                    .LogError(new EventId(), ex, "EnyimMemcached Failed.");
            }
        }
    }
}
