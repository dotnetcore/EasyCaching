namespace EasyCaching.Serialization.Json;

using System;
using EasyCaching.Core.Configurations;
using EasyCaching.Core.Serialization;
using EasyCaching.Serialization.MemoryPack;
using global::MemoryPack;
using Microsoft.Extensions.DependencyInjection;

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
        this._name = name;
        this._configure = configure;
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

        services.AddSingleton<IEasyCachingSerializer, DefaultMemoryPackSerializer>(x =>
        {
            var optionsMon = x.GetRequiredService<Microsoft.Extensions.Options.IOptionsMonitor<EasyCachingMemPackSerializerOptions>>();
            var easyCachingOptions = optionsMon.Get(_name);
            var options = new MemoryPackSerializerOptions { StringEncoding = easyCachingOptions.StringEncoding };
            return new DefaultMemoryPackSerializer(_name, options);
        });
    }
}
