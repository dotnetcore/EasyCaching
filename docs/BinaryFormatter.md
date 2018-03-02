## BinaryFormatter

This is the default Serializer in EasyCaching when we use **EasyCaching.Redis**. 

**EasyCaching.Memcached** is not use BinaryFormatter for the default Serializer ! 

Because [EnyimMemcachedCore](https://github.com/cnblogs/EnyimMemcachedCore) use [Bson](https://github.com/cnblogs/EnyimMemcachedCore/blob/dotnetcore/Enyim.Caching/Memcached/Transcoders/DefaultTranscoder.cs) as its defalut Serializer. 

Also, EnyimMemcachedCore implements BinaryFormatterTranscoder based on BinaryFormatter.

