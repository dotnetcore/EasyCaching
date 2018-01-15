namespace EasyCaching.Core
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

        ///// <summary>
        ///// Deserialize the specified bytes and target.
        ///// </summary>
        ///// <returns>The deserialize.</returns>
        ///// <param name="bytes">Bytes.</param>
        ///// <param name="target">Target.</param>
        //object Deserialize(byte[] bytes, Type target);

        /// <summary>
        /// Deserialize the specified bytes.
        /// </summary>
        /// <returns>The deserialize.</returns>
        /// <param name="bytes">Bytes.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        T Deserialize<T>(byte[] bytes);

        ArraySegment<byte> SerializeObject(object obj);

        object DeserializeObject(ArraySegment<byte> value);
    }
}
