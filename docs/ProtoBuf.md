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

        services.AddEasyCaching(option =>
        {
            //specify to use protobuf serializer
            option.WithProtobuf();
        });
    }
}
```
