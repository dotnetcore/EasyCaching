namespace Microsoft.Extensions.DependencyInjection
{
    using EasyCaching.Core.Configurations;
    using EasyCaching.Serialization.Json;
    using System;

    /// <summary>
    /// EasyCaching options extensions.
    /// </summary>
    public static class EasyCachingOptionsExtensions
    {
        /// <summary>
        /// Withs the json serializer.
        /// </summary>        
        /// <param name="options">Options.</param>        
        /// <param name="name">The name of this serializer instance.</param>        
        public static EasyCachingOptions WithJson(this EasyCachingOptions options, string name = "json") => options.WithJson(x => { }, name);

        /// <summary>
        /// Withs the json serializer.
        /// </summary>        
        /// <param name="options">Options.</param>
        /// <param name="configure">Configure serializer settings.</param>
        /// <param name="name">The name of this serializer instance.</param>     
        public static EasyCachingOptions WithJson(this EasyCachingOptions options, Action<EasyCachingJsonSerializerOptions> configure, string name)
        {
            options.RegisterExtension(new JsonOptionsExtension(name, configure));

            return options;
        }
    }
}
