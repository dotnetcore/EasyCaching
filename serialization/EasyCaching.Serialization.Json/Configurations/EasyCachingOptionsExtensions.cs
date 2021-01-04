namespace Microsoft.Extensions.DependencyInjection
{
    using EasyCaching.Core.Configurations;
    using EasyCaching.Serialization.Json;
    using Newtonsoft.Json;
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
        public static EasyCachingOptions WithJson(this EasyCachingOptions options, string name = "json") => options.WithJson(configure: x => { }, name);

        /// <summary>
        /// Withs the json serializer.
        /// </summary>        
        /// <param name="options">Options.</param>
        /// <param name="configure">Configure serializer settings.</param>
        /// <param name="name">The name of this serializer instance.</param>     
        public static EasyCachingOptions WithJson(this EasyCachingOptions options, Action<EasyCachingJsonSerializerOptions> configure, string name)
        {
            var easyCachingJsonSerializerOptions = new EasyCachingJsonSerializerOptions();

            configure(easyCachingJsonSerializerOptions);

            Action<JsonSerializerSettings> jsonSerializerSettings = x =>
            {
                x.ReferenceLoopHandling = easyCachingJsonSerializerOptions.ReferenceLoopHandling;
                x.TypeNameHandling = easyCachingJsonSerializerOptions.TypeNameHandling;
                x.MetadataPropertyHandling = easyCachingJsonSerializerOptions.MetadataPropertyHandling;
                x.MissingMemberHandling = easyCachingJsonSerializerOptions.MissingMemberHandling;
                x.NullValueHandling = easyCachingJsonSerializerOptions.NullValueHandling;
                x.DefaultValueHandling = easyCachingJsonSerializerOptions.DefaultValueHandling;
                x.ObjectCreationHandling = easyCachingJsonSerializerOptions.ObjectCreationHandling;
                x.PreserveReferencesHandling = easyCachingJsonSerializerOptions.PreserveReferencesHandling;
                x.ConstructorHandling = easyCachingJsonSerializerOptions.ConstructorHandling;
            };

            options.RegisterExtension(new JsonOptionsExtension(name, jsonSerializerSettings));

            return options;
        }

        /// <summary>
        /// Withs the json serializer.
        /// </summary>        
        /// <param name="options">Options.</param>
        /// <param name="jsonSerializerSettingsConfigure">Configure serializer settings.</param>
        /// <param name="name">The name of this serializer instance.</param>     
        public static EasyCachingOptions WithJson(this EasyCachingOptions options, Action<JsonSerializerSettings> jsonSerializerSettingsConfigure, string name)
        {
            options.RegisterExtension(new JsonOptionsExtension(name, jsonSerializerSettingsConfigure));

            return options;
        }
    }
}
