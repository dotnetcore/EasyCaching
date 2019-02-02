namespace EasyCaching.Serialization.Json
{
    using System;
    using System.IO;
    using System.Text;
    using EasyCaching.Core.Internal;
    using EasyCaching.Core.Serialization;
    using Microsoft.Extensions.Options;
    using Newtonsoft.Json;

    /// <summary>
    /// Default json serializer.
    /// </summary>
    public class DefaultJsonSerializer : IEasyCachingSerializer
    {
        /// <summary>
        /// The json serializer.
        /// </summary>
        private readonly JsonSerializer jsonSerializer;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:EasyCaching.Serialization.Json.DefaultJsonSerializer"/> class.
        /// </summary>
        /// <param name="options">Options.</param>
        public DefaultJsonSerializer(IOptions<EasyCachingJsonSerializerOptions> optionsAcc)
        {
            var options = optionsAcc.Value;
            jsonSerializer = new JsonSerializer
            { 
                ReferenceLoopHandling = options.ReferenceLoopHandling,
                TypeNameHandling = options.TypeNameHandling,
                MetadataPropertyHandling = options.MetadataPropertyHandling,
                MissingMemberHandling = options.MissingMemberHandling,
                NullValueHandling = options.NullValueHandling,
                DefaultValueHandling = options.DefaultValueHandling,
                ObjectCreationHandling = options.ObjectCreationHandling,
                PreserveReferencesHandling = options.PreserveReferencesHandling,
                ConstructorHandling = options.ConstructorHandling
            };
        }

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
        #region Mainly For Memcached  
        /// <summary>
        /// Serializes the object.
        /// </summary>
        /// <returns>The object.</returns>
        /// <param name="obj">Object.</param>
        public ArraySegment<byte> SerializeObject(object obj)
        {
            var typeName = TypeHelper.BuildTypeName(obj.GetType()); // Get type 

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

        /// <summary>
        /// Deserializes the object.
        /// </summary>
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
                    var type = Type.GetType(typeName, throwOnError: true);// Get type

                    // read object
                    jr.Read();
                    return jsonSerializer.Deserialize(jr, type);
                }
                else
                {
                    throw new InvalidDataException("JsonTranscoder only supports [\"TypeName\", object]");
                }
            }
        }
        #endregion           
    }
}
