namespace EasyCaching.Memcached
{
    using EasyCaching.Core;
    using EasyCaching.Core.Serialization;
    using Enyim.Caching.Memcached;
    using System;

    /// <summary>
    /// EasyCaching transcoder.
    /// </summary>
    public class EasyCachingTranscoder : DefaultTranscoder
    {    
        /// <summary>
        /// The serializer.
        /// </summary>
        private readonly IEasyCachingSerializer _serializer;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:EasyCaching.Memcached.FormatterTranscoder"/> class.
        /// </summary>
        /// <param name="serializer">Serializer.</param>
        public EasyCachingTranscoder(IEasyCachingSerializer serializer)
        {
            this._serializer = serializer;
        }        

        /// <summary>
        /// Serializes the object.
        /// </summary>
        /// <returns>The object.</returns>
        /// <param name="value">Value.</param>
        protected override ArraySegment<byte> SerializeObject(object value)
        {            
            return _serializer.SerializeObject(value);            
        }

        /// <summary>
        /// Deserialize the specified item.
        /// </summary>
        /// <returns>The deserialize.</returns>
        /// <param name="item">Item.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public override T Deserialize<T>(CacheItem item)
        {
            return (T)base.Deserialize(item);
        }

        /// <summary>
        /// Deserializes the object.
        /// </summary>
        /// <returns>The object.</returns>
        /// <param name="value">Value.</param>
        protected override object DeserializeObject(ArraySegment<byte> value)
        {
            return _serializer.DeserializeObject(value);            
        }
    }
}
