# DefaultMemoryPackSerializer

DefaultMemoryPackSerializer is is a serializer based on **MemoryPack**.

# How to Use?

## Install the package via Nuget

```
Install-Package EasyCaching.Serialization.MemoryPack
```

## Configuration

```
public class Startup
{
    //others...

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddEasyCaching(options => 
        {
            // with a default name [mempack]
            options.WithMemoryPack();

            // with a custom name [myname]
            options.WithMemoryPack("myname");

            // add some serialization settings
            options.WithMemoryPack(x => 
            {
                x.StringEncoding = StringEncoding.Utf8;
            }, "cus");
        });
    }
}
```
