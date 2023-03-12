using System;
using EasyCaching.Core;
using EasyCaching.Core.Configurations;
using EasyCaching.Core.Serialization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EasyCaching.Etcd
{
    /// <summary>
    /// Etcd options extension.
    /// </summary>
    internal sealed class EtcdOptionsExtension : IEasyCachingOptionsExtension
    {
        /// <summary>
        /// The name.
        /// </summary>
        private readonly string _name;

        /// <summary>
        /// The configure.
        /// </summary>
        private readonly Action<EtcdCachingOptions> _configure;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:EasyCaching.Etcd.EtcdOptionsExtension"/> class.
        /// </summary>
        /// <param name="name">Name.</param>
        /// <param name="configure">Configure.</param>
        public EtcdOptionsExtension(string name, Action<EtcdCachingOptions> configure)
        {
            _name = name;
            _configure = configure;
        }

        /// <summary>
        /// Adds the services.
        /// </summary>
        /// <param name="services">Services.</param>
        public void AddServices(IServiceCollection services)
        {
            services.AddOptions();

            services.Configure(_name, _configure);

            services.TryAddSingleton<IEasyCachingProviderFactory, DefaultEasyCachingProviderFactory>();

            services.AddSingleton<IEtcdCaching, EtcdCaching>(x =>
            {
                var optionsMon = x.GetRequiredService<IOptionsMonitor<EtcdCachingOptions>>();
                var options = optionsMon.Get(_name);
                var factory = x.GetService<ILoggerFactory>();
                var serializers = x.GetServices<IEasyCachingSerializer>();
                return new EtcdCaching(_name, options,serializers,factory);
            });

            services.AddSingleton<IEasyCachingProvider, DefaultEtcdCachingProvider>(x =>
            {
                var mCache = x.GetServices<IEtcdCaching>();
                var optionsMon = x.GetRequiredService<IOptionsMonitor<EtcdCachingOptions>>();
                var options = optionsMon.Get(_name);
                var factory = x.GetService<ILoggerFactory>();
                var serializers = x.GetServices<IEasyCachingSerializer>();
                return new DefaultEtcdCachingProvider(_name,mCache, options, serializers, factory);
            });
        }
    }
}
