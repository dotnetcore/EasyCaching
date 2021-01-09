namespace EasyCaching.UnitTests
{
    using Core;
    using Core.Configurations;
    using Core.Decoration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;
    using System;

    public class FakeOptionsExtensions : IEasyCachingOptionsExtension
    {
        /// <summary>
        /// The name.
        /// </summary>
        private readonly string _name;

        /// <summary>
        /// The configure.
        /// </summary>
        private readonly Action<FakeOptions> configure;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:EasyCaching.InMemory.FakeOptionsExtensions"/> class.
        /// </summary>
        /// <param name="name">Name.</param>
        /// <param name="configure">Configure.</param>
        public FakeOptionsExtensions(string name, Action<FakeOptions> configure)
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
            services.AddSingleton<IEasyCachingProvider>(serviceProvider =>
            {
                var optionsMon = serviceProvider.GetRequiredService<Microsoft.Extensions.Options.IOptionsMonitor<FakeOptions>>();
                var options = optionsMon.Get(_name);
                return options.CreateDecoratedProvider(
                    _name,
                    serviceProvider,
                    options.ProviderFactory);
            });
        }
    }
}