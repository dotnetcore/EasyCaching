using EasyCaching.Core.Internal;
using EasyCaching.Core.Serialization;
using System;
using System.IO;
using System.Text;
using System.Text.Json;

namespace EasyCaching.Serialization.SystemTextJson
{
    /// <summary>
    /// Default json serializer.
    /// </summary>
    public class DefaultJsonSerializer : IEasyCachingSerializer
    {
        /// <summary>
        /// The json serializer.
        /// </summary>
        private readonly JsonSerializerOptions jsonSerializerOption;
        /// <summary>
        /// The name.
        /// </summary>
        private readonly string _name;
        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>The name.</value>
        public string Name => _name;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:EasyCaching.Serialization.SystemTextJson.DefaultJsonSerializer"/> class.
        /// </summary>
        /// <param name="name">Name.</param>
        /// <param name="serializerSettings">serializerSettings.</param>
        public DefaultJsonSerializer(string name, JsonSerializerOptions serializerSettings)
        {
            _name = name;
            jsonSerializerOption = serializerSettings;
        }

        /// <summary>
        /// Deserialize the specified bytes.
        /// </summary>
        /// <returns>The deserialize.</returns>
        /// <param name="bytes">Bytes.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public T Deserialize<T>(byte[] bytes)
        {
            return JsonSerializer.Deserialize<T>(bytes, jsonSerializerOption);
        }
        /// <summary>
        /// Deserialize the specified bytes.
        /// </summary>
        /// <returns>The deserialize.</returns>
        /// <param name="bytes">Bytes.</param>
        /// <param name="type">Type.</param>
        public object Deserialize(byte[] bytes, Type type)
        {
            return JsonSerializer.Deserialize(bytes, type, jsonSerializerOption);
        }
        /// <summary>
        /// Deserializes the object.
        /// </summary>
        /// <returns>The object.</returns>
        /// <param name="value">Value.</param>
        public object DeserializeObject(ArraySegment<byte> value)
        {
            var jr = new Utf8JsonReader(value);
            jr.Read();
            if (jr.TokenType == JsonTokenType.StartArray)
            {
                jr.Read();
                var typeName = Encoding.UTF8.GetString(jr.ValueSpan.ToArray());
                var type = Type.GetType(typeName, throwOnError: true);

                jr.Read();
                return JsonSerializer.Deserialize(ref jr, type, jsonSerializerOption);
            }
            else
            {
                throw new InvalidDataException("JsonTranscoder only supports [\"TypeName\", object]");
            }
        }

        /// <summary>
        /// Serialize the specified value.
        /// </summary>
        /// <returns>The serialize.</returns>
        /// <param name="value">Value.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public byte[] Serialize<T>(T value)
        {
            return JsonSerializer.SerializeToUtf8Bytes(value, jsonSerializerOption);
        }
        /// <summary>
        /// Serializes the object.
        /// </summary>
        /// <returns>The object.</returns>
        /// <param name="obj">Object.</param>
        public ArraySegment<byte> SerializeObject(object obj)
        {
            var typeName = TypeHelper.BuildTypeName(obj.GetType());

            using (var ms = new MemoryStream())
            using (var jw = new Utf8JsonWriter(ms))
            {
                jw.WriteStartArray();
                jw.WriteStringValue(typeName);

                JsonSerializer.Serialize(jw, obj, jsonSerializerOption);

                jw.WriteEndArray();

                jw.Flush();

                return new ArraySegment<byte>(ms.ToArray(), 0, (int)ms.Length);
            }
        }
    }
}
