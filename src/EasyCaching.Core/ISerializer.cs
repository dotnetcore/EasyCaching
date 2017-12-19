namespace EasyCaching.Core
{
    /// <summary>
    /// Serializer.
    /// </summary>
    public interface ISerializer
    {
        /// <summary>
        /// Serialize the specified item.
        /// </summary>
        /// <returns>The serialize.</returns>
        /// <param name="obj">Object.</param>
        string Serialize(object obj);

        /// <summary>
        /// Deserialize the specified str.
        /// </summary>
        /// <returns>The deserialize.</returns>
        /// <param name="str">String.</param>
        object Deserialize(string str);

        /// <summary>
        /// Deserialize the specified str.
        /// </summary>
        /// <returns>The deserialize.</returns>
        /// <param name="str">String.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        T Deserialize<T>(string str);
    }
}
