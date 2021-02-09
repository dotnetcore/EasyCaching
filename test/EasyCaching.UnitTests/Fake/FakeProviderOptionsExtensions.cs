namespace EasyCaching.UnitTests
{
    using Core;
    using Core.Configurations;
    using Core.Decoration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;
    using System;

    public class FakeProviderOptionsExtensions : IEasyCachingOptionsExtension
    {
        /// <summary>
        /// The name.
        /// </summary>
        private readonly string _name;

        /// <summary>
        /// The configure.
        /// </summary>
        private readonly Action<FakeProviderOptions> _configure;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:EasyCaching.InMemory.FakeOptionsExtensions"/> class.
        /// </summary>
        /// <param name="name">Name.</param>
        /// <param name="configure">Configure.</param>
        public FakeProviderOptionsExtensions(string name, Action<FakeProviderOptions> configure)
        {
            this._name = name;
            this._configure = configure;
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
            services.AddSingleton<IEasyCachingProvider>(serviceProvider =>
            {
                var optionsMon = serviceProvider.GetRequiredService<Microsoft.Extensions.Options.IOptionsMonitor<FakeProviderOptions>>();
                var options = optionsMon.Get(_name);
                return options.CreateDecoratedProvider(
                    _name,
                    serviceProvider,
                    options.ProviderFactory);
            });
        }
    }
}