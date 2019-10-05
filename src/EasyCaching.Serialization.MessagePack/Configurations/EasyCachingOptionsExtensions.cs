namespace Microsoft.Extensions.DependencyInjection
{
    using EasyCaching.Core.Configurations;
    using EasyCaching.Serialization.MessagePack;

    /// <summary>
    /// Easy caching options extensions.
    /// </summary>
    public static class EasyCachingOptionsExtensions
    {
        /// <summary>
        /// Withs the message pack serializer.
        /// </summary>
        /// <param name="options">Options.</param>
        /// <param name="name">The name of this serializer instance.</param>        
        public static EasyCachingOptions WithMessagePack(this EasyCachingOptions options, string name = "msgpack")
        {
            options.RegisterExtension(new MessagePackOptionsExtension(name));

            return options;
        }
    }
}
