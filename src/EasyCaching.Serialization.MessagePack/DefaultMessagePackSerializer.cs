namespace EasyCaching.Serialization.MessagePack
{
    using System;
    using EasyCaching.Core.Serialization;
    using global::MessagePack;
    using global::MessagePack.Resolvers;

    /// <summary>
    /// Default messagepack serializer.
    /// </summary>
    public class DefaultMessagePackSerializer : IEasyCachingSerializer
    {
        /// <summary>
        /// The name.
        /// </summary>
        private readonly string _name;

        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="T:EasyCaching.Serialization.MessagePack.DefaultMessagePackSerializer"/> class.
        /// </summary>
        /// <param name="name">Name.</param>
        public DefaultMessagePackSerializer(string name)
        {
            _name = name;
            MessagePackSerializer.SetDefaultResolver(ContractlessStandardResolver.Instance);
        }

        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>The name.</value>
        public string Name => _name;

        /// <summary>
        /// Deserialize the specified bytes.
        /// </summary>
        /// <returns>The deserialize.</returns>
        /// <param name="bytes">Bytes.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public T Deserialize<T>(byte[] bytes)
        {
            return MessagePackSerializer.Deserialize<T>(bytes);
        }

        /// <summary>
        /// Deserialize the specified bytes.
        /// </summary>
        /// <returns>The deserialize.</returns>
        /// <param name="bytes">Bytes.</param>
        /// <param name="type">Type.</param>
        public object Deserialize(byte[] bytes, Type type)
        {
            return MessagePackSerializer.NonGeneric.Deserialize(type, bytes);
        }

        /// <summary>
        /// Serialize the specified value.
        /// </summary>
        /// <returns>The serialize.</returns>
        /// <param name="value">Value.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public byte[] Serialize<T>(T value)
        {
            return MessagePackSerializer.Serialize(value);
        }

        /// <summary>
        /// Serializes the object.
        /// </summary>
        /// <returns>The object.</returns>
        /// <param name="value">Value.</param>
        public ArraySegment<byte> SerializeObject(object value)
        {
            return MessagePackSerializer.SerializeUnsafe<object>(value, TypelessContractlessStandardResolver.Instance);
        }

        /// <summary>
        /// Deserializes the object.
        /// </summary>
        /// <returns>The object.</returns>
        /// <param name="value">Value.</param>
        public object DeserializeObject(ArraySegment<byte> value)
        {
            return MessagePackSerializer.Deserialize<object>(value, TypelessContractlessStandardResolver.Instance);
        }

    }
}
