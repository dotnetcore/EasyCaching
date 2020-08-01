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
        /// The options.
        /// </summary>
        private readonly EasyCachingMsgPackSerializerOptions _options;

        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="T:EasyCaching.Serialization.MessagePack.DefaultMessagePackSerializer"/> class.
        /// </summary>
        /// <param name="name">Name.</param>
        /// <param name="options">Options.</param>
        public DefaultMessagePackSerializer(string name, EasyCachingMsgPackSerializerOptions options)
        {
            _name = name;
            _options = options;

            if (!options.EnableCustomResolver)
            {
                MessagePackSerializer.DefaultOptions = MessagePackSerializerOptions.Standard.WithResolver(ContractlessStandardResolver.Instance);
            }
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
            return _options.EnableCustomResolver
                ? MessagePackSerializer.Deserialize<T>(bytes, MessagePackSerializerOptions.Standard.WithResolver(_options.CustomResolvers))
                : MessagePackSerializer.Deserialize<T>(bytes);
        }

        /// <summary>
        /// Deserialize the specified bytes.
        /// </summary>
        /// <returns>The deserialize.</returns>
        /// <param name="bytes">Bytes.</param>
        /// <param name="type">Type.</param>
        public object Deserialize(byte[] bytes, Type type)
        {
            return _options.EnableCustomResolver
                ? MessagePackSerializer.Deserialize(type, bytes, MessagePackSerializerOptions.Standard.WithResolver(_options.CustomResolvers))
                : MessagePackSerializer.Deserialize(type, bytes);
        }

        /// <summary>
        /// Serialize the specified value.
        /// </summary>
        /// <returns>The serialize.</returns>
        /// <param name="value">Value.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public byte[] Serialize<T>(T value)
        {
            return _options.EnableCustomResolver
                ? MessagePackSerializer.Serialize(value, MessagePackSerializerOptions.Standard.WithResolver(_options.CustomResolvers))
                : MessagePackSerializer.Serialize(value);
        }

        /// <summary>
        /// Serializes the object.
        /// </summary>
        /// <returns>The object.</returns>
        /// <param name="value">Value.</param>
        public ArraySegment<byte> SerializeObject(object value)
        {
            byte[] bytes = MessagePackSerializer.Serialize<object>(value, TypelessContractlessStandardResolver.Options);
            return new ArraySegment<byte>(bytes);
        }

        /// <summary>
        /// Deserializes the object.
        /// </summary>
        /// <returns>The object.</returns>
        /// <param name="value">Value.</param>
        public object DeserializeObject(ArraySegment<byte> value)
        {
            return MessagePackSerializer.Deserialize<object>(value, TypelessContractlessStandardResolver.Options);
        }
    }
}
