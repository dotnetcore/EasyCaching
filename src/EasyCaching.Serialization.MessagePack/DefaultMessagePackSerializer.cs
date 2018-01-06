namespace EasyCaching.Serialization.MessagePack
{
    using EasyCaching.Core;
    //using MessagePack;
    using System;
    using global::MessagePack;
    using global::MessagePack.Resolvers;

    /// <summary>
    /// Default messagepack serializer.
    /// </summary>
    public class DefaultMessagePackSerializer : IEasyCachingSerializer
    {
        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="T:EasyCaching.Serialization.MessagePack.DefaultMessagePackSerializer"/> class.
        /// </summary>
        public DefaultMessagePackSerializer()
        {
            MessagePackSerializer.SetDefaultResolver(ContractlessStandardResolver.Instance);
        }

        ///// <summary>
        ///// Deserialize the specified bytes and target.
        ///// </summary>
        ///// <returns>The deserialize.</returns>
        ///// <param name="bytes">Bytes.</param>
        ///// <param name="target">Target.</param>
        //public object Deserialize(byte[] bytes, Type target)
        //{            
        //    return MessagePackSerializer.Deserialize<object>(bytes);
        //}

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
        /// Serialize the specified value.
        /// </summary>
        /// <returns>The serialize.</returns>
        /// <param name="value">Value.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public byte[] Serialize<T>(T value)
        {
            return MessagePackSerializer.Serialize(value);
        }
    }
}
