using MemoryPack;
using EasyCaching.Core.Configurations;
using EasyCaching.Serialization.Json;

namespace EasyCaching.Serialization.MemoryPack;

/// <summary>
/// Easy caching options extensions.
/// </summary>
public static class EasyCachingOptionsExtensions
{
    /// <summary>
    /// Withs the memory pack serializer.
    /// </summary>
    /// <param name="options">Options.</param>
    /// <param name="name">The name of this serializer instance.</param>        
    public static EasyCachingOptions WithMemoryPack(this EasyCachingOptions options, string name = "mempack")
    {
        options.RegisterExtension(new MemoryPackOptionsExtension(name, null));

        return options;
    }

    /// <summary>
    /// Withs the memory pack serializer.
    /// </summary>        
    /// <param name="options">Options.</param>
    /// <param name="serializerOptions">Configure serializer settings.</param>
    /// <param name="name">The name of this serializer instance.</param>     
    public static EasyCachingOptions WithMemoryPack(this EasyCachingOptions options, Action<EasyCachingMemPackSerializerOptions> serializerOptions, string name)
    {
        options.RegisterExtension(new MemoryPackOptionsExtension(name, serializerOptions));

        return options;
    }
}
