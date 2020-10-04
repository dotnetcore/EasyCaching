namespace EasyCaching.Serialization.MessagePack
{
    using EasyCaching.Core.Configurations;
    using EasyCaching.Core.Serialization;
    using Microsoft.Extensions.DependencyInjection;
    using System;

    /// <summary>
    /// Message pack options extension.
    /// </summary>
    internal sealed class MessagePackOptionsExtension : IEasyCachingOptionsExtension
    {
        /// <summary>
        /// The name.
        /// </summary>
        private readonly string _name;

        /// <summary>
        /// The configure.
        /// </summary>
        private readonly Action<EasyCachingMsgPackSerializerOptions> _configure;

        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="T:EasyCaching.Serialization.MessagePack.MessagePackOptionsExtension"/> class.
        /// </summary>
        /// <param name="name">Name.</param>
        /// <param name="configure">Configure.</param>
        public MessagePackOptionsExtension(string name, Action<EasyCachingMsgPackSerializerOptions> configure)
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
            Action<EasyCachingMsgPackSerializerOptions> configure = x => { };

            if (_configure != null) configure = _configure;

            services.AddOptions();
            services.Configure(_name, configure);
            services.AddSingleton<IEasyCachingSerializer, DefaultMessagePackSerializer>(x =>
            {
                var optionsMon = x.GetRequiredService<Microsoft.Extensions.Options.IOptionsMonitor<EasyCachingMsgPackSerializerOptions>>();
                var options = optionsMon.Get(_name);
                return new DefaultMessagePackSerializer(_name, options);
            });
        }
    }
}
