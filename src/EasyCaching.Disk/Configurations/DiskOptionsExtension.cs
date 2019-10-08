namespace EasyCaching.Disk
{
    using System;
    using EasyCaching.Core;
    using EasyCaching.Core.Configurations;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;

    internal sealed class DiskOptionsExtension : IEasyCachingOptionsExtension
    {
        /// <summary>
        /// The name.
        /// </summary>
        private readonly string _name;

        /// <summary>
        /// The configure.
        /// </summary>
        private readonly Action<DiskOptions> configure;

        public DiskOptionsExtension(string name, Action<DiskOptions> configure)
        {
            this._name = name;
            this.configure = configure;
        }

        public void AddServices(IServiceCollection services)
        {
            services.AddOptions();
            services.Configure(_name, configure);
                                     
            services.TryAddSingleton<IEasyCachingProviderFactory, DefaultEasyCachingProviderFactory>();
            services.AddSingleton<IEasyCachingProvider, DefaultDiskCachingProvider>(x =>
            {
                var optionsMon = x.GetRequiredService<Microsoft.Extensions.Options.IOptionsMonitor<DiskOptions>>();
                var options = optionsMon.Get(_name);
                // ILoggerFactory can be null
                var factory = x.GetService<Microsoft.Extensions.Logging.ILoggerFactory>();
                return new DefaultDiskCachingProvider(_name, options, factory);
            });
        }
    }
}
