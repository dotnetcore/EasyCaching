# DefaultMessagePackSerializer

DefaultMessagePackSerializer is is a serializer based on **MessagePack**.

# How to Use?

## Install the package via Nuget

```
Install-Package EasyCaching.Serialization.MessagePack
```

## Configuration

```
public class Startup
{
    //others...

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddMvc();

        services.AddEasyCaching(options => 
        {
            // with a default name [mskpack]
            options.WithMessagePack();

            // with a custom name [myname]
            options.WithMessagePack("myname");

            // add some serialization settings
            options.WithMessagePack(x => 
            {
                // If this setting is true, you should custom the resolver by yourself
                // If this setting is false, also the default behavior, it will use ContractlessStandardResolver only
                x.EnableCustomResolver = true;
            }, "cus");
        });
    }
}
```

## Precautions

If you should serialize Datetime and do not use UTC time, it will lead to the loss of time zone information.

In the version of <= v0.8.0, the default implementation uses the ContractlessStandardResolver Resolver. This is chosen because there is no need to add features to each class and property for seamless integration.

In the v0.8.1 version, a configuration of EnableCustomResolver was added. When this configuration is true, it means that the custom Resolver is enabled, and vice versa.

Time issues can be solved using a combination of NativeDateTimeResolver+ContractlessStandardResolver. The following are specific examples.

```cs
public void ConfigureServices(IServiceCollection services)
{
    services.AddControllers();

    services.AddEasyCaching(option =>
    {
        option.UseCSRedis(config =>
        {
            config.DBConfig = new EasyCaching.CSRedis.CSRedisDBOptions
            {
                ConnectionStrings = new List<string> { "127.0.0.1:6379,defaultDatabase=11,poolsize=10" }
            };
            config.SerializerName = "mymsgpack";
        }, "redis1");

        // use MessagePack
        option.WithMessagePack( x => 
        {
            x.EnableCustomResolver = true; 

            // x.CustomResolvers = CompositeResolver.Create(
            //     // This can solve DateTime time zone problem
            //     NativeDateTimeResolver.Instance,
            //     ContractlessStandardResolver.Instance
            // );

            // due to api changed
            x.CustomResolvers = CompositeResolver.Create(new MessagePack.IFormatterResolver[]
            {
                // This can solve DateTime time zone problem
                NativeDateTimeResolver.Instance,
                ContractlessStandardResolver.Instance 
            });
        },"mymsgpack");
    });
}
```

