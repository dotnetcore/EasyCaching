namespace EasyCaching.Serialization.MessagePack
{
    using EasyCaching.Core.Configurations;
    using EasyCaching.Core.Serialization;
    //using Microsoft.AspNetCore.Builder;
    using Microsoft.Extensions.DependencyInjection;

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
        /// Initializes a new instance of the
        /// <see cref="T:EasyCaching.Serialization.MessagePack.MessagePackOptionsExtension"/> class.
        /// </summary>
        /// <param name="name">Name.</param>
        public MessagePackOptionsExtension(string name)
        {
            this._name = name;
        }

        /// <summary>
        /// Adds the services.
        /// </summary>
        /// <param name="services">Services.</param>
        public void AddServices(IServiceCollection services)
        {
            services.AddSingleton<IEasyCachingSerializer, DefaultMessagePackSerializer>(x =>
            {
                return new DefaultMessagePackSerializer(_name);
            });
        }

        ///// <summary>
        ///// Withs the services.
        ///// </summary>
        ///// <param name="services">Services.</param>
        //public void WithServices(IApplicationBuilder services)
        //{
        //    // Method intentionally left empty.
        //}
    }
}
