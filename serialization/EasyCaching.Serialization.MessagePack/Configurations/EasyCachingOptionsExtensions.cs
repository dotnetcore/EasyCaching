namespace Microsoft.Extensions.DependencyInjection
{
    using EasyCaching.Core.Configurations;
    using EasyCaching.Serialization.MessagePack;
    using System;

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
            Action<EasyCachingMsgPackSerializerOptions> action = x => 
            {
                x.EnableCustomResolver = false;
            };

            options.RegisterExtension(new MessagePackOptionsExtension(name, action));

            return options;
        }

        /// <summary>
        /// Withs the message pack serializer.
        /// </summary>        
        /// <param name="options">Options.</param>
        /// <param name="action">Configure serializer settings.</param>
        /// <param name="name">The name of this serializer instance.</param>     
        public static EasyCachingOptions WithMessagePack(this EasyCachingOptions options, Action<EasyCachingMsgPackSerializerOptions> action, string name)
        {
            options.RegisterExtension(new MessagePackOptionsExtension(name, action));

            return options;
        }
    }
}
