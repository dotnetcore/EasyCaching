namespace EasyCaching.Serialization.Json
{
    using EasyCaching.Core.Configurations;
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
        public static EasyCachingOptions WithJson(this EasyCachingOptions options) => options.WithJson(x => { });
        /// <summary>
        /// Withs the json.
        /// </summary>
        /// <returns>The json.</returns>
        /// <param name="options">Options.</param>
        /// <param name="configure">Configure.</param>
        public static EasyCachingOptions WithJson(this EasyCachingOptions options, Action<EasyCachingJsonSerializerOptions> configure)
        {
            options.RegisterExtension(new JsonOptionsExtension(configure));

            return options;
        }
    }
}
