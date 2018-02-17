namespace EasyCaching.Serialization.Json
{
    using System;
    using System.Collections.Concurrent;
    using System.IO;
    using System.Text;
    using System.Text.RegularExpressions;
    using EasyCaching.Core;
    using Newtonsoft.Json;

    /// <summary>
    /// Default json serializer.
    /// </summary>
    public class DefaultJsonSerializer : IEasyCachingSerializer
    {
        static readonly ConcurrentDictionary<string, Type> readCache = new ConcurrentDictionary<string, Type>();
        static readonly ConcurrentDictionary<Type, string> writeCache = new ConcurrentDictionary<Type, string>();
        static readonly JsonSerializer jsonSerializer = new JsonSerializer();
        static readonly Regex SubtractFullNameRegex = new Regex(@", Version=\d+.\d+.\d+.\d+, Culture=\w+, PublicKeyToken=\w+", RegexOptions.Compiled);

        /// <summary>
        /// Deserialize the specified bytes.
        /// </summary>
        /// <returns>The deserialize.</returns>
        /// <param name="bytes">Bytes.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public T Deserialize<T>(byte[] bytes)
        {
            using (var ms = new MemoryStream(bytes))
            using (var sr = new StreamReader(ms, Encoding.UTF8))
            using (var jtr = new JsonTextReader(sr))
            {
                return jsonSerializer.Deserialize<T>(jtr);
            }
        }

        /// <summary>
        /// Deserializes the object.
        /// </summary>
        /// <remarks>
        /// This is following https://github.com/neuecc/MemcachedTranscoder.
        /// </remarks>
        /// <returns>The object.</returns>
        /// <param name="value">Value.</param>
        public object DeserializeObject(ArraySegment<byte> value)
        {
            using (var ms = new MemoryStream(value.Array, value.Offset, value.Count, writable: false))
            using (var tr = new StreamReader(ms))
            using (var jr = new JsonTextReader(tr))
            {
                jr.Read();
                if (jr.TokenType == JsonToken.StartArray)
                {
                    // read type
                    var typeName = jr.ReadAsString();
                    var type = readCache.GetOrAdd(typeName, x => Type.GetType(x, throwOnError: true)); // Get type or Register type

                    // read object
                    jr.Read();
                    var deserializedValue = jsonSerializer.Deserialize(jr, type);

                    return deserializedValue;
                }
                else
                {
                    throw new InvalidDataException("JsonTranscoder only supports [\"TypeName\", object]");
                }
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
            using (var ms = new MemoryStream())
            {
                using (var sr = new StreamWriter(ms, Encoding.UTF8))
                using (var jtr = new JsonTextWriter(sr))
                {
                    jsonSerializer.Serialize(jtr, value);
                }
                return ms.ToArray();
            }
        }

        /// <summary>
        /// Serializes the object.
        /// </summary>
        /// <remarks>
        /// This is following https://github.com/neuecc/MemcachedTranscoder.
        /// </remarks>
        /// <returns>The object.</returns>
        /// <param name="obj">Object.</param>
        public ArraySegment<byte> SerializeObject(object obj)
        {
            var type = obj.GetType();
            var typeName = writeCache.GetOrAdd(type, BuildTypeName); // Get type or Register type

            using (var ms = new MemoryStream())
            using (var tw = new StreamWriter(ms))
            using (var jw = new JsonTextWriter(tw))
            {
                jw.WriteStartArray(); // [
                jw.WriteValue(typeName); // "type",
                jsonSerializer.Serialize(jw, obj); // obj

                jw.WriteEndArray(); // ]

                jw.Flush();

                return new ArraySegment<byte>(ms.ToArray(), 0, (int)ms.Length);
            }
        }

        // see:http://msdn.microsoft.com/en-us/library/w3f99sx1.aspx
        // subtract Version, Culture and PublicKeyToken from AssemblyQualifiedName 
        /// <summary>
        /// Builds the name of the type.
        /// </summary>
        /// <remarks>
        /// This is following https://github.com/neuecc/MemcachedTranscoder.
        /// </remarks>
        /// <returns>The type name.</returns>
        /// <param name="type">Type.</param>
        internal static string BuildTypeName(Type type)
        {
            return SubtractFullNameRegex.Replace(type.AssemblyQualifiedName, "");
        }
    }
}
