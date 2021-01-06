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

        services.AddEasyCaching(options => 
        {
            // with a default name [json]
            options.WithJson();

            // with a custom name [myname]
            options.WithJson("myname");               

            // add some serialization settings
            Action<EasyCaching.Serialization.Json.EasyCachingJsonSerializerOptions> easycaching = x => 
            {
                x.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore;
            };
            options.WithJson(easycaching, "easycaching_setting");

            // add some serialization settings
            // after version 0.8.1, full control of JsonSerializerSettings
            Action<Newtonsoft.Json.JsonSerializerSettings> jsonNET = x =>
            {
                x.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore;
                x.ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver();
            };
            options.WithJson(jsonNET, "json.net_setting");
        });
    }
}
```
