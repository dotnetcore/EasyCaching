using MemoryPack;

namespace EasyCaching.Serialization.MemoryPack;

/// <summary>
/// EasyCaching memory pack serializer options.
/// </summary>
public record EasyCachingMemPackSerializerOptions
{
    public StringEncoding StringEncoding { set; get; }
}


