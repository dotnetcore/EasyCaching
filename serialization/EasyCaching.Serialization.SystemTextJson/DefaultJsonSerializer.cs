using EasyCaching.Core.Serialization;
using System.Text.Json;

namespace EasyCaching.Serialization.SystemTextJson
{
    /// <summary>
    /// Default json serializer.
    /// </summary>
    public class DefaultJsonSerializer : DefaultSerializer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:EasyCaching.Serialization.SystemTextJson.DefaultJsonSerializer"/> class.
        /// </summary>
        /// <param name="name">Name.</param>
        /// <param name="serializerSettings">serializerSettings.</param>
        public DefaultJsonSerializer(string name, JsonSerializerOptions serializerSettings)
            : base(name, serializerSettings)
        {
        }
    }
}
