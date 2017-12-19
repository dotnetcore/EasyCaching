namespace EasyCaching.Core.Serialization
{
    /// <summary>
    /// Default json serializer.
    /// </summary>
    public class DefaultJsonSerializer : ISerializer
    {
        /// <summary>
        /// Deserialize the specified string.
        /// </summary>
        /// <returns>The deserialize.</returns>
        /// <param name="str">String.</param>
        public object Deserialize(string str)
        {
            return Newtonsoft.Json.JsonConvert.DeserializeObject(str);
        }

        /// <summary>
        /// Deserialize the specified string.
        /// </summary>
        /// <returns>The deserialize.</returns>
        /// <param name="str">String.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public T Deserialize<T>(string str)
        {
            return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(str);
        }

        /// <summary>
        /// Serialize the specified object.
        /// </summary>
        /// <returns>The serialize.</returns>
        /// <param name="obj">Object.</param>
        public string Serialize(object obj)
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(obj);
        }
    }
}
