namespace EasyCaching.Serialization.Json
{
    using System;
    using EasyCaching.Core.Configurations;
    using EasyCaching.Core.Serialization;
    using Microsoft.Extensions.DependencyInjection;
    using Newtonsoft.Json;

    /// <summary>
    /// Json options extension.
    /// </summary>
    internal sealed class JsonOptionsExtension : IEasyCachingOptionsExtension
    {
        /// <summary>
        /// The name.
        /// </summary>
        private readonly string _name;

        /// <summary>
        /// The configure.
        /// </summary>
        private readonly Action<JsonSerializerSettings> _configure;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:EasyCaching.Serialization.Json.JsonOptionsExtension"/> class.
        /// </summary>
        /// <param name="name">Name.</param>
        /// <param name="configure">Configure.</param>
        public JsonOptionsExtension(string name, Action<JsonSerializerSettings> configure)
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
            Action<JsonSerializerSettings> configure = x => { };

            if (_configure != null) configure = _configure;

            services.AddOptions();
            services.Configure(_name, configure);
            services.AddSingleton<IEasyCachingSerializer, DefaultJsonSerializer>(x=> 
            {
                var optionsMon = x.GetRequiredService<Microsoft.Extensions.Options.IOptionsMonitor<JsonSerializerSettings>>();
                var options = optionsMon.Get(_name);
                return new DefaultJsonSerializer(_name, options);
            });
        }
    }
}
