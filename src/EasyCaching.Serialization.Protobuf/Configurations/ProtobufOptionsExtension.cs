namespace EasyCaching.Serialization.Protobuf
{
    using EasyCaching.Core.Configurations;
    using EasyCaching.Core.Serialization;
    //using Microsoft.AspNetCore.Builder;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// Protobuf options extension.
    /// </summary>
    internal sealed class ProtobufOptionsExtension : IEasyCachingOptionsExtension
    {
        /// <summary>
        /// The name.
        /// </summary>
        private readonly string _name;

        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="T:EasyCaching.Serialization.Protobuf.ProtobufOptionsExtension"/> class.
        /// </summary>
        /// <param name="name">Name.</param>
        public ProtobufOptionsExtension(string name)
        {
            this._name = name;
        }

        /// <summary>
        /// Adds the services.
        /// </summary>
        /// <param name="services">Services.</param>
        public void AddServices(IServiceCollection services)
        {
            services.AddSingleton<IEasyCachingSerializer, DefaultProtobufSerializer>(x =>
            {
                return new DefaultProtobufSerializer(_name);
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
