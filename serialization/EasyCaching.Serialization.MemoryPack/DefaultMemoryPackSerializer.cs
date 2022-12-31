using EasyCaching.Core.Serialization;
using MemoryPack;

namespace EasyCaching.Serialization.MemoryPack;

/// <summary>
/// Default MemoryPack serializer
/// </summary>
public class DefaultMemoryPackSerializer : IEasyCachingSerializer
{
    private readonly string _name;
    private readonly MemoryPackSerializerOptions _memoryPackSerializerOptions;

    public string Name => _name;

    public DefaultMemoryPackSerializer(string name, MemoryPackSerializerOptions options = null)
    {
        _name = name;
        _memoryPackSerializerOptions = options;
    }

    public T Deserialize<T>(byte[] bytes) => MemoryPackSerializer.Deserialize<T>(bytes, _memoryPackSerializerOptions);
    public object Deserialize(byte[] bytes, Type type) => MemoryPackSerializer.Deserialize(type, bytes, _memoryPackSerializerOptions);
    public object DeserializeObject(ArraySegment<byte> value) => throw new NotImplementedException("this is not supported in MemoryPack serializer");
    public byte[] Serialize<T>(T value) => MemoryPackSerializer.Serialize(value, _memoryPackSerializerOptions);

    public ArraySegment<byte> SerializeObject(object obj)
    {
        var bytes = MemoryPackSerializer.Serialize(obj.GetType(), obj, _memoryPackSerializerOptions);
        return new ArraySegment<byte>(bytes);
    }
}

