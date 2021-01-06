using EasyCaching.Core.Configurations;
using System;
using System.Linq;
using System.Text.Json;

namespace EasyCaching.Serialization.SystemTextJson.Configurations
{

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
        public static EasyCachingOptions WithSystemTextJson(this EasyCachingOptions options, string name = "json") => options.WithSystemTextJson(configure: x => { }, name);

        /// <summary>
        /// Withs the json serializer.
        /// </summary>        
        /// <param name="options">Options.</param>
        /// <param name="configure">Configure serializer settings.</param>
        /// <param name="name">The name of this serializer instance.</param>     
        public static EasyCachingOptions WithSystemTextJson(this EasyCachingOptions options, Action<EasyCachingJsonSerializerOptions> configure, string name)
        {
            var easyCachingJsonSerializerOptions = new EasyCachingJsonSerializerOptions();

            configure(easyCachingJsonSerializerOptions);

            void jsonSerializerSettings(JsonSerializerOptions x)
            {
                x.MaxDepth = easyCachingJsonSerializerOptions.MaxDepth;
                x.AllowTrailingCommas = easyCachingJsonSerializerOptions.AllowTrailingCommas;
                x.Converters.Union(easyCachingJsonSerializerOptions.Converters);
                x.DefaultBufferSize = easyCachingJsonSerializerOptions.DefaultBufferSize;
                x.DefaultIgnoreCondition = easyCachingJsonSerializerOptions.DefaultIgnoreCondition;
                x.DictionaryKeyPolicy = easyCachingJsonSerializerOptions.DictionaryKeyPolicy;
                x.Encoder = easyCachingJsonSerializerOptions.Encoder;
                x.IgnoreReadOnlyFields = easyCachingJsonSerializerOptions.IgnoreReadOnlyFields;
                x.IgnoreReadOnlyProperties = easyCachingJsonSerializerOptions.IgnoreReadOnlyProperties;
                x.IncludeFields = easyCachingJsonSerializerOptions.IncludeFields;
                x.NumberHandling = easyCachingJsonSerializerOptions.NumberHandling;
                x.PropertyNameCaseInsensitive = easyCachingJsonSerializerOptions.PropertyNameCaseInsensitive;
                x.PropertyNamingPolicy = easyCachingJsonSerializerOptions.PropertyNamingPolicy;
                x.ReadCommentHandling = easyCachingJsonSerializerOptions.ReadCommentHandling;
                x.ReferenceHandler = easyCachingJsonSerializerOptions.ReferenceHandler;
                x.WriteIndented = easyCachingJsonSerializerOptions.WriteIndented;
            }

            options.RegisterExtension(new JsonOptionsExtension(name, jsonSerializerSettings));

            return options;
        }

        /// <summary>
        /// Withs the json serializer.
        /// </summary>        
        /// <param name="options">Options.</param>
        /// <param name="jsonSerializerSettingsConfigure">Configure serializer settings.</param>
        /// <param name="name">The name of this serializer instance.</param>     
        public static EasyCachingOptions WithSystemTextJson(this EasyCachingOptions options, Action<JsonSerializerOptions> jsonSerializerSettingsConfigure, string name)
        {
            options.RegisterExtension(new JsonOptionsExtension(name, jsonSerializerSettingsConfigure));

            return options;
        }
    }
}