namespace EasyCaching.Core.Serialization
{
    using System;
    using System.IO;
    using System.Runtime.Serialization.Formatters.Binary;

    /// <summary>
    /// Default binary formatter serializer.
    /// </summary>
    public class DefaultBinaryFormatterSerializer : IEasyCachingSerializer
    {
        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>The name.</value>
        public string Name => EasyCachingConstValue.DefaultSerializerName;

        /// <summary>
        /// Deserialize the specified bytes.
        /// </summary>
        /// <returns>The deserialize.</returns>
        /// <param name="bytes">Bytes.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public T Deserialize<T>(byte[] bytes)
        {
            if (bytes.Length == 0)
            {
                return (T)(object) null;
            }
            
            using (var ms = new MemoryStream(bytes))
            {
                return (T)(new BinaryFormatter().Deserialize(ms));
            }
        }

        /// <summary>
        /// Deserialize the specified bytes.
        /// </summary>
        /// <returns>The deserialize.</returns>
        /// <param name="bytes">Bytes.</param>
        /// <param name="type">Type.</param>
        public object Deserialize(byte[] bytes, Type type)
        {
            if (bytes.Length == 0)
            {
                return null;
            }
            
            using (var ms = new MemoryStream(bytes))
            {
                return (new BinaryFormatter().Deserialize(ms));
            }
        }

        /// <summary>
        /// Deserializes the object.
        /// </summary>
        /// <returns>The object.</returns>
        /// <param name="value">Value.</param>
        public object DeserializeObject(ArraySegment<byte> value)
        {
            using (var ms = new MemoryStream(value.Array, value.Offset, value.Count))
            {
                return new BinaryFormatter().Deserialize(ms);
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
            if (value == null)
            {
                return Array.Empty<byte>();
            }
            
            using (var ms = new MemoryStream())
            {
                new BinaryFormatter().Serialize(ms, value);
                return ms.ToArray();
            }
        }

        /// <summary>
        /// Serializes the object.
        /// </summary>
        /// <returns>The object.</returns>
        /// <param name="obj">Object.</param>
        public ArraySegment<byte> SerializeObject(object obj)
        {
            if (obj == null)
            {
                return new ArraySegment<byte>(Array.Empty<byte>());
            }
            
            using (var ms = new MemoryStream())
            {
                new BinaryFormatter().Serialize(ms, obj);
                return new ArraySegment<byte>(ms.GetBuffer(), 0, (int)ms.Length);
            }
        }
    }
}
