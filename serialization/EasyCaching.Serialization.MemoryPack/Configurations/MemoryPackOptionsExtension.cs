using EasyCaching.Core.Configurations;
using EasyCaching.Core.Serialization;
using MemoryPack;
using Microsoft.Extensions.DependencyInjection;

namespace EasyCaching.Serialization.MemoryPack;

/// <summary>
/// MemoryPack options extension.
/// </summary>
internal sealed class MemoryPackOptionsExtension : IEasyCachingOptionsExtension
{
    /// <summary>
    /// The name.
    /// </summary>
    private readonly string _name;

    /// <summary>
    /// The configure.
    /// </summary>
    private readonly Action<EasyCachingMemPackSerializerOptions> _configure;

    /// <summary>
    /// Initializes a new instance of the <see cref="T:EasyCaching.Serialization.MemoryPack.MemoryPackOptionsExtension"/> class.
    /// </summary>
    /// <param name="name">Name.</param>
    /// <param name="configure">Configure.</param>
    public MemoryPackOptionsExtension(string name, Action<EasyCachingMemPackSerializerOptions> configure)
    {
        _name = name;
        _configure = configure;
    }

    /// <summary>
    /// Adds the services.
    /// </summary>
    /// <param name="services">Services.</param>
    public void AddServices(IServiceCollection services)
    {
        Action<EasyCachingMemPackSerializerOptions> configure = _configure ?? (_ => { });

        services.AddOptions();
        services.Configure(_name, configure);

        services.AddSingleton<IEasyCachingSerializer, DefaultMemoryPackSerializer>(services =>
        {
            var optionsMon = services.GetRequiredService<Microsoft.Extensions.Options.IOptionsMonitor<EasyCachingMemPackSerializerOptions>>();
            var easyCachingOptions = optionsMon.Get(_name);

            var options = MemoryPackSerializerOptions.Default;
            typeof(MemoryPackSerializerOptions)
                .GetProperty(nameof(MemoryPackSerializerOptions.StringEncoding))
                .SetValue(options, easyCachingOptions.StringEncoding);

            return new DefaultMemoryPackSerializer(_name, options);
        });
    }
}
