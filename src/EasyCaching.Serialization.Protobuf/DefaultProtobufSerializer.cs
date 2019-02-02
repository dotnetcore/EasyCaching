namespace EasyCaching.Serialization.Protobuf
{
    using System;
    using System.IO;
    using System.Text;
    using EasyCaching.Core.Internal;
    using EasyCaching.Core.Serialization;
    using ProtoBuf;

    /// <summary>
    /// Default protobuf serializer.
    /// </summary>
    public class DefaultProtobufSerializer : IEasyCachingSerializer
    {
        /// <summary>
        /// Deserialize the specified bytes.
        /// </summary>
        /// <returns>The deserialize.</returns>
        /// <param name="bytes">Bytes.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public T Deserialize<T>(byte[] bytes)
        {
            using (MemoryStream ms = new MemoryStream(bytes))
            {
                return Serializer.Deserialize<T>(ms);
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
                Serializer.Serialize<T>(ms, value);
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
            using (var ms = new MemoryStream())
            {
                WriteType(ms, obj.GetType());
                Serializer.NonGeneric.Serialize(ms, obj);

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
            var raw = value.Array;
            var count = value.Count;
            var offset = value.Offset;
            var type = ReadType(raw, ref offset, ref count);

            using (var ms = new MemoryStream(raw, offset, count, writable: false))
            {
                return Serializer.NonGeneric.Deserialize(type, ms);
            }
        }

        /// <summary>
        /// Reads the type.
        /// </summary>
        /// <returns>The type.</returns>
        /// <param name="buffer">Buffer.</param>
        /// <param name="offset">Offset.</param>
        /// <param name="count">Count.</param>
        private Type ReadType(byte[] buffer, ref int offset, ref int count)
        {
            if (count < 4) throw new EndOfStreamException();

            // len is size of header typeName(string)
            var len = (int)buffer[offset++]
                    | (buffer[offset++] << 8)
                    | (buffer[offset++] << 16)
                    | (buffer[offset++] << 24);
            count -= 4; // count is message total size, decr typeName length(int)
            if (count < len) throw new EndOfStreamException();
            var keyOffset = offset;
            offset += len; // skip typeName body size
            count -= len; // decr typeName body size

            // avoid encode string
            var key = new ArraySegment<byte>(buffer, keyOffset, len);

            var typeName = Encoding.UTF8.GetString(key.Array, key.Offset, key.Count);

            return Type.GetType(typeName, throwOnError: true);
        }

        /// <summary>
        /// Writes the type.
        /// </summary>
        /// <param name="ms">Ms.</param>
        /// <param name="type">Type.</param>
        private void WriteType(MemoryStream ms, Type type)
        {
            var typeName = TypeHelper.BuildTypeName(type);
            var typeArray = Encoding.UTF8.GetBytes(typeName);

            var len = typeArray.Length;
            // BinaryWrite Int32
            ms.WriteByte((byte)len);
            ms.WriteByte((byte)(len >> 8));
            ms.WriteByte((byte)(len >> 16));
            ms.WriteByte((byte)(len >> 24));
            // BinaryWrite String
            ms.Write(typeArray, 0, len);
        }
        #endregion
    }
}
