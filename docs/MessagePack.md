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

        services.AddEasyCaching(option =>
        {
            //specify to use messagepack serializer
            option.WithMessagePack();
        });
    }
}
```
