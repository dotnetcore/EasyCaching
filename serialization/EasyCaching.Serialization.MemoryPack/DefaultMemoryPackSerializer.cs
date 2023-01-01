using System;
using System.Text.Json;
using System.Threading;
using EasyCaching.Core.Serialization;
using MemoryPack;
using MemoryPack.Internal;
using Microsoft.Extensions.Options;

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

    /// <summary>
    /// Deserializes the object. @jy
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="buf"></param>
    /// <returns></returns>
    public T Deserialize<T>(ReadOnlySpan<byte> buf)
    {
        return MemoryPackSerializer.Deserialize<T>(buf);
    }

    /// <summary>
    /// Serialize the specified value.
    /// </summary>
    /// <param name="stream"></param>
    /// <param name="value">Value.</param>
    /// <typeparam name="T">The 1st type parameter.</typeparam>
    public void Serialize<T>(Stream stream, T value)
    {
        ReusableLinkedArrayBufferWriter tempWriter = ReusableLinkedArrayBufferWriterPool.Rent();
        try
        {
            MemoryPackSerializer.Serialize(in tempWriter, in value);

            //TODO:待实现同步方法
            tempWriter.WriteToAndResetAsync(stream, default).GetAwaiter().GetResult();
            stream.Flush();
        }
        finally
        {
            ReusableLinkedArrayBufferWriterPool.Return(tempWriter);
        }
    }
}

