namespace EasyCaching.Memcached
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using EasyCaching.Core;
    using EasyCaching.Core.Serialization;
    using Enyim.Caching.Memcached;

    /// <summary>
    /// EasyCaching transcoder.
    /// </summary>
    public class EasyCachingTranscoder : DefaultTranscoder
    {
        /// <summary>
        /// The name.
        /// </summary>
        private readonly string _name;        

        /// <summary>
        /// The serializer.
        /// </summary>
        private readonly IEasyCachingSerializer _serializer;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:EasyCaching.Memcached.EasyCachingTranscoder"/> class.
        /// </summary>
        /// <param name="name">Name.</param>
        /// <param name="options">Options.</param>
        /// <param name="serializers">Serializers.</param>
        public EasyCachingTranscoder(string name, MemcachedOptions options, IEnumerable<IEasyCachingSerializer> serializers)
        {

            if (string.IsNullOrWhiteSpace(options.SerializerName))
            {
                this._name = name;
                this._serializer = serializers.FirstOrDefault(x => x.Name.Equals(_name)) ?? serializers.Single(x => x.Name.Equals(EasyCachingConstValue.DefaultSerializerName));
            }
            else
            {
                this._name = options.SerializerName;
                this._serializer = serializers.Single(x=>x.Name.Equals(options.SerializerName));
            }

            //this._serializer = !string.IsNullOrWhiteSpace(options.SerializerName)
            //    ? serializers.Single(x => x.Name.Equals(options.SerializerName))
            //    : serializers.FirstOrDefault(x => x.Name.Equals(_name)) ?? serializers.Single(x => x.Name.Equals(EasyCachingConstValue.DefaultSerializerName));
        }

        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>The name.</value>
        public string Name => _name;

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
