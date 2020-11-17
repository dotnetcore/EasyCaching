using System;
using System.Collections.Generic;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace EasyCaching.Serialization.SystemTextJson.Configurations
{
    public class EasyCachingJsonSerializerOptions
    {
        /// <summary>
        /// Gets or sets the encoder to use when escaping strings, or null to use the default
        /// encoder.
        /// </summary>
        public JavaScriptEncoder Encoder => JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
        /// <summary>
        /// Gets or sets a value that defines how comments are handled during deserialization.
        /// </summary>
        public JsonCommentHandling ReadCommentHandling => JsonCommentHandling.Disallow;
        /// <summary>
        /// Gets or sets a value that specifies the policy used to convert a property's name
        /// on an object to another format, such as camel-casing, or null to leave property
        /// names unchanged.
        /// </summary>
        public JsonNamingPolicy PropertyNamingPolicy => null;
        /// <summary>
        /// Gets or sets a value that determines whether a property's name uses a case-insensitive
        /// comparison during deserialization. The default value is false.
        /// </summary>
        public bool PropertyNameCaseInsensitive => false;
        /// <summary>
        /// Specifies how number types should be handled when serializing or deserializing.
        /// </summary>
        public JsonNumberHandling NumberHandling => JsonNumberHandling.Strict;
        /// <summary>
        /// Gets or sets the maximum depth allowed when serializing or deserializing JSON,
        /// with the default value of 0 indicating a maximum depth of 64.
        /// </summary>
        public int MaxDepth => 0;
        /// <summary>
        /// Determines whether fields are handled serialization and deserialization. The
        /// default value is false.
        /// </summary>
        public bool IncludeFields => false;
        /// <summary>
        /// Gets a value that determines whether read-only properties are ignored during
        /// serialization. The default value is false.
        /// </summary>
        public bool IgnoreReadOnlyProperties => false;
        /// <summary>
        /// Determines whether read-only fields are ignored during serialization. A property
        /// is read-only if it isn't marked with the readonly keyword. The default value is false.
        /// </summary>
        public bool IgnoreReadOnlyFields => false;
        /// <summary>
        /// Gets or sets a value that determines whether null values are ignored during serialization
        /// and deserialization. The default value is false.
        /// </summary>
        public bool IgnoreNullValues = false;
        /// <summary>
        /// Gets or sets the policy used to convert a System.Collections.IDictionary key's
        /// name to another format, such as camel-casing.
        /// </summary>
        public JsonNamingPolicy DictionaryKeyPolicy => null;
        /// <summary>
        /// Specifies a condition to determine when properties with default values are ignored
        /// during serialization or deserialization. The default value is System.Text.Json.Serialization.JsonIgnoreCondition.Never.
        /// </summary>
        public JsonIgnoreCondition DefaultIgnoreCondition => JsonIgnoreCondition.Never;
        /// <summary>
        /// Gets or sets the default buffer size, in bytes, to use when creating temporary buffers.
        /// </summary>
        public int DefaultBufferSize => 16 * 1024;
        /// <summary>
        /// Gets the list of user-defined converters that were registered.
        /// </summary>
        public IList<JsonConverter> Converters => new List<JsonConverter>();
        /// <summary>
        /// Get or sets a value that indicates whether an extra comma at the end of a list
        /// of JSON values in an object or array is allowed (and ignored) within the JSON
        /// payload being deserialized.
        /// </summary>
        public bool AllowTrailingCommas => false;
        /// <summary>
        /// Configures how object references are handled when reading and writing JSON.
        /// </summary>
        public ReferenceHandler ReferenceHandler => null;
        /// <summary>
        /// Gets or sets a value that defines whether JSON should use pretty printing. By
        /// default, JSON is serialized without any extra white space.
        /// </summary>
        public bool WriteIndented => false;
    }
}
