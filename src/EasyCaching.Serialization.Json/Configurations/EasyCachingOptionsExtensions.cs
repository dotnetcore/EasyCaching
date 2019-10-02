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
        /// Withs the json.
        /// </summary>
        /// <returns>The json.</returns>
        /// <param name="options">Options.</param>        
        /// <param name="name">Name.</param>        
        public static EasyCachingOptions WithJson(this EasyCachingOptions options, string name = "json") => options.WithJson(x => { }, name);

        /// <summary>
        /// Withs the json.
        /// </summary>
        /// <returns>The json.</returns>
        /// <param name="options">Options.</param>
        /// <param name="configure">Configure.</param>
        /// <param name="name">Name.</param>        
        public static EasyCachingOptions WithJson(this EasyCachingOptions options, Action<EasyCachingJsonSerializerOptions> configure, string name)
        {
            options.RegisterExtension(new JsonOptionsExtension(name, configure));

            return options;
        }
    }
}
