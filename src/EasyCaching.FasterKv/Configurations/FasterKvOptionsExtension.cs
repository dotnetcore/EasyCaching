using System;
using EasyCaching.Core;
using EasyCaching.Core.Configurations;
using EasyCaching.Core.Serialization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EasyCaching.FasterKv.Configurations
{
    /// <summary>
    /// LiteDB options extension.
    /// </summary>
    internal sealed class FasterKvOptionsExtension : IEasyCachingOptionsExtension
    {
        /// <summary>
        /// The name.
        /// </summary>
        private readonly string _name;

        /// <summary>
        /// The configure.
        /// </summary>
        private readonly Action<FasterKvCachingOptions> _configure;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:EasyCaching.LiteDB.LiteDBOptionsExtension"/> class.
        /// </summary>
        /// <param name="name">Name.</param>
        /// <param name="configure">Configure.</param>
        public FasterKvOptionsExtension(string name, Action<FasterKvCachingOptions> configure)
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

            services.AddSingleton<IEasyCachingProvider, DefaultFasterKvCachingProvider>(x =>
            {
                var optionsMon = x.GetRequiredService<IOptionsMonitor<FasterKvCachingOptions>>();
                var options = optionsMon.Get(_name);
                var factory = x.GetService<ILoggerFactory>();
                var serializers = x.GetServices<IEasyCachingSerializer>();
                return new DefaultFasterKvCachingProvider(_name, options, serializers, factory);
            });
        }
    }
}
