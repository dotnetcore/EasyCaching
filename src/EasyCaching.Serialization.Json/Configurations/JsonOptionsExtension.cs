namespace EasyCaching.Serialization.Json
{
    using EasyCaching.Core;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.Extensions.DependencyInjection;
    using System;

    /// <summary>
    /// Json options extension.
    /// </summary>
    internal sealed class JsonOptionsExtension : IEasyCachingOptionsExtension
    {
        /// <summary>
        /// The configure.
        /// </summary>
        private readonly Action<EasyCachingJsonSerializerOptions> _configure;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:EasyCaching.Serialization.Json.JsonOptionsExtension"/> class.
        /// </summary>
        /// <param name="configure">Configure.</param>
        public JsonOptionsExtension(Action<EasyCachingJsonSerializerOptions> configure)
        {
            this._configure = configure;
        }

        /// <summary>
        /// Adds the services.
        /// </summary>
        /// <param name="services">Services.</param>
        public void AddServices(IServiceCollection services)
        {
            Action<EasyCachingJsonSerializerOptions> configure = x => { };

            if (_configure != null) configure = _configure;

            services.AddOptions();
            services.Configure(configure);
            services.AddSingleton<IEasyCachingSerializer, DefaultJsonSerializer>();
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
