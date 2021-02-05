namespace EasyCaching.UnitTests
{
    using Core;
    using Core.Bus;
    using Core.Configurations;
    using Core.Decoration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;
    using System;

    public class FakeBusOptionsExtensions : IEasyCachingOptionsExtension
    {
        /// <summary>
        /// The name.
        /// </summary>
        private readonly string _name;

        /// <summary>
        /// The configure.
        /// </summary>
        private readonly Action<FakeBusOptions> _configure;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:EasyCaching.InMemory.FakeOptionsExtensions"/> class.
        /// </summary>
        /// <param name="name">Name.</param>
        /// <param name="configure">Configure.</param>
        public FakeBusOptionsExtensions(string name, Action<FakeBusOptions> configure)
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

            services.AddSingleton<IEasyCachingBus>(serviceProvider =>
            {
                var optionsMon = serviceProvider.GetRequiredService<Microsoft.Extensions.Options.IOptionsMonitor<FakeBusOptions>>();
                var options = optionsMon.Get(_name);
                return options.CreateDecoratedBus(
                    _name,
                    serviceProvider,
                    options.BusFactory);
            });
        }
    }
}