namespace Microsoft.Extensions.DependencyInjection
{
    using EasyCaching.Core.Configurations;
    using EasyCaching.Serialization.Protobuf;

    /// <summary>
    /// EasyCaching options extensions.
    /// </summary>
    public static class EasyCachingOptionsExtensions
    {
        /// <summary>
        /// Withs the protobuf serializer.
        /// </summary>        
        /// <param name="options">Options.</param>
        /// <param name="name">The name of this serializer instance.</param>   
        public static EasyCachingOptions WithProtobuf(this EasyCachingOptions options, string name = "proto")
        {
            options.RegisterExtension(new ProtobufOptionsExtension(name));

            return options;
        }
    }
}
