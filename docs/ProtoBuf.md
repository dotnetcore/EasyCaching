# DefaultProtobufSerializer

DefaultProtobufSerializer is a serializer based on **protobuf-net**.

# How to Use?

## Install the package via Nuget

```
Install-Package EasyCaching.Serialization.Protobuf
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
            // with a default name [proto]
            options.WithProtobuf();

            // with a custom name [myname]
            options.WithProtobuf("myname");
        });
    }
}
```
