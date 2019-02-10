namespace EasyCaching.Core.Serialization
{
    using System;

    /// <summary>
    /// Easy caching serializer.
    /// </summary>
    public interface IEasyCachingSerializer
    {
        /// <summary>
        /// Serialize the specified value.
        /// </summary>
        /// <returns>The serialize.</returns>
        /// <param name="value">Value.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        byte[] Serialize<T>(T value);

        /// <summary>
        /// Deserialize the specified bytes.
        /// </summary>
        /// <returns>The deserialize.</returns>
        /// <param name="bytes">Bytes.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        T Deserialize<T>(byte[] bytes);

        /// <summary>
        /// Serializes the object.
        /// </summary>
        /// <returns>The object.</returns>
        /// <param name="obj">Object.</param>
        ArraySegment<byte> SerializeObject(object obj);

        /// <summary>
        /// Deserializes the object.
        /// </summary>
        /// <returns>The object.</returns>
        /// <param name="value">Value.</param>
        object DeserializeObject(ArraySegment<byte> value);
    }
}
