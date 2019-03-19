# DefaultJsonSerializer

DefaultJsonSerializer is a serializer based on **Newtonsoft.Json**.

# How to Use?

## Install the package via Nuget

```
Install-Package EasyCaching.Serialization.Json
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
            //specify to use json serializer
            option.WithJson();
        });
    }
}
```
