namespace EasyCaching.Serialization.MessagePack
{
    using System;
    using System.IO;
    using EasyCaching.Core.Serialization;
    using global::MessagePack;
    using global::MessagePack.Resolvers;

    /// <summary>
    /// Default messagepack serializer.
    /// </summary>
    public class DefaultMessagePackSerializer : IEasyCachingSerializer
    {
        readonly Microsoft.IO.RecyclableMemoryStreamManager _recManager = new Microsoft.IO.RecyclableMemoryStreamManager();


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

        /// <summary>
        /// Deserializes the object. @jy
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="buf"></param>
        /// <returns></returns>
        public T Deserialize<T>(ReadOnlySpan<byte> buf)
        {
            using (var stream = _recManager.GetStream(buf))
            {
                return _options.EnableCustomResolver
                    ? MessagePackSerializer.Deserialize<T>(stream, MessagePackSerializerOptions.Standard.WithResolver(_options.CustomResolvers))
                    : MessagePackSerializer.Deserialize<T>(stream, TypelessContractlessStandardResolver.Options);
            }
        }

        /// <summary>
        /// Serialize the specified value.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="value">Value.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public void Serialize<T>(Stream stream, T value)
        {
            if (_options.EnableCustomResolver)
            {
                MessagePackSerializer.Serialize(stream, value, MessagePackSerializerOptions.Standard.WithResolver(_options.CustomResolvers));
            }
            else
            {
                MessagePackSerializer.Serialize(stream, value);
            }
        }
    }
}
