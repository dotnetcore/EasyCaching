using MemoryPack;

namespace EasyCaching.Serialization.MemoryPack;

/// <summary>
/// EasyCaching memory pack serializer options.
/// </summary>
public record EasyCachingMemPackSerializerOptions
{
    /// <summary>
    /// Gets or sets the string encoding. (Defaults to <see cref="StringEncoding.Utf8"/>)
    /// </summary>
    /// <value>
    /// The string encoding.
    /// </value>
    public StringEncoding StringEncoding { set; get; } = StringEncoding.Utf8;
}
