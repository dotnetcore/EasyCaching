# Introduction 

When we use distributed caching, it's inevitable to serialize the contents of the cache.

How do you serialize the cached content? Normally, serializing an object to byte[] would be the first choice. This is also the implementation of EasyCaching.

Serialization use
EasyCaching currently offers 5 serialization options, Newtonsoft.Json, MessagePack, System.Text.Json, MemoryPack and Protobuf.

~~The BinaryFormatter is the default, without adding any third-party serialized packages.~~

There is a Breaking Change you should take a look.

> In the version of < 0.6.0, only one serialization method can be used. Since EasyCaching supports the creation of multiple different Provider instances, support for different serialization methods for different providers is essential, so >= 0.6.0 version, named serialization selection is supported.

> BinaryFormatter was removed by EasyCaching v1.7.0, due to after version .net 5 , it will no longer support.