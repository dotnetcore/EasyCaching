# DefaultJsonSerializer

DefaultJsonSerializer is a serializer based on **System.Text.Json**.

# How to Use?

## Install the package via Nuget

```
Install-Package EasyCaching.Serialization.SystemTextJson
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
            // with a default name [json]
            options.WithSystemTextJson();

            // with a custom name [myname]
            options.WithSystemTextJson("myname");               

            // add some serialization settings
            Action<EasyCachingJsonSerializerOptions> easycaching = x => 
            {

            };
            options.WithSystemTextJson(easycaching, "easycaching_setting");
        });
    }
}
```
